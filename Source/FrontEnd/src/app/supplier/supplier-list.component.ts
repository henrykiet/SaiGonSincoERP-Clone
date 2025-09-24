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
export class SupplierGridComponent {
  initData: GirdInitData = {
    id: 'supplier',
    title: 'Nhà cung cấp',
    headers: [
      {
        key: 'supplier_id',
        label: 'Mã nhà cung cấp(Mã số thuế)',
        type: 'text',
      },
      {
        key: 'supplier_name',
        label: 'Tên nhà cung cấp',
        type: 'text',
      },
      {
        key: 'supplier_type_id',
        label: 'Loại nhà cung cấp',
        type: 'select',
      },
      {
        key: 'industry_group_id',
        label: 'Nhóm ngành hàng cung cấp',
        type: 'select',
      },
      {
        key: 'phone_number',
        label: 'Số điện thoại',
        type: 'text',
      },
      {
        key: 'email',
        label: 'Email',
        type: 'text',
      },
      {
        key: 'address',
        label: 'Địa chỉ',
        type: 'text',
      },
      {
        key: 'province',
        label: 'Tỉnh/Thành phố',
        type: 'text',
      },
      {
        key: 'district',
        label: 'Quận/Huyện',
        type: 'text',
      },
      {
        key: 'status',
        label: 'Trạng thái',
        type: 'select',
      },
      {
        key: 'customer_id',
        label: 'Liên kết khách hàng',
        type: 'select',
      },
    ],
    query: {
      formId: {
        controller: 'supplier.page.json',
        formId: 'supplier',
        primaryKey: ['supplier_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['supplier', 'supplierContact', 'supplierBankAccount'],
        VCDate: '',
      },
    },
    sort: 'supplier_id',
    actions: [],
  };
}
