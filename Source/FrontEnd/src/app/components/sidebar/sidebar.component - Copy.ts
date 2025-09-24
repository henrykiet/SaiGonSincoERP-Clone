import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface MenuItem {
  path: string;
  icon: string;
  label: string;
  children?: MenuItem[];
  isExpanded?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  @Input() isOpen = false;
  @Input() isCollapsed = false;
  @Output() toggleCollapse = new EventEmitter<void>();

menuItems: MenuItem[] = [
  {
    path: '/system',
    icon: 'fas fa-cogs',
    label: 'Hệ thống',
    isExpanded: false,
    children: [
      { path: '/dashboard', icon: 'fas fa-chart-line', label: 'Dashboard' },
      { path: '/users', icon: 'fas fa-users', label: 'Người sử dụng' },
      { path: '/user-permissions', icon: 'fas fa-user-shield', label: 'Phân quyền người sử dụng' },
      { path: '/file-attachment', icon: 'fas fa-paperclip', label: 'Quản lý tệp đính kèm' }
    ]
  },
  {
    path: '/sales',
    icon: 'fas fa-shopping-cart',
    label: 'Bán hàng',
    isExpanded: false,
    children: [
      { path: '/sales-process', icon: 'fas fa-project-diagram', label: 'Quy trình' },
      { path: '/customer', icon: 'fas fa-address-book', label: 'Danh mục khách hàng' },
      { path: '/dynamic-page', icon: 'fas fa-file-alt', label: 'Form metadata' },
      { path: '/grid-master', icon: 'fas fa-table', label: 'Grid metadata' },
      { path: '/dynamic-data-grid', icon: 'fas fa-list', label: 'List metadata' }
    ]
  },
  {
    path: '/reports',
    icon: 'fas fa-chart-pie',
    label: 'Báo cáo',
    isExpanded: false,
    children: [
      { path: '/sales-report-summary', icon: 'fas fa-file-invoice-dollar', label: 'Tổng hợp bán hàng' },
      { path: '/sales-report-by-customer', icon: 'fas fa-user-tag', label: 'Theo khách hàng' },
      { path: '/sales-report-by-product', icon: 'fas fa-box', label: 'Theo sản phẩm' },
      { path: '/sales-report-by-date', icon: 'fas fa-calendar-alt', label: 'Theo ngày' }
    ]
  }
];



  toggleSubmenu(item: MenuItem) {
    if (!this.isCollapsed) {
      item.isExpanded = !item.isExpanded;
    }
  }

  onToggleCollapse() {
    this.toggleCollapse.emit();
  }
} 