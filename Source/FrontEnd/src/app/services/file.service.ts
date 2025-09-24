import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { map, Observable, throwError, firstValueFrom } from 'rxjs';
import {
  HttpClient,
  HttpErrorResponse,
  HttpResponse,
} from '@angular/common/http';
import * as XLSX from 'xlsx';
import { Material } from '../models/Material.model';

// Interface khớp với Backend response
export interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
  statusCode: number;
}

// Interface khớp với Backend model
export interface FileAttachment {
  controller: string;
  sysKey: string;
  fileName: string;
  fileContent: number[] | Uint8Array;
  contentType: string;
}

@Injectable({
  providedIn: 'root',
})
export class FileService {
  private apiUrl = `${environment.apiUrl}/api/AttachedFile`;

  constructor(private http: HttpClient) {}

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Đã xảy ra lỗi';
    if (error.error instanceof ErrorEvent) {
      errorMessage = error.error.message;
    } else {
      errorMessage = error.error?.Message || error.message;
    }
    return throwError(() => new Error(errorMessage));
  }

  private async fileToFileAttachment(
    file: File,
    controller: string,
    sysKey: string
  ): Promise<FileAttachment> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        try {
          const arrayBuffer = e.target.result;
          const fileAttachment: FileAttachment = {
            controller: controller,
            sysKey: sysKey,
            fileName: file.name,
            fileContent: new Uint8Array(arrayBuffer),
            contentType: file.type || 'application/octet-stream',
          };
          resolve(fileAttachment);
        } catch (err) {
          reject(err);
        }
      };
      reader.onerror = (err) => reject(err);
      reader.readAsArrayBuffer(file);
    });
  }

  sendTemplateRequest(formData: FormData): Observable<HttpResponse<Blob>> {
    return this.http.post(`${this.apiUrl}/import-file`, formData, {
      observe: 'response',
      responseType: 'blob',
    });
  }

  sendImportRequest(formData: FormData): Observable<HttpResponse<any>> {
    return this.http.post<HttpResponse<any>>(
      `${this.apiUrl}/import-file`,
      formData,
      {
        observe: 'response',
        responseType: 'json',
      }
    );
  }

  async convertXLSXToCSVFile(file: File): Promise<File> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = (e: any) => {
        try {
          const data = e.target.result;
          const workbook = XLSX.read(data, { type: 'array' });
          const sheetName = workbook.SheetNames[0];
          const worksheet = workbook.Sheets[sheetName];

          // Chuyển worksheet thành mảng JSON
          const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet, {
            defval: '',
          });

          // Lọc bỏ các dòng trắng hoàn toàn
          const filteredData = jsonData.filter((row) => {
            return Object.values(row).some(
              (cell) => String(cell).trim() !== ''
            );
          });

          // Chuyển lại sang CSV
          const csvData = XLSX.utils.sheet_to_csv(
            XLSX.utils.json_to_sheet(filteredData)
          );

          const blob = new Blob([csvData], { type: 'text/csv' });
          const csvFile = new File([blob], 'convert.csv', { type: 'text/csv' });
          resolve(csvFile);
        } catch (err) {
          reject(err);
        }
      };

      reader.onerror = (err) => reject(err);
      reader.readAsArrayBuffer(file);
    });
  }

  exportFile(formData: any): Observable<HttpResponse<Blob>> {
    return this.http.post(this.apiUrl + '/export', formData, {
      observe: 'response',
      responseType: 'blob',
    });
  }
  getAllMaterial(): Observable<Material[]> {
    return this.http.get<Material[]>(this.apiUrl + '/Materials');
  }
  uploadFile(
    file: File,
    controllerName: string = 'customer',
    sysKey: string = 'KH001'
  ): Observable<ServiceResponse<boolean>> {
    if (!file) {
      return throwError(() => new Error('File không được để trống'));
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('controllerName', controllerName);
    formData.append('sysKey', sysKey);
    console.log('Giá trị:', formData);
    return this.http
      .post<ServiceResponse<boolean>>(`${this.apiUrl}`, formData)
      .pipe(map((response) => response));
  }

  getFiles(
    controller: string,
    sysKey: string
  ): Observable<ServiceResponse<FileAttachment[]>> {
    if (!controller || !sysKey) {
      return throwError(
        () => new Error('Controller và sysKey không được để trống')
      );
    }

    return this.http
      .get<ServiceResponse<FileAttachment[]>>(
        `${this.apiUrl}/${controller}/${sysKey}`
      )
      .pipe(map((response) => response));
  }

  getFile(
    controller: string,
    sysKey: string,
    fileName: string
  ): Observable<ServiceResponse<FileAttachment>> {
    if (!controller || !sysKey || !fileName) {
      return throwError(
        () => new Error('Controller, sysKey và fileName không được để trống')
      );
    }

    return this.http
      .get<ServiceResponse<FileAttachment>>(
        `${this.apiUrl}/${controller}/${sysKey}/${fileName}`
      )
      .pipe(map((response) => response));
  }

  deleteFile(
    controller: string,
    sysKey: string,
    fileName: string
  ): Observable<ServiceResponse<boolean>> {
    if (!controller || !sysKey || !fileName) {
      return throwError(
        () => new Error('Controller, sysKey và fileName không được để trống')
      );
    }

    return this.http
      .delete<ServiceResponse<boolean>>(
        `${this.apiUrl}/${controller}/${sysKey}/${fileName}`
      )
      .pipe(map((response) => response));
  }
}
