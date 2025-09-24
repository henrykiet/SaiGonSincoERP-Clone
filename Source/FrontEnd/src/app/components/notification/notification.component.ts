import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NotificationService, NotificationMessage } from '../../services/notification.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notification-container">
      <div 
        *ngFor="let notification of notifications; trackBy: trackByFn" 
        class="notification"
        [ngClass]="'notification-' + notification.type"
        [@slideIn]
      >
        <div class="notification-content">
          <div class="notification-icon">
            <ng-container [ngSwitch]="notification.type">
              <span *ngSwitchCase="'success'">✓</span>
              <span *ngSwitchCase="'warning'">⚠</span>
              <span *ngSwitchCase="'error'">✗</span>
              <span *ngSwitchCase="'info'">ℹ</span>
            </ng-container>
          </div>
          <div class="notification-message">{{ notification.message }}</div>
          <button 
            class="notification-close" 
            (click)="close(notification.id!)"
            aria-label="Đóng thông báo"
          >
            ×
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .notification-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      max-width: 400px;
      pointer-events: none;
    }

    .notification {
      background: white;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      margin-bottom: 12px;
      pointer-events: auto;
      border-left: 4px solid;
      animation: slideIn 0.3s ease-out;
    }

    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    .notification-success {
      border-left-color: #10b981;
      background: #f0fdf4;
    }

    .notification-warning {
      border-left-color: #f59e0b;
      background: #fffbeb;
    }

    .notification-error {
      border-left-color: #ef4444;
      background: #fef2f2;
    }

    .notification-info {
      border-left-color: #3b82f6;
      background: #eff6ff;
    }

    .notification-content {
      display: flex;
      align-items: flex-start;
      padding: 16px;
      gap: 12px;
    }

    .notification-icon {
      flex-shrink: 0;
      width: 20px;
      height: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      font-weight: bold;
      font-size: 14px;
    }

    .notification-success .notification-icon {
      background: #10b981;
      color: white;
    }

    .notification-warning .notification-icon {
      background: #f59e0b;
      color: white;
    }

    .notification-error .notification-icon {
      background: #ef4444;
      color: white;
    }

    .notification-info .notification-icon {
      background: #3b82f6;
      color: white;
    }

    .notification-message {
      flex: 1;
      font-size: 14px;
      line-height: 1.5;
      color: #374151;
    }

    .notification-close {
      flex-shrink: 0;
      background: none;
      border: none;
      font-size: 20px;
      cursor: pointer;
      color: #6b7280;
      padding: 0;
      width: 24px;
      height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 4px;
      transition: background-color 0.2s;
    }

    .notification-close:hover {
      background: rgba(0, 0, 0, 0.1);
      color: #374151;
    }

    /* Responsive */
    @media (max-width: 640px) {
      .notification-container {
        left: 20px;
        right: 20px;
        max-width: none;
      }
      
      .notification {
        margin-bottom: 8px;
      }
      
      .notification-content {
        padding: 12px;
      }
    }
  `]
})
export class NotificationComponent implements OnInit, OnDestroy {
  notifications: NotificationMessage[] = [];
  private subscription?: Subscription;

  constructor(private notificationService: NotificationService) {}

  ngOnInit() {
    this.subscription = this.notificationService.notifications$.subscribe(
      notifications => {
        this.notifications = notifications;
      }
    );
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  close(id: string) {
    this.notificationService.remove(id);
  }

  trackByFn(index: number, item: NotificationMessage): string {
    return item.id || index.toString();
  }
}
