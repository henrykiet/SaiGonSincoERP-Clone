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

export class quotationPaperGridComponent {
  initData: GirdInitData = {
    id: "quotationPaper",
    title: "GRID.QUOTATION_PAPER",
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
        "label": "POPUP.QUOTATION_PAPER.VOUCHER_DATE",
        "type": "date"
      },
      {
        "key": "voucherNumber",
        "label": "POPUP.QUOTATION_PAPER.VOUCHER_NUMBER",
        "type": "text"
      },
      {
        "key": "customerCode",
        "label": "POPUP.QUOTATION_PAPER.CUSTOMER_CODE",
        "type": "text"
      },
      {
        "key": "jobCode",
        "label": "POPUP.QUOTATION_PAPER.JOB_CODE",
        "type": "text"
      },
      {
        "key": "contactPerson",
        "label": "POPUP.QUOTATION_PAPER.CONTACT_PERSON",
        "type": "text"
      },
      {
        "key": "phonePerson",
        "label": "POPUP.QUOTATION_PAPER.PHONE_PERSON",
        "type": "text"
      },
      {
				"key": "employeeName",
				"label": "Tên nhân viên",
				"type": "text",
		  },
      {
        "key": "emailPerson",
        "label": "POPUP.QUOTATION_PAPER.EMAIL_PERSON",
        "type": "text"
      },
      {
        "key": "status",
        "label": "POPUP.QUOTATION_PAPER.STATUS",
        "type": "text"
      }
    ],
    query: {
      formId: {
        controller: "quotationPaper.page.json",
        formId: "QuotationPaper",
        primaryKey: ["idGui"],
        type: "voucher",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "Z02",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["QuotationPaper", "QuotationPaperDetail"],
        VCDate: "",
        isFileHandle: true,
      },
    },
    sort: 'voucherDate',
    actions: [
      {
        controller: "Order",
        id: "Order.page.json",
        label: "Tạo đơn hàng",
        target: "order/popup",
        color: "orange"
      },
      {
        controller: "Order",
        id: "Order.page.json",
        label: "Sao chép",
        target: "order/popup",
        color: "blue"
      }
    ]
  }
}
