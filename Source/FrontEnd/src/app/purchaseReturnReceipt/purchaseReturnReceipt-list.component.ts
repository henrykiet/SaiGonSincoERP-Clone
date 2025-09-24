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

export class PurchaseReturnReceiptGridComponent {
  initData: GirdInitData = {
    id: "PurchaseReturnReceipt",
    title: "Phiếu nhập hàng trả lại",
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
        "label": "Ngày nhập hàng",
        "type": "date"
      },
      {
        "key": "voucherNumber",
        "label": "Số phiếu nhập hàng",
        "type": "text"
      },
      {
        "key": "customerGroupCode",
        "label": "Nhóm khách hàng",
        "type": "text"
      },
      {
        "key": "customerCode",
        "label": "Khách hàng",
        "type": "text"
      },
      {
        "key": "addressCustomer",
        "label": "Địa chỉ",
        "type": "text"
      },
      {
        "key": "phoneCustomer",
        "label": "Điện thoại",
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
        controller: "purchaseReturnReceipt.page.json",
        formId: "purchaseReturn",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z09",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["purchaseReturn", "purchaseReturnDetail"],
        VCDate: "",
        isFileHandle: true,
      },
    },
    sort: 'voucherDate',
    actions: [
      
    ]
  }
}
