import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface NotificationMessage {
  message: string;
  type: 'success' | 'warning' | 'error' | 'info';
  duration?: number;
  id?: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<NotificationMessage[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  show(message: string, type: 'success' | 'warning' | 'error' | 'info' = 'info', duration: number = 5000) {
    const notification: NotificationMessage = {
      message,
      type,
      duration,
      id: this.generateId()
    };

    const currentNotifications = this.notificationsSubject.value;
    this.notificationsSubject.next([...currentNotifications, notification]);

    // Auto remove after duration
    if (duration > 0) {
      setTimeout(() => {
        this.remove(notification.id!);
      }, duration);
    }
  }

  success(message: string, duration: number = 5000) {
    this.show(message, 'success', duration);
  }

  warning(message: string, duration: number = 7000) {
    this.show(message, 'warning', duration);
  }

  error(message: string, duration: number = 8000) {
    this.show(message, 'error', duration);
  }

  info(message: string, duration: number = 5000) {
    this.show(message, 'info', duration);
  }

  remove(id: string) {
    const currentNotifications = this.notificationsSubject.value;
    this.notificationsSubject.next(
      currentNotifications.filter(notification => notification.id !== id)
    );
  }

  clear() {
    this.notificationsSubject.next([]);
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }
}
