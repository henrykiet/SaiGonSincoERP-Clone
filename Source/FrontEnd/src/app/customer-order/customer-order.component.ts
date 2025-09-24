import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { PageMetadata, Field, CalculationEngine, ValidationEngine, CalculationRule } from '../models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-customer-order',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-order.component.html',
})
export class CustomerOrderComponent implements OnInit {
  metadata?: PageMetadata;
  selectedTab = 0;
  selectedDetailIndex = 0; // New: Track which detail section is active
  formData: { [key: number]: { [key: string]: any } } = {};

  detailRowsData: { [tabIndex: number]: { [detailIndex: number]: any[] } } = {};
  filteredDetailRowsData: { [tabIndex: number]: { [detailIndex: number]: any[] } } = {};

  errors: { [key: string]: string } = {};

  columnFiltersData: { [tabIndex: number]: { [detailIndex: number]: { [key: string]: string } } } = {};
  filterMode: 'all' | 'any' = 'all';

  // Aggregation state
  masterAggregates: { [key: string]: any } = {};
  calculatedValues: { [key: string]: any } = {};

  // Dynamic summary
  currentSummaryData: any = null;

  constructor(private http: HttpClient, private router: Router) { }

  ngOnInit(): void {
    this.http.get<PageMetadata>('metadata/customer-order.page.json').subscribe((meta) => {
      this.metadata = meta;
      this.initializeFormData();
      this.loadInitialDetailData();
    });
  }

  isValidDateInput(value: string): boolean {
    const regex = /^\d{4}-\d{2}-\d{2}$/;
    if (!regex.test(value)) return false;

    const date = new Date(value);
    return !isNaN(date.getTime()) && date.toISOString().slice(0, 10) === value;
  }

  private initializeFormData(): void {
    if (!this.metadata) return;

    this.metadata.tabs.forEach((tab, index) => {
      if (tab.form) {
        tab.form.fields.forEach(field => {
          let value = (tab.form as any)?.initialData?.[field.key]
          switch (field.type) {
            case "number":
              if (isNaN(Number(value))) value = ""
              break;
            case "date":
              if (!this.isValidDateInput(value)) value = ""
              break;
            default:
              break;
          }
          if (!this.formData[index]) {
            this.formData[index] = {};
          }
          this.formData[index][field.key] = value || '';
        });
      }

      // Initialize detail data structures
      if (tab.detail && Array.isArray(tab.detail)) {
        this.detailRowsData[index] = {};
        this.filteredDetailRowsData[index] = {};
        this.columnFiltersData[index] = {};

        tab.detail.forEach((_, detailIndex) => {
          this.detailRowsData[index][detailIndex] = [];
          this.filteredDetailRowsData[index][detailIndex] = [];
          this.columnFiltersData[index][detailIndex] = {};
        });
      }
    });
  }

  loadInitialDetailData(): void {
    if (!this.metadata) return;

    this.metadata.tabs.forEach((tab, tabIndex) => {
      if (tab.detail && Array.isArray(tab.detail)) {
        tab.detail.forEach((detailSection, detailIndex) => {
          if (detailSection.initialData && Array.isArray(detailSection.initialData)) {
            this.detailRowsData[tabIndex][detailIndex] = [...detailSection.initialData];
            console.log(`Loaded initial detail data for tab ${tabIndex}, detail ${detailIndex}:`,
              this.detailRowsData[tabIndex][detailIndex]);
          } else {
            this.detailRowsData[tabIndex][detailIndex] = [];
          }
        });
      }
    });

    this.applyFilters();
    this.calculateAggregateValues();
    this.updateMasterAggregations();
  }


  // Method để tính toán aggregate values (subtotal, total_tax, etc.)
  updateMasterAggregations(): void {
    const currentTab = this.metadata?.tabs[this.selectedTab];
    if (!currentTab?.form) return;

    // Find master fields with aggregation config
    const aggregateFields = currentTab.form.fields.filter(f => f.aggregation);

    aggregateFields.forEach(field => {
      const config = field.aggregation!;
      const aggregateValue = this.calculateDetailAggregate(config, this.currentDetailRows);

      // Update master form data and aggregates
      this.formData[this.selectedTab][field.key] = aggregateValue;
      this.masterAggregates[field.key] = aggregateValue;

      // Trigger dependent calculations
      this.triggerCalculationsForField(field.key, this.selectedTab);
    });

    // Check for master aggregations defined in detail section
    const detailSection = this.currentDetailSection;
    if (detailSection?.masterAggregations) {
      Object.keys(detailSection.masterAggregations).forEach(masterFieldKey => {
        const config = detailSection.masterAggregations![masterFieldKey];
        const aggregateValue = this.calculateDetailAggregate(config, this.currentDetailRows);

        this.formData[this.selectedTab][masterFieldKey] = aggregateValue;
        this.masterAggregates[masterFieldKey] = aggregateValue;

        // Trigger dependent calculations
        this.triggerCalculationsForField(masterFieldKey, this.selectedTab);
      });
    }
  }

  updateMasterCalculations(): void {
    const currentTab = this.metadata?.tabs[this.selectedTab];
    if (!currentTab?.form) return;

    // Step 1: Update aggregation fields (total_amount, item_count, etc.)
    this.updateAggregationFields(currentTab);

    // Step 2: Update calculated fields (discount_amount, tax_amount, etc.)
    this.updateCalculatedFields(currentTab);
  }

  // Update calculated fields based on formulas
  updateCalculatedFields(currentTab: any): void {
    const calculatedFields = currentTab.form.fields.filter((f: any) => f.calculation?.calculations);

    // Apply calculations multiple times to handle dependencies
    for (let iteration = 0; iteration < 3; iteration++) {
      calculatedFields.forEach((field: any) => {
        const value = this.calculateField(field);
        if (value !== null) {
          this.formData[this.selectedTab][field.key] = value;
        }
      });
    }
  }

  updateAggregationFields(currentTab: any): void {
    const aggregateFields = currentTab.form.fields.filter((f: any) => f.aggregation);

    aggregateFields.forEach((field: any) => {
      const value = this.calculateAggregation(field.aggregation);
      this.formData[this.selectedTab][field.key] = value;
    });
  }

  // Calculate aggregation values
  calculateAggregation(config: any): number {
    const detailRows = this.currentDetailRows;
    let result = 0;

    switch (config.type) {
      case 'sum':
        result = detailRows.reduce((sum, row) =>
          sum + (parseFloat(row[config.sourceField]) || 0), 0);
        break;
      case 'count':
        if (config.sourceField) {
          result = detailRows.filter(row =>
            row[config.sourceField] !== null &&
            row[config.sourceField] !== undefined &&
            row[config.sourceField] !== ''
          ).length;
        } else {
          result = detailRows.length;
        }
        break;
      case 'average':
        if (detailRows.length > 0) {
          const sum = detailRows.reduce((sum, row) =>
            sum + (parseFloat(row[config.sourceField]) || 0), 0);
          result = sum / detailRows.length;
        }
        break;
    }

    return config.precision !== undefined ?
      Number(result.toFixed(config.precision)) : result;
  }

  // Calculate field value based on formula
  calculateField(field: any): number | null {
    if (!field.calculation?.calculations) return null;

    for (const calc of field.calculation.calculations) {
      const hasAllDependencies = calc.dependencies.every((dep: string) => {
        const value = this.formData[this.selectedTab][dep];
        return value !== undefined && value !== null && value !== '';
      });
    }

    return null;
  }

  calculateAggregateValues(): void {
    const currentDetailSection = this.currentDetailSection;
    if (!currentDetailSection?.calculations) return;

    const detailData = this.currentDetailRows;

    for (const calc of currentDetailSection.calculations) {
      const result = CalculationEngine.evaluateFormula(calc.formula, {}, detailData);

      // Extract field name from formula (e.g., "[subtotal]" -> "subtotal")
      const fieldMatch = calc.formula.match(/\[([^\]]+)\]\s*=/);
      if (fieldMatch) {
        const fieldName = fieldMatch[1];
        this.calculatedValues[fieldName] = calc.precision !== undefined ?
          Number(result.toFixed(calc.precision)) : result;
      }
    }

    // Trigger master form calculations that depend on detail calculations
    this.calculateMasterFormValues();
  }

  // Method để tính toán master form values
  calculateMasterFormValues(): void {
    const currentTab = this.metadata?.tabs[this.selectedTab];
    if (!currentTab?.form) return;

    for (const field of currentTab.form.fields) {
      if (field.calculation?.calculations) {
        const mergedData = {
          ...this.formData[this.selectedTab],
          ...this.calculatedValues
        };
        const result = CalculationEngine.applyCalculations(field, mergedData);

        if (result !== null) {
          this.formData[this.selectedTab][field.key] = result;
        }
      }
    }
  }

  calculateDetailAggregate(config: any, detailRows: any[]): number {
    // Apply condition filter if specified
    let filteredRows = detailRows;
    if (config.condition) {
      filteredRows = detailRows.filter(row => {
        try {
          const condition = config.condition!.replace(/\[([^\]]+)\]/g, (_: string, fieldName: string) => {
              const value = row[fieldName];
            return typeof value === 'string' ? `"${value}"` : (value || 0);
          });
          return new Function('return ' + condition)();
        } catch {
          return true;
        }
      });
    }

    let result = 0;
    switch (config.type) {
      case 'sum':
        result = filteredRows.reduce((sum, row) =>
          sum + (parseFloat(row[config.sourceField]) || 0), 0);
        break;
    }

    return config.precision !== undefined ?
      Number(result.toFixed(config.precision)) : result;
  }

  // Get master field value for summary display
  getCurrentMasterFields(): any[] {
    return this.metadata?.tabs[this.selectedTab]?.form?.fields || [];
  }

  getSummaryFields(): any[] {
    return this.getCurrentMasterFields().filter(field =>
      field.disabled && field.type !== 'hidden'
    );
  }

  // Get master field value for summary display
  getMasterFieldValue(fieldKey: string): any {
    return this.formData[this.selectedTab]?.[fieldKey] || 0;
  }
  // Enhanced field change handler với calculation
  onFieldChange(fieldKey: string, value: any, tabIndex: number = this.selectedTab): void {
    // Update value
    this.formData[tabIndex][fieldKey] = value;

    // Trigger calculations for dependent fields
    this.triggerCalculationsForField(fieldKey, tabIndex);
  }


  // Method để trigger calculations khi field thay đổi
  triggerCalculationsForField(changedFieldKey: string, tabIndex: number): void {
    const currentTab = this.metadata?.tabs[tabIndex];
    if (!currentTab?.form) return;

    // Find fields that are triggered by this field change
    const triggeredFields = currentTab.form.fields.filter(field =>
      field.trigger?.includes(changedFieldKey)
    );

    // Recalculate triggered fields
    for (const field of triggeredFields) {
      if (field.calculation?.calculations) {
        const mergedData = {
          ...this.formData[tabIndex],
          ...this.calculatedValues
        };
        const result = CalculationEngine.applyCalculations(field, mergedData);

        if (result !== null) {
          this.formData[tabIndex][field.key] = result;
        }
      }
    }
  }

  // Apply calculations for a specific row
  applyRowCalculations(row: any, changedField: Field): void {
    const allFields = this.getAllDetailFields();

    // Find fields that should be calculated based on this change
    const fieldsToCalculate = allFields.filter(field =>
      field.calculation?.calculations?.some((calc: CalculationRule) =>
        calc.dependencies.includes(changedField.key)
      )
    );

    // Apply calculations with enhanced support
    for (const field of fieldsToCalculate) {
      const result = CalculationEngine.applyCalculations(field, row, this.currentDetailRows);
      if (result !== null) {
        row[field.key] = result;

        // Log which formula was used (for debugging)
        if (field.calculation?.calculations?.length > 1) {
          console.log(`Field ${field.key} calculated using multiple formulas, result: ${result}`);
        }
      }
    }
  }

  // Trigger calculations for fields that depend on the changed field
  triggerRowCalculations(row: any, changedFieldKey: string): void {
    const allFields = this.getAllDetailFields();

    // Find fields triggered by this change
    const triggeredFields = allFields.filter(field =>
      field.trigger?.includes(changedFieldKey)
    );

    // Apply calculations for triggered fields
    for (const field of triggeredFields) {
      if (field.calculation?.calculations) {
        const result = CalculationEngine.applyCalculations(field, row, this.currentDetailRows);
        if (result !== null) {
          row[field.key] = result;
        }
      }
    }
  }
  // Apply all calculations for a row
  applyAllRowCalculations(row: any): void {
    const allFields = this.getAllDetailFields();
    const calculatedFields = allFields.filter(f => f.calculation?.calculations);

    // Apply calculations multiple times to handle dependencies
    for (let i = 0; i < 3; i++) { // Max 3 iterations to resolve dependencies
      for (const field of calculatedFields) {
        const result = CalculationEngine.applyCalculations(field, row, this.currentDetailRows);
        if (result !== null) {
          row[field.key] = result;
        }
      }
    }
  }

  // Getter methods for current active detail section
  get currentDetailRows(): any[] {
    return this.detailRowsData[this.selectedTab]?.[this.selectedDetailIndex] || [];
  }

  get currentFilteredDetailRows(): any[] {
    return this.filteredDetailRowsData[this.selectedTab]?.[this.selectedDetailIndex] || [];
  }

  get currentColumnFilters(): { [key: string]: string } {
    return this.columnFiltersData[this.selectedTab]?.[this.selectedDetailIndex] || {};
  }

  get currentDetailSections(): any[] {
    const currentTab = this.metadata?.tabs[this.selectedTab];
    return currentTab?.detail || [];
  }

  get currentDetailSection(): any {
    return this.currentDetailSections[this.selectedDetailIndex];
  }

  onSelectChange(): void {
    console.log('Tab selection changed to:', this.selectedTab);
    this.selectedDetailIndex = 0; // Reset to first detail section when switching tabs
    this.applyFilters();
  }

  onDetailSectionChange(detailIndex: number): void {
    this.selectedDetailIndex = detailIndex;
    this.applyFilters();
  }

  onSubmit(): void {
    this.validateForm();

    if (Object.keys(this.errors).length === 0) {
      this.http.post("https://localhost:44310/api/Dynamic/save", this.buildInsertPayload()).subscribe({
        next: (response) => {
          console.log('Order created:', response);
          alert("Thành công")
          this.router.navigate(['/order']);
        },
        error: (err) => {
          alert("Gửi thất bại. Dữ liệu đã gửi:" + JSON.stringify(this.buildInsertPayload()))
          console.error('Error creating order:', err);
        }
      })
    } else {
      console.warn('Form có lỗi:', this.errors);
    }
  }

  onCancel(): void {
    if (confirm('Bạn có chắc chắn muốn hủy? Tất cả thay đổi sẽ bị mất.')) {
      this.initializeFormData();
      this.detailRowsData = {};
      this.filteredDetailRowsData = {};
      this.errors = {};
      this.columnFiltersData = {};

      console.log('Form cancelled and reset');
      alert('Đã hủy và làm mới form');
    }
  }

  mergeFormData(): { [key: string]: any } {
    const result: { [key: string]: any } = {};

    for (const key in this.formData) {
      Object.assign(result, this.formData[key]);
    }

    return result;
  }

  buildInsertPayload(): any {
    const postActions = this.metadata?.dataProcessing?.actions?.post;
    const details: any[] = []

    if (Array.isArray(postActions)) {
      this.metadata?.tabs.forEach((tab, selectedTab) => {
        for (const action of postActions) {
          action.query = action.query.replace(/@(\w+)/g, (_: string, key: string) => {
            const value = this.formData[selectedTab][key];
            return value !== undefined ? String(value) : '';
          })
        }

        // Handle multiple detail sections
        if (tab?.detail && Array.isArray(tab.detail)) {
          tab.detail.forEach((detailSection, detailIndex) => {
            const detailData = this.detailRowsData[this.selectedTab]?.[detailIndex] || [];
            if (detailData.length > 0) {
              details.push({
                controllerDetail: detailSection.controllerDetail,
                formIdDetail: detailSection.formId,
                foreignKey: detailSection.foreignKey,
                //data: detailData
                data: detailData.map(row => this.normalizeValues(row))
              });
            }
          });
        }
      });
    }

    // const formData = this.mergeFormData()
    const formData = this.normalizeValues(this.mergeFormData());
    const payload = {
      controller: this.metadata?.controller,
      formId: this.metadata?.formId,
      action: 'insert',
      type: this.metadata?.type,
      userId: '1',
      unit: 'CTY',
      language: 'vn',
      VCDate: this.metadata?.VCDate ? formData[this.metadata?.VCDate] : "",
      idVC: this.metadata?.idVC,
      primaryKey: this.metadata?.primaryKey,
      data: {
        ...formData,
        details: details
      },
      dataProcessing: {
        actions: {
          post: postActions
        }
      }
    };
    return payload
  }

  private validateForm(): void {
    this.errors = {};
    const currentTab = this.metadata?.tabs[this.selectedTab];

    if (currentTab?.form) {
      currentTab.form.fields.forEach(field => {
        if (field.required && !this.formData[this.selectedTab][field.key]) {
          this.errors[field.key] = `${field.label} là bắt buộc`;
        }
      });
    }
  }

  getFieldError(fieldKey: string): string | null {
    return this.errors[fieldKey] || null;
  }

  getAllDetailFields(): any[] {
    return this.currentDetailSection?.fields || [];
  }

  addDetailRow(): void {
    if (!this.currentDetailSection) return;

    const newRow: any = {};
    const currentRows = this.currentDetailRows;

    // Get the highest line_nbr and increment
    const maxId = currentRows.reduce((max, row) =>
      Math.max(max, row.line_nbr || 0), 0);
    newRow.line_nbr = maxId + 1;

    // Initialize ALL fields (including hidden ones) with default values
    this.getAllDetailFields().forEach(field => {
      if (field.type === 'hidden') {
        newRow[field.key] = field.default || '';
      } else {
        newRow[field.key] = field.default || '';
      }
    });

    this.detailRowsData[this.selectedTab][this.selectedDetailIndex].push(newRow);

    // Update master calculations
    this.updateMasterCalculations();

    this.applyFilters();
  }

  removeDetailRow(index: number): void {
    const rowToRemove = this.currentFilteredDetailRows[index];
    const actualIndex = this.currentDetailRows.findIndex(row => row === rowToRemove);

    if (actualIndex > -1) {
      this.detailRowsData[this.selectedTab][this.selectedDetailIndex].splice(actualIndex, 1);
      this.applyFilters();
    }
  }

  onDetailFieldChange(rowIndex: number, fieldKey: string, value: any): void {
    const row = this.currentFilteredDetailRows[rowIndex];
    if (!row) return;

    row[fieldKey] = value;
    // Apply calculations for this row
    const field = this.getAllDetailFields().find(f => f.key === fieldKey);
    if (field) {
      this.applyRowCalculations(row, field);

      // Trigger calculations for dependent fields in the same row
      this.triggerRowCalculations(row, fieldKey);
    }

    // Update master calculations
    this.updateMasterCalculations();

    // Re-apply filters if the changed field has a filter
    if (this.currentColumnFilters[fieldKey]) {
      this.applyFilters();
    }
    // Recalculate aggregate values
    this.calculateAggregateValues();
  }

  trackByIndex(index: number, item: any): any {
    return item.line_nbr || index;
  }

  // Filter methods updated for multiple detail sections
  onFilterChange(fieldKey: string, value: string): void {
    const filters = this.columnFiltersData[this.selectedTab][this.selectedDetailIndex];
    if (value && value.trim()) {
      filters[fieldKey] = value.trim();
    } else {
      delete filters[fieldKey];
    }
    this.applyFilters();
  }
  private normalizeValues(obj: any): any {
    const normalized: any = {};
    for (const key in obj) {
      let value = obj[key];
      if (typeof value === 'string' && value.trim() === '') {
        value = null; // Chuỗi rỗng chuyển thành null
      }
      if (!isNaN(value) && value !== null && value !== '') {
        value = Number(value); // ép kiểu nếu có thể
      }
      normalized[key] = value;
    }
    return normalized;
  }

  applyFilters(): void {
    const currentRows = this.currentDetailRows;
    if (!currentRows || currentRows.length === 0) {
      this.filteredDetailRowsData[this.selectedTab][this.selectedDetailIndex] = [];
      return;
    }

    const filters = this.currentColumnFilters;
    const activeFilters = Object.keys(filters).filter(key =>
      filters[key] && filters[key].trim()
    );

    if (activeFilters.length === 0) {
      this.filteredDetailRowsData[this.selectedTab][this.selectedDetailIndex] = [...currentRows];
      return;
    }

    this.filteredDetailRowsData[this.selectedTab][this.selectedDetailIndex] = currentRows.filter(row => {
      const matches = activeFilters.map(fieldKey => {
        const filterValue = filters[fieldKey].toLowerCase();
        const rowValue = (row[fieldKey] || '').toString().toLowerCase();

        const field = this.getAllDetailFields()?.find(f => f.key === fieldKey);

        if (field?.type === 'select') {
          return rowValue === filterValue;
        } else if (field?.type === 'number') {
          if (filterValue.startsWith('>')) {
            const num = parseFloat(filterValue.substring(1));
            return !isNaN(num) && parseFloat(rowValue) > num;
          } else if (filterValue.startsWith('<')) {
            const num = parseFloat(filterValue.substring(1));
            return !isNaN(num) && parseFloat(rowValue) < num;
          } else if (filterValue.includes('-')) {
            const [min, max] = filterValue.split('-').map(v => parseFloat(v.trim()));
            const value = parseFloat(rowValue);
            return !isNaN(min) && !isNaN(max) && !isNaN(value) && value >= min && value <= max;
          } else {
            return rowValue.includes(filterValue);
          }
        } else {
          return rowValue.includes(filterValue);
        }
      });

      return this.filterMode === 'all' ?
        matches.every(match => match) :
        matches.some(match => match);
    });
  }

  clearFilter(fieldKey: string): void {
    delete this.columnFiltersData[this.selectedTab][this.selectedDetailIndex][fieldKey];
    this.applyFilters();
  }

  clearAllFilters(): void {
    this.columnFiltersData[this.selectedTab][this.selectedDetailIndex] = {};
    this.applyFilters();
  }

  hasActiveFilters(): boolean {
    const filters = this.currentColumnFilters;
    return Object.keys(filters).some(key =>
      filters[key] && filters[key].trim()
    );
  }

  getActiveFilterCount(): number {
    const filters = this.currentColumnFilters;
    return Object.keys(filters).filter(key =>
      filters[key] && filters[key].trim()
    ).length;
  }

  toggleFilterMode(): void {
    this.filterMode = this.filterMode === 'all' ? 'any' : 'all';
    this.applyFilters();
  }

  getColumnWidth(fieldType: string): string {
    const widthMap: { [key: string]: string } = {
      'text': '200px',
      'number': '120px',
      'select': '150px',
      'date': '130px'
    };
    return widthMap[fieldType] || '120px';
  }

  shouldShowSummary(): boolean {
    return this.currentDetailRows.length > 0 && this.selectedTab === 1;
  }

 
  formatValue(value: any, fieldType: string): string {
    if (!value) return '';

    switch (fieldType) {
      case 'number':
        return this.formatNumber(value);
      case 'date':
        return this.formatDate(value);
      default:
        return value.toString();
    }
  }

  formatFieldValue(row: any, field: any): string {
    const value = row[field.key];
    if (value === null || value === undefined || value === '') return '';

    switch (field.type) {
      case 'number':
        if (field.key === 'total' || field.key === 'price') {
          return this.formatCurrency(parseFloat(value));
        }
        return this.formatNumber(value);
      case 'date':
        return this.formatDate(value);
      default:
        return value.toString();
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount);
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('vi-VN').format(value);
  }

  formatDate(date: string): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('vi-VN');
  }

  getOptionLabel(fieldKey: string, value: string): string {
    const field = this.getAllDetailFields().find(f => f.key === fieldKey);
    if (!field || !field.options) return value;

    const option = field.options.find((opt: any) => opt.value === value);
    return option ? option.label : value;
  }
}
