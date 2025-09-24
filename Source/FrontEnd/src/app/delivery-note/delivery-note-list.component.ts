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

export class DeliveryNoteGridComponent {
  initData: GirdInitData = {
    id: "delivery-note",
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
        "label": "Ngày xuất hàng",
        "type": "date"
      },
      {
        "key": "voucherNumber",
        "label": "Số phiếu",
        "type": "text"
      },
      {
        "key": "customer_id",
        "label": "Khách hàng",
        "type": "text"
      },
      {
        "key": "jobCode",
        "label": "Dự án",
        "type": "text"
      },
      {
        "key": "contactPerson",
        "label": "Người liên hệ",
        "type": "text"
      },
      {
        "key": "phonePerson",
        "label": "Điện thoại",
        "type": "text"
      },
      {
        "key": "emailPerson",
        "label": "Email",
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
        controller: "delivery-note.page.json",
        formId: "deliveryNote",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z05",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["deliveryNote", "deliveryNoteDetail"],
        VCDate: ""
      },
    },
    sort: 'voucherDate',
    actions: [
      
    ]
  }
}
