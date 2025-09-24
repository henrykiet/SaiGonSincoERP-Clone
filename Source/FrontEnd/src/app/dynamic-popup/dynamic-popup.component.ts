import {
    Component,
    Input,
    ElementRef,
    HostListener,
    OnInit,
    OnChanges,
    SimpleChanges,
    ViewChild,
    ChangeDetectorRef,
} from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { RouterModule } from '@angular/router'
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { PageMetadata, ApiResponse, GirdInitData, Field, CalculationEngine, ValidationEngine, CalculationRule } from '../models'
import { Router, ActivatedRoute } from '@angular/router'
import { FileAttachmentComponent, FileAttachmentData } from '../components/file-attachment/file-attachment.component'
import { FileHandleComponent } from '../file-handle/file-handle.component'
import { DynamicLookupComponent } from '../dynamic-lookup/dynamic-lookup.component'
import { TaxcodeInputComponent } from '../shared/taxcode-input/taxcode-input.component'
import { environment } from '../../environments/environment'
import { TranslateService, TranslateModule } from '@ngx-translate/core'
import { ArrowNavigationDirective } from '../components/directive/arrow-navigation.directive';

@Component({
    selector: 'app-popup',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        FileHandleComponent,
        FileAttachmentComponent,
        DynamicLookupComponent,
        TranslateModule,
        TaxcodeInputComponent,
        ArrowNavigationDirective,
    ],
    templateUrl: './dynamic-popup.component.html',
})
export class DynamicPopupComponent implements OnInit {
    @Input({ required: true }) id!: string;
    @Input({ required: true }) name!: string;

    @ViewChild(FileAttachmentComponent) fileAttachmentComponent?: FileAttachmentComponent;

    mode?: string;
    metadata?: PageMetadata;
    girdData?: GirdInitData;
    selectedTab = 0;
    selectedDetailIndex = 0;
    formData: { [key: number]: { [key: string]: any } } = {};
    fileAttachmentData?: FileAttachmentData = undefined;

    detailRowsData: { [tabIndex: number]: { [detailIndex: number]: any[] } } = {};
    filteredDetailRowsData: {
        [tabIndex: number]: { [detailIndex: number]: any[] }
    } = {};

    // Store original primary key values for comparison
    originalPrimaryKeyValues: { [key: string]: any } = {};

    errors: { [key: string]: string } = {};
    columnFiltersData: {
        [tabIndex: number]: { [detailIndex: number]: { [key: string]: string } }
    } = {};
    filterMode: 'all' | 'any' = 'all';

    masterAggregates: { [key: string]: any } = {};
    calculatedValues: { [key: string]: any } = {};

    masterSubtotals: { [key: string]: any } = {};

    // Dynamic summary
    currentSummaryData: any = null;
    // Flag để track việc đang load dữ liệu ban đầu
    private isInitialLoading = false;
    private isResizing: boolean = false;
    private resizingColumn: string = '';
    private startX: number = 0;
    private startWidth: number = 0;
    private columnWidths: { [key: string]: number } = {};
    private minColumnWidth: number = 50;
    private maxColumnWidth: number = 1000;
    detailErrors: Record<number, Record<string, string>> = {};

    constructor(
        private http: HttpClient,
        private router: Router,
        private route: ActivatedRoute,
        public translate: TranslateService,
        private cdr: ChangeDetectorRef
    ) {
        this.translate.setDefaultLang(localStorage.getItem("language") ?? "vi");
    }

    async ngOnInit(): Promise<void> {
        this.isInitialLoading = true;

        // Expose debug methods to global window for console access
        (window as any).debugPopup = () => this.debugState();
        (window as any).debugLookupField = (rowIndex: number, fieldKey: string) => this.debugLookupField(rowIndex, fieldKey);
        (window as any).debugUIAfterChange = (rowIndex: number) => this.debugUIAfterChange(rowIndex);
        (window as any).debugAutoGenerate = () => this.debugAutoGenerateFields();

        const actionStr = localStorage.getItem(`action_${this.id}`);


        if (actionStr) localStorage.removeItem(`param_${this.id}`)
        const girdDataStr = localStorage.getItem(`param_${this.id}`)


        this.girdData = girdDataStr
            ? (JSON.parse(girdDataStr) as GirdInitData)
            : undefined

        if (this.girdData) {
            this.http
                .post<ApiResponse>(
                    `${environment.apiUrl}/api/FormConfig/GetFormData`,
                    this.girdData.query.formId
                )
                .subscribe(async (meta) => {
                    this.metadata = meta.Data as PageMetadata
                    this.mode = this.girdData?.mode
                    await this.initializeFormData()
                    this.loadInitialDetailData()
                })
        } else if (actionStr) {
            localStorage.removeItem(`action_${this.id}`)

            this.metadata = JSON.parse(actionStr) as PageMetadata;
            await this.initializeFormData()
            this.loadInitialDetailData()

        } else {

            this.http
                .put<PageMetadata>(
                    `${environment.apiUrl}/api/FormConfig/${this.name}`,
                    null
                )
                .subscribe(async (meta) => {
                    this.metadata = meta
                    await this.initializeFormData()
                    this.loadInitialDetailData()
                })
      }

      if (this.metadata) {
        this.initializeColumnWidths();
      }

    }

    handleOnChange(fieldConfig: any, value: any) {
        const onChange = fieldConfig.onChange
        // Build value array theo thứ tự params
        const values = onChange.params.map((param: string) => {
            // Ưu tiên lấy từ form, nếu không có thì lấy value vừa nhập
            return this.formData?.[this.selectedTab]?.[param] ?? value
        })

        const body = {
            userId: localStorage.getItem('userId'), // hoặc lấy từ context
            unit: localStorage.getItem('unit') ?? 'CTY',
            language: localStorage.getItem('language') ?? 'vi',
            params: onChange.params,
            dataType: onChange.dataType,
            value: values,
            query: onChange.query,
        }

        if (!this.isInitialLoading) {
            this.http
                .post(environment.apiUrl + onChange.api, body)
                .subscribe((res: any) => {
                    if (res && res.data && res.data.length > 0) {
                        const map = onChange.map
                        if (this.formData?.[this.selectedTab]) {
                            Object.keys(map).forEach((fieldKey) => {
                                const apiField = map[fieldKey]
                                if (this.formData[this.selectedTab][fieldKey] !== undefined) {
                                    this.formData[this.selectedTab][fieldKey] =
                                        res.data[0][apiField]
                                }
                            })
                        }
                    }
                })
        }
    }

    handleOnChangeDetail(fieldConfig: any, value: any, rowIndex: number) {
        const onChange = fieldConfig.onChange
        const row = this.currentFilteredDetailRows[rowIndex]

        if (!row) {
            return
        }
        // Set giá trị mới vào row trước khi lấy values
        row[fieldConfig.key] = value
        // Build value array theo thứ tự params
        const values = onChange.params.map((param: string) => {
            // Lấy giá trị từ row sau khi đã set giá trị mới
            const paramValue = row[param] || '';
            return paramValue;
        });

        const body = {
            userId: localStorage.getItem('userId'), // hoặc lấy từ context
            unit: localStorage.getItem('unit') ?? 'CTY',
            language: localStorage.getItem('language') ?? 'vi',
            params: onChange.params,
            dataType: onChange.dataType,
            value: values,
            query: onChange.query,
        }

        // Chỉ trigger onChange khi KHÔNG PHẢI đang initial loading
        if (!this.isInitialLoading) {
            this.http
                .post(environment.apiUrl + onChange.api, body)
                .subscribe({
                    next: (res: any) => {

                        if (res && res.data && res.data.length > 0) {
                            const map = onChange.map

                            // Map dữ liệu vào row hiện tại
                            Object.keys(map).forEach((fieldKey) => {
                                const apiField = map[fieldKey]

                                // Khởi tạo field nếu chưa tồn tại
                                if (!(fieldKey in row)) {
                                    row[fieldKey] = ''
                                }

                                // Gán giá trị từ API response
                                if (res.data[0][apiField] !== undefined) {
                                    const oldValue = row[fieldKey]
                                    row[fieldKey] = res.data[0][apiField]

                                    // Force change detection trigger
                                    /*
                                    if (fieldKey === 'uom') {
                                      this.cdr.detectChanges()
                                      setTimeout(() => {
                                        console.log('UOM field value after timeout:', row[fieldKey])
                                      }, 100)
                                    }
                                    */
                                } else {
                                    console.warn('API field not found:', apiField)
                                }
                            })

                            // Trigger calculations cho tất cả fields được map từ API
                            Object.keys(map).forEach((mappedFieldKey) => {
                                this.triggerRowCalculations(row, mappedFieldKey)
                            })
                        } else {
                            console.warn('No data in API response or empty response:', res)
                        }

                        // Trigger calculations cho field đang thay đổi
                        this.applyRowCalculations(row, fieldConfig)
                        this.triggerRowCalculations(row, fieldConfig.key)

                        // Update master calculations
                        this.updateMasterCalculations()
                        this.calculateAggregateValues()

                        // Force change detection after all mapping complete
                        this.cdr.detectChanges()

                        // Debug: Check field states after API call
                        this.debugRowFields(rowIndex)

                        // Debug UI after change detection
                        this.debugUIAfterChange(rowIndex)
                    },
                    error: (err) => {
                        console.error('Error in handleOnChangeDetail:', err)

                        // Vẫn trigger calculations ngay cả khi API call thất bại
                        this.applyRowCalculations(row, fieldConfig)
                        this.triggerRowCalculations(row, fieldConfig.key)

                        // Update master calculations
                        this.updateMasterCalculations()
                        this.calculateAggregateValues()
                    }
                })
        } else {
            // Nếu đang initial loading, chỉ apply calculations mà không gọi API
            this.applyRowCalculations(row, fieldConfig)
            this.triggerRowCalculations(row, fieldConfig.key)

            // Update master calculations
            this.updateMasterCalculations()
            this.calculateAggregateValues()
        }
    }

    isValidDateInput(value: string): boolean {
        const regex = /^\d{4}-\d{2}-\d{2}$/

        if (!regex.test(value)) return false

        const date = new Date(value)
        return !isNaN(date.getTime()) && date.toISOString().slice(0, 10) === value
    }

    /**
     * Gọi API để lấy số tiếp theo cho field tự động sinh
     * @param controller Tên controller (ví dụ: QuotationPaper)
     * @param field Tên field cần sinh số (ví dụ: voucherNumber)
     * @param formId Tùy chọn: ID form nếu cần
     * @returns Promise<string> Số tiếp theo hoặc chuỗi rỗng nếu lỗi
     */
    async getNextFieldNumber(controller: string, field: string, formId?: string): Promise<string> {
        try {
            let params = new HttpParams()
                .set('controller', controller)
                .set('field', field);

            if (formId) {
                params = params.set('formId', formId);
            }

            const response = await this.http.get<any>(
                `${environment.apiUrl}/api/Dynamic/next-field-number`,
                { params }
            ).toPromise();

            if (response?.success && response?.data) {
                return response.data;
            } else {
                return '';
            }
        } catch (error) {
            return '';
        }
    }

    /**
     * Kiểm tra field có thuộc tính autoGenerate không
     * @param field Field config
     * @returns boolean - Trả về true nếu autoGenerate = true, false nếu không có hoặc = false
     */
    private hasAutoGenerate(field: any): boolean {
        // Kiểm tra thuộc tính autoGenerate có tồn tại và có giá trị true không
        // Nếu không có thuộc tính này thì mặc định là false
        return field && field.hasOwnProperty('autoGenerate') ? field.autoGenerate === true : false;
    }

    /**
     * Kiểm tra giá trị boolean một cách an toàn
     * @param value Giá trị cần kiểm tra
     * @returns boolean - Trả về true chỉ khi value === true
     */
    private isTrueBooleanValue(value: any): boolean {
        return value === true;
    }

    /**
     * Kiểm tra field có thuộc tính readonly không (bao gồm cả autoGenerate)
     * @param field Field config
     * @returns boolean
     */
    isFieldReadonly(field: any): boolean {
        return this.isTrueBooleanValue(field?.readonly) || this.hasAutoGenerate(field);
    }

    /**
     * Debug method để kiểm tra cấu hình autoGenerate của các field
     */
    debugAutoGenerateFields(): void {

        if (!this.metadata) {
            return;
        }


        this.metadata.tabs.forEach((tab, tabIndex) => {
            if (tab.form) {

                tab.form.fields.forEach(field => {
                    const hasAutoGen = this.hasAutoGenerate(field);
                    const isReadonly = this.isFieldReadonly(field);
                    const currentValue = this.formData[tabIndex]?.[field.key];
                });

            }
        });


    }

    /**
     * Kiểm tra có phải đang ở chế độ insert không
     * @returns boolean
     */
    private isInsertMode(): boolean {
        return this.mode === 'insert' || !this.girdData;
    }

    /**
     * Refresh lại giá trị auto-generate cho một field cụ thể
     * @param fieldKey Tên field cần refresh
     * @param tabIndex Index của tab (mặc định là tab hiện tại)
     */
    async refreshAutoGeneratedField(fieldKey: string, tabIndex?: number): Promise<void> {
        if (!this.metadata) return;

        const currentTabIndex = tabIndex ?? this.selectedTab;
        const tab = this.metadata.tabs[currentTabIndex];

        if (!tab?.form) return;

        const field = tab.form.fields.find(f => f.key === fieldKey);
        if (!field || !this.hasAutoGenerate(field)) return;

        try {
            const controller = this.metadata?.controller || this.metadata?.formId || '';
            const generatedValue = await this.getNextFieldNumber(controller, field.key, this.metadata?.formId);

            if (generatedValue) {
                if (!this.formData[currentTabIndex]) {
                    this.formData[currentTabIndex] = {};
                }
                this.formData[currentTabIndex][fieldKey] = generatedValue;
            }
        } catch (error) {
        }
    }

    private async initializeFormData(): Promise<void> {
        if (!this.metadata) return

        // Process each tab
        for (const [index, tab] of this.metadata.tabs.entries()) {
            if (tab.form) {
                // Process each field
                for (const field of tab.form.fields) {
                    let value = (tab.form as any)?.initialData?.[field.key]
                    if (!value && field.type != "lookup" && field.default) {
                        if (field.type == "date" && field.default == "now") {
                            const today = new Date();
                            const yyyy = today.getFullYear();
                            const mm = String(today.getMonth() + 1).padStart(2, '0');
                            const dd = String(today.getDate()).padStart(2, '0');
                            value = `${yyyy}-${mm}-${dd}`;
                        }
                        else
                            value = field.default
                    }
                    // Kiểm tra tự động sinh số cho field có autoGenerate = true
                    if (this.hasAutoGenerate(field) && this.isInsertMode() && !value) {
                        try {
                            const controller = this.metadata?.controller || this.metadata?.formId || '';
                            if (controller) {
                                const generatedValue = await this.getNextFieldNumber(controller, field.key, this.metadata?.formId);
                                if (generatedValue) {
                                    value = generatedValue;
                                }
                            }
                        } catch (error) {

                        }
                    }

                    switch (field.type) {
                        case 'number':
                            if (isNaN(Number(value))) value = ''
                            break
                        case 'date':
                            value = value?.substring(0, 10) ?? ''
                            if (!this.isValidDateInput(value))
                                value = ''
                            break
                        default:
                            break
                    }

                    if (!this.formData[index]) {
                        this.formData[index] = {}
                    }
                    this.formData[index][field.key] = value || ''

                    // Store original primary key values for comparison (only when editing)
                    if (this.girdData && this.metadata?.primaryKey?.includes(field.key)) {
                        this.originalPrimaryKeyValues[field.key] = value || ''
                    }
                }
            }

            // Initialize detail data structures
            if (tab.detail && Array.isArray(tab.detail)) {
                this.detailRowsData[index] = {}
                this.filteredDetailRowsData[index] = {}
                this.columnFiltersData[index] = {}

                tab.detail.forEach((_, detailIndex) => {
                    this.detailRowsData[index][detailIndex] = []
                    this.filteredDetailRowsData[index][detailIndex] = []
                    this.columnFiltersData[index][detailIndex] = {}
                })
            }
        }
    }

    loadInitialDetailData(): void {
        if (!this.metadata) return

        this.metadata.tabs.forEach((tab, tabIndex) => {
            if (tab.detail && Array.isArray(tab.detail)) {
                tab.detail.forEach((detailSection, detailIndex) => {
                    let dateKeys: { [key: string]: boolean } = {}
                    console.warn(detailSection)
                    detailSection.fields.forEach(field => {
                        if (field.type == 'date') {
                            dateKeys[field.key] = true
                        }
                    });
                    if (
                        detailSection.initialData &&
                        Array.isArray(detailSection.initialData)
                    ) {
                        for (let i = 0; i < detailSection.initialData.length; i++) {
                            for (const key in detailSection.initialData[i]) {
                                if (dateKeys[key]) {
                                    detailSection.initialData[i][key] = detailSection.initialData[i][key]?.substring(0, 10) ?? ''
                                }
                            }
                        }

                        this.detailRowsData[tabIndex][detailIndex] = [
                            ...detailSection.initialData,
                        ]

                    } else {
                        this.detailRowsData[tabIndex][detailIndex] = []
                    }
                })
            }
        })

        this.applyFilters()

        // Hoàn thành initial loading
        this.isInitialLoading = false
    }

    // Getter methods for current active detail section
    get currentDetailRows(): any[] {
        return (
            this.detailRowsData[this.selectedTab]?.[this.selectedDetailIndex] || []
        )
    }

    get currentFilteredDetailRows(): any[] {
        return (
            this.filteredDetailRowsData[this.selectedTab]?.[
            this.selectedDetailIndex
            ] || []
        )
    }

    get currentColumnFilters(): { [key: string]: string } {
        return (
            this.columnFiltersData[this.selectedTab]?.[this.selectedDetailIndex] || {}
        )
    }

    get currentDetailSections(): any[] {
        const currentTab = this.metadata?.tabs[this.selectedTab]
        return currentTab?.detail || []
    }

    get currentDetailSection(): any {
        return this.currentDetailSections[this.selectedDetailIndex]
    }

    onSelectChange(): void {
        this.selectedDetailIndex = 0
        this.applyFilters()
    }

    onDetailSectionChange(detailIndex: number): void {
        this.selectedDetailIndex = detailIndex
        this.applyFilters()
    }

    onSubmit(): void {
        this.validateForm()
        this.validateDetail()
        if (Object.keys(this.detailErrors).length > 0) {
            alert('Có trường chi tiết chưa được nhập liệu!!!');
            return;
        }
        if (Object.keys(this.errors).length === 0 && Object.keys(this.detailErrors).length === 0) {
            // Show warning if primary key has changed

            const primaryKeyChanged = this.hasPrimaryKeyChanged()
            if (primaryKeyChanged) {
                const confirmed = confirm(
                    'Bạn đang thay đổi khóa chính của bản ghi. Bạn có chắc chắn muốn tiếp tục?'
                )
                if (!confirmed) {
                    return
                }
            }
            const payload = this.buildInsertPayload();

            if (!payload) {
                return;
            }
            this.http
                .post(
                    `${environment.apiUrl}/api/Dynamic/save`,
                    payload
                )
                .subscribe({
                    next: (response) => {
                        // Reset file attachment data sau khi save thành công
                        if (this.fileAttachmentComponent) {
                            this.fileAttachmentComponent.resetTempData();
                        }
                        this.fileAttachmentData = undefined;

                        this.router.navigate(['../'], { relativeTo: this.route })
                    },
                    error: (err) => {
                        alert(
                            !err?.error?.success
                                ? `${(err.error.errors as { message: string }[])?.map((e: { message: string }) => e.message).join('\n') || ''}`
                                : 'Lỗi không xác định'
                        );
                    },
                })
        } else {
            console.warn('Form có lỗi:', this.errors)
        }
    }

    onCancel(): void {
        this.router.navigate(['../'], { relativeTo: this.route })
    }

    mergeFormData(): { [key: string]: any } {
        const result: { [key: string]: any } = {}

        for (const key in this.formData) {
            Object.assign(result, this.formData[key])
        }

        return result
    }

    // Check if primary key values have changed
    hasPrimaryKeyChanged(): boolean {
        if (!this.girdData || !this.metadata?.primaryKey) {
            return false // New record or no primary key defined
        }

        const currentFormData = this.mergeFormData()

        for (const pkField of this.metadata.primaryKey) {
            const originalValue = this.originalPrimaryKeyValues[pkField]
            const currentValue = currentFormData[pkField]

            // Compare values (handle null/undefined/empty string cases)
            const normalizedOriginal = originalValue ?? ''
            const normalizedCurrent = currentValue ?? ''

            if (String(normalizedOriginal) !== String(normalizedCurrent)) {
                console.log(`Primary key field '${pkField}' changed from '${normalizedOriginal}' to '${normalizedCurrent}'`)
                return true
            }
        }

        return false
    }

    buildInsertPayload(): any {
        const postActions = this.metadata?.dataProcessing?.actions?.post
        const details: any[] = []

        if (Array.isArray(postActions)) {
            this.metadata?.tabs.forEach((tab, selectedTab) => {
                for (const action of postActions) {
                    action.query = action.query.replace(
                        /@(\w+)/g,
                        (_: string, key: string) => {
                            const value = this.formData[selectedTab][key]
                            return value !== undefined ? String(value) : ''
                        }
                    )
                }

                // Handle multiple detail sections
                if (tab?.detail && Array.isArray(tab.detail)) {
                    tab.detail.forEach((detailSection, detailIndex) => {
                        const detailData =
                            this.detailRowsData[this.selectedTab]?.[detailIndex] || []
                        if (detailData.length > 0) {
                            details.push({
                                controllerDetail: detailSection.controllerDetail,
                                formIdDetail: detailSection.formId,
                                foreignKey: detailSection.foreignKey,
                                //data: detailData
                                data: detailData.map((row) => this.normalizeValues(row)),
                            })
                        }
                    })
                }
            })
        }

        // const formData = this.mergeFormData()
        const formData = this.normalizeValues(this.mergeFormData())

        // Determine action based on whether this is a new record or update
        let action = 'insert'
        if (this.girdData) {
            // This is an update operation
            if (this.hasPrimaryKeyChanged()) {
                action = 'update_primary_key'
                console.log('🔄 Using update_primary_key action due to primary key changes')
                console.log('📋 Original PK values:', this.originalPrimaryKeyValues)
                console.log('📋 Current PK values:', this.metadata?.primaryKey?.reduce((acc, pk) => {
                    acc[pk] = formData[pk]
                    return acc
                }, {} as any))
            } else {
                action = 'update'
                console.log('✅ Using standard update action (no primary key changes)')
            }
        } else {
            console.log('🆕 Using insert action for new record')
        }

        const payload = {
            controller: this.metadata?.controller,
            formId: this.metadata?.formId,
            action: action,
            type: this.metadata?.type,
            userId: localStorage.getItem('userId'),
            unit: localStorage.getItem('unit') ?? 'CTY',
            language: localStorage.getItem('language') ?? 'vi',
            VCDate: this.metadata?.VCDate ? formData[this.metadata?.VCDate] : '',
            idVC: this.metadata?.idVC,
            primaryKey: this.metadata?.primaryKey,
            data: {
                ...formData,
                details: details,
            },
            fileAttachments: this.fileAttachmentData,
            dataProcessing: {
                actions: {
                    post: postActions
                }
            },
        }

        // Nếu là voucher, lấy ngày từ voucherDate
        if (this.metadata?.type === 'voucher') {
            const voucherDate = this.formData[this.selectedTab]?.['voucherDate']
            if (voucherDate) {
                // Đảm bảo đúng format yyyy-MM-dd
                payload.VCDate =
                    typeof voucherDate === 'string'
                        ? voucherDate
                        : this.formatDate(voucherDate)
            }
        }

        return payload
    }

    private validateForm(): void {
        this.errors = {}
        const currentTab = this.metadata?.tabs[this.selectedTab]

        if (currentTab?.form) {
            currentTab.form.fields.forEach((field) => {
                if (field.required && !this.formData[this.selectedTab][field.key]) {
                    this.errors[field.key] = `${field.label} là bắt buộc`
                }
            })
        }
    }

    private validateDetail(): void {
        this.detailErrors = {}
        this.currentDetailRows.forEach((row, rowIndex) => {
            this.detailErrors[rowIndex] = {}

            for (const field of this.getAllDetailFields()) {
                if (field.required && (row[field.key] === '' || row[field.key] === null || row[field.key] === undefined)) {
                    this.detailErrors[rowIndex][field.key] = `${field.label || field.key} là bắt buộc`;

                    setTimeout(() => {
                        delete this.detailErrors[rowIndex][field.key];
                    }, 5000);
                }
            }



            if (Object.keys(this.detailErrors[rowIndex]).length === 0) {
                delete this.detailErrors[rowIndex]
            }
        })
    }

    getFieldError(fieldKey: string): string | null {
        return this.errors[fieldKey] || null
    }

    getAllDetailFields(): any[] {
        return this.currentDetailSection?.fields || []
    }

    addDetailRow(): void {
        if (!this.currentDetailSection) return

        const newRow: any = {}
        const currentRows = this.currentDetailRows

        // Get the highest line_nbr and increment
        const maxId = currentRows.reduce(
            (max, row) => Math.max(max, row.line_nbr || 0),
            0
        )

        // Initialize ALL fields (including hidden ones) with default values
        this.getAllDetailFields().forEach((field) => {
            if (field.type === 'lookup') {
                newRow[field.key] = ''
            } else {
                newRow[field.key] = field.default || ''
            }
        })

        // Đảm bảo tất cả fields từ mapping config được khởi tạo
        this.getAllDetailFields().forEach((field) => {
            if (field.onChange && field.onChange.map) {
                Object.keys(field.onChange.map).forEach((mappedField) => {
                    if (!(mappedField in newRow)) {
                        newRow[mappedField] = ''
                        //console.log('PhongNN2 - Initialized mapped field:', mappedField)
                    }
                })
            }
        })
        newRow.line_nbr = maxId + 1
        this.detailRowsData[this.selectedTab][this.selectedDetailIndex].push(
            newRow
        )
        this.applyFilters()
    }

    removeDetailRow(index: number): void {
        const rowToRemove = this.currentFilteredDetailRows[index]
        const actualIndex = this.currentDetailRows.findIndex(
            (row) => row === rowToRemove
        )

        if (actualIndex > -1) {
            this.detailRowsData[this.selectedTab][this.selectedDetailIndex].splice(
                actualIndex,
                1
            )
            this.applyFilters()
        }
        this.updateMasterCalculations()
    }
    /////////

    // Trigger calculations for fields that depend on the changed field
    triggerRowCalculations(row: any, changedFieldKey: string, visited: Set<string> = new Set()): void {
        // Prevent infinite loops
        if (visited.has(changedFieldKey)) {
            return
        }
        visited.add(changedFieldKey)

        const allFields = this.getAllDetailFields()

        // Find fields triggered by this change (check both trigger and dependencies)
        const triggeredFields = allFields.filter(field => {
            // Don't trigger itself
            if (field.key === changedFieldKey) {
                return false
            }

            // Check trigger array
            if (field.trigger?.includes(changedFieldKey)) {
                return true
            }

            // Check dependencies in calculation formulas
            if (field.calculation?.calculations) {
                return field.calculation.calculations.some((calc: any) =>
                    calc.dependencies?.includes(changedFieldKey)
                )
            }

            return false
        })

        console.log(`triggerRowCalculations for ${changedFieldKey}:`, triggeredFields.map(f => f.key))

        // Apply calculations for triggered fields
        for (const field of triggeredFields) {
            if (field.calculation?.calculations) {
                const result = CalculationEngine.applyCalculations(field, row, this.currentDetailRows)
                if (result !== null) {
                    console.log(`Calculated ${field.key} = ${result}`)
                    row[field.key] = result

                    // Trigger calculations for fields that depend on this newly calculated field
                    this.triggerRowCalculations(row, field.key, visited)
                }
            }
        }
    }

    // Apply calculations for a specific row
    applyRowCalculations(row: any, changedField: Field): void {
        const allFields = this.getAllDetailFields()

        // Find fields that should be calculated based on this change
        const fieldsToCalculate = allFields.filter(field =>
            field.calculation?.calculations?.some((calc: CalculationRule) =>
                calc.dependencies.includes(changedField.key)
            )
        )

        // Apply calculations with enhanced support
        for (const field of fieldsToCalculate) {
            const result = CalculationEngine.applyCalculations(field, row, this.currentDetailRows)
            if (result !== null) {
                row[field.key] = result

                // Log which formula was used (for debugging)
                if (field.calculation?.calculations?.length > 1) {
                    console.log(`Field ${field.key} calculated using multiple formulas, result: ${result}`)
                }
            }
        }
    }
    calculateAggregation(config: any): number {
        const detailRows = this.currentDetailRows
        let result = 0

        switch (config.type) {
            case 'sum':
                result = detailRows.reduce((sum, row) =>
                    sum + (parseFloat(row[config.sourceField]) || 0), 0)
                break
            case 'count':
                if (config.sourceField) {
                    result = detailRows.filter(row =>
                        row[config.sourceField] !== null &&
                        row[config.sourceField] !== undefined &&
                        row[config.sourceField] !== ''
                    ).length
                } else {
                    result = detailRows.length
                }
                break
            case 'average':
                if (detailRows.length > 0) {
                    const sum = detailRows.reduce((sum, row) =>
                        sum + (parseFloat(row[config.sourceField]) || 0), 0)
                    result = sum / detailRows.length
                }
                break
        }

        return config.precision !== undefined ?
            Number(result.toFixed(config.precision)) : result
    }

    calculateField(field: any): number | null {
        if (!field.calculation?.calculations) return null;

        for (const calc of field.calculation.calculations) {
            if (calc.formula.includes('SUM([')) {
                // Use CalculationEngine with detail data for SUM formulas
                const result = CalculationEngine.evaluateFormula(
                    calc.formula,
                    this.formData[this.selectedTab], // Master data
                    this.currentDetailRows // Detail data for SUM operations
                );

                return calc.precision !== undefined ?
                    Number(result.toFixed(calc.precision)) : result;
            }
        }

        return null;
    }

    updateCalculatedFields(currentTab: any): void {
        const calculatedFields = currentTab.form.fields.filter((f: any) => f.calculation?.calculations)

        for (let iteration = 0; iteration < 3; iteration++) {
            calculatedFields.forEach((field: any) => {
                const value = this.calculateField(field)
                if (value !== null) {
                    this.formData[this.selectedTab][field.key] = value
                }
            })
        }
    }

    updateAggregationFields(currentTab: any): void {
        const aggregateFields = currentTab.form.fields.filter((f: any) => f.aggregation)

        aggregateFields.forEach((field: any) => {
            const value = this.calculateAggregation(field.aggregation)
            this.formData[this.selectedTab][field.key] = value
        })
    }

    updateMasterCalculations(): void {
        const currentTab = this.metadata?.tabs[this.selectedTab]
        if (!currentTab?.form) return

        this.updateAggregationFields(currentTab)

        this.updateCalculatedFields(currentTab)

        this.updateMasterSubtotals()
    }

    calculateMasterFormValues(): void {
        const currentTab = this.metadata?.tabs[this.selectedTab]
        if (!currentTab?.form) return

        for (const field of currentTab.form.fields) {
            if (field.calculation?.calculations) {
                const mergedData = {
                    ...this.formData[this.selectedTab],
                    ...this.calculatedValues
                }
                const result = CalculationEngine.applyCalculations(field, mergedData)

                if (result !== null) {
                    this.formData[this.selectedTab][field.key] = result
                }
            }
        }
    }

    calculateAggregateValues(): void {
        const currentDetailSection = this.currentDetailSection
        if (!currentDetailSection?.calculations) return

        const detailData = this.currentDetailRows

        for (const calc of currentDetailSection.calculations) {
            const result = CalculationEngine.evaluateFormula(calc.formula, {}, detailData)

            // Extract field name from formula (e.g., "[subtotal]" -> "subtotal")
            const fieldMatch = calc.formula.match(/\[([^\]]+)\]\s*=/)
            if (fieldMatch) {
                const fieldName = fieldMatch[1]
                this.calculatedValues[fieldName] = calc.precision !== undefined ?
                    Number(result.toFixed(calc.precision)) : result
            }
        }

        // Trigger master form calculations that depend on detail calculations
        this.calculateMasterFormValues()
    }

    calculateDetailAggregate(config: any, detailRows: any[]): number {
        // Apply condition filter if specified
        let filteredRows = detailRows;
        if (config.condition) {
            filteredRows = detailRows.filter(row => {
                try {
                    const condition = config.condition!.replace(/\[([^\]]+)\]/g, (_: string, fieldName: string) => {
                        const value = row[fieldName];
                        return typeof value === 'string' ? `"${value}"` : (value || 0);
                    });
                    return new Function('return ' + condition)();
                } catch {
                    return true;
                }
            });
        }

        let result = 0;
        switch (config.type) {
            case 'sum':
                result = filteredRows.reduce((sum, row) =>
                    sum + (parseFloat(row[config.sourceField]) || 0), 0);
                break;
        }

        return config.precision !== undefined ?
            Number(result.toFixed(config.precision)) : result;
    }

    // Get master field value for summary display
    getCurrentMasterFields(): any[] {
        return this.metadata?.tabs[this.selectedTab]?.form?.fields || [];
    }

    getSummaryFields(): any[] {
        return this.getCurrentMasterFields().filter(field =>
            field.disabled && field.type !== 'hidden'
        );
    }

    // Get master field value for summary display
    getMasterFieldValue(fieldKey: string): any {
        console.log(this.formData[this.selectedTab]?.[fieldKey])
        return this.formData[this.selectedTab]?.[fieldKey] || 0;
    }
    isCurrencyField(fieldKey: string, fieldObject?: any): boolean {
        if (!fieldKey) return false

        if (fieldObject && fieldObject.currency) {
            return true
        }
        return false
    }

    formatCurrencyNumber(amount: number, fieldKey?: string, fieldObject?: any): string {
        if (amount === null || amount === undefined || isNaN(amount)) {
            return ''
        }

        // Get currency type directly from field object or default to 'VN'
        const currencyType = fieldObject?.currency || 'VN'

        // Get appropriate locale for the currency type
        const locale = this.getLocaleForCurrency(currencyType)

        return new Intl.NumberFormat(locale, {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount)
    }

    parseNumberInput(value: any): number {
        if (typeof value === 'number') {
            return value
        }

        if (typeof value === 'string') {
            const cleanValue = value
                .replace(/[₫$€£¥]/g, '') // Remove currency symbols
                .replace(/\s/g, '') // Remove spaces
                .replace(/\./g, '') // Remove thousand separators (dots)
                .replace(/,/g, '.') // Convert comma to decimal if present

            const numericValue = parseFloat(cleanValue)
            return isNaN(numericValue) ? 0 : numericValue
        }

        return 0
    }

    onCurrencyBlur(event: any, rowIndex: number, fieldKey: string): void {
        const inputElement = event.target
        const numericValue = this.parseNumberInput(inputElement.value)

        this.onDetailFieldChange(rowIndex, fieldKey, numericValue)

        inputElement.value = this.formatCurrencyNumber(numericValue)
    }

    getLocaleForCurrency(currencyType: string): string {
        const localeMap: { [key: string]: string } = {
            'VN': 'vi-VN',
            'USD': 'en-US',
            'EUR': 'de-DE',
            'GBP': 'en-GB',
            'JPY': 'ja-JP',
            'KRW': 'ko-KR',
            'CNY': 'zh-CN',
            'THB': 'th-TH',
            'SGD': 'en-SG',
            'MYR': 'ms-MY'
        }

        return localeMap[currencyType.toUpperCase()] || 'vi-VN'
    }
    onDetailFieldChange(rowIndex: number, fieldKey: string, value: any): void {
        const row = this.currentFilteredDetailRows[rowIndex]
        if (!row) return

        // Apply calculations for this row
        const field = this.getAllDetailFields().find(f => f.key === fieldKey)
        if (field) {
            // Handle onChange if field has onChange configuration
            if (field.onChange) {
                // handleOnChangeDetail sẽ tự set giá trị vào row
                this.handleOnChangeDetail(field, value, rowIndex)
            } else {
                // Nếu không có onChange thì set giá trị và apply calculations
                row[fieldKey] = value

                // Apply calculations for this row
                this.applyRowCalculations(row, field)

                // Trigger calculations for dependent fields in the same row
                this.triggerRowCalculations(row, fieldKey)
            }
        } else {
            // Nếu không tìm thấy field config thì chỉ set giá trị
            row[fieldKey] = value
        }

        // Update master calculations
        this.updateMasterCalculations()

        // Re-apply filters if the changed field has a filter
        if (this.currentColumnFilters[fieldKey]) {
            this.applyFilters()
        }
        // Recalculate aggregate values
        this.calculateAggregateValues()
        // Update master calculations
        this.updateMasterCalculations()
    }

    trackByIndex(index: number, item: any): any {
        return item.line_nbr || index
    }

    // Filter methods updated for multiple detail sections
    onFilterChange(fieldKey: string, value: string): void {
        const filters =
            this.columnFiltersData[this.selectedTab][this.selectedDetailIndex]
        if (value && value.trim()) {
            filters[fieldKey] = value.trim()
        } else {
            delete filters[fieldKey]
        }
        this.applyFilters()
    }
    private normalizeValues(obj: any): any {
        const normalized: any = {}
        for (const key in obj) {
            let value = obj[key]
            if (typeof value === 'string' && value.trim() === '') {
                value = null // Chuỗi rỗng chuyển thành null
            }
            normalized[key] = value
        }
        return normalized
    }

    applyFilters(): void {
        const currentRows = this.currentDetailRows
        if (!currentRows || currentRows.length === 0) {
            this.filteredDetailRowsData[this.selectedTab][this.selectedDetailIndex] =
                []
            return
        }

        const filters = this.currentColumnFilters
        const activeFilters = Object.keys(filters).filter(
            (key) => filters[key] && filters[key].trim()
        )

        if (activeFilters.length === 0) {
            this.filteredDetailRowsData[this.selectedTab][this.selectedDetailIndex] =
                [...currentRows]
            return
        }

        this.filteredDetailRowsData[this.selectedTab][this.selectedDetailIndex] =
            currentRows.filter((row) => {
                const matches = activeFilters.map((fieldKey) => {
                    const filterValue = filters[fieldKey].toLowerCase()
                    const rowValue = (row[fieldKey] || '').toString().toLowerCase()

                    const field = this.getAllDetailFields()?.find(
                        (f) => f.key === fieldKey
                    )

                    if (field?.type === 'select') {
                        return rowValue === filterValue
                    } else if (field?.type === 'number') {
                        return this.applyNumberFilter(rowValue, filterValue)


                    }
                    else if (field?.type === 'date') {
                        return this.applyDateFilter(rowValue, filterValue)
                    }
                    else {
                        // String/text fields - case insensitive contains
                        const searchValue = filterValue.toLowerCase()

                        return rowValue.includes(searchValue)
                    }
                })

                return this.filterMode === 'all'
                    ? matches.every((match) => match)
                    : matches.some((match) => match)
            })
    }

    private applyNumberFilter(rawValue: any, filterValue: string): boolean {
        const numericValue = this.parseNumberInput(rawValue)

        // Handle comparison operators
        if (filterValue.startsWith('>=')) {
            const num = parseFloat(filterValue.substring(2).trim())
            return !isNaN(num) && numericValue >= num
        }
        else if (filterValue.startsWith('<=')) {
            const num = parseFloat(filterValue.substring(2).trim())
            return !isNaN(num) && numericValue <= num
        }
        else if (filterValue.startsWith('>')) {
            const num = parseFloat(filterValue.substring(1).trim())
            return !isNaN(num) && numericValue > num
        }
        else if (filterValue.startsWith('<')) {
            const num = parseFloat(filterValue.substring(1).trim())
            return !isNaN(num) && numericValue < num
        }
        else if (filterValue.startsWith('=')) {
            const num = parseFloat(filterValue.substring(1).trim())
            return !isNaN(num) && numericValue === num
        }
        else if (filterValue.startsWith('!=') || filterValue.startsWith('<>')) {
            const num = parseFloat(filterValue.substring(2).trim())
            return !isNaN(num) && numericValue !== num
        }
        else if (filterValue.includes('..')) {
            // Range with .. separator (e.g., "10..20")
            const [min, max] = filterValue.split('..').map(v => parseFloat(v.trim()))
            return !isNaN(min) && !isNaN(max) && numericValue >= min && numericValue <= max
        }
        else if (filterValue.includes('-') && !filterValue.startsWith('-')) {
            // Range with - separator (e.g., "10-20"), but not negative numbers
            const parts = filterValue.split('-')
            if (parts.length === 2) {
                const [min, max] = parts.map(v => parseFloat(v.trim()))
                return !isNaN(min) && !isNaN(max) && numericValue >= min && numericValue <= max
            }
        }

        // Default: exact match or contains for partial numbers
        const searchNum = parseFloat(filterValue)
        if (!isNaN(searchNum)) {
            return numericValue === searchNum
        } else {
            // Allow partial matching for numbers (e.g., searching "12" finds "120", "1234", etc.)
            return numericValue.toString().includes(filterValue)
        }
    }

    private applyDateFilter(rawValue: any, filterValue: string): boolean {
        if (!rawValue || !filterValue) return false

        const dateValue = new Date(rawValue)
        const filterDate = new Date(filterValue)

        // If filter value is not a valid date, try partial matching
        if (isNaN(filterDate.getTime())) {
            const dateString = rawValue.toString()
            return dateString.includes(filterValue)
        }

        // Normalize dates to compare only date parts (ignore time)
        const normalizeDate = (date: Date) => new Date(date.getFullYear(), date.getMonth(), date.getDate())

        const normalizedDateValue = normalizeDate(dateValue)
        const normalizedFilterDate = normalizeDate(filterDate)

        return normalizedDateValue.getTime() === normalizedFilterDate.getTime()
    }

    clearFilter(fieldKey: string): void {
        delete this.columnFiltersData[this.selectedTab][this.selectedDetailIndex][
            fieldKey
        ]
        this.applyFilters()
    }

    clearAllFilters(): void {
        this.columnFiltersData[this.selectedTab][this.selectedDetailIndex] = {}
        this.applyFilters()
    }

    hasActiveFilters(): boolean {
        const filters = this.currentColumnFilters
        return Object.keys(filters).some(
            (key) => filters[key] && filters[key].trim()
        )
    }

    getActiveFilterCount(): number {
        const filters = this.currentColumnFilters
        return Object.keys(filters).filter(
            (key) => filters[key] && filters[key].trim()
        ).length
    }

    toggleFilterMode(): void {
        this.filterMode = this.filterMode === 'all' ? 'any' : 'all'
        this.applyFilters()
    }

  getColumnWidth(fieldType: string): string {
    const widthMap: { [key: string]: string } = {
      text: '200px',
      number: '120px',
      select: '150px',
      date: '130px',
    }
    return widthMap[fieldType] || '120px'
  }

    shouldShowSummary(): boolean {
        return this.currentDetailRows.length > 0 && this.selectedTab === 1
    }

    calculateSubtotal(): number {
        return this.currentDetailRows.reduce((sum, row) => {
            const quantity = parseFloat(row.quantity) || 0
            const price = parseFloat(row.price) || 0
            return sum + quantity * price
        }, 0)
    }

    calculateTax(): number {
        const subtotal = this.calculateSubtotal()
        const taxRate = 0.1
        return subtotal * taxRate
    }

    calculateDiscount(): number {
        return 0
    }

    calculateTotal(): number {
        return (
            this.calculateSubtotal() + this.calculateTax() - this.calculateDiscount()
        )
    }

    calculateTotalQuantity(): number {
        return this.currentDetailRows.reduce((sum, row) => {
            return sum + (parseFloat(row.quantity) || 0)
        }, 0)
    }

    calculateFilteredSubtotal(): number {
        return this.currentFilteredDetailRows.reduce((sum, row) => {
            const quantity = parseFloat(row.quantity) || 0
            const price = parseFloat(row.price) || 0
            return sum + quantity * price
        }, 0)
    }

    calculateFilteredQuantity(): number {
        return this.currentFilteredDetailRows.reduce((sum, row) => {
            return sum + (parseFloat(row.quantity) || 0)
        }, 0)
    }

    calculateFilteredPercentage(): string {
        const total = this.calculateSubtotal()
        const filtered = this.calculateFilteredSubtotal()
        if (total === 0) return '0'
        return ((filtered / total) * 100).toFixed(1)
    }

    formatValue(value: any, fieldType: string): string {
        if (!value) return ''

        switch (fieldType) {
            case 'number':
                return this.formatNumber(value)
            case 'date':
                return this.formatDate(value)
            default:
                return value.toString()
        }
    }

    formatFieldValue(row: any, field: any): string {
        const value = row[field.key]
        if (value === null || value === undefined || value === '') return ''

        switch (field.type) {
            case 'number':
                if (field.key === 'total' || field.key === 'price') {
                    return this.formatCurrency(parseFloat(value))
                }
                return this.formatNumber(value)
            case 'date':
                return this.formatDate(value)
            default:
                return value.toString()
        }
    }

    onCurrencyFilterChange(fieldKey: string, event: any): void {
        const inputElement = event.target
        const rawValue = inputElement.value

        this.onFilterChange(fieldKey, rawValue)
    }

    onCurrencyFilterBlur(fieldKey: string, event: any, field: any): void {
        const inputElement = event.target
        const rawValue = inputElement.value

        const numericValue = this.parseNumberInput(rawValue)
        if (!isNaN(numericValue) && numericValue !== 0) {
            inputElement.value = this.formatCurrencyNumber(numericValue, fieldKey, field)
        }
    }

    getFormattedMasterValue(fieldKey: string, field: any): string {
        const fieldValue = this.formData[this.selectedTab][fieldKey];

        // Return empty string for null/undefined/empty values
        if (fieldValue === null || fieldValue === undefined || fieldValue === '') {
            return '';
        }

        // For currency fields, format the number
        if (this.isCurrencyField(fieldKey, field)) {
            const numericValue = this.parseNumberInput(fieldValue);
            if (!isNaN(numericValue) && numericValue !== 0) {
                return this.formatCurrencyNumber(numericValue, fieldKey, field);
            }
        }

        return fieldValue.toString();
    }

    onMasterCurrencyInput(fieldKey: string, event: any, field: any): void {
        const inputElement = event.target;
        const rawValue = inputElement.value;

        const numericValue = this.parseNumberInput(rawValue);
        this.formData[this.selectedTab][fieldKey] = numericValue;
        this.updateMasterSubtotals();

    }

    onMasterCurrencyBlur(fieldKey: string, event: any, field: any): void {
        const inputElement = event.target;
        const numericValue = this.formData[this.selectedTab][fieldKey];

        if (!isNaN(numericValue) && numericValue !== null && numericValue !== undefined) {
            const formattedValue = this.formatCurrencyNumber(numericValue, fieldKey, field);
            inputElement.value = formattedValue;

            if (field.onChange) {
                this.handleOnChange(field, numericValue);
            }
        } else {

            inputElement.value = '';
            this.formData[this.selectedTab][fieldKey] = null;
            this.updateMasterSubtotals();
        }
    }

    getFormattedFilterValue(fieldKey: string, field: any): string {
        const filterValue = this.currentColumnFilters[fieldKey]
        if (!filterValue) return ''

        // If it's a currency field and the filter value is a number, format it
        if (this.isCurrencyField(fieldKey, field)) {
            const numericValue = this.parseNumberInput(filterValue)
            if (!isNaN(numericValue) && numericValue !== 0) {
                // Check if the filter contains operators (>, <, =, etc.)
                if (/^[><=!]/.test(filterValue.trim())) {
                    return filterValue // Keep operators as-is
                }
                return this.formatCurrencyNumber(numericValue, fieldKey, field)
            }
        }

        return filterValue
    }
    formatCurrency(amount: number): string {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND',
        }).format(amount)
    }

    formatNumber(value: number): string {
        return new Intl.NumberFormat('vi-VN').format(value)
    }

    formatDate(date: string): string {
        if (!date) return ''
        return new Date(date).toLocaleDateString('vi-VN')
    }

    getOptionLabel(fieldKey: string, value: string): string {
        const field = this.getAllDetailFields().find((f) => f.key === fieldKey)
        if (!field || !field.options) return value

        const option = field.options.find((opt: any) => opt.value === value)
        return option ? option.label : value
    }

    getFieldWidth(field: Field): string {
        if (field.width) {
            return field.width;
        }

        // Return default width based on field type
        return '200px';
    }

    onValueChange(data: any, key: string): void {
        this.formData[0][key] = data
    }

    /**
     * Lấy controller name từ metadata
     */
    getController(): string {
        return this.metadata?.controller || '';
    }

    /**
     * Tính sysKey từ primary key values
     * Nếu có nhiều primary key thì nối bằng dấu '|'
     */
    getSysKey(): string {
        if (!this.metadata?.primaryKey || this.metadata.primaryKey.length === 0) {
            return '';
        }

        const currentFormData = this.mergeFormData();
        const keyValues: string[] = [];

        // Lấy giá trị của từng primary key
        for (const pkField of this.metadata.primaryKey) {
            const value = currentFormData[pkField];
            // Chuyển về string và xử lý null/undefined
            const stringValue = value != null ? String(value).trim() : '';
            keyValues.push(stringValue);
        }

        // Nối các giá trị bằng dấu '|'
        return keyValues.join('|');
    }

    /**
     * Handle file attachment data changes từ file attachment component
     */
    onFileAttachmentDataChange(fileData: FileAttachmentData): void {
        this.fileAttachmentData = fileData;
        console.log('File attachment data changed:', fileData);
    }

    onFieldValueChange(value: any, field: any) {
        this.formData[this.selectedTab][field.key] = value
        if (field.onChange) {
            this.handleOnChange(field, value)
        }

        this.updateMasterSubtotals();
    }


    onDetailLookupChange(rowIndex: number, fieldKey: string, value: any): void {
        //console.log(`PhongNN2 - onDetailLookupChange called: rowIndex=${rowIndex}, fieldKey=${fieldKey}, value=`, value);
        //console.log('PhongNN2 - Value type:', typeof value);
        //console.log('PhongNN2 - Value is array:', Array.isArray(value));

        const row = this.currentFilteredDetailRows[rowIndex];
        if (!row) {
            // console.error('PhongNN2 - Row not found at index:', rowIndex);
            return;
        }

        //console.log('PhongNN2 - Current row before change:', JSON.stringify(row, null, 2));

        // Find field config and handle onChange if exists
        const field = this.getAllDetailFields().find(f => f.key === fieldKey)
        if (field) {
            //console.log('PhongNN2 - Field config found:', JSON.stringify(field, null, 2))
            //console.log('PhongNN2 - Field has onChange:', !!field.onChange)

            if (field.onChange) {
                console.log('onChange config:', JSON.stringify(field.onChange, null, 2))
            }

            // Handle onChange if field has onChange configuration
            if (field.onChange) {
                //console.log('PhongNN2 - Calling handleOnChangeDetail...')
                // handleOnChangeDetail sẽ tự set giá trị vào row
                this.handleOnChangeDetail(field, value, rowIndex)
            } else {
                // Nếu không có onChange thì set giá trị và apply calculations
                //console.log('PhongNN2 - No onChange, setting value directly')
                row[fieldKey] = value

                // Apply calculations for this row
                this.applyRowCalculations(row, field)

                // Trigger calculations for dependent fields in the same row
                this.triggerRowCalculations(row, fieldKey)
            }
        } else {
            // Nếu không tìm thấy field config thì chỉ set giá trị
            //console.error('PhongNN2 - Field config not found for:', fieldKey)
            row[fieldKey] = value
        }

        //console.log('PhongNN2 - Row after change:', row)

        // Update master calculations
        this.updateMasterCalculations()

        // Re-apply filters if the changed field has a filter
        if (this.currentColumnFilters[fieldKey]) {
            this.applyFilters()
        }

        // Recalculate aggregate values
        this.calculateAggregateValues()
    }

    // Debug method to check field values
    debugRowFields(rowIndex: number): void {
        //console.log('PhongNN2 - DEBUG: Checking row fields...')
        const row = this.currentFilteredDetailRows[rowIndex]
        if (row) {
            //console.log('PhongNN2 - Row data:', JSON.stringify(row))
            //console.log('PhongNN2 - uom field value:', row['uom'])
            //console.log('PhongNN2 - itemCode field value:', row['itemCode'])

            // Check if fields exist in metadata
            const allFields = this.getAllDetailFields()
            const uomField = allFields.find(f => f.key === 'uom')
            const itemCodeField = allFields.find(f => f.key === 'itemCode')

            //console.log('PhongNN2 - uom field config:', uomField)
            //console.log('PhongNN2 - itemCode field config:', itemCodeField)
        } else {
            console.error(' No row found at index:', rowIndex)
        }
    }

    // Global debug method - call this from browser console
    debugState(): void {
        console.log('===  DEBUG STATE ===');
        console.log('isInitialLoading:', this.isInitialLoading);
        console.log('selectedTab:', this.selectedTab);
        console.log('selectedDetailIndex:', this.selectedDetailIndex);
        console.log('metadata:', this.metadata);
        console.log('currentDetailRows length:', this.currentDetailRows.length);
        console.log('currentFilteredDetailRows length:', this.currentFilteredDetailRows.length);

        if (this.currentFilteredDetailRows.length > 0) {
            console.log('First row:', JSON.stringify(this.currentFilteredDetailRows[0], null, 2));

            // Debug lookup fields specifically
            const allFields = this.getAllDetailFields();
            const lookupFields = allFields.filter(f => f.type === 'lookup');
            console.log('Lookup fields in metadata:', lookupFields.map(f => ({ key: f.key, multiple: f.multiple, hasOnChange: !!f.onChange })));

            lookupFields.forEach(field => {
                const currentValue = this.currentFilteredDetailRows[0][field.key];
                console.log(`Lookup field "${field.key}" current value:`, currentValue);
                console.log(`Lookup field "${field.key}" default query:`, field.default);
            });
        }

        const allFields = this.getAllDetailFields();
        console.log('All detail fields:', allFields.map(f => ({ key: f.key, type: f.type, hasOnChange: !!f.onChange })));

        console.log('=== END DEBUG STATE ===');
    }

    // Debug specific row and field
    debugLookupField(rowIndex: number, fieldKey: string): void {
        console.log(`=== DEBUG LOOKUP FIELD ${fieldKey} ===`);
        const row = this.currentFilteredDetailRows[rowIndex];
        if (!row) {
            console.error('Row not found at index:', rowIndex);
            return;
        }

        const field = this.getAllDetailFields().find(f => f.key === fieldKey);
        if (!field) {
            console.error('Field not found:', fieldKey);
            return;
        }

        console.log('Field config:', JSON.stringify(field, null, 2));
        console.log('Current row value:', row[fieldKey]);
        console.log('Field type:', field.type);
        console.log('Field has onChange:', !!field.onChange);

        if (field.onChange) {
            console.log('onChange config:', JSON.stringify(field.onChange, null, 2));
        }

        console.log('=== END DEBUG LOOKUP FIELD ===');
    }

    // Debug UI after change detection
    debugUIAfterChange(rowIndex: number): void {
        console.log('=== DEBUG UI AFTER CHANGE ===');
        setTimeout(() => {
            const row = this.currentFilteredDetailRows[rowIndex];
            if (row) {
                console.log('Row data after change detection:', JSON.stringify(row, null, 2));

                // Check lookup components specifically
                const lookupElements = document.querySelectorAll('app-lookup');
                console.log('Number of lookup components found:', lookupElements.length);

                lookupElements.forEach((el, index) => {
                    console.log(`Lookup component ${index}:`, el);
                });
            }
            console.log('=== END DEBUG UI AFTER CHANGE ===');
        }, 200);
    }



    getMasterSubtotalFields(): any[] {
        return this.getCurrentMasterFields().filter(field =>
            (field.subtotal_master && field.type !== 'hidden') ||
            (field.masterSubtotalConfig && field.type !== 'hidden')
        );
    }


    calculateMasterSubtotal(targetField: Field): number {
        const currentFormData = this.formData[this.selectedTab];

        if (targetField.masterSubtotalConfig?.calculations) {
            const result = CalculationEngine.applyCalculations(
                {
                    key: targetField.key,
                    calculation: targetField.masterSubtotalConfig
                } as Field,
                currentFormData
            );
            return result !== null ? result : 0;
        }

        return this.getCurrentMasterFields()
            .filter(f => f.type === 'number' && f.key !== targetField.key && !f.disabled)
            .reduce((sum, field) => {
                return sum + (Number(currentFormData[field.key]) || 0);
            }, 0);
    }

    updateMasterSubtotals(): void {
        const masterSubtotalFields = this.getMasterSubtotalFields();

        masterSubtotalFields.forEach(field => {
            const calculatedValue = this.calculateMasterSubtotal(field);
            this.formData[this.selectedTab][field.key] = calculatedValue;
            this.masterSubtotals[field.key] = calculatedValue;
        });
    }
    onMasterFieldChange(fieldKey: string, value: any): void {
        this.formData[this.selectedTab][fieldKey] = value;
        this.updateMasterSubtotals();
        const field = this.getCurrentMasterFields().find(f => f.key === fieldKey);
        if (field?.onChange) {
            this.handleOnChange(field, value);
        }
    }

    updateLineNumbers(startIndex: number = 0, check: number): void {
        // 0: up, 1: down
        const currentRows = this.detailRowsData[this.selectedTab][this.selectedDetailIndex];

        if (check == 0) { // moveRowUp - row moved from startIndex to startIndex-1
            // The row that moved up gets the smaller line number
            currentRows[startIndex - 1].line_nbr = startIndex// 1-based indexing
            // The row that moved down gets the larger line number  
            currentRows[startIndex].line_nbr = startIndex + 1; // 1-based indexing
        } else { // moveRowDown - row moved from startIndex to startIndex+1
            // The row that moved down gets the larger line number
            currentRows[startIndex + 1].line_nbr = startIndex + 2; // 1-based indexing
            // The row that moved up gets the smaller line number
            currentRows[startIndex].line_nbr = startIndex + 1; // 1-based indexing
        }
    }
    moveRowUp(index: number): void {
        const rowToMove = this.currentFilteredDetailRows[index];
        const actualIndex = this.currentDetailRows.findIndex(row => row === rowToMove);

        if (actualIndex > 0) {
            const currentRows = this.detailRowsData[this.selectedTab][this.selectedDetailIndex];

            // Swap positions
            [currentRows[actualIndex], currentRows[actualIndex - 1]] =
                [currentRows[actualIndex - 1], currentRows[actualIndex]];

            // Update line numbers
            this.updateLineNumbers(actualIndex, 0);

            // Re-apply filters to maintain correct display
            this.applyFilters();

            // Trigger change detection
            this.cdr.detectChanges();
        }
    }

    moveRowDown(index: number): void {
        const rowToMove = this.currentFilteredDetailRows[index];
        const actualIndex = this.currentDetailRows.findIndex(row => row === rowToMove);
        const currentRows = this.detailRowsData[this.selectedTab][this.selectedDetailIndex];

        if (actualIndex < currentRows.length - 1) {
            // Swap positions
            [currentRows[actualIndex], currentRows[actualIndex + 1]] =
                [currentRows[actualIndex + 1], currentRows[actualIndex]];

            // Update line numbers
            this.updateLineNumbers(actualIndex, 1);

            // Re-apply filters to maintain correct display
            this.applyFilters();

            // Trigger change detection
            this.cdr.detectChanges();
        }
    }

    canMoveUp(index: number): boolean {
        const rowToMove = this.currentFilteredDetailRows[index];
        const actualIndex = this.currentDetailRows.findIndex(row => row === rowToMove);
        return actualIndex > 0;
    }

    canMoveDown(index: number): boolean {
        const rowToMove = this.currentFilteredDetailRows[index];
        const actualIndex = this.currentDetailRows.findIndex(row => row === rowToMove);
        const currentRows = this.detailRowsData[this.selectedTab][this.selectedDetailIndex];
        return actualIndex < currentRows.length - 1;
  }

  initializeColumnWidths(): void {
    const savedWidths = localStorage.getItem(`columnWidths_${this.metadata?.formId || 'default'}`);
    if (savedWidths) {
      try {
        this.columnWidths = JSON.parse(savedWidths);
      } catch (e) {
        this.setDefaultColumnWidths();
      }
    } else {
      this.setDefaultColumnWidths();
    }
  }

  setDefaultColumnWidths(): void {
    this.columnWidths = {
      'actions': 80,
      'rowNumber': 40
    };

    this.getAllDetailFields().forEach(field => {
      this.columnWidths[field.key] = this.getDefaultWidthForField(field);
    });
  }

  getDefaultWidthForField(field: any): number {
    const widthMap: { [key: string]: number } = {
      'text': 150,
      'number': 100,
      'select': 120,
      'date': 120,
      'lookup': 180
    };
    return field.width ? parseInt(field.width) : (widthMap[field.type] || 150);
  }

  // Resize event handlers
  startResize(event: MouseEvent, fieldKey: string): void {
    event.preventDefault();
    event.stopPropagation();

    this.isResizing = true;
    this.resizingColumn = fieldKey;
    this.startX = event.clientX;
    this.startWidth = this.columnWidths[fieldKey] || 150;

    const resizeHandle = event.target as HTMLElement;
    resizeHandle.classList.add('resizing');

    // Show resize line
    const resizeLine = document.getElementById('resizeLine');
    if (resizeLine) {
      resizeLine.style.display = 'block';
      resizeLine.style.left = event.clientX + 'px';
    }

    // Add global mouse event listeners
    document.addEventListener('mousemove', this.handleResize.bind(this));
    document.addEventListener('mouseup', this.stopResize.bind(this));

    // Prevent text selection during resize
    document.body.style.userSelect = 'none';
  }

  handleResize(event: MouseEvent): void {
    if (!this.isResizing) return;

    const deltaX = event.clientX - this.startX;
    const newWidth = Math.max(
      this.minColumnWidth,
      Math.min(this.maxColumnWidth, this.startWidth + deltaX)
    );

    // Update column width
    this.columnWidths[this.resizingColumn] = newWidth;

    // Update resize line position
    const resizeLine = document.getElementById('resizeLine');
    if (resizeLine) {
      resizeLine.style.left = event.clientX + 'px';
    }

    // Show width indicator
    this.showWidthIndicator(event.clientX, event.clientY, newWidth);

    // Apply new width immediately
    this.updateColumnWidthInDOM(this.resizingColumn, newWidth);
  }

  stopResize(event: MouseEvent): void {
    if (!this.isResizing) return;

    this.isResizing = false;

    // Remove resize styling
    const resizeHandles = document.querySelectorAll('.resize-handle.resizing');
    resizeHandles.forEach(handle => handle.classList.remove('resizing'));

    // Hide resize line and width indicator
    const resizeLine = document.getElementById('resizeLine');
    const widthIndicator = document.getElementById('widthIndicator');
    if (resizeLine) resizeLine.style.display = 'none';
    if (widthIndicator) widthIndicator.style.display = 'none';

    // Remove global listeners
    document.removeEventListener('mousemove', this.handleResize.bind(this));
    document.removeEventListener('mouseup', this.stopResize.bind(this));

    // Restore text selection
    document.body.style.userSelect = '';

    // Save column widths
    this.saveColumnWidths();

    // Trigger change detection
    this.cdr.detectChanges();

    this.resizingColumn = '';
  }

  updateColumnWidthInDOM(fieldKey: string, width: number): void {
    const headers = document.querySelectorAll(`th[data-field-key="${fieldKey}"]`);
    const cellSelector = `td:nth-child(${this.getColumnIndex(fieldKey) + 1})`;
    const cells = document.querySelectorAll(cellSelector);

    const widthPx = width + 'px';
    headers.forEach(header => (header as HTMLElement).style.width = widthPx);
    cells.forEach(cell => (cell as HTMLElement).style.width = widthPx);
  }

  getColumnIndex(fieldKey: string): number {
    if (fieldKey === 'actions') return 0;
    if (fieldKey === 'rowNumber') return 1;

    const fields = this.getAllDetailFields();
    const fieldIndex = fields.findIndex(f => f.key === fieldKey);
    return fieldIndex >= 0 ? fieldIndex + 2 : -1;
  }

  showWidthIndicator(x: number, y: number, width: number): void {
    const indicator = document.getElementById('widthIndicator');
    if (indicator) {
      indicator.textContent = `${width}px`;
      indicator.style.left = (x + 10) + 'px';
      indicator.style.top = (y - 30) + 'px';
      indicator.style.display = 'block';
    }
  }

  autoResizeColumn(event: MouseEvent, fieldKey: string): void {
    event.preventDefault();
    event.stopPropagation();

    const optimalWidth = this.calculateOptimalWidth(fieldKey);
    this.columnWidths[fieldKey] = optimalWidth;
    this.updateColumnWidthInDOM(fieldKey, optimalWidth);
    this.saveColumnWidths();
    this.cdr.detectChanges();
  }

  calculateOptimalWidth(fieldKey: string): number {
    const field = this.getAllDetailFields().find(f => f.key === fieldKey);
    if (!field) return 150;

    let maxLength = field.label?.length || 0;

    // Check content length in current data
    this.currentFilteredDetailRows.forEach(row => {
      const value = this.getFieldDisplayValue(row, field);
      maxLength = Math.max(maxLength, value.length);
    });

    // Convert character length to pixels (approximate)
    const charWidth = 8;
    const padding = 20;
    const calculatedWidth = (maxLength * charWidth) + padding;

    return Math.max(
      this.minColumnWidth,
      Math.min(this.maxColumnWidth, calculatedWidth)
    );
  }

  getFieldDisplayValue(row: any, field: any): string {
    const value = row[field.key];
    if (value === null || value === undefined || value === '') return '';

    switch (field.type) {
      case 'number':
        if (this.isCurrencyField(field.key, field)) {
          return this.formatCurrencyNumber(parseFloat(value));
        }
        return this.formatNumber(value);
      case 'date':
        return this.formatDate(value);
      case 'select':
        return this.getOptionLabel(field.key, value);
      default:
        return value.toString();
    }
  }

  resetAllColumnWidths(): void {
    this.setDefaultColumnWidths();
    this.applyAllColumnWidths();
    this.saveColumnWidths();
  }

  autoResizeAllColumns(): void {
    this.getAllDetailFields().forEach(field => {
      const optimalWidth = this.calculateOptimalWidth(field.key);
      this.columnWidths[field.key] = optimalWidth;
    });
    this.applyAllColumnWidths();
    this.saveColumnWidths();
  }

  applyAllColumnWidths(): void {
    Object.keys(this.columnWidths).forEach(fieldKey => {
      this.updateColumnWidthInDOM(fieldKey, this.columnWidths[fieldKey]);
    });
    this.cdr.detectChanges();
  }

  saveColumnWidths(): void {
    localStorage.setItem(
      `columnWidths_${this.metadata?.formId || 'default'}`,
      JSON.stringify(this.columnWidths)
    );
  }

}
