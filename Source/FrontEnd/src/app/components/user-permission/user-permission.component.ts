import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user.model';
import { PermissionDialogComponent } from './permission-dialog/permission-dialog.component';

@Component({
  selector: 'app-user-permission',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './user-permission.component.html',
  styleUrls: ['./user-permission.component.scss']
})
export class UserPermissionComponent implements OnInit {
  users: User[] = [];
  displayedColumns: string[] = ['userId', 'userName', 'fullName', 'isLocked', 'createdAt', 'permissions'];
  searchText: string = '';
  isLoading: boolean = false;

  constructor(
    private userService: UserService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.isLoading = false;
      },
      error: (error) => {
        this.showMessage('Lỗi khi tải danh sách người dùng');
        this.isLoading = false;
      }
    });
  }

  searchUsers() {
    if (this.searchText.trim()) {
      this.isLoading = true;
      this.userService.searchUsers(this.searchText).subscribe({
        next: (users) => {
          this.users = users;
          this.isLoading = false;
        },
        error: (error) => {
          this.showMessage('Lỗi khi tìm kiếm người dùng');
          this.isLoading = false;
        }
      });
    } else {
      this.loadUsers();
    }
  }

  openPermissionDialog(user: User) {
    const dialogRef = this.dialog.open(PermissionDialogComponent, {
      data: { userId: user.userId },
      width: '800px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Refresh user list or show success message
        this.loadUsers();
      }
    });
  }

  private showMessage(message: string) {
    this.snackBar.open(message, 'Đóng', {
      duration: 3000
    });
  }
}