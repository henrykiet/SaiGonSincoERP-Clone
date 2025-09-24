import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DynamicPopupComponent } from '../dynamic-popup/dynamic-popup.component';

@Component({
  selector: 'app-quotationPaper-popup',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DynamicPopupComponent],
  templateUrl: '../dynamic-popup/dynamic-popup-parent.component.html',
})

export class quotationPaperPopupComponent {
  id: string = "quotationPaper"
  name: string = "quotationPaper.page.json"
}
