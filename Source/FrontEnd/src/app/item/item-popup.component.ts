import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DynamicPopupComponent } from '../dynamic-popup/dynamic-popup.component';

@Component({
  selector: 'app-item-popup',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicPopupComponent],
  templateUrl: '../dynamic-popup/dynamic-popup-parent.component.html',
})

export class ItemPopupComponent {
  id: string = "item"
  name: string = "item.page.json"
}
