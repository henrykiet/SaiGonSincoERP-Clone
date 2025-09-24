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

export class OrderGridComponent {
  initData: GirdInitData = {

    id: "Order",
    title: "GRID.ORDER",

    headers: [
      {
        "key": "idGui",
        "label": "ID",
        "type": "text",
        hidden: true,
        "sortable": true
      },
      {
        "key": "voucherDate",
        "label": "POPUP.ORDER.VOUCHER_DATE",
        "type": "date"
      },
      {
        "key": "voucherNumber",
        "label": "POPUP.ORDER.VOUCHER_NUMBER",
        "type": "text"
      },
      {
        "key": "customerID",
        "label": "POPUP.ORDER.CUSTOMER_ID",
        "type": "text"
      },
      {
        "key": "jobCode",
        "label": "POPUP.ORDER.JOB_CODE",
        "type": "text"
      },
      {
        "key": "deliveryDate",
        "label": "POPUP.ORDER.DELIVERY_DATE",
        "type": "date"
      },
      {
        "key": "deliveryCust",
        "label": "POPUP.ORDER.DELIVERY_CUST",
        "type": "text"
      },
      {
        "key": "deliveryAddress",
        "label": "POPUP.ORDER.DELIVERY_ADDRESS",
        "type": "text"
      },
      {
        "key": "status",
        "label": "POPUP.ORDER.STATUS",
        "type": "text"
      }
    ],
    query: {
      formId: {
        controller: "Order.page.json",
        formId: "Order",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z02",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["Order", "OrderDetail"],
        VCDate: ""
      },
    },
    sort: 'voucherDate',
    actions: [
      {
        controller: "contract",
        id: "contract.page.json",
        label: "Tạo hợp đồng",
        target: "contract/popup",
        color: "orange"
      }
    ]
  }
}
