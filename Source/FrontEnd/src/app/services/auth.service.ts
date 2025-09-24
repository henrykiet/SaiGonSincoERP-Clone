import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserLoginDto } from '../models/user.model';

interface LoginResponse {
  data: {
    token: string;
    unit: string;
    userId: number;
    userName: string;
    sessionId: string;
  };
  success: boolean;
  message: string;
  hasExistingSession?: boolean;
  role?: string;
}

export interface LoginDto {
  username: string;
  password: string;
}

export interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/api/auth`;
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check if token exists in localStorage on service initialization
    const token = localStorage.getItem('token');
    this.isAuthenticatedSubject.next(!!token);
  }

  login(credentials: UserLoginDto): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response: LoginResponse) => {
        if (response.success && response.data) {
          // Lưu token
          localStorage.setItem('token', response.data.token);
          
          // Lưu sessionId
          if (response.data.sessionId) {
            localStorage.setItem('sessionId', response.data.sessionId);
          }
          
          // Lưu userId và userName
          if (response.data.userId) {
            localStorage.setItem('userId', response.data.userId.toString());
          }
          if (response.data.userName) {
            localStorage.setItem('userName', response.data.userName);
          }
          
          // Lưu unit vào localStorage và environment
          if (response.data.unit) {
            localStorage.setItem('unit', response.data.unit);
            environment.unit = response.data.unit;
          }

          // Hiển thị cảnh báo nếu có session khác đang hoạt động
          if (response.hasExistingSession) {
            //console.warn('Cảnh báo: Tài khoản này đã được đăng nhập ở trình duyệt khác và đã được đăng xuất.');
          }

          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  logout() {
    // Gọi API logout nếu có token
    const token = this.getToken();
    if (token) {
      try {
        // Gọi API logout (không cần await vì có thể server đã down)
        this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
          next: (response) => {
            //console.log('Logout API thành công:', response);
          },
          error: (error) => {
            //console.warn('Logout API failed, nhưng vẫn logout local:', error);
          }
        });
      } catch (error) {
        //console.warn('Lỗi khi gọi logout API:', error);
      }
    }

    // Clear local storage
    localStorage.removeItem('token');
    localStorage.removeItem('sessionId');
    localStorage.removeItem('unit');
    localStorage.removeItem('userId');
    localStorage.removeItem('userName');
    environment.unit = '';
    this.isAuthenticatedSubject.next(false);
  }

  public isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUserId(): number | null {
    const userId = localStorage.getItem('userId');
    return userId ? parseInt(userId, 10) : null;
  }

  getUserName(): string | null {
    return localStorage.getItem('userName');
  }

  getUser(): any {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  getUnit(): string | null {
    return localStorage.getItem('unit');
  }

  getSessionId(): string | null {
    return localStorage.getItem('sessionId');
  }

  /**
   * Kiểm tra xem token có hết hạn không
   */
  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp < currentTime;
    } catch (error) {
      return true;
    }
  }

  /**
   * Tự động logout khi token hết hạn
   */
  handleTokenExpiration(): void {
    if (this.isTokenExpired()) {
      this.logout();
      // Import Router nếu chưa có
      if (typeof window !== 'undefined') {
        window.location.href = '/login';
      }
    }
  }

  /**
   * Kiểm tra session với server
   */
  validateSession(): Observable<ServiceResponse<boolean>> {
    return this.http.get<ServiceResponse<boolean>>(`${this.apiUrl}/validate-session`);
  }

  /**
   * Kiểm tra session và xử lý khi không hợp lệ
   */
  async checkSessionValidity(): Promise<boolean> {
    const token = this.getToken();
    const sessionId = this.getSessionId();
    
    if (!token || !sessionId) {
      this.logout();
      if (typeof window !== 'undefined') {
        window.location.href = '/login';
      }
      return false;
    }

    // Kiểm tra token expiration trước
    if (this.isTokenExpired()) {
      this.logout();
      if (typeof window !== 'undefined') {
        window.location.href = '/login';
      }
      return false;
    }

    try {
      const response = await firstValueFrom(this.validateSession());
      if (!response?.success) {

        this.logout();
        if (typeof window !== 'undefined') {
          window.location.href = '/login';
        }
        return false;
      }
      return true;
    } catch (error: any) {

      if (error.status === 401) {
        this.logout();
        if (typeof window !== 'undefined') {
          window.location.href = '/login';
        }
      }
      return false;
    }
  }
}
