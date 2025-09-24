import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StepComponent } from '../step/step.component'; // tùy đường dẫn
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sales-process',
  standalone: true,
  imports: [CommonModule, StepComponent, RouterModule],
  templateUrl: './sales-process.component.html',
})
export class SalesProcessComponent {}
