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
export class NoteGridComponent {
  initData: GirdInitData = {
    id: 'note',
    title: 'Ghi chú',
    headers: [
      {
        key: 'note_id',
        label: 'Mã ghi chú',
        type: 'text',
      },
      {
        key: 'note_content',
        label: 'Ghi chú default',
        type: 'textarea',
      },
      {
        key: 'feature_code',
        label: 'Tính năng',
        type: 'lookup',
      },
      {
        key: 'status',
        label: 'Trạng thái',
        type: 'select',
      },
    ],
    query: {
      formId: {
        controller: 'note.page.json',
        formId: 'note',
        primaryKey: ['note_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['note'],
        VCDate: '',
      },
    },
    sort: 'note_id',
    actions: [],
  };
}
