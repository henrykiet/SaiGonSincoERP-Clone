import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DynamicGridComponent } from '../dynamic-gird/dynamic-grid.component';
import { GirdInitData } from '../models';

@Component({
  selector: 'app-grid-master',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicGridComponent],
  templateUrl: '../dynamic-gird/dynamic-grid-parent.component.html',
})
export class CompanyGridComponent {
  initData: GirdInitData = {
    id: "company",
    title: "GRID.COMPANY",
    headers: [
      {
        key: "company_id",
        label: "POPUP.COMPANY.ID",
        type: "text",
        sortable: true
      },
      {
        key: "company_name",
        label: "POPUP.COMPANY.NAME",
        type: "text"
      },
      {
        key: "address",
        label: "POPUP.COMPANY.ADDRESS",
        type: "text"
      },
      {
        key: "phone_number",
        label: "POPUP.COMPANY.PHONE_NUMBER",
        type: "text"
      },
      {
        key: "fax_number",
        label: "POPUP.COMPANY.FAX_NUMBER",
        type: "text"
      },
      {
        key: "mst",
        label: "POPUP.COMPANY.MST",
        type: "text"
      }
    ],
    query: {
      formId: {
        controller: 'company.page.json',
        formId: 'company',
        primaryKey: ['company_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['company'],
        VCDate: '',
      },
    },
    sort: 'company_id',
    actions: [],
  };
}
