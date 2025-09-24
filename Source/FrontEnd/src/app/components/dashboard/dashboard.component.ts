import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { StatisticsCardComponent } from './statistics-card/statistics-card.component';
import { ChartCardComponent } from './chart-card/chart-card.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    StatisticsCardComponent,
    ChartCardComponent
  ]
})
export class DashboardComponent implements OnInit {
  today: Date = new Date();
  errorMessage: string = '';
  isLoading: boolean = true;

  statistics = [
    {
      title: 'Tổng doanh thu',
      value: '$24,780',
      icon: 'fas fa-dollar-sign',
      trend: 12,
      trendText: 'so với tháng trước',
      trendUp: true,
      color: '#4CAF50'
    },
    {
      title: 'Số lượng đơn hàng',
      value: '1,234',
      icon: 'fas fa-shopping-cart',
      trend: 8,
      trendText: 'so với tháng trước',
      trendUp: true,
      color: '#2196F3'
    },
    {
      title: 'Khách hàng mới',
      value: '456',
      icon: 'fas fa-users',
      trend: 5,
      trendText: 'so với tháng trước',
      trendUp: true,
      color: '#FF9800'
    },
    {
      title: 'Tỷ lệ chuyển đổi',
      value: '3.2%',
      icon: 'fas fa-chart-line',
      trend: 2,
      trendText: 'so với tháng trước',
      trendUp: false,
      color: '#F44336'
    }
  ];

  charts = [
    {
      title: 'Doanh thu theo tháng',
      type: 'line',
      data: [65, 59, 80, 81, 56, 55, 40],
      labels: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6', 'Tháng 7'],
      color: '#4CAF50'
    },
    {
      title: 'Phân loại sản phẩm',
      type: 'pie',
      data: [300, 500, 100],
      labels: ['Điện thoại', 'Laptop', 'Phụ kiện'],
      color: '#2196F3'
    }
  ];

  constructor(private router: Router) {}

  ngOnInit() {
    // Kiểm tra xem người dùng đã đăng nhập chưa
    const token = localStorage.getItem('token');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    // Giả lập loading data
    setTimeout(() => {
      this.isLoading = false;
    }, 1000);
  }

  refreshData() {
    this.isLoading = true;
    // Giả lập refresh data
    setTimeout(() => {
      this.isLoading = false;
    }, 1000);
  }
}
