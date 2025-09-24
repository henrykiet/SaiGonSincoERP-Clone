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
export class ItemGridComponent {
  initData: GirdInitData = {
    id: "item",
    title: "GRID.ITEM",
    headers: [
      {
        key: "item_id",
        label: "POPUP.ITEM.ID",
        type: "text",
        sortable: true
      },
      {
        key: "item_name",
        label: "POPUP.ITEM.NAME",
        type: "text"
      },
      {
        key: "group_item_id",
        label: "POPUP.ITEM.GROUP_ID",
        type: "text"
      },
      {
        key: "group_item_name",
        label: "POPUP.ITEM.GROUP_NAME",
        type: "text"
      },
      {
        key: "uom",
        label: "POPUP.ITEM.UOM",
        type: "text"
      },
      {
        key: "purchase_price",
        label: "POPUP.ITEM.PURCHASE_PRICE",
        type: "number"
      },
      {
        key: "sale_price",
        label: "POPUP.ITEM.SALE_PRICE",
        type: "number"
      },
      {
        key: "min_inventory",
        label: "POPUP.ITEM.MIN_INVENTORY",
        type: "number"
      },
      {
        key: "max_inventory",
        label: "POPUP.ITEM.MAX_INVENTORY",
        type: "number"
      },
      {
        key: "status",
        label: "POPUP.ITEM.STATUS",
        type: "text"
      }
    ],
    query: {
      formId: {
        controller: 'item.page.json',
        formId: 'item',
        primaryKey: ['item_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['item'],
        VCDate: '',
        isFileHandle: true,
      },
    },
    sort: 'item_id',
    actions: []
  };
}
