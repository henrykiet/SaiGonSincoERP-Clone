import { Component, OnInit, Input, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FileService, FileAttachment, ServiceResponse } from '../../services/file.service';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { firstValueFrom } from 'rxjs';

interface TempFile {
  file: File;
  fileName: string;
  contentType: string;
  size: number;
  isNew: boolean;
}

export interface FileAttachmentData {
  newFiles: TempFileForUpload[];
  filesToDelete: string[];
  existingFiles: FileAttachment[];
}

interface TempFileForUpload {
  fileName: string;
  contentType: string;
  fileContent: string; // base64 string
  size: number;
}

@Component({
  selector: 'app-file-attachment',
  standalone: true,
  imports: [
    CommonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule
  ],
  templateUrl: './file-attachment.component.html',
  styleUrls: ['./file-attachment.component.scss']
})
export class FileAttachmentComponent implements OnInit, OnChanges {
  @Input() controller: string = '';
  @Input() sysKey: string = '';
  @Input() isReadOnly: boolean = false; // Để disable upload khi đang trong chế độ chỉ đọc
  
  @Output() fileDataChange = new EventEmitter<FileAttachmentData>();
  
  files: FileAttachment[] = [];
  tempFiles: TempFile[] = [];
  filesToDelete: string[] = [];
  loading = false;
  hasChanges = false;
  
  // Remove hard-coded values
  // private readonly controller = 'customer';
  // private readonly sysKey = 'KH001';

  constructor(
    private fileService: FileService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    // Only load files if both controller and sysKey are provided
    if (this.controller && this.sysKey) {
      this.loadFiles();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Reload files when controller or sysKey changes
    if ((changes['controller'] || changes['sysKey']) && this.controller && this.sysKey) {
      this.loadFiles();
    }
  }

  loadFiles(): void {
    this.loading = true;
    
    this.fileService.getFiles(this.controller, this.sysKey).subscribe({
      next: (response: ServiceResponse<FileAttachment[]>) => {
        if (response.success) {
          this.files = response.data;
        } else {
          this.showError(response.message);
        }
      },
      error: (error) => this.showError(error.message),
      complete: () => this.loading = false
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.addTempFile(file);
      input.value = ''; // Reset input
    }
  }

  private addTempFile(file: File): void {
    const tempFile: TempFile = {
      file: file,
      fileName: file.name,
      contentType: file.type || 'application/octet-stream',
      size: file.size,
      isNew: true
    };
    
    this.tempFiles.push(tempFile);
    this.hasChanges = true;
    this.emitFileDataChange();
  }

  removeTempFile(index: number): void {
    this.tempFiles.splice(index, 1);
    this.hasChanges = this.tempFiles.length > 0 || this.filesToDelete.length > 0;
    this.emitFileDataChange();
  }

  /**
   * Emit file data changes to parent component
   */
  private async emitFileDataChange(): Promise<void> {
    // Convert tempFiles to base64
    const newFilesPromises = this.tempFiles.map(async (tempFile) => {
      const base64Content = await this.fileToBase64(tempFile.file);
      return {
        fileName: tempFile.fileName,
        contentType: tempFile.contentType,
        fileContent: base64Content,
        size: tempFile.size
      } as TempFileForUpload;
    });

    const newFiles = await Promise.all(newFilesPromises);

    const fileData: FileAttachmentData = {
      newFiles: newFiles,
      filesToDelete: [...this.filesToDelete],
      existingFiles: [...this.files]
    };
    this.fileDataChange.emit(fileData);
  }

  /**
   * Convert File to base64 string
   */
  private fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        // Remove data:...base64, prefix
        const base64String = (reader.result as string).split(',')[1];
        resolve(base64String);
      };
      reader.onerror = error => reject(error);
    });
  }

  deleteFile(file: FileAttachment): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Xác nhận xóa',
        message: `Bạn có chắc chắn muốn xóa file "${file.fileName}"?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Thêm vào danh sách files để xóa
        this.filesToDelete.push(file.fileName);
        // Xóa khỏi danh sách hiển thị
        this.files = this.files.filter(f => f.fileName !== file.fileName);
        this.hasChanges = true;
        this.emitFileDataChange();
      }
    });
  }

  /**
   * Reset temporary data sau khi save thành công
   */
  resetTempData(): void {
    this.tempFiles = [];
    this.filesToDelete = [];
    this.hasChanges = false;
    this.loadFiles(); // Reload danh sách files từ server
  }

  /**
   * Get current file attachment data để gửi lên parent
   */
  async getCurrentFileData(): Promise<FileAttachmentData> {
    // Convert tempFiles to base64
    const newFilesPromises = this.tempFiles.map(async (tempFile) => {
      const base64Content = await this.fileToBase64(tempFile.file);
      return {
        fileName: tempFile.fileName,
        contentType: tempFile.contentType,
        fileContent: base64Content,
        size: tempFile.size
      } as TempFileForUpload;
    });

    const newFiles = await Promise.all(newFilesPromises);

    return {
      newFiles: newFiles,
      filesToDelete: [...this.filesToDelete],
      existingFiles: [...this.files]
    };
  }

  downloadFile(file: FileAttachment): void {
    this.loading = true;
    this.fileService.getFile(this.controller, this.sysKey, file.fileName).subscribe({
      next: (response: ServiceResponse<FileAttachment>) => {
        if (response.success) {
          const fileData = response.data;
          const fileContent = Array.isArray(fileData.fileContent) 
            ? new Uint8Array(fileData.fileContent)
            : fileData.fileContent;
          const blob = new Blob([fileContent], { type: fileData.contentType });
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = fileData.fileName;
          link.click();
          window.URL.revokeObjectURL(url);
        } else {
          this.showError(response.message);
        }
      },
      error: (error) => this.showError(error.message),
      complete: () => this.loading = false
    });
  }

  canPreviewFile(file: FileAttachment): boolean {
    const previewableTypes = [
      'application/pdf',
      'image/jpeg',
      'image/jpg',
      'image/png',
      'image/gif',
      'image/bmp',
      'image/webp',
      'image/svg+xml'
    ];
    
    // Kiểm tra theo contentType
    if (file.contentType && previewableTypes.includes(file.contentType.toLowerCase())) {
      return true;
    }
    
    // Kiểm tra theo extension của file nếu contentType không có hoặc không khớp
    const fileName = file.fileName?.toLowerCase() || '';
    const previewableExtensions = ['.pdf', '.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg'];
    
    return previewableExtensions.some(ext => fileName.endsWith(ext));
  }

  canPreviewTempFile(tempFile: TempFile): boolean {
    const previewableTypes = [
      'application/pdf',
      'image/jpeg',
      'image/jpg',
      'image/png',
      'image/gif',
      'image/bmp',
      'image/webp',
      'image/svg+xml'
    ];
    return previewableTypes.includes(tempFile.contentType?.toLowerCase());
  }

  previewTempFile(tempFile: TempFile): void {
    const url = window.URL.createObjectURL(tempFile.file);
    window.open(url, '_blank');
    
    // Cleanup URL sau một thời gian
    setTimeout(() => {
      window.URL.revokeObjectURL(url);
    }, 1000);
  }

  previewFile(file: FileAttachment): void {
    this.loading = true;
    this.fileService.getFile(this.controller, this.sysKey, file.fileName).subscribe({
      next: (response: ServiceResponse<FileAttachment>) => {
        if (response.success) {
          const fileData = response.data;
          const fileContent = Array.isArray(fileData.fileContent) 
            ? new Uint8Array(fileData.fileContent)
            : fileData.fileContent;
          const blob = new Blob([fileContent], { type: fileData.contentType });
          const url = window.URL.createObjectURL(blob);
          
          // Mở file trong tab mới hoặc window mới
          window.open(url, '_blank');
          
          // Cleanup URL sau một thời gian
          setTimeout(() => {
            window.URL.revokeObjectURL(url);
          }, 1000);
        } else {
          this.showError(response.message);
        }
      },
      error: (error) => {
        console.error('Error previewing file:', error);
        this.showError(error.message);
      },
      complete: () => this.loading = false
    });
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Đóng', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['success-snackbar']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Đóng', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['error-snackbar']
    });
  }
} 