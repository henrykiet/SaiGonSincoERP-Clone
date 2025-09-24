import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnChanges,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import {
  ListApiResponse,
  GirdInitData,
  GridAction,
  GirdHeader,
  FilterCondition,
} from '../models';
import { DynamicPaginationComponent } from '../dynamic-pagination/dynamic-pagination.component';
import {
  AdvancedFilterComponent,
  type FilterResult,
} from '../shared/advanced-filter';
import { Router, ActivatedRoute } from '@angular/router';
import { FileHandleComponent } from '../file-handle/file-handle.component';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FileService } from '../services/file.service';
import { environment } from '../../environments/environment';

import { TranslateModule, TranslateService } from '@ngx-translate/core'

import { DataService } from '../services/data.service';

@Component({
  selector: 'app-grid',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    DynamicPaginationComponent,
    AdvancedFilterComponent,
    FileHandleComponent,
    MatButtonModule,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './dynamic-grid.component.html',
})
export class DynamicGridComponent implements OnInit {
  @Input({ required: true }) girdData!: GirdInitData;
  @ViewChild('fileHandler') fileHandler!: FileHandleComponent;
  response?: ListApiResponse;
  initFilter: FilterCondition[] = [];
  exportData: { [key: string]: any[] } = {};
  userData: { [key: string]: string } = {};
  masterPrimaryKeys: string[] = [];
  primarykey: string = '';
  controll = '';
  isFileHandle: boolean = false;
  isListNotSuccess : boolean = false;
  objectKeys = Object.keys;
  private exportCount = 0;
  private exportKeyMap: Record<string, number> = {};
  selectedOptions: Record<string, any> = {};
  currentLanguage: string = 'vi'; // Track current language

  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
    private fileService: FileService,
    public translate: TranslateService,
    private dataService: DataService

  ) {
    //this.translate.setDefaultLang(localStorage.getItem("language") ?? "vi");
    this.currentLanguage = localStorage.getItem("language") ?? "vi";
    this.translate.setDefaultLang(this.currentLanguage);
    this.translate.use(this.currentLanguage);
  }


  ngOnInit(): void {
    // Subscribe to language changes
    this.translate.onLangChange.subscribe((event) => {
      this.currentLanguage = event.lang;
      localStorage.setItem("language", this.currentLanguage);
      // Update form language in query
      if (this.girdData.query.formId) {
        this.girdData.query.formId.language = this.currentLanguage;
      }
    });
    this.loadData();
  }

  // Method to switch language
  switchLanguage(language: 'vi' | 'en'): void {
    this.currentLanguage = language;
    this.translate.use(language);
    localStorage.setItem("language", language);

    // Update form language and reload data
    if (this.girdData.query.formId) {
      this.girdData.query.formId.language = language;
    }
    this.loadData();
  }

  loadData() {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem(`token`)}`,
      'Custom-Header': 'CustomValue',
    });
    const params = this.buildDynamicQueryParams();

    // Reset toàn bộ khi loadData
    this.selectedOptions = {};
    this.exportData = {};
    this.primarykey = '';
    localStorage.removeItem(`selection_${this.girdData.id}`);
    localStorage.removeItem(`exportData_${this.girdData.id}`);

    this.http
      .get<ListApiResponse>(`${environment.apiUrl}/api/Dynamic/filter`, {
        params,
        headers,
      })
      .subscribe({
        next: (reqData) => {
          this.response = reqData;
          this.controll = this.response.tableName;
          this.userData = {
            user_id0: this.girdData.query.formId.userId,
            datetime0: new Date().toISOString(),
          };
          this.isFileHandle = this.response.isFileHandle ?? false;
          if(this.controll == 'QuotationPaper') this.isListNotSuccess = true;
        },
        error: (error) => {
          console.error('Error loading grid metadata:', error);
        },
      });
  }

  setValue(data: Record<string, string>): void {
    this.girdData.query.formId.value = [];
    this.girdData.query.formId.primaryKey.forEach((prikey) => {
      this.girdData.query.formId.value.push(data[prikey]);
    });
    if (this.girdData.query.formId.type === 'voucher') {
      this.girdData.query.formId.VCDate = data['voucherDate'];
    }
  }

  buildDynamicQueryParams(): HttpParams {

    this.girdData.query.formId.action = "loading"
    this.girdData.query.formId.language = this.currentLanguage;

    const filterStr = localStorage.getItem(`filter_${this.girdData.id}`);
    this.initFilter = filterStr
      ? (JSON.parse(filterStr) as FilterCondition[])
      : [];
    this.girdData.query.filter = this.initFilter;
    return new HttpParams()
      .set('formId', JSON.stringify(this.girdData.query.formId || {}))
      .set('filter', JSON.stringify(this.girdData.query.filter))
      .set('page', (this.girdData.query.page ?? 1).toString())
      .set('pageSize', (this.girdData.query.pageSize ?? 10).toString())
      .set('sort', this.girdData.sort || '');
  }

  onPageChange(newPage: number): void {
    if (newPage !== this.girdData.query.page) {
      this.girdData.query.page = newPage;
      this.loadData();
    }
  }

  onFiltersApplied(result: FilterResult): void {
    if (
      localStorage.getItem(`filter_${this.girdData.id}`) !=
      JSON.stringify(result.filterGroup.conditions)
    ) {
      localStorage.setItem(
        `filter_${this.girdData.id}`,
        JSON.stringify(result.filterGroup.conditions)
      );
      this.loadData();
    }
  }

  handleClickDelete(data: Record<string, string>): void {
    const confirmed = confirm('Bạn có chắc chắn muốn xóa không?');
    if (!confirmed) {
      return;
    }

    this.setValue(data);
    this.girdData.query.formId.action = 'delete';
    this.girdData.query.formId.language = this.currentLanguage;

    this.http
      .post(
        `${environment.apiUrl}/api/dynamic/delete`,
        this.girdData.query.formId
      )
      .subscribe({
        next: (response) => {
          const res = response as { message: string };
          alert(res.message || 'Thành công.');
          this.loadData();
        },
        error: (err) => {
          alert(
            !err?.error?.success
              ? `${
                  (err.error.errors as { message: string }[])
                    ?.map((e: { message: string }) => e.message)
                    .join('\n') || ''
                }`
              : 'Lỗi không xác định'
          );
        },
      });
  }
  handleClickAdd(): void {
    this.selectedRefresh();
    localStorage.removeItem(`param_${this.girdData.id}`);
    this.router.navigate([`${this.router.url}/popup`]);
  }

  handleClickUpdate(data: Record<string, string>): void {
    this.selectedRefresh();
    this.setValue(data);

    this.girdData.mode = "update"
    this.girdData.query.formId.language = this.currentLanguage;

    localStorage.setItem(
      `param_${this.girdData.id}`,
      JSON.stringify(this.girdData)
    );
    this.router.navigate([`${this.router.url}/popup`]);
  }

  handleClickView(data: Record<string, string>): void {
    this.selectedRefresh();
    this.setValue(data);

    this.girdData.mode = "view"
    this.girdData.query.formId.language = this.currentLanguage;

    localStorage.setItem(
      `param_${this.girdData.id}`,
      JSON.stringify(this.girdData)
    );
    this.router.navigate([`${this.router.url}/popup`]);
  }

  onGridButtonClick(action: GridAction): void {
    localStorage.removeItem(`param_${this.girdData.id}`);

    const dataSelectStr = localStorage.getItem(`selection_${this.girdData.id}`);
    let payload: string[] = [];
    const actId = action.id;

    if (dataSelectStr) {
      try {
        const data = JSON.parse(dataSelectStr);
        payload = Object.values(data).map((item: any) => item.idGui);
      } catch (err) {
        console.error('❌ Lỗi parse JSON:', err);
      }
    }

    const formId: any = {
      ...this.girdData.query.formId,
      ids: payload,
      IdSync: `${actId}`,
      language: this.currentLanguage,
    };

    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem(`token`)}`,
      'Content-Type': 'application/json',
      'Custom-Header': 'CustomValue',
    });
    this.http
      .post(`${environment.apiUrl}/api/FormConfig/SyncData`, formId, {
        headers,
      })
      .subscribe(
        (meta: any) => {
          if (meta.StatusCode != 200) {
            alert(meta.message);
          } else {
            localStorage.setItem(
              `action_${action.controller}`,
              JSON.stringify(meta.Data)
            );
            // ✅ Mở tab mới
            const url = this.router.serializeUrl(
              this.router.createUrlTree([action.target])
            );
            window.open(url, '_blank');
          }
        },
        (error) => {
          console.error('❌ Lỗi gọi API:', error);
          alert(error?.error?.message || 'Lỗi không xác định!');
        }
      );
  }

  onGridButtonClickV2(action: GridAction): void {
    localStorage.removeItem(`param_${this.girdData.id}`);

    localStorage.setItem(
      `action_${action.id}`,
      localStorage.getItem(`selection_${this.girdData.id}`) ?? ''
    );

    this.router.navigate([action.target]);
  }
  // ánh xạ primaryKey -> index export
  private loadFullRecordForExport(row: any): void {
    const primaryKeys = this.girdData.query.formId.primaryKey;
    const keyValues = primaryKeys.map((k) => row?.[k]);
    const rowKey = row[primaryKeys[0]];

    // Nếu chưa có ánh xạ thì gán mới
    if (this.exportKeyMap[rowKey] === undefined) {
      this.exportKeyMap[rowKey] = this.exportCount++;
    }

    const suffix = this.exportKeyMap[rowKey];
    const masterPrefix =
      this.girdData.query.formId.formId?.toLowerCase() || 'form';

    // Nếu còn export dạng list thì xóa (chuyển sang detail)
    const listKey = this.controll.toLowerCase();
    if (this.exportData[listKey]) {
      delete this.exportData[listKey];
    }

    const formId: any = {
      ...this.girdData.query.formId,
      value: keyValues,
      action: 'update',
      language: this.currentLanguage,
    };

    if (formId.type === 'voucher') {
      formId.VCDate = row['voucherDate'];
    }

    this.http
      .post<any>(`${environment.apiUrl}/api/FormConfig/GetFormData`, formId)
      .subscribe({
        next: (res) => {
          const metadata = res.Data;
          const pk = metadata.primaryKey as string;
          const masterKey = `${masterPrefix}${suffix}`;
          const detailKey = `${masterPrefix}detail${suffix}`;

          const masterData = metadata.tabs
            .filter((tab: any) => !!tab.form)
            .map((tab: any) => tab.form.initialData || {})
            .reduce((a: any, b: any) => ({ ...a, ...b }), {});
          const pkVal = masterData[pk];
          if (pkVal) {
            this.masterPrimaryKeys.push(pkVal);
          }
          console.log('masterPrimaryKeys khi chọn: ', this.masterPrimaryKeys);

          const detailData: any[] = [];
          metadata.tabs.forEach((tab: any) => {
            if (Array.isArray(tab.detail)) {
              tab.detail.forEach((detail: any) => {
                if (Array.isArray(detail.initialData)) {
                  detailData.push(...detail.initialData);
                }
              });
            }
          });

          this.exportData[masterKey] = [masterData];
          this.exportData[detailKey] = detailData;

          localStorage.setItem(
            `exportData_${this.girdData.id}`,
            JSON.stringify(this.exportData)
          );
          localStorage.setItem(
            `selection_${this.girdData.id}`,
            JSON.stringify(this.selectedOptions)
          );

          console.log(
            `Gán exportData[${masterKey}] & [${detailKey}]`,
            this.exportData
          );
        },
        error: (err) => {
          console.error('Lỗi khi load full dữ liệu để export:', err);
        },
      });
  }

  handleSelection(event: Event, row: any) {
    const input = event.target as HTMLInputElement;
    const key = row[this.girdData.query.formId.primaryKey[0]];
    if (!key) return;
    const listKey = this.controll.toLowerCase();
    if (this.exportData[listKey]) {
      delete this.exportData[listKey];
    }

    // Nếu không còn bản ghi chi tiết nào → Gọi lại export đơn giản
    const isDetailExportEmpty = Object.keys(this.exportData).length === 0;
    if (isDetailExportEmpty) {
      this.updateExportData();
    }

    if (input.checked) {
      this.selectedOptions[key] = row;
      this.loadFullRecordForExport(row);
    } else {
      delete this.selectedOptions[key];

      const suffix = this.exportKeyMap[key];
      if (suffix !== undefined) {
        const masterPrefix =
          this.girdData.query.formId.formId?.toLowerCase() || 'form';
        const masterKey = `${masterPrefix}${suffix}`;
        const detailKey = `${masterPrefix}detail${suffix}`;

        delete this.exportData[masterKey];
        delete this.exportData[detailKey];
        delete this.exportKeyMap[key]; // Xoá ánh xạ
      }
      this.masterPrimaryKeys = this.masterPrimaryKeys.filter(
        (pk) => pk !== key
      );
      localStorage.setItem(
        `exportData_${this.girdData.id}`,
        JSON.stringify(this.exportData)
      );
      localStorage.setItem(
        `selection_${this.girdData.id}`,
        JSON.stringify(this.selectedOptions)
      );
      console.log(`pks sau khi bỏ chọn`, this.masterPrimaryKeys);
    }
  }

  private updateExportData() {
    // Chỉ nên dùng nếu muốn export list đơn giản (không gọi API chi tiết)
    const allowedKeys = this.girdData.headers.map((h) => h.key);
    const selectedRows = Object.values(this.selectedOptions);
    const exportRows =
      selectedRows.length > 0 ? selectedRows : this.response?.data ?? [];

    const cleanData = exportRows.map((item) => {
      const filtered: any = {};
      allowedKeys.forEach((key) => (filtered[key] = item[key]));
      return filtered;
    });

    this.exportData = {
      [this.controll.toLowerCase()]: cleanData,
    };
  }

  countSelected(): number {
    return Object.values(this.selectedOptions).filter((v) => v).length;
  }

  selectedRefresh(): void {
    this.selectedOptions = {};
    this.exportData = {};
    this.exportCount = 0;

    // Xóa localStorage
    localStorage.removeItem(`selection_${this.girdData.id}`);
    localStorage.removeItem(`param_${this.girdData.id}`);
    localStorage.removeItem(`exportData_${this.girdData.id}`);
    localStorage.removeItem(`selection_${this.girdData.id}`);

    // Nếu muốn reset về export đơn giản (list thô)
    this.updateExportData();
    console.log(`Đã reset exportData:`, this.exportData);
  }
  getButtonClasses(color: string): string {
    switch (color) {
      case 'orange':
        return 'bg-orange-500 hover:bg-orange-600 text-white';
      case 'green':
        return 'bg-green-600 hover:bg-green-700 text-white';
      case 'red':
        return 'bg-red-600 hover:bg-red-700 text-white';
      case 'white':
        return 'bg-white text-black border border-gray-300';
      default:
        return 'bg-gray-300 text-black';
    }
  }
  handleMultiDeleted(): void {

    if (
      !this.masterPrimaryKeys ||
      Object.keys(this.masterPrimaryKeys).length === 0
    ) {
      alert('Không có dữ liệu để xóa');

      return;
    }
    const deletedForm = {
      ids: this.masterPrimaryKeys,
      action: 'delete',
      formId: this.girdData.query.formId.formId,
      status: '*',
      userId: this.girdData.query.formId.userId,
      datetime: new Date().toISOString(),
      primaryKey: this.girdData.query.formId.primaryKey[0],
      language: this.currentLanguage,
    }

    this.dataService.deletedMultiData(deletedForm).subscribe({
      next: (res) => {
        if (res.success) {
          alert(res.message || 'Xóa thành công.');
          this.selectedRefresh();
          this.loadData();
        } else {
          alert(res.message || 'Xóa thất bại.');
        }
      },
      error: (err) => {
        const errors = err.error?.errors;
        if (errors) {
          const msgs = Object.values(errors).flat().join('\n');
          alert(msgs);
        } else {
          alert(err.error?.message || 'Lỗi không xác định');
        }
      },
    });
  }
  getFieldWidth(field: string | GirdHeader[]): string {
    if (Array.isArray(field)) {
      return '50px'; // Fixed width for checkbox column
    }
    const header = this.girdData.headers.find((h) => h.key === field);
    if (header && header.width) {
      return header.width;
    }

    return '200px';
  }
}
