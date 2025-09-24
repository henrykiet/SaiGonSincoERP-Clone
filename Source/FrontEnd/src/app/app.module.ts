import { NgModule } from '@angular/core';
import { MatTreeModule } from '@angular/material/tree';
import { MatDialogModule } from '@angular/material/dialog';
import { PermissionDialogComponent } from './components/user-permission/permission-dialog/permission-dialog.component';
import { FileAttachmentComponent } from './components/file-attachment/file-attachment.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgxSpinnerModule } from "ngx-spinner";
@NgModule({
  declarations: [],
  imports: [
    PermissionDialogComponent,
    FileAttachmentComponent,
    MatTreeModule,
    MatDialogModule,
    PermissionDialogComponent,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatSnackBarModule,
    BrowserAnimationsModule,
    NgxSpinnerModule,
  ],
})
export class AppModule {}
