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

export class UomGridComponent {
  initData: GirdInitData = {
    id: "uom",
    title:"GRID.UOM",
    headers: [
      {
        "key": "uomCode",
        "label": "POPUP.UOM.CODE",
        "type": "text",
        "sortable": true
      },
      {
        "key": "uomName",
        "label": "POPUP.UOM.NAME_VN",
        "type": "text"
      },
      {
        "key": "uomName2",
        "label": "POPUP.UOM.NAME_EN",
        "type": "text"
      },
      {
        "key": "status",
        "label": "POPUP.UOM.STATUS",
        "type": "text"
      }
    ],
    query: {
      formId: {
        controller: "uom.page.json",
        formId: "uom",
        primaryKey: ["uomCode"],
        type: "list",
        action: "loading",
        language: localStorage.getItem("language") ?? "vn",
        unit: localStorage.getItem('unit') ?? 'CTY',
        idVC: "",
        userId: localStorage.getItem("userId") ?? "",
        value: [],
        listTable: ["uom"],
        VCDate: ""
      },
    },
    sort: 'uomCode',
    actions: []
  }
}
