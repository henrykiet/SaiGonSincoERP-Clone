import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { User, UserCreateDto, UserUpdateDto } from '../models/user.model';
import { environment } from '../../environments/environment';

interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/api/Auth`;

  constructor(private http: HttpClient) { }

  getAllUsers(): Observable<User[]> {
    return this.http.get<ApiResponse<User[]>>(`${this.apiUrl}`).pipe(
      map(response => response.data)
    );
  }

  getUserById(id: number): Observable<User> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data)
    );
  }

  createUser(user: UserCreateDto): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/register`, user);
  }

  updateUser(id: number, user: UserUpdateDto): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/users/${id}`, user);
  }

  deleteUser(id: number): Observable<any> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }

  searchUsers(searchText: string): Observable<User[]> {
    return this.http.get<ApiResponse<User[]>>(`${this.apiUrl}/search/${searchText}`).pipe(
      map(response => response.data)
    );
  }
} 