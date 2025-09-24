import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
// example custom validator
export function minDate(minDate: Date): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value) return null;
    const controlDate = new Date(value);
    return controlDate >= minDate ? null : { minDate: { requiredDate: minDate, actualDate: controlDate } };
  };
}

export function maxDate(maxDate: Date): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value) return null;
    const controlDate = new Date(value);
    return controlDate <= maxDate ? null : { maxDate: { requiredDate: maxDate, actualDate: controlDate } };
  };
}

export function minSelected(min: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    return control.value && control.value.length < min
      ? { minSelected: { required: min, actual: control.value.length } }
      : null;
  };
}

export function maxSelected(max: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    return control.value && control.value.length > max
      ? { maxSelected: { allowed: max, actual: control.value.length } }
      : null;
  };
}