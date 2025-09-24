import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DynamicPopupComponent } from '../dynamic-popup/dynamic-popup.component';

@Component({
  selector: 'app-poin-popup',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicPopupComponent],
  templateUrl: '../dynamic-popup/dynamic-popup-parent.component.html',
})

export class PoinPopupComponent {
  id: string = "poin"
  name: string = "poin.page.json"
}
