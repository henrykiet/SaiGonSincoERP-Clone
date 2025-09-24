import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UserGroupService } from '../../services/user-group.service';
import { UserGroupDto } from '../../models/user-group.model';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { UserGroupDialogComponent } from './user-group-dialog.component';

@Component({
  selector: 'app-user-group-management',
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
    MatSnackBarModule,
    UserGroupDialogComponent
  ],
  templateUrl: './user-group-management.component.html',
  styleUrls: ['./user-group-management.component.scss']
})
export class UserGroupManagementComponent implements OnInit {
  userGroups: UserGroupDto[] = [];
  displayedColumns: string[] = ['userGroupId', 'groupName', 'description', 'listUser', 'createdAt', 'actions'];
  searchText: string = '';

  constructor(
    protected userGroupService: UserGroupService,
    protected dialog: MatDialog,
    protected snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadUserGroups();
  }

  loadUserGroups() {
    this.userGroupService.getAllUserGroups().subscribe({
      next: (userGroups) => {
        this.userGroups = userGroups;
      },
      error: (error) => {
        this.showMessage('Lỗi khi tải danh sách nhóm người dùng');
      }
    });
  }

  searchUserGroups() {
    if (this.searchText.trim()) {
      // Filter locally since there's no search API
      const searchTerm = this.searchText.toLowerCase();
      this.userGroups = this.userGroups.filter(group => 
        group.groupName.toLowerCase().includes(searchTerm) ||
        group.description?.toLowerCase().includes(searchTerm)
      );
    } else {
      this.loadUserGroups();
    }
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(UserGroupDialogComponent, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.userGroupService.createUserGroup(result).subscribe({
          next: () => {
            this.loadUserGroups();
            this.showMessage('Tạo nhóm người dùng thành công');
          },
          error: (error) => {
            this.showMessage('Lỗi khi tạo nhóm người dùng');
          }
        });
      }
    });
  }

  openEditDialog(userGroup: UserGroupDto) {
    const dialogRef = this.dialog.open(UserGroupDialogComponent, {
      width: '600px',
      data: { mode: 'edit', userGroup }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const updateData = {
          ...result,
          userGroupId: userGroup.userGroupId
        };
        this.userGroupService.updateUserGroup(updateData).subscribe({
          next: () => {
            this.loadUserGroups();
            this.showMessage('Cập nhật nhóm người dùng thành công');
          },
          error: (error) => {
            this.showMessage('Lỗi khi cập nhật nhóm người dùng');
          }
        });
      }
    });
  }

  deleteUserGroup(userGroupId: number) {
    if (confirm('Bạn có chắc chắn muốn xóa nhóm người dùng này?')) {
      this.userGroupService.deleteUserGroup(userGroupId).subscribe({
        next: () => {
          this.loadUserGroups();
          this.showMessage('Xóa nhóm người dùng thành công');
        },
        error: (error) => {
          this.showMessage('Lỗi khi xóa nhóm người dùng');
        }
      });
    }
  }

  getUsersDisplay(listUser: string): string {
    if (!listUser) return '';
    
    try {
      const users = listUser.split(',');
      return users.length > 3 
        ? `${users.slice(0, 3).join(', ')}... (+${users.length - 3})`
        : users.join(', ');
    } catch {
      return listUser;
    }
  }

  protected showMessage(message: string) {
    this.snackBar.open(message, 'Đóng', {
      duration: 3000
    });
  }
}
