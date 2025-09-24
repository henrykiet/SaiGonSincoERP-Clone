import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-statistics-card',
  templateUrl: './statistics-card.component.html',
  styleUrls: ['./statistics-card.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class StatisticsCardComponent {
  @Input() title: string = '';
  @Input() value: string | number = '';
  @Input() icon: string = '';
  @Input() trend: number = 0;
  @Input() trendText: string = '';
  @Input() trendUp: boolean = true;
  @Input() color: string = '#007bff';
}
