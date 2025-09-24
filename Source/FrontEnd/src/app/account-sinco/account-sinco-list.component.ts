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
export class AccountSincoGridComponent {
  initData: GirdInitData = {
    id: 'account-sinco',
    title: 'GRID.ACCOUNT_SINCO',
    headers: [
      {
        key: 'account_sinco_id',
        label: 'POPUP.ACCOUNT_SINCO.CODE',
        type: 'text',
        sortable: true,
      },
      {
        key: 'account_sinco_name',
        label: 'POPUP.ACCOUNT_SINCO.NAME',
        type: 'text',
      },
      {
        key: 'account_type',
        label: 'POPUP.ACCOUNT_SINCO.TYPE',
        type: 'select',
      },
      {
        key: 'bank_name',
        label: 'POPUP.ACCOUNT_SINCO.BANK_NAME',
        type: 'lookup',
      },
      {
        key: 'account_number',
        label: 'POPUP.ACCOUNT_SINCO.ACCOUNT_NUMBER',
        type: 'text',
      },
      {
        key: 'bank_branch',
        label: 'POPUP.ACCOUNT_SINCO.BANK_BRANCH',
        type: 'text',
      },
      {
        key: 'note',
        label: 'POPUP.ACCOUNT_SINCO.NOTE',
        type: 'textarea',
      },
      {
        key: 'status',
        label: 'POPUP.ACCOUNT_SINCO.STATUS',
        type: 'select',
      }
    ],
    query: {
      formId: {
        controller: 'account-sinco.page.json',
        formId: 'accountSinco',
        primaryKey: ['account_sinco_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['accountSinco'],
        VCDate: '',
      },
    },
    sort: 'account_sinco_id',
    actions: []
  };
}
