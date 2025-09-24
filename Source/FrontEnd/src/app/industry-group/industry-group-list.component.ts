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
export class IndustryGroupGridComponent {
  initData: GirdInitData = {
    id: 'industry-group',
    title: 'Ngành hàng',
    headers: [
      {
        key: 'industry_group_id',
        label: 'Mã ngành',
        type: 'text',
        sortable: true,
      },
      {
        key: 'industry_group_name',
        label: 'Tên ngành',
        type: 'text',
      },
      {
        key: 'note',
        label: 'Ghi chú',
        type: 'text',
      },
    ],
    query: {
      formId: {
        controller: 'industry-group.page.json',
        formId: 'industryGroup',
        primaryKey: ['industry_group_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['industryGroup', 'itemGroup'],
        VCDate: '',
      },
    },
    sort: 'industry_group_id',
    actions: [],
  };
}
