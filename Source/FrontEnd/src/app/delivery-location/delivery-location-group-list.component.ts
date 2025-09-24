import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DynamicGridComponent } from '../dynamic-gird/dynamic-grid.component';
import { GirdInitData } from '../models';

@Component({
  selector: 'app-grid-master',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicGridComponent],
  templateUrl: '../dynamic-gird/dynamic-grid-parent.component.html',
})
export class DeliveryLocationGridComponent {
  initData: GirdInitData = {
    id: 'delivery-location',
    title:"Giao hàng",
    headers: [
      {
        key: 'delivery_id',
        label: 'Mã nơi giao hàng',
        type: 'text',
        sortable: true,
      },
      {
        key: 'delivery_name',
        label: 'Tên nơi giao hàng',
        type: 'text',
      },
      {
        key: 'address',
        label: 'Địa chỉ',
        type: 'text',
      },
      {
        key: 'phone_number',
        label: 'Điện thoại',
        type: 'text',
      },
      {
        key: 'location',
        label: 'Khu vực giao hàng',
        type: 'text',
      },
      {
        key: 'note',
        label: 'Ghi chú',
        type: 'text',
      },
    ],
    query: {
      formId: {
        controller: 'delivery-location.page.json',
        formId: 'deliveryLocation',
        primaryKey: ['delivery_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['deliveryLocation'],
        VCDate: '',
      },
    },
    sort: 'delivery_id',
    actions: []
  };
}
