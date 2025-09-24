import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { GirdInitData } from '../models';
import { DynamicGridComponent } from '../dynamic-gird/dynamic-grid.component';

@Component({
  selector: 'app-grid-master',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicGridComponent],
  templateUrl: '../dynamic-gird/dynamic-grid-parent.component.html',
})
export class PurchaseGridComponent {
  initData: GirdInitData = {
    id: 'purchase',
    title: 'Giá mua',
    headers: [
      {
        key: 'purchase_id',
        label: 'Mã bảng giá mua',
        type: 'text',
      },
      {
        key: 'purchase_name',
        label: 'Tên bảng giá mua',
        type: 'text',
      },
      {
        key: 'supplier_id',
        label: 'Nhà cung cấp',
        type: 'select',
      },
      {
        key: 'start_date',
        label: 'Ngày hiệu lực',
        type: 'date',
      },
      {
        key: 'end_date',
        label: 'Ngày hết hiệu lực',
        type: 'date',
      },
      {
        key: 'status',
        label: 'Trạng thái',
        type: 'select',
      },
    ],
    query: {
      formId: {
        controller: 'purchase.page.json',
        formId: 'purchase',
        primaryKey: ['purchase_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['purchase', 'purchaseDetail'],
        VCDate: '',
      },
    },
    sort: 'purchase_id',
    actions: [],
  };
}
