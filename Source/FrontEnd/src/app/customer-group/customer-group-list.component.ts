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
export class CustomerGroupGridComponent {
  initData: GirdInitData = {
    id: "customer-group",
    title: "GRID.CUSTOMER_GROUP",
    headers: [
      {
        key: "customer_group_id",
        label: "POPUP.CUSTOMER_GROUP.ID",
        type: "text",
        sortable: true
      },
      {
        key: "customer_group_name",
        label: "POPUP.CUSTOMER_GROUP.NAME",
        type: "text"
      },
      {
        key: "note",
        label: "POPUP.CUSTOMER_GROUP.NOTE",
        type: "text"
      },
      {
        key: "status",
        label: "POPUP.CUSTOMER_GROUP.STATUS",
        type: "text"
      }
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
      },
    },
    sort: 'customer_group_id',
    actions: []
  };
}
