export interface PageMetadata {
  controller: string;
  type: string;
  formId: string;
  primaryKey: string[];
  idVC?: string;
  VCDate?: string;
  title: string;
  class?: string;
  tabs: TabMetadata[];
  dataProcessing: DataProcessing;
}

export interface AggregateConfig {
  type: 'sum' | 'count' | 'avg' | 'min' | 'max';
  sourceField: string; // Field trong detail
  condition?: string; // Điều kiện lọc rows
  precision?: number;
  min?: number; // Validation cho aggregate value
  max?: number;
  message?: string;
}

export interface DataProcessing {
  actions: {
    post: PostAction[];
  };
}

export interface PostAction {
  step: string;
  type: string;
  query: string;
}

export interface TabMetadata {
  title: string;
  class?: string;
  type?: string;
  form?: FormMetadata;
  detail?: FormMetadata[];
}

export interface FormMetadata {
  title?: string;
  class?: string;
  entity?: string;
  fields: Field[];
  controllerDetail?: string;
  formId?: string;
  foreignKey?: string;
  initialData: any;

  // Master aggregations từ detail
  masterAggregations?: {
    [masterFieldKey: string]: AggregateConfig;
  };

  // Detail-level calculations
  calculations?: CalculationRule[];
}


export interface FieldValidator {
  minLength?: number;
  maxLength?: number;
  pattern?: string;
  min?: number;
  max?: number;
  minDate?: string;
  maxDate?: string;
  minSelected?: number;
  maxSelected?: number;
  class?: string;
}

export interface MasterSubtotalCalculation {
  calculations?: CalculationRule[];
}

export interface FieldCalculation {
  calculations?: CalculationRule[];
}

export interface FieldOption {
  label: string;
  value: string;
}

export interface Field {
  key: string;
  label: string;
  type: string;
  default?: any;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  readonly?: boolean; // Cho calculated fields
  validators?: FieldValidator; // Backward compatibility
  calculation?: FieldCalculation; // Cho field tính toán
  options?: FieldOption[];
  rows?: number;
  cols?: number;
  multiple?: boolean;
  min?: number;
  max?: number;
  step?: number;
  trigger?: string[]; // Các field bị ảnh hưởng khi thay đổi
  onChange?: any; // Callback khi field thay đổi
  aggregation?: AggregateConfig; // Config cho tính toán tổng hợp
  aggregateValidation?: AggregateConfig; // Validation tổng hợp
  width?: string;
  subtotal?: boolean; // Có hiển thị subtotal không
  masterSubtotalConfig?: FieldCalculation; // Config cho master subtotal
}

export interface CalculationRule {
  formula: string; // e.g., "[total] = [quantity] * [price]"
  dependencies: string[]; // Các field trigger calculation này
  condition?: string; // Điều kiện để thực hiện calculation
  precision?: number; // Số chữ số thập phân
}


// src/app/models/form-metadata.model.ts - Updated với validation và calculation
export interface PageMetadata {
  title: string;
  class?: string;
  tabs: TabMetadata[];
}

export interface TabMetadata {
  title: string;
  class?: string;
  form?: FormMetadata;
  detail?: FormMetadata[]; // Cập nhật để hỗ trợ multiple details
}

//export interface FormMetadata {
//  title?: string;
//  class?: string;
//  fields: Field[];
//  calculations?: CalculationRule[]; // Thêm calculation toàn form
//}

// Thêm interface cho Calculation Rules
export interface CalculationRule {
  formula: string; // e.g., "[total] = [quantity] * [price]"
  dependencies: string[]; // Các field trigger calculation này
  condition?: string; // Điều kiện để thực hiện calculation
  precision?: number; // Số chữ số thập phân
}

// Thêm interface cho Field Calculation
export interface FieldCalculation {
  calculations?: CalculationRule[];
}

export interface FieldValidator {
  minLength?: number;
  maxLength?: number;
  pattern?: string;
  min?: number;
  max?: number;
  minDate?: string;
  maxDate?: string;
  minSelected?: number;
  maxSelected?: number;
  class?: string;
}

export interface FieldOption {
  label: string;
  value: string;
}

 // src/app/models/form-metadata.model.ts - Updated với calculation only
export interface PageMetadata {
  title: string;
  class?: string;
  tabs: TabMetadata[];
}

export interface TabMetadata {
  title: string;
  class?: string;
  form?: FormMetadata;
  detail?: FormMetadata[]; // Cập nhật để hỗ trợ multiple details
}

// Thêm interface cho Calculation Rules
export interface CalculationRule {
  formula: string; // e.g., "[total] = [quantity] * [price]"
  dependencies: string[]; // Các field trigger calculation này
  condition?: string; // Điều kiện để thực hiện calculation
  precision?: number; // Số chữ số thập phân
}

// Thêm interface cho Field Calculation
export interface FieldCalculation {
  calculations?: CalculationRule[];
}

export interface FieldValidator {
  minLength?: number;
  maxLength?: number;
  pattern?: string;
  min?: number;
  max?: number;
  minDate?: string;
  maxDate?: string;
  minSelected?: number;
  maxSelected?: number;
  class?: string;
}

export interface FieldOption {
  label: string;
  value: string;
}
// Class để xử lý calculations
export class CalculationEngine {

  static evaluateFormula(formula: string, rowData: any, aggregateData?: any[]): number {
    try {
      // Extract the right side of the equation (after =)
      const parts = formula.split('=');
      if (parts.length !== 2) {
        console.error('Invalid formula format. Expected: [field] = expression');
        return 0;
      }

      let expression = parts[1].trim(); // Get right side of equation

      // Handle SUM functions directly
      expression = expression.replace(/SUM\(\[([^\]]+)\]\)/g, (match, fieldName) => {
        if (!aggregateData || aggregateData.length === 0) return '0';
            const sum = aggregateData.reduce((total, item) => {
            const value = Number(item[fieldName]) || 0;
            let numValue = 0;
            if (value !== null && value !== undefined) {
              numValue = Number(value) || 0;
            }
              return total + numValue;
        }, 0);

        return String(sum);
      });

      // Replace field references [fieldName] with values
      expression = expression.replace(/\[([^\]]+)\]/g, (match, fieldName) => {
        const value = rowData[fieldName];
        const numValue = Number(value) || 0;
        let val = 0;
        if (numValue !== null && numValue !== undefined) {
          val = Number(numValue) || 0;
        }
        return String(val);
      });

      // Handle conditional expressions (ternary operators)
      expression = this.handleConditionals(expression, rowData);

      // Clean up any extra spaces around operators
      expression = expression.replace(/\s+/g, ' ').trim();

      // Safe evaluation
      return this.safeEval(expression);
    } catch (error) {
      console.error('Calculation error in formula:', formula, error);
      return 0;
    }
  }

  private static handleConditionals(expression: string, rowData: any): string {
    // Handle simple conditional logic: [field] === 'value' ? result1 : result2
    return expression.replace(/\[([^\]]+)\]\s*===\s*'([^']+)'\s*\?\s*([^:]+)\s*:\s*(.+)/g,
      (match, fieldName, compareValue, trueValue, falseValue) => {
        const fieldValue = rowData[fieldName];
        return fieldValue === compareValue ? trueValue.trim() : falseValue.trim();
      }
    );
  }

  private static safeEval(expression: string): number {
    // Remove any potentially dangerous characters but keep math operators
    const sanitized = expression.replace(/[^0-9+\-*/.() ]/g, '');

    // Validate that expression only contains numbers and basic math operators
    if (!/^[0-9+\-*/.() ]+$/.test(sanitized)) {
      console.error('Invalid mathematical expression:', expression);
      return 0;
    }

    try {
      // Use Function constructor for safer evaluation than eval
      const result = new Function('return ' + sanitized)();
      return Number(result) || 0;
    } catch (error) {
      console.error('Error evaluating expression:', sanitized, error);
      return 0;
    }
  }

  static applyCalculations(field: Field, rowData: any, allRowData?: any[]): number | null {
    if (!field.calculation?.calculations) return null;

    for (const calc of field.calculation.calculations) {
      // Check if all dependencies have values
      const hasAllDependencies = calc.dependencies.every(dep => {
        const value = rowData[dep];
        return value !== undefined && value !== null && value !== '';
      });

      if (hasAllDependencies) {
        const result = this.evaluateFormula(calc.formula, rowData, allRowData);

        // Apply precision if specified
        if (calc.precision !== undefined) {
          return Number(result.toFixed(calc.precision));
        }

        return result;
      }
    }

    return null;
  }
}

// Validation engine - chỉ giữ basic validation từ validators
export class ValidationEngine {

  static validateField(field: Field, value: any, formData: any = {}): string[] {
    const errors: string[] = [];

    // Skip validation for readonly/disabled fields
    if (field.readonly || field.disabled) return errors;

    // Chỉ sử dụng old validation system từ validators
    if (field.validators) {
      const legacyErrors = this.validateLegacy(field, value);
      errors.push(...legacyErrors);
    }

    return errors;
  }

  private static validateLegacy(field: Field, value: any): string[] {
    const errors: string[] = [];
    const validators = field.validators;

    if (!validators) return errors;

    if (field.required && (!value || value.toString().trim() === '')) {
      errors.push(`${field.label} là bắt buộc`);
    }

    if (value) {
      //if (validators.minLength && value.toString().length < validators.minLength) {
      //  errors.push(`${field.label} phải có ít nhất ${validators.minLength} ký tự`);
      //}

      //if (validators.maxLength && value.toString().length > validators.maxLength) {
      //  errors.push(`${field.label} không được quá ${validators.maxLength} ký tự`);
      //}

      //if (validators.min !== undefined && Number(value) < validators.min) {
      //  errors.push(`${field.label} phải lớn hơn hoặc bằng ${validators.min}`);
      //}

      //if (validators.max !== undefined && Number(value) > validators.max) {
      //  errors.push(`${field.label} phải nhỏ hơn hoặc bằng ${validators.max}`);
      //}

      if (validators.pattern && !new RegExp(validators.pattern).test(value.toString())) {
        errors.push(`${field.label} không đúng định dạng`);
      }
    }

    return errors;
  }

}
