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
export class EmployeeGridComponent {
  initData: GirdInitData = {
    id: 'employee',
    title: 'Nhân viên',
    headers: [
      {
        key: 'employee_id',
        label: 'Mã nhân viên',
        type: 'text',
      },
      {
        key: 'employee_role_id',
        label: 'Vai trò',
        type: 'text',
      },
      {
        key: 'full_name',
        label: 'Họ và tên',
        type: 'text',
      },
      {
        key: 'abbreviation',
        label: 'Tên viết tắt',
        type: 'text',
      },
      {
        key: 'gender',
        label: 'Giới tính',
        type: 'select',
      },
      {
        key: 'birth_date',
        label: 'Ngày sinh',
        type: 'date',
      },
      {
        key: 'starting_date',
        label: 'Ngày bắt đầu làm việc',
        type: 'date',
      },
      {
        key: 'end_date',
        label: 'Ngày nghỉ việc',
        type: 'date',
      },
      {
        key: 'status',
        label: 'Trạng thái',
        type: 'select',
      },
      {
        key: 'email',
        label: 'Email',
        type: 'text',
      },
      {
        key: 'phone_number',
        label: 'Số điện thoại',
        type: 'text',
      },
      {
        key: 'note',
        label: 'Ghi chú',
        type: 'text',
      },
      {
        key: 'reason_leave_job',
        label: 'Lý do nghỉ việc',
        type: 'text',
      },
    ],
    query: {
      formId: {
        controller: 'employee.page.json',
        formId: 'employee',
        primaryKey: ['employee_id'],
        type: 'list',
        action: 'loading',
        language: localStorage.getItem('language') ?? 'vi',
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: '',
        userId: localStorage.getItem('userId') ?? '',
        value: [],
        listTable: ['employee', 'employeeAddress', 'employeeCollectedInfo'],
        VCDate: '',
      },
    },
    sort: 'employee_id',
    actions: [],
  };
}
