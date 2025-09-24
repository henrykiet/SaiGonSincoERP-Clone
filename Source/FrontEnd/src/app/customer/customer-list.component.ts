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
export class CustomerGridComponent {
  initData: GirdInitData = {
    id: 'customer',
    title: "GRID.CUSTOMER",
    headers: [
      {
        key: 'customer_id',
        label: 'POPUP.CUSTOMER.CODE',
        type: 'text',
        sortable: true,
      },
      {
        key: 'customer_name',
        label: 'POPUP.CUSTOMER.NAME',
        type: 'text',
        width: "450px",
      },
      {
        key: 'customer_group_id',
        label: 'POPUP.CUSTOMER.GROUP',
        type: 'text',
      },
      {
        key: 'phone_number',
        label: 'POPUP.CUSTOMER.PHONE',
        type: 'text',
      },
      {
        key: 'email',
        label: 'POPUP.CUSTOMER.EMAIL',
        type: 'text',
      },
      {
        key: 'address',

        label: 'POPUP.CUSTOMER.ADDRESS',
        width: "450px",
        type: 'text',
      },
      {
        key: 'city',
        label: 'POPUP.CUSTOMER.CITY',
        type: 'text',
      },
      {
        key: 'district',
        label: 'POPUP.CUSTOMER.DISTRICT',
        type: 'text',
      },
      {
        key: 'status',
        label: 'POPUP.CUSTOMER.STATUS',
        type: 'select',
      },
    ],
    query: {
      formId: {
        controller: 'customer.page.json',
        formId: 'customer',
        primaryKey: ['customer_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['customer', 'customerDetail'],
        VCDate: '',
        isFileHandle: true,
      },
    },
    sort: 'customer_id',
    actions: []
  };
}
