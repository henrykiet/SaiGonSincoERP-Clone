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
export class TaxGridComponent {
  initData: GirdInitData = {
    id: 'tax',
    title: 'Thuế suất',
    headers: [
      {
        key: 'tax_id',
        label: 'Mã thuế',
        type: 'text',
      },
      {
        key: 'tax_type',
        label: 'Loại thuế',
        type: 'text',
      },
      {
        key: 'tax_number',
        label: 'Mức thuế %',
        type: 'number',
      },
    ],
    query: {
      formId: {
        controller: 'tax.page.json',
        formId: 'tax',
        primaryKey: ['tax_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['tax'],
        VCDate: '',
      },
    },
    sort: 'tax_id',
    actions: [],
  };
}
