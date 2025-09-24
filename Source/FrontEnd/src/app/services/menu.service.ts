import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { MenuResponse } from '../models/menu.model';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private apiUrl = `${environment.apiUrl}/api/menu`;

  constructor(private http: HttpClient) {}

  getUserMenu(userId: number): Observable<MenuResponse> {
    return this.http.get<MenuResponse>(`${this.apiUrl}/user-menu/${userId}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  getAllMenu(): Observable<MenuResponse> {
    return this.http.get<MenuResponse>(`${this.apiUrl}/getAll-menu`)
      .pipe(
        catchError(this.handleError)
      );
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Đã xảy ra lỗi khi tải menu';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Lỗi: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Lỗi ${error.status}: ${error.message}`;
    }
    
    console.error('Menu Service Error:', error);
    return throwError(() => new Error(errorMessage));
  }
} 