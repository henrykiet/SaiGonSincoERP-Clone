import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DynamicReportComponent } from '../dynamic-report/dynamic-report.component';
import { GirdInitData } from '../models';

@Component({
  selector: 'app-grid-master',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicReportComponent],
  templateUrl: '../dynamic-report/dynamic-report-parent.component.html',
})
export class ReportExampleComponent {
  initData: GirdInitData = {
    id: 'customer-group',
    title:"Nhóm khách hàng",
    headers: [
      {
        key: 'customer_group_id',
        label: 'Mã nhóm',
        type: 'text',
        sortable: true,
      },
      {
        key: 'customer_group_name',
        label: 'Tên nhóm',
        type: 'text',
      },
      {
        key: 'note',
        label: 'Ghi chú',
        type: 'text',
      },
      {
        key: 'status',
        label: 'Trạng thái',
        type: 'text',
      },
    ],
    query: {
      formId: {
        controller: 'customer-group.page.json',
        formId: 'customerGroup',
        primaryKey: ['customer_group_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['customerGroup'],
        VCDate: '',
        isFileHandle: true,
      },
    },
    sort: 'customer_group_id',
    actions: []
  };
}
