import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { 
  UserGroup, 
  UserGroupDto, 
  UserGroupCreateDto, 
  UserGroupUpdateDto, 
  TreeViewPermissionDto 
} from '../models/user-group.model';
import { environment } from '../../environments/environment';

interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserGroupService {
  private apiUrl = `${environment.apiUrl}/api/UserGroup`;

  constructor(private http: HttpClient) { }

  getAllUserGroups(): Observable<UserGroupDto[]> {
    return this.http.get<ApiResponse<UserGroupDto[]>>(`${this.apiUrl}/getAllUserGroups`).pipe(
      map(response => response.data)
    );
  }

  getUserGroupById(id: number): Observable<UserGroupDto> {
    return this.http.get<ApiResponse<UserGroupDto>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data)
    );
  }

  createUserGroup(userGroup: UserGroupCreateDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/createUserGroup`, userGroup);
  }

  updateUserGroup(userGroup: UserGroupUpdateDto): Observable<ApiResponse<UserGroupDto>> {
    return this.http.put<ApiResponse<UserGroupDto>>(`${this.apiUrl}/updateUserGroup`, userGroup);
  }

  deleteUserGroup(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/deleteUserGroup/${id}`);
  }

  updateTreeViewPermissions(permissions: TreeViewPermissionDto): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/updateTreeViewPermissions`, permissions);
  }
}
