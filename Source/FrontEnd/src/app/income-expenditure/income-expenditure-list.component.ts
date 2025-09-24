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

export class IncomeExpenditureComponent {
  initData: GirdInitData = {
    id: "income-expenditure",
    title: "GRID.INCOME_EXPENDITURE",
    headers: [
      {
        key: "type",
        label: "POPUP.INCOME_EXPENDITURE.TYPE",
        type: "text",
        sortable: true
      },
      {
        key: "incomeExpenditure_id",
        label: "POPUP.INCOME_EXPENDITURE.CODE",
        type: "text",
        sortable: true
      },
      {
        key: "incomeExpenditure_Name",
        label: "POPUP.INCOME_EXPENDITURE.NAME_VN",
        type: "text"
      },
      {
        key: "incomeExpenditure_Name2",
        label: "POPUP.INCOME_EXPENDITURE.NAME_EN",
        type: "text"
      },
      {
        key: "serial",
        label: "POPUP.INCOME_EXPENDITURE.SERIAL",
        type: "number"
      },
      {
        key: "status",
        label: "POPUP.INCOME_EXPENDITURE.STATUS",
        type: "text"
      }
    ],
    query: {
      formId: {
        controller: "income-expenditure.page.json",
        formId: "incomeExpenditure",
        primaryKey: ["incomeExpenditure_id"],
        type: "list",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["incomeExpenditure"],
        VCDate: ""
      },
    },
    sort: 'incomeExpenditure_id',
    actions: []
  }
}
