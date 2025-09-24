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
export class PriceGridComponent {
  initData: GirdInitData = {
    id: 'price',
    title: 'Giá bán',
    headers: [
      {
        key: 'price_id',
        label: 'Mã bảng giá bán',
        type: 'text',
      },
      {
        key: 'price_name',
        label: 'Tên bảng giá bán',
        type: 'text',
      },
      {
        key: 'customer_group_id',
        label: 'Nhóm khách hàng',
        type: 'text',
      },
      {
        key: 'effective_date',
        label: 'Ngày hiệu lực',
        type: 'date',
      },
      {
        key: 'expired_date',
        label: 'Ngày hết hiệu lực',
        type: 'date',
      },
    ],
    query: {
      formId: {
        controller: 'price.page.json',
        formId: 'price',
        primaryKey: ['price_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['price', 'priceDetail'],
        VCDate: '',
      },
    },
    sort: 'price_id',
    actions: [],
  };
}
