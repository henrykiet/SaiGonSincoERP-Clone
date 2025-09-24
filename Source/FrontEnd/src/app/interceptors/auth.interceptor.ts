import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);
  const token = localStorage.getItem('token');
  
  // Kiểm tra token hết hạn trước khi gửi request
  if (token && authService.isTokenExpired()) {
    console.warn('Token hết hạn, đăng xuất và chuyển về login');
    notificationService.warning('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
    authService.logout();
    
    // Sử dụng window.location để đảm bảo redirect hoạt động
    setTimeout(() => {
      window.location.href = '/login';
    }, 100);
    
    return throwError(() => new Error('Token expired'));
  }
  
  if (token) {
    const cloned = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    
    return next(cloned).pipe(
      catchError((error: HttpErrorResponse) => {
        // Xử lý lỗi 401 (Unauthorized) hoặc session không hợp lệ
        if (error.status === 401) {
          
          // Kiểm tra xem có phải do session không hợp lệ
          const errorMessage = error.error?.message || error.message || '';
          if (errorMessage.includes('Session không hợp lệ') || 
              errorMessage.includes('session') || 
              error.status === 401) {
            
            notificationService.error('Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.');
            authService.logout();
            
            // Sử dụng window.location để đảm bảo redirect
            setTimeout(() => {
              window.location.href = '/login';
            }, 500);
          }
        }
        
        // Xử lý lỗi 403 (Forbidden)
        if (error.status === 403) {
          notificationService.error('Bạn không có quyền truy cập tính năng này.');
        }
        
        return throwError(() => error);
      })
    );
  }
  
  return next(req);
}; 