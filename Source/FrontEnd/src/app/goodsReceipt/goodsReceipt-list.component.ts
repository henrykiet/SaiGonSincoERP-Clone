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

export class GoodsReceiptGridComponent {
  initData: GirdInitData = {
    id: "goodsReceipt",
    title: "Phiếu nhập hàng",
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
        "key": "number_invoice",
        "label": "Số hóa đơn",
        "type": "text"
      },
      {
        "key": "voucherNumber",
        "label": "Số phiếu nhập hàng",
        "type": "text"
      },
      {
        "key": "supplierCode",
        "label": "Khách hàng",
        "type": "text"
      },
      {
        "key": "addressSupplier",
        "label": "Địa chỉ",
        "type": "text"
      },
      {
        "key": "phoneSupplier",
        "label": "Điện thoại",
        "type": "text"
      }, 
      {
        "key": "note",
        "label": "Ghi chú",
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
        controller: "goodsReceipt.page.json",
        formId: "goodsReceipt",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z10",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["goodsReceipt", "goodsReceiptDetail"],
        VCDate: ""
      },
    },
    sort: 'voucherDate',
    actions: [
      
    ]
  }
}
