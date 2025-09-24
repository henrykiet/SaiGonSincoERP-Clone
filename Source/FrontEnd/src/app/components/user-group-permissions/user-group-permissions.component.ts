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
import { UserGroupService } from '../../services/user-group.service';
import { UserGroupDto } from '../../models/user-group.model';
import { UserGroupPermissionDialogComponent } from './user-group-permission-dialog/user-group-permission-dialog.component';

@Component({
  selector: 'app-user-group-permissions',
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
  templateUrl: './user-group-permissions.component.html',
  styleUrls: ['./user-group-permissions.component.scss']
})
export class UserGroupPermissionsComponent implements OnInit {
  userGroups: UserGroupDto[] = [];
  displayedColumns: string[] = ['userGroupId', 'groupName', 'description', 'listUser', 'createdAt', 'permissions'];
  searchText: string = '';
  isLoading: boolean = false;

  constructor(
    private userGroupService: UserGroupService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadUserGroups();
  }

  loadUserGroups() {
    this.isLoading = true;
    this.userGroupService.getAllUserGroups().subscribe({
      next: (userGroups) => {
        this.userGroups = userGroups;
        this.isLoading = false;
      },
      error: (error) => {
        this.showMessage('Lỗi khi tải danh sách nhóm người dùng');
        this.isLoading = false;
      }
    });
  }

  searchUserGroups() {
    if (this.searchText.trim()) {
      this.isLoading = true;
      // Filter locally since there's no search API
      const searchTerm = this.searchText.toLowerCase();
      this.userGroups = this.userGroups.filter(group => 
        group.groupName.toLowerCase().includes(searchTerm) ||
        group.description?.toLowerCase().includes(searchTerm)
      );
      this.isLoading = false;
    } else {
      this.loadUserGroups();
    }
  }

  openPermissionDialog(userGroup: UserGroupDto) {
    const dialogRef = this.dialog.open(UserGroupPermissionDialogComponent, {
      data: { userGroupId: userGroup.userGroupId, groupName: userGroup.groupName },
      width: '800px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Refresh user group list or show success message
        this.showMessage('Cập nhật phân quyền thành công');
      }
    });
  }

  getUsersDisplay(listUser: string): string {
    if (!listUser) return 'Không có';
    
    try {
      const users = listUser.split(',');
      return users.length > 3 
        ? `${users.slice(0, 3).join(', ')}... (+${users.length - 3})`
        : users.join(', ');
    } catch {
      return listUser;
    }
  }

  private showMessage(message: string) {
    this.snackBar.open(message, 'Đóng', {
      duration: 3000
    });
  }
}
