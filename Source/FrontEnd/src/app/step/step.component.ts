import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-step',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="step-box">
      <i [class]="'fas fa-' + icon" class="text-2xl text-indigo-600 mb-2"></i>
      <div class="text-sm font-medium text-center">{{ title }}</div>
    </div>
  `,
  styles: [`
    .step-box {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      border: 1px solid #d1d5db;
      border-radius: 0.5rem;
      padding: 1rem;
      background-color: #ffffff;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
      min-height: 100px;
    }
  `]
})
export class StepComponent {
  @Input() title: string = '';
  @Input() icon: string = ''; // ví dụ: 'file-alt', 'chart-line', 'money-bill-wave'
}
