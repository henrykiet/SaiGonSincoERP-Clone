import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileService } from '../services/file.service';

@Component({
  selector: 'app-file-attachment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-attachment.component.html',
})
export class FileAttachmentComponent {
  files: File[] = [];
  showExportOptions = false;
  tableName = 'TenBang';
  @Output() filesChange = new EventEmitter<File[]>();
  constructor(private fileService: FileService) {}
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      const selectedFiles = Array.from(input.files);
      this.files.push(...selectedFiles);
      this.filesChange.emit(this.files);
    }
  }

  removeFile(index: number) {
    this.files.splice(index, 1);
    this.filesChange.emit(this.files);
  }
  toggleExportOptions() {
    this.showExportOptions = !this.showExportOptions;
  }

  onExportClick() {
    this.showExportOptions = true;
  }
}
