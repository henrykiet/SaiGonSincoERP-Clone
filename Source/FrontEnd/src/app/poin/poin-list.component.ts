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

export class PoinGridComponent {
  initData: GirdInitData = {
    id: "poin",
    title:"Đơn đặt nhập hàng",
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
        "label": "Ngày tạo",
        "type": "date"
      },
      {
        "key": "voucherNumber",
        "label": "Số phiếu",
        "type": "text"
      },
      {
        "key": "vendorGroupCode",
        "label": "Nhóm NCC",
        "type": "text"
      },
      {
        "key": "expectedDate",
        "label": "Ngày dự kiến",
        "type": "date"
      },
      {
        "key": "approveDate",
        "label": "Ngày duyệt",
        "type": "date"
      },
      {
        "key": "vendorCode",
        "label": "Mã NCC",
        "type": "text"
      },
      {
        "key": "vendorName",
        "label": "Nhà cung cấp",
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
        controller: "poin.page.json",
        formId: "poin",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z03",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["poin", "poindetail"],
        VCDate: ""
      },
    },
    sort: 'voucherDate',
    actions: [
      {
        controller: "goodsReceipt",
        id: "goodsReceipt.page.json",
        label: "Tạo phiếu nhập hàng",
        target: "goodsReceipt/popup",
        color: "orange"
      }
    ]
  }
}
