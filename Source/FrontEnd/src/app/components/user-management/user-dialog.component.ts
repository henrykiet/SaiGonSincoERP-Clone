import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  ValidatorFn,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogModule,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { User, UserCreateDto, UserUpdateDto } from '../../models/user.model';

@Component({
  selector: 'app-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
  ],
  templateUrl: './user-dialog.component.html',
  styleUrls: ['./user-dialog.component.scss'],
})
export class UserDialogComponent implements OnInit {
  userForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<UserDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: { mode: 'create' | 'edit'; user?: User }
  ) {
    this.userForm = this.fb.group(
      {
        userName: ['', [Validators.required]],
        fullName: ['', [Validators.required]],
        email: ['', [Validators.required, Validators.email]],
        password: [
          '',
          this.data.mode === 'create'
            ? [Validators.required, Validators.minLength(6)]
            : [],
        ],
        confirmPassword: [
          '',
          this.data.mode === 'create' ? [Validators.required] : [],
        ],
        isLocked: [false],
      },
      {
        validators:
          this.data.mode === 'create'
            ? (this.passwordMatchValidator as ValidatorFn)
            : [],
      }
    );
  }

  passwordMatchValidator: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors | null => {
    const form = control as FormGroup;
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');

    if (
      password &&
      confirmPassword &&
      password.value !== confirmPassword.value
    ) {
      return { passwordMismatch: true };
    }
    return null;
  };

  ngOnInit() {
    if (this.data.mode === 'edit' && this.data.user) {
      this.userForm.patchValue({
        userName: this.data.user.userName,
        fullName: this.data.user.fullName,
        email: this.data.user.email,
        isLocked: this.data.user.isLocked,
      });
    }
  }

  onSubmit() {
    if (this.userForm.valid) {
      const formValue = this.userForm.value;
      if (this.data.mode === 'create') {
        this.dialogRef.close(formValue);
      } else {
        this.dialogRef.close(formValue);
      }
    }
  }

  onCancel() {
    this.dialogRef.close();
  }
}
