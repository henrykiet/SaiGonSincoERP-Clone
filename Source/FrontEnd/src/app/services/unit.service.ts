import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UnitResponse } from '../models/unit.model';

@Injectable({
  providedIn: 'root'
})
export class UnitService {
  private apiUrl = `${environment.apiUrl}/api/unit`;

  constructor(private http: HttpClient) { }

  getUnits(): Observable<UnitResponse> {
    return this.http.get<UnitResponse>(`${this.apiUrl}/list`);
  }
} 