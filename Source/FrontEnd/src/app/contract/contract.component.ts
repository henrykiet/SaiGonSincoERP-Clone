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

export class ContractGridComponent {
  initData: GirdInitData = {
    id: "contract",
    title:"Hợp đồng",
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
        "key": "number_ddh",
        "label": "Số ĐĐH",
        "type": "text"
      },
      {
        "key": "customer_id",
        "label": "Mã khách hàng",
        "type": "text"
      },
      {
        "key": "quotationCode",
        "label": "Mã báo giá",
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
        controller: "contract.page.json",
        formId: "contract",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z04",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["Contract", "ContractDetail"],
        VCDate: ""
      },
    },
    sort: 'voucherDate',
    actions: []
  }
}
