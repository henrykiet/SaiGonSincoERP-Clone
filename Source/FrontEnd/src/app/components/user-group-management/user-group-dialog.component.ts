import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogModule,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { UserGroupDto } from '../../models/user-group.model';
import { LookupApiQuery } from '../../models/lookup.model';
import { DynamicLookupComponent } from '../../dynamic-lookup/dynamic-lookup.component';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-user-group-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    DynamicLookupComponent,
  ],
  templateUrl: './user-group-dialog.component.html',
  styleUrls: ['./user-group-dialog.component.scss'],
})
export class UserGroupDialogComponent implements OnInit {
  userGroupForm: FormGroup;
  selectedUsers: string[] = [];

  // Lookup query for users
  userLookupQuery: LookupApiQuery = {
    controller: 'user',
    language: 'vi',
    unit: localStorage.getItem('unit') || '',
    userId: localStorage.getItem('userId') || '',
    filter: []  
  };

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    public dialogRef: MatDialogRef<UserGroupDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: { mode: 'create' | 'edit'; userGroup?: UserGroupDto }
  ) {
    // Debug localStorage values
    //console.log('🏪 [UserGroupDialog] LocalStorage unit:', localStorage.getItem('unit'));
    //console.log('👤 [UserGroupDialog] LocalStorage userId:', localStorage.getItem('userId'));
    
    this.userGroupForm = this.fb.group({
      groupName: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      listUser: ['', [Validators.maxLength(1024)]],
      treeViewPermissions: ['']
    });
  }

  ngOnInit() {
  
    if (this.data.mode === 'edit' && this.data.userGroup) {
      this.userGroupForm.patchValue({
        groupName: this.data.userGroup.groupName,
        description: this.data.userGroup.description,
        listUser: this.data.userGroup.listUser,
        treeViewPermissions: this.data.userGroup.treeViewPermissions
      });

      // Initialize selected users from listUser string
      if (this.data.userGroup.listUser) {
        this.selectedUsers = this.data.userGroup.listUser.split(',').filter(u => u.trim());
        //console.log('👥 [UserGroupDialog] Initialized selected users:', this.selectedUsers);
      }
    }
  }


  onUsersSelectionChange(selectedUsers: string[]) {
    this.selectedUsers = selectedUsers;
    const listUserString = selectedUsers.join(',');
    this.userGroupForm.patchValue({
      listUser: listUserString
    });
  }

  onSubmit() {
    if (this.userGroupForm.valid) {
      const formValue = this.userGroupForm.value;
      this.dialogRef.close(formValue);
    }
  }

  onCancel() {
    this.dialogRef.close();
  }

  // Get form control error messages
  getErrorMessage(fieldName: string): string {
    const control = this.userGroupForm.get(fieldName);
    if (control?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)} là bắt buộc`;
    }
    if (control?.hasError('maxlength')) {
      const maxLength = control.getError('maxlength')?.requiredLength;
      return `${this.getFieldDisplayName(fieldName)} không được vượt quá ${maxLength} ký tự`;
    }
    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      'groupName': 'Mã nhóm',
      'description': 'Tên nhóm',
      'listUser': 'Danh sách người dùng'
    };
    return displayNames[fieldName] || fieldName;
  }
}
