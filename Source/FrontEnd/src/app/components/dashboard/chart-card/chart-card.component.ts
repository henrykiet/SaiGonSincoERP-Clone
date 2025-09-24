import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-chart-card',
  templateUrl: './chart-card.component.html',
  styleUrls: ['./chart-card.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class ChartCardComponent {
  @Input() title: string = '';
  @Input() chartType: string = 'line';
  @Input() data: any[] = [];
  @Input() labels: string[] = [];
  @Input() color: string = '#007bff';

  getLinePoints(): string {
    if (!this.data || this.data.length === 0) return '';
    
    const maxValue = Math.max(...this.data);
    const minValue = Math.min(...this.data);
    const range = maxValue - minValue || 1;
    
    const points = this.data.map((value, index) => {
      const x = (index / (this.data.length - 1)) * 360 + 20; // 20px padding
      const y = 180 - ((value - minValue) / range) * 160 + 10; // 10px padding
      return `${x},${y}`;
    });
    
    return points.join(' ');
  }

  getPieSlices() {
    if (!this.data || this.data.length === 0) return [];
    
    const total = this.data.reduce((sum, value) => sum + value, 0);
    let currentAngle = 0;
    
    return this.data.map(value => {
      const percentage = value / total;
      const angle = percentage * 360;
      const startAngle = currentAngle;
      const endAngle = currentAngle + angle;
      
      const x1 = Math.cos((startAngle - 90) * Math.PI / 180) * 80;
      const y1 = Math.sin((startAngle - 90) * Math.PI / 180) * 80;
      const x2 = Math.cos((endAngle - 90) * Math.PI / 180) * 80;
      const y2 = Math.sin((endAngle - 90) * Math.PI / 180) * 80;
      
      const largeArc = angle > 180 ? 1 : 0;
      
      const path = `M 0 0 L ${x1} ${y1} A 80 80 0 ${largeArc} 1 ${x2} ${y2} Z`;
      
      currentAngle += angle;
      return { path };
    });
  }

  getSliceColor(index: number): string {
    const colors = [
      '#4CAF50', '#2196F3', '#FF9800', '#F44336', 
      '#9C27B0', '#00BCD4', '#FFEB3B', '#795548'
    ];
    return colors[index % colors.length];
  }
}