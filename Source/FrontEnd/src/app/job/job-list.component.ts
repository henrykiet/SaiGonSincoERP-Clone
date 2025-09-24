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

export class JobGridComponent {
  initData: GirdInitData = {
    id: "job",
    title:"Dự án",
    headers: [
      {
        "key": "jobCode",
        "label": "Mã dự án",
        "type": "text",
        "sortable": true
      },
      {
        "key": "jobName",
        "label": "Tên dự án (VN)",
        "type": "text"
      },
      {
        "key": "jobName2",
        "label": "Tên khác",
        "type": "text"
      }, 
      {
        "key": "status",
        "label": "Trạng thái",
        "type": "text"
      }
    ],
    query: {
      formId: {
        controller: "job.page.json",
        formId: "job",
        primaryKey: ["jobCode"],
        type: "list",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["job"],
        VCDate: ""
      },
    },
    sort: 'jobCode',
    actions: []
  }
}
