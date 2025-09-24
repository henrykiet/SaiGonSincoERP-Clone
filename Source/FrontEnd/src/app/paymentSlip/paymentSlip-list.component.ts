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

export class PaymentSlipGridComponent {
  initData: GirdInitData = {
    id: "paymentSlip",
    title:"Phiếu chi",
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
        "label": "Ngày phiếu chi",
        "type": "date"
      }, 
      {
        "key": "receiptCode",
        "label": "Phiếu nhập hàng",
        "type": "text"
      },
      {
        "key": "invoiceNumber",
        "label": "Số hóa đơn",
        "type": "text"
      },
      {
        "key": "supplierCode",
        "label": "Mã NCC",
        "type": "text"
      },
      {
        "key": "employeeCode",
        "label": "Nhân viên nhận tiền",
        "type": "text"
      },
      {
        "key": "cashier",
        "label": "Nhân viên chi",
        "type": "text"
      },
      {
        "key": "reason",
        "label": "Lý do chi",
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
        controller: "paymentSlip.page.json",
        formId: "PaymentSlip",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z06",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["PaymentSlip", "PaymentSlipDetail"],
        VCDate: ""
      },
    },
    sort: 'voucherDate',
    actions: [
    ]
  }
}
