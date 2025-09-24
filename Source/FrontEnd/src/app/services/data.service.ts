import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
interface ApiResponse<T> {
  data?: T;
  success: boolean;
  message: string;
  statusCode :number;
}
@Injectable({
  providedIn: 'root',
})
export class DataService {
  private apiUrl = `${environment.apiUrl}/api/data`;
  constructor(private http: HttpClient) {}
  getMetadata(formId: string): Observable<any> {
    return this.http
      .get<ApiResponse<any>>(`${this.apiUrl}/metadata/${formId}`)
      .pipe(map((response) => response.data));
  }
  getDataList(data: any): Observable<any> {
    return this.http
      .get<ApiResponse<any>>(`${this.apiUrl}/list`, { params: data })
      .pipe(map((response) => response.data));
  }

  postDataSave(data: any): Observable<any> {
    return this.http
      .post<ApiResponse<any>>(`${this.apiUrl}/save/`, data)
      .pipe(map((response) => response.data));
  }

  deleteData(data: any): Observable<any> {
    return this.http
      .request<ApiResponse<any>>('delete', `${this.apiUrl}/delete/`, {
        body: data,
        headers: { 'Content-Type': 'application/json' },
      })
      .pipe(map((response) => response.data));
  }
  deletedMultiData(data: any): Observable<any> {
    return this.http
      .request<ApiResponse<any>>('delete', `${this.apiUrl}/deletedMulti/`, {
        body: data,
        headers: { 'Content-Type': 'application/json' },
      })
      // .pipe(map((response) => response.data));
  }
  getDataOptions(optionId: string, data: any): Observable<any> {
    return this.http
      .get<ApiResponse<any>>(`${this.apiUrl}/options/${optionId}`, {
        params: data,
      })
      .pipe(map((response) => response.data));
  }

  getDataLookup(optionId: string, invoice: string, data: any): Observable<any> {
    return this.http
      .get<ApiResponse<any>>(`${this.apiUrl}/lookup/${optionId}/${invoice}`, {
        params: data,
      })
      .pipe(map((response) => response.data));
  }

  getDataReport(formId: string, data: any): Observable<any> {
    return this.http
      .get<ApiResponse<any>>(`${this.apiUrl}/report/${formId}`, {
        params: data,
      })
      .pipe(map((response) => response.data));
  }

  getDataReportPivot(formId: string, data: any): Observable<any> {
    return this.http
      .get<ApiResponse<any>>(`${this.apiUrl}/report/pivot/${formId}`, {
        params: data,
      })
      .pipe(map((response) => response.data));
  }

  postDataCustom(data: any): Observable<any> {
    return this.http
      .post<ApiResponse<any>>(`${this.apiUrl}/custom`, data)
      .pipe(map((response) => response.data));
  }
}
