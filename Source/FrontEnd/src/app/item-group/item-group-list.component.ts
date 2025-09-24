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
export class ItemGroupGridComponent {
  initData: GirdInitData = {
    id: 'item-group',
    title:"Nhóm vật tư",
    headers: [
      {
        key: 'item_group_id',
        label: 'Mã nhóm',
        type: 'text',
      },
      {
        key: 'item_group_name',
        label: 'Tên nhóm',
        type: 'text',
      },
      {
        key: 'group_index',
        label: 'Thứ tự hiển thị',
        type: 'number',
      },
      {
        key: 'industry_group_id',
        label: 'Nhóm ngành hàng',
        type: 'select',
      },
      {
        key: 'note',
        label: 'Ghi chú',
        type: 'text',
      },
      {
        key: 'status',
        label: 'Trạng thái',
        type: 'select',
      },
    ],
    query: {
      formId: {
        controller: 'item-group.page.json',
        formId: 'itemGroup',
        primaryKey: ['item_group_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: 'Z02',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['itemGroup', 'industryGroup'],
        VCDate: '',
        isFileHandle: true,
      },
    },
    sort: 'item_group_id',
	actions: []
  };
}
