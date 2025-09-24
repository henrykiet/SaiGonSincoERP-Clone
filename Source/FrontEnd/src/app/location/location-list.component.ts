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
export class LocationGridComponent {
  initData: GirdInitData = {
    id: 'location',
    title: 'GRID.LOCATION',
    headers: [
      {
        key: "province_code",
        label: "POPUP.LOCATION.PROVINCE_CODE",
        type: "text",
        sortable: true
      },
      {
        key: "province_name",
        label: "POPUP.LOCATION.PROVINCE_NAME",
        type: "text"
      },
      {
        key: "district_code",
        label: "POPUP.LOCATION.DISTRICT_CODE",
        type: "text",
        sortable: true
      },
      {
        key: "district_name",
        label: "POPUP.LOCATION.DISTRICT_NAME",
        type: "text"
      },
      {
        key: "ward_code",
        label: "POPUP.LOCATION.WARD_CODE",
        type: "text",
        sortable: true
      },
      {
        key: "ward_name",
        label: "POPUP.LOCATION.WARD_NAME",
        type: "text",
      },
      {
        key: "unit_type",
        label: "POPUP.LOCATION.UNIT_TYPE",
        type: "select",
      },
      {
        key: "status",
        label: "POPUP.LOCATION.STATUS",
        type: "select",
      }
    ],
    query: {
      formId: {
        controller: 'location.page.json',
        formId: 'location',
        primaryKey: ['province_code', 'district_code', 'ward_code'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['location'],
        VCDate: '',
      },
    },
    sort: 'province_code',
    actions: [],
  };
}
