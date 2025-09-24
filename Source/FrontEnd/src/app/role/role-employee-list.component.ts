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
export class RoleEmployeeGridComponent {
  initData: GirdInitData = {
    id: 'employeeRole',
    title: 'GRID.EMPLOYEE_ROLE',
    headers: [
      {
        key: "employee_role_id",
        label: "POPUP.EMPLOYEE_ROLE.CODE",
        type: "text",
        sortable: true
      },
      {
        key: "role_name",
        label: "POPUP.EMPLOYEE_ROLE.NAME",
        type: "text",
      },
      {
        key: "status",
        label: "POPUP.EMPLOYEE_ROLE.STATUS",
        type: "select"
      }
    ],
    query: {
      formId: {
        controller: 'role-employee.page.json',
        formId: 'employeeRole',
        primaryKey: ['employee_role_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['employeeRole'],
        VCDate: '',
      },
    },
    sort: 'employee_role_id',
    actions: [],
  };
}
