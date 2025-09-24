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
import { ListApiResponse, GirdInitData, GridAction, FilterCondition } from '../models';
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
import { TranslateModule } from '@ngx-translate/core'

@Component({
  selector: 'app-report',
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
  templateUrl: './dynamic-report.component.html',
})
export class DynamicReportComponent implements OnInit {
  @Input({ required: true }) girdData!: GirdInitData;
  @ViewChild('fileHandler') fileHandler!: FileHandleComponent;
  response?: ListApiResponse;
  initFilter: FilterCondition[] = [];
  exportData: { [key: string]: any[] } = {};
  userData: { [key: string]: string } = {};
  controll = '';
  objectKeys = Object.keys;
  private exportCount = 0;
  private exportKeyMap: Record<string, number> = {};
  selectedOptions: Record<string, any> = {};
  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
    private fileService: FileService,
  ) {
  }

  ngOnInit(): void {
  }
  loadData() {
    this.response = {
      "controller": "customer-group.page.json",
      "tableName": "customerGroup",
      "primaryKey": [
        "customer_group_id"
      ],
      "language": "en",
      "unit": "CTY",
      "idVC": "",
      "type": "list",
      "action": "loading",
      "sort": "customer_group_id",
      "userId": "1",
      "data": [
        {
          "customer_group_id": "CG001",
          "customer_group_name": "Khách hàng VIP",
          "status": "1",
          "note": "Khách hàng có mức chi tiêu cao, được ưu đãi đặc biệt",
          "datetime0": "0001-01-01T00:00:00",
          "datetime2": "0001-01-01T00:00:00"
        },
        {
          "customer_group_id": "CG002",
          "customer_group_name": "Khách hàng thân thiết",
          "status": "1",
          "note": "Khách hàng quay lại thường xuyên",
          "datetime0": "0001-01-01T00:00:00",
          "datetime2": "0001-01-01T00:00:00"
        },
        {
          "customer_group_id": "CG003",
          "customer_group_name": "Khách hàng tiềm năng",
          "status": "0",
          "note": "Chưa phát sinh đơn hàng nhưng có tiềm năng",
          "datetime0": "0001-01-01T00:00:00",
          "datetime2": "0001-01-01T00:00:00"
        },
        {
          "customer_group_id": "CG004",
          "customer_group_name": "Khách hàng doanh nghiệp",
          "status": "0",
          "note": "Công ty hoặc tổ chức mua hàng số lượng lớn",
          "datetime0": "0001-01-01T00:00:00",
          "datetime2": "0001-01-01T00:00:00"
        },
        {
          "customer_group_id": "CG005",
          "customer_group_name": "Khách hàng một lần",
          "status": "0",
          "note": "Khách hàng chỉ mua hàng 1 lần, không có lịch sử quay lại",
          "datetime0": "0001-01-01T00:00:00",
          "datetime2": "0001-01-01T00:00:00"
        }
      ],
      "total": 5,
      "page": 1,
      "pageSize": 10
    }
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
    this.loadData();
  }

  handleClickDelete(data: Record<string, string>): void {
    this.setValue(data);
    this.girdData.query.formId.action = 'delete';
    this.http
      .post(
        `${environment.apiUrl}/api/dynamic/delete`,
        this.girdData.query.formId
      )
      .subscribe({
        next: (response) => {
          const res = response as { message: string }
          alert(res.message || 'Thành công.');
          this.loadData();
        },
        error: (err) => {
          alert(
            !err?.error?.success
              ? `${(err.error.errors as { message: string }[])?.map((e: { message: string }) => e.message).join('\n') || ''}`
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
    };

    const headers = new HttpHeaders({
      Authorization: `Bearer ${localStorage.getItem(`token`)}`,
      'Content-Type': 'application/json',
      'Custom-Header': 'CustomValue'
    });
    this.http.post(`${environment.apiUrl}/api/FormConfig/SyncData`, formId, { headers })
      .subscribe((meta: any) => {
        if (meta.StatusCode != 200) {
          alert(meta.message);

        } else {

          localStorage.setItem(`action_${action.controller}`, JSON.stringify(meta.Data));
          // ✅ Mở tab mới
          const url = this.router.serializeUrl(
            this.router.createUrlTree([action.target])
          );
          window.open(url, '_blank');
        }
      }, error => {
        console.error('❌ Lỗi gọi API:', error);
        alert(error?.error?.message || 'Lỗi không xác định!');
      });

  }


  onGridButtonClickV2(action: GridAction): void {

    localStorage.removeItem(`param_${this.girdData.id}`);

    localStorage.setItem(
      `action_${action.id}`,
      localStorage.getItem(`selection_${this.girdData.id}`) ?? ""
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
    const masterPrefix = this.girdData.query.formId.formId?.toLowerCase() || 'form';

    // Nếu còn export dạng list thì xóa (chuyển sang detail)
    const listKey = this.controll.toLowerCase();
    if (this.exportData[listKey]) {
      delete this.exportData[listKey];
    }

    const formId: any = {
      ...this.girdData.query.formId,
      value: keyValues,
      action: 'update',
    };

    if (formId.type === 'voucher') {
      formId.VCDate = row['voucherDate'];
    }

    this.http
      .post<any>(`${environment.apiUrl}/api/FormConfig/GetFormData`, formId)
      .subscribe({
        next: (res) => {
          const metadata = res.Data;

          const masterKey = `${masterPrefix}${suffix}`;
          const detailKey = `${masterPrefix}detail${suffix}`;

          const masterData = metadata.tabs
            .filter((tab: any) => !!tab.form)
            .map((tab: any) => tab.form.initialData || {})
            .reduce((a: any, b: any) => ({ ...a, ...b }), {});

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

          localStorage.setItem(`exportData_${this.girdData.id}`, JSON.stringify(this.exportData));
          localStorage.setItem(`selection_${this.girdData.id}`, JSON.stringify(this.selectedOptions));

          console.log(`Gán exportData[${masterKey}] & [${detailKey}]`, this.exportData);
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
        const masterPrefix = this.girdData.query.formId.formId?.toLowerCase() || 'form';
        const masterKey = `${masterPrefix}${suffix}`;
        const detailKey = `${masterPrefix}detail${suffix}`;

        delete this.exportData[masterKey];
        delete this.exportData[detailKey];
        delete this.exportKeyMap[key]; // Xoá ánh xạ
      }

      localStorage.setItem(`exportData_${this.girdData.id}`, JSON.stringify(this.exportData));
      localStorage.setItem(`selection_${this.girdData.id}`, JSON.stringify(this.selectedOptions));
      console.log(`Gán exportData sau khi selected`, this.exportData);
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
    localStorage.removeItem(`selection_${this.girdData.id}`)

    // Nếu muốn reset về export đơn giản (list thô)
    this.updateExportData();
    console.log(`Đã reset exportData:`, this.exportData);
  }
  getButtonClasses(color: string): string {
    switch (color) {
      case 'orange': return 'bg-orange-500 hover:bg-orange-600 text-white';
      case 'green': return 'bg-green-600 hover:bg-green-700 text-white';
      case 'red': return 'bg-red-600 hover:bg-red-700 text-white';
      case 'white': return 'bg-white text-black border border-gray-300';
      default: return 'bg-gray-300 text-black';
    }
  }
}
