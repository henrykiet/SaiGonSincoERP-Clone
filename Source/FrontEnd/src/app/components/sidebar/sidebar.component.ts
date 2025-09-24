import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MenuService } from '../../services/menu.service';
import { AuthService } from '../../services/auth.service';
import { MenuDto } from '../../models/menu.model';

// Giữ interface cũ để backward compatibility
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
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() isOpen = false;
  @Input() isCollapsed = false;
  @Output() toggleCollapse = new EventEmitter<void>();
  
  menuItems: MenuDto[] = [];
  isLoading = false;
  error: string = '';
  private destroy$ = new Subject<void>();

  constructor(
    private menuService: MenuService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUserMenu();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadUserMenu() {
    const userId = this.authService.getUserId();
    
    if (!userId) {
      this.error = 'Không tìm thấy thông tin người dùng';
      console.error('UserId not found in localStorage');
      return;
    }

    this.isLoading = true;
    this.error = '';

    this.menuService.getUserMenu(userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.menuItems = response.data;
            // Khởi tạo trạng thái isExpanded cho các menu có children
            this.initializeMenuExpansion(this.menuItems);
          } else {
            this.error = response.message || 'Lỗi khi tải menu';
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.error = error.message || 'Lỗi khi tải menu';
          this.isLoading = false;
          console.error('Error loading menu:', error);
        }
      });
  }

  private initializeMenuExpansion(items: MenuDto[]) {
    items.forEach(item => {
      if (this.hasChildren(item)) {
        item.isExpanded = false;
        this.initializeMenuExpansion(item.children!);
      }
    });
  }

  toggleSubmenu(item: MenuDto) {
    if (!this.isCollapsed && this.hasChildren(item)) {
      item.isExpanded = !item.isExpanded;
    }
  }

  onToggleCollapse() {
    this.toggleCollapse.emit();
  }

  // Method để refresh menu khi cần
  refreshMenu() {
    this.loadUserMenu();
  }

  // Helper method để kiểm tra children an toàn
  hasChildren(item: MenuDto): boolean {
    return !!(item.children && item.children.length > 0);
  }

  // Method để kiểm tra quyền truy cập
  hasAccess(item: MenuDto): boolean {
    return item.hasAccess;
  }

  // Method để kiểm tra quyền insert
  hasInsert(item: MenuDto): boolean {
    return item.hasInsert;
  }

  // Method để kiểm tra quyền update
  hasUpdate(item: MenuDto): boolean {
    return item.hasUpdate;
  }

  // Method để kiểm tra quyền delete
  hasDelete(item: MenuDto): boolean {
    return item.hasDel;
  }
}
