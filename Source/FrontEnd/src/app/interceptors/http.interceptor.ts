import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, tap } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const httpInterceptor: HttpInterceptorFn = (
  request: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  // Log request
  console.log('Request:', {
    url: request.url,
    method: request.method,
    headers: request.headers,
    body: request.body
  });

  // Add auth header if needed
  const token = localStorage.getItem('token');
  if (token) {
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(request).pipe(
    tap(response => {
      // Log response
      //console.log('Response:', response);
    }),
    catchError((error: HttpErrorResponse) => {
      // Log error
      console.error('API Error:', {
        url: request.url,
        status: error.status,
        message: error.message,
        error: error.error
      });
      return throwError(() => error);
    })
  );
}; 