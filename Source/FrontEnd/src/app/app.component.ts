import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, RouterModule } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { LayoutService } from './services/layout.service';
import { AuthService } from './services/auth.service';
import { Observable } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent, SidebarComponent],
  template: `
    <ng-container *ngIf="showLayout$ | async">
      <app-navbar (toggleSidebar)="toggleSidebar()"></app-navbar>
      <app-sidebar [isOpen]="isSidebarOpen"></app-sidebar>
    </ng-container>
    <main [class.sidebar-open]="isSidebarOpen">
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
    }

    main {
      margin-left: 0;
      padding: 1rem;
      transition: margin-left 0.3s ease;
      background-color: #f8f9fa;
      min-height: 100vh;
    }

    main.sidebar-open {
      margin-left: 280px;
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  isSidebarOpen = false;
  showLayout$: Observable<boolean>;
  private sessionCheckInterval: any;

  constructor(
    private layoutService: LayoutService, 
    private translate: TranslateService,
    private authService: AuthService
  ) {
    this.showLayout$ = this.layoutService.showLayout$;
    this.translate.setDefaultLang(localStorage.getItem('Language') ?? 'vi');
  }

  ngOnInit() {
    // Kiểm tra route hiện tại để quyết định hiển thị layout
    const currentPath = window.location.pathname;
    if (currentPath === '/login') {
      this.layoutService.hideLayout();
    } else {
      this.layoutService.showLayout();
    }

    // Thiết lập kiểm tra session định kỳ (mỗi 5 phút)
    this.startSessionCheck();
    
    // Thêm kiểm tra session khi window được focus lại
    this.setupWindowFocusCheck();
  }

  ngOnDestroy() {
    // Dọn dẹp interval khi component bị destroy
    if (this.sessionCheckInterval) {
      clearInterval(this.sessionCheckInterval);
    }
    
    // Dọn dẹp window focus listener
    window.removeEventListener('focus', this.checkSession.bind(this));
  }

  private startSessionCheck() {
    // Kiểm tra ngay lập tức
    this.checkSession();
    
    // Kiểm tra mỗi 5 phút (300000ms)
    this.sessionCheckInterval = setInterval(() => {
      this.checkSession();
    }, 5 * 60 * 1000);
  }

  private async checkSession() {
    // Chỉ kiểm tra khi user đã authenticated và không ở trang login
    const currentPath = window.location.pathname;
    if (currentPath === '/login') {
      return;
    }

    if (this.authService.isAuthenticated()) {
      try {
        const isValid = await this.authService.checkSessionValidity();
        if (!isValid) {
          console.warn('Session không hợp lệ, đã chuyển về trang login');
        }
      } catch (error) {
        console.error('Lỗi kiểm tra session:', error);
      }
    }
  }

  private setupWindowFocusCheck() {
    // Kiểm tra session khi user focus lại vào window
    // Điều này giúp phát hiện session hết hạn nhanh hơn khi user quay lại
    window.addEventListener('focus', () => {
      this.checkSession();
    });
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }
} 