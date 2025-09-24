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
export class PositionGridComponent {
  initData: GirdInitData = {
    id: 'position',
    headers: [
      {
        key: 'position_id',
        label: 'Mã Chức Vụ',
        type: 'text',
        sortable: true,
      },
      {
        "key": "position_name",
        "label": "Tên chức vụ",
        "type": "text",
      },
      {
        "key": "position_group_id",
        "label": "Nhóm chức vụ",
        "type": "select",
      },
      {
        "key": "level",
        "label": "Cấp bậc",
        "type": "select",
      },
      {
        "key": "description",
        "label": "Mô tả chức vụ",
        "type": "text",
      },
      {
        "key": "status",
        "label": "Trạng thái",
        "type": "select",
      },
      {
        "key": "display_order",
        "label": "Thứ tự hiển thị",
        "type": "number",
      }
    ],
    query: {
      formId: {
        controller: 'position.page.json',
        formId: 'position',
        primaryKey: ['position_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['position'],
        VCDate: '',
      },
    },
    sort: 'position_id',
    actions: []
  };
}
