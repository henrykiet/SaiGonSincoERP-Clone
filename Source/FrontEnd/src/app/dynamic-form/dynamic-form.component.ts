import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { FileAttachmentComponent } from '../file-attachment/file-attachment.component';
import { minDate, maxDate, minSelected, maxSelected } from './validators';
@Component({
  selector: 'app-dynamic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FileAttachmentComponent],
  templateUrl: './dynamic-form.component.html'
})
export class DynamicFormComponent implements OnInit {
  form!: FormGroup;
  metadata: any
  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    this.form = this.fb.group({});
    this.metadata = {
      title: "Thông tin người dùng",
      fields: [
        {
          key: 'fullName',
          label: 'Họ và tên',
          type: 'text',
          default: '',
          required: false,
          disabled: false,
          placeholder: 'Nhập họ và tên',
          validators: {
            minlength: 3,
            maxlength: 50,
            pattern: '^[A-Za-zÀ-ỹ\\s]+$'
          }
        },
        {
          key: "age",
          label: "Tuổi",
          type: "number",
          default: 1,
          required: false,
          disabled: true,
          validators: {
            min: 18,
            max: 99
          }
        },
        {
          key: "email",
          label: "Email",
          type: "email",
          default: '',
          placeholder: "example@gmail.com",
          required: false,
          validators: {
            pattern: "^\\S+@\\S+\\.\\S+$"
          }
        },
        {
          key: 'password',
          label: 'Mật khẩu',
          type: 'password',
          default: '',
          placeholder: 'Nhập mật khẩu',
          required: false,
          disabled: false,
          validators: {
            minlength: 8,
            maxlength: 32,
            pattern: '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$'
          }
        },
        {
          key: 'description',
          label: 'Mô tả chi tiết',
          type: 'textarea',
          default: '',
          required: false,
          disabled: false,
          placeholder: 'Nhập mô tả...',
          rows: 4,
          cols: 50,
          validators: {
            minlength: 10,
            maxlength: 500
          }
        },
        {
          key: "gender",
          label: "Giới tính",
          type: "select",
          required: false,
          default: [],
          options: [
            { label: "Nam", value: "male" },
            { label: "Nữ", value: "female" },
            { label: "Khác", value: "other" }
          ]
        },
        {
          key: 'skills',
          label: 'Kỹ năng',
          type: 'multi-select',
          default: [],
          required: false,
          disabled: false,
          placeholder: 'Chọn kỹ năng',
          options: [
            { label: 'JavaScript', value: 'js' },
            { label: 'Python', value: 'python' },
            { label: 'Go', value: 'go' },
            { label: 'Rust', value: 'rust' }
          ],
          validators: {
            minSelected: 1,
            maxSelected: 3
          }
        },
        {
          key: 'paymentMethod',
          label: 'Phương thức thanh toán',
          type: 'radio',
          default: 'credit_card',
          required: true,
          disabled: false,
          options: [
            { label: 'Thẻ tín dụng', value: 'credit_card' },
            { label: 'Paypal', value: 'paypal' },
            { label: 'Chuyển khoản', value: 'bank_transfer' }
          ]
        },
        {
          key: 'hobbies',
          label: 'Sở thích',
          type: 'checkboxes',
          required: true,
          disabled: false,
          options: [
            { label: 'Đọc sách', value: 'reading' },
            { label: 'Nghe nhạc', value: 'music' },
            { label: 'Thể thao', value: 'sports' },
            { label: 'Du lịch', value: 'travel' }
          ]
        },
        {
          key: 'documents',
          label: 'Tài liệu đính kèm',
          type: 'file',
          required: true,
          multiple: true,
          disabled: false,
          validators: {
            required: true
          }
        },
        {
          key: 'birthDate',
          label: 'Ngày sinh',
          type: 'date',
          default: '2000-01-01',
          placeholder: 'Chọn ngày sinh',
          required: true,
          disabled: false,
          validators: {
            minDate: '1900-01-01',
            maxDate: '2025-12-31'
          }
        },
        {
          key: 'satisfaction',
          label: 'Mức độ hài lòng',
          type: 'range',
          default: 5,
          required: true,
          disabled: false,
          min: 0,
          max: 1000,
          step: 5,
          validators: {
            min: 0,
            max: 10
          }
        },
        {
          key: 'receiveNotifications',
          label: 'Nhận thông báo',
          type: 'toggle',
          default: false,
          required: false,
          disabled: false
        },
        {
          key: "agreeTerms",
          label: "Tôi đồng ý với điều khoản",
          type: "checkbox",
          required: true
        },
        {
          key: 'favoriteColor',
          label: 'Màu yêu thích',
          type: 'color',
          default: '#ff0000',
          required: false,
          disabled: false
        },
      ]
    }
    this.metadata.fields.forEach((field: any) => {
      const validators = [];

      if (field.required) validators.push(Validators.required);
      if (field.validators?.minlength) validators.push(Validators.minLength(field.validators.minlength));
      if (field.validators?.maxlength) validators.push(Validators.maxLength(field.validators.maxlength));
      if (field.validators?.min) validators.push(Validators.min(field.validators.min));
      if (field.validators?.max) validators.push(Validators.max(field.validators.max));
      if (field.validators?.pattern) validators.push(Validators.pattern(field.validators.pattern));
      if (field.validators?.email) validators.push(Validators.email);
      if (field.validators?.minDate) { validators.push(minDate(new Date(field.validators.minDate))); }
      if (field.validators?.maxDate) { validators.push(maxDate(new Date(field.validators.maxDate))); }
      if (field.validators?.minSelected) { validators.push(minSelected(field.validators.minSelected)); }
      if (field.validators?.maxSelected) { validators.push(maxSelected(field.validators.max)); }
      console.log(field.default)
      this.form.addControl(field.key, this.fb.control({ value: field.default || '', disabled: field.disabled || false }, validators));
    });
  }

  handleFileChange(key: string, files: File[]) {
    this.form.get(key)?.setValue(files);
  }

  onSubmit(): void {
    if (this.form.valid) {
      console.log('Dữ liệu form:', this.form.value);
    } else {
      console.warn('Form không hợp lệ');
      this.form.markAllAsTouched();
    }
  }

  getErrorMessage(fieldKey: string): string | null {
    const control = this.form.get(fieldKey);
    if (!control || !control.touched || control.valid) return null;

    const errors = control.errors;

    if (errors?.['required']) return 'Trường này là bắt buộc.';
    if (errors?.['minlength'])
      return `Độ dài tối thiểu là ${errors['minlength'].requiredLength} ký tự.`;
    if (errors?.['maxlength'])
      return `Độ dài tối đa là ${errors['maxlength'].requiredLength} ký tự.`;
    if (errors?.['min'])
      return `Giá trị tối thiểu là ${errors['min'].min}.`;
    if (errors?.['max'])
      return `Giá trị tối đa là ${errors['max'].max}.`;
    if (errors?.['pattern'])
      return 'Giá trị không đúng định dạng.';
    if (errors?.['email'])
      return 'Email không hợp lệ.';
    if (errors?.['minDate']) {
      const requiredDate = new Date(errors['minDate'].requiredDate);
      return `Ngày phải sau hoặc bằng ${requiredDate.toLocaleDateString()}.`;
    }
    if (errors?.['maxDate']) {
      const requiredDate = new Date(errors['maxDate'].requiredDate);
      return `Ngày phải trước hoặc bằng ${requiredDate.toLocaleDateString()}.`;
    }

    return 'Giá trị không hợp lệ.';
  }
}
