import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { GirdInitData } from '../models';
import { DynamicGridComponent } from '../dynamic-gird/dynamic-grid.component';
import { TranslateModule } from '@ngx-translate/core'
@Component({
  selector: 'app-grid-master',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicGridComponent,TranslateModule],
  templateUrl: '../dynamic-gird/dynamic-grid-parent.component.html',
})

export class ReceiptGridComponent {
  initData: GirdInitData = {
    id: "receipt",
    title: "Phiếu thu",
    headers: [
      {
        "key": "idGui",
        "label": "ID",
        "type": "text",
        "hidden": true,
        "sortable": true
      },
      {
        "key": "voucherDate",
        "label": "Ngày tạo",
        "type": "date"
      },
      {
        "key": "voucherNumber",
        "label": "Số phiếu thu",
        "type": "text"
      },
      {
        "key": "customerCode",
        "label": "Mã khách hàng",
        "type": "text"
      },
      {
        "key": "invoiceNumber",
        "label": "Số hóa đơn",
        "type": "text"
      },
      {
        "key": "status",
        "label": "Trạng thái",
        "type": "text"
      }
    ],
    query: {
      formId: {
        controller: "receipt.page.json",
        formId: "receipt",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z07",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["receipt", "receiptDetail"],
        VCDate: ""
      },
    },
    sort: 'voucherDate',
    actions: []
  }
}
