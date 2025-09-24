import { Router } from '@angular/router';
import {
  Component,
  HostListener,
  Input,
  OnInit,
  ViewChild,
  ElementRef,
  Output,
  EventEmitter,
} from '@angular/core';
import { FileService } from '../services/file.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule } from '@ngx-translate/core';
import { NgxSpinnerModule, NgxSpinnerService } from 'ngx-spinner';
import { delayWhen, finalize, timer } from 'rxjs';
import { ConfirmDialogComponent } from '../components/confirm-dialog/confirm-dialog.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-file-handle',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    TranslateModule,
    NgxSpinnerModule,
  ],
  templateUrl: './file-handle.component.html',
  styleUrl: './file-handle.component.scss',
})
export class FileHandleComponent implements OnInit {
  @ViewChild('exportMenu') exportMenuRef!: ElementRef;
  @ViewChild('importMenu') importMenuRef!: ElementRef;
  @ViewChild('importInput') importInputRef!: ElementRef<HTMLInputElement>;
  @Output() importCompleted = new EventEmitter<void>();
  @Input() minimalMode = false;
  @Input() controll: string = '';
  @Input() exportData: { [key: string]: any[] } = {};
  @Input() user: { [key: string]: string } = {};
  showImportOptions = false;
  pendingImportType: 'template' | 'import' | null = null;
  showExportOptions = false;
  pendingExportType: 'pdf' | 'excel' | null = null;
  constructor(
    private fileService: FileService,
    private snackBar: MatSnackBar,
    private spinner: NgxSpinnerService,
    private dialog: MatDialog,
    private Routes: Router,
    private samitizer: DomSanitizer
  ) {}

  ngOnInit(): void {}

  importCSV(
    event: any | null,
    user: any | null,
    controll: string,
    type: 'template' | 'import'
  ) {
    if (!controll || !controll.trim()) {
      this.showError('Controller is required.');
      return;
    }
    this.spinner.show();
    //Trường hợp tải file mẫu → không có file, chỉ gửi controll + type
    if (type === 'template' && event === null) {
      const formData = new FormData();
      formData.append('controll', controll);
      formData.append('type', type);
      //show popup loading
      this.fileService
        .sendTemplateRequest(formData)
        .pipe(
          delayWhen(() => timer(500)), // Luôn delay 0.5 giây trước khi next/error
          finalize(() => this.spinner.hide())
        )
        .subscribe({
          next: (res: HttpResponse<Blob>) => {
            const contentDisposition = res.headers.get('content-disposition');
            let fileName = 'template.xlsx';
            if (contentDisposition) {
              const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(
                contentDisposition
              );
              if (matches != null && matches[1]) {
                fileName = matches[1].replace(/['"]/g, '');
              }
            }

            const blob = new Blob([res.body!], {
              type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            a.click();
            window.URL.revokeObjectURL(url);
            //load lại data
            this.importCompleted.emit();
          },
          error: (err) => {
            // Nếu error.error là Blob → giải mã ra JSON
            if (
              err?.error instanceof Blob &&
              err.error.type === 'application/json'
            ) {
              const reader = new FileReader();
              reader.onload = () => {
                try {
                  const errorJson = JSON.parse(reader.result as string);
                  this.showError(
                    errorJson.message || 'Không thể tải file mẫu.'
                  );
                } catch (e) {
                  this.showError('Lỗi khi đọc phản hồi lỗi từ server.');
                }
              };
              reader.onerror = () => {
                this.showError('Lỗi khi đọc dữ liệu lỗi.');
              };
              reader.readAsText(err.error); // GIẢI MÃ BLOB
            } else {
              this.showError(err?.error?.message || 'Không thể tải file mẫu.');
            }
          },
        });
      return;
    }

    //Trường hợp import dữ liệu cần có file
    const input = event?.target as HTMLInputElement;
    const file = input?.files?.[0];

    if (!file) {
      this.showError('Vui lòng chọn tệp hợp lệ.');
      return;
    }

    this.fileService.convertXLSXToCSVFile(file).then((csvFile) => {
      const formData = new FormData();
      formData.append('file', csvFile);
      formData.append('controll', controll);
      formData.append('type', type);
      formData.append('user', JSON.stringify(user)); //parse về dạng json
      console.time('ImportCSV Execution Time');
      this.fileService
        .sendImportRequest(formData)
        .pipe(
          delayWhen(() => timer(500)), // Luôn delay 0.5 giây trước khi next/error
          finalize(() => this.spinner.hide())
        )
        .subscribe({
          next: () => {
            this.showSuccess('Import thành công!');
            input.value = '';
            console.timeEnd('ImportCSV Execution Time');
            this.importCompleted.emit();
          },
          error: (res) => {
            input.value = '';
            console.timeEnd('ImportCSV Execution Time');
            console.log('Import error:', res?.status);
            // Lấy message và errors từ response trả về
            const statusCode = res?.status;
            const errorMessage = res?.error?.message;
            const errorDetails = res?.error?.data;
            //bắt dữ liệu trùng
            if (statusCode === 409) {
              const dialogRef = this.dialog.open(ConfirmDialogComponent, {
                width: '600px',
                data: {
                  title: 'Xác nhận ghi đè',
                  message: `${errorMessage}. Bạn có muốn ghi đè không?`,
                  details: errorDetails ?? errorMessage,
                },
              });
              dialogRef.afterClosed().subscribe((result) => {
                if (result == true) {
                  //gọi lại api ghi đè
                  formData.append('overwrite', 'true'); // Thêm cờ ghi đè
                  this.fileService.sendImportRequest(formData).subscribe({
                    next: () => {
                      this.showSuccess('Import thành công (ghi đè)!');
                      this.importCompleted.emit();
                    },
                    error: (err2) => {
                      this.showError(
                        `Import thất bại khi ghi đè: ${err2?.error?.message}`,
                        err2?.status
                      );
                    },
                  });
                }
              });
            } else {
              // Show lỗi gồm message + chi tiết
              this.showError(errorMessage, errorDetails);
            }
          },
        });
    });
  }

  exportToReport(controll: string, type: 'pdf' | 'excel') {
    // Gọi API export hoặc xử lý export
    if (!this.controll || Object.keys(this.controll).length === 0) {
      this.showError('Không có controll để export.');
      return;
    }
    if (!this.exportData || Object.keys(this.exportData).length === 0) {
      this.showError('Không có dữ liệu để export.');
      return;
    }
    const payload = {
      controll: controll,
      tables: this.exportData,
      isPdfOrExcel: type,
    };
    this.spinner.show();
    this.fileService
      .exportFile(payload)
      .pipe(
        delayWhen(() => timer(500)), // Luôn delay 0.5 giây trước khi next/error
        finalize(() => this.spinner.hide())
      )
      .subscribe({
        next: (res: HttpResponse<Blob>) => {
          const contentDisposition = res.headers.get('content-disposition');
          let fileName = 'download';
          if (contentDisposition) {
            const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(
              contentDisposition
            );
            if (matches != null && matches[1]) {
              fileName = matches[1].replace(/['"]/g, '');
            }
          }

          const blob = new Blob([res.body!], {
            type:
              type === 'pdf'
                ? 'application/pdf'
                : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          });

          const url = window.URL.createObjectURL(blob);
          console.log(url);
          const newTab = window.open(url, '_blank');

          if (newTab) {
            newTab.document.write(`
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="UTF-8">
                  <title>Export Preview</title>
                  <style>
                    html, body { margin:0; padding:0; width:100%; height:100%; }
                    iframe { width:100%; height:100%; border:none; }
                    #downloadBtn {
                      position: absolute;
                      top: 12px;
                      right: 80px;
                      z-index: 1000;
                      padding: 8px 12px;
                      background-color: #007bff;
                      color: white;
                      border: none;
                      border-radius: 4px;
                      cursor: pointer;
                    }
                  </style>
                </head>
                <body>
                  <button id="downloadBtn">Download</button>
                  <iframe src="${url}" frameborder="0"></iframe>
                  <script>
                    const btn = document.getElementById('downloadBtn');
                    btn.addEventListener('click', () => {
                      const a = document.createElement('a');
                      a.href = '${url}';
                      a.download = '${fileName}';
                      a.click();
                    });
                  </script>
                </body>
                </html>
              `);
            newTab.document.close();
          }
          // const a = document.createElement('a');
          // a.href = url;
          // a.download = fileName;
          // a.click();
          // window.URL.revokeObjectURL(url);
        },
        error: (err: HttpErrorResponse) => {
          this.showError(err.error, err.status, err.statusText);
        },
      });
    this.showExportOptions = false;
  }

  triggerImport(type: 'template' | 'import'): void {
    this.pendingImportType = type;
    if (this.importInputRef) {
      this.importInputRef.nativeElement.click();
    }
  }

  selectImportType(type: 'template' | 'import') {
    this.pendingImportType = type;
    this.showImportOptions = false;

    if (type === 'template') {
      // Không cần chọn file → truyền null vào `event`
      this.importCSV(null, null, this.controll, 'template');
    } else if (type === 'import') {
      // Mở input ẩn để chọn file
      //alert(this.importInputRef);
      this.importInputRef?.nativeElement.click();
    }
  }
  selectExportType(type: 'pdf' | 'excel') {
    this.pendingExportType = type;
    this.showExportOptions = false;

    this.exportToReport(this.controll, this.pendingExportType);
    this.pendingExportType = null;
  }

  toggleExportOptions(): void {
    this.showExportOptions = !this.showExportOptions;
    if (this.showExportOptions) {
      this.showImportOptions = false;
    }
  }

  toggleImportOptions(): void {
    this.showImportOptions = !this.showImportOptions;
    if (this.showImportOptions) {
      this.showExportOptions = false;
    }
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Đóng', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['success-snackbar'],
    });
  }

  private showError(
    error: any,
    statusCode?: number,
    statusText?: string
  ): void {
    let displayMessage = 'Lỗi không xác định.';
    const lineBreak = String.fromCharCode(10);

    if (typeof error === 'string') {
      // Lỗi nghiệp vụ tự truyền vào, ví dụ "Không có dữ liệu để export."
      displayMessage = error;

      this.snackBar.open(displayMessage, 'Đóng', {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        panelClass: ['error-snackbar'],
      });
      return;
    }

    if (error instanceof Blob) {
      const reader = new FileReader();
      reader.onload = () => {
        try {
          const errorJson = JSON.parse(reader.result as string);
          if (errorJson && errorJson.message) {
            displayMessage = errorJson.message;
          } else {
            displayMessage =
              'Lỗi server: Phản hồi không phải JSON hợp lệ hoặc thiếu thông báo.';
            console.error('Error JSON from Blob:', errorJson);
          }
        } catch (e) {
          displayMessage = 'Lỗi server: Phản hồi không phải JSON hợp lệ.';
          console.error(
            'Error parsing server response from Blob:',
            reader.result,
            e
          );
        } finally {
          this.snackBar.open(displayMessage, 'Đóng', {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            panelClass: ['error-snackbar'],
          });
        }
      };
      reader.readAsText(error);
      return;
    }

    if (typeof error === 'object' && error !== null && error.message) {
      displayMessage = error.message;

      if (Array.isArray(error.errorDetails) && error.errorDetails.length > 0) {
        displayMessage += lineBreak;
        displayMessage += error.errorDetails
          .map((errDetail: string, idx: number) => `${idx + 1}. ${errDetail}`)
          .join('\n');
      }

      this.snackBar.open(displayMessage, 'Đóng', {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        panelClass: ['error-snackbar'],
      });
      return;
    }

    // Lỗi HTTP hoặc các lỗi không xác định khác
    displayMessage = `Lỗi HTTP: ${statusCode || 'Không xác định'} - ${
      statusText || 'Unknown error'
    }`;
    console.error('Server error (non-JSON or missing message):', error);

    this.snackBar.open(displayMessage, 'Đóng', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['error-snackbar'],
    });
  }

  // Lắng nghe click ngoài
  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    if (
      this.exportMenuRef &&
      !this.exportMenuRef.nativeElement.contains(event.target) &&
      this.importMenuRef &&
      !this.importMenuRef.nativeElement.contains(event.target)
    ) {
      this.showImportOptions = false;
      this.showExportOptions = false;
    }
  }
}
