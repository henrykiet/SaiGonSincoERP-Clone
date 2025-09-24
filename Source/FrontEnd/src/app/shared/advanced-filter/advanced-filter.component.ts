// src/app/shared/advanced-filter/advanced-filter.component.ts
import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FilterCondition } from '../../models';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

export interface FilterField {
  key: string;
  label: string;
  type: string;
  options?: { label: string; value: any }[];
  placeholder?: string;
  required?: boolean;
}

export interface FilterGroup {
  id: string;
  conditions: FilterCondition[];
  logicalOperator: 'AND' | 'OR';
}

export interface FilterResult {
  filteredData: any[];
  filterGroup: FilterGroup;
  summary: string;
  activeFilterCount: number;
}

@Component({
  selector: 'app-advanced-filter',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    <!-- Advanced Filter Component Template -->
    <div class="advanced-filter-container">

      <!-- Filter Header -->
      <div class="filter-header">
        <div class="filter-header-content">
          <div class="filter-title-section">

          </div>

          <div class="filter-actions">
            <!-- Logical Operator Toggle -->
            <button
              type="button"
              class="btn-logical-operator"
              [class.btn-and]="filterGroup.logicalOperator === 'AND'"
              [class.btn-or]="filterGroup.logicalOperator === 'OR'"
              (click)="toggleLogicalOperator()"
              [disabled]="filterGroup.conditions.length < 2"
              [title]="'FILTER.SWITCH_LOGIC' | translate">
              <span class="operator-text">{{ filterGroup.logicalOperator }}</span>
              <span class="operator-description">
                {{ filterGroup.logicalOperator === 'AND' ? ('FILTER.ALL_CONDITIONS' | translate) : ('FILTER.ANY_CONDITION' | translate) }}
              </span>
            </button>

            <!-- Add Filter Button -->
            <button
              type="button"
              class="btn-add-filter"
              (click)="addFilterCondition()">
              <span class="btn-icon">+</span>
              <span class="btn-text">{{ 'FILTER.ADD_FILTER' | translate }}</span>
            </button>

            <!-- Clear All Button -->
            <button
              type="button"
              class="btn-clear-all"
              [disabled]="!hasAnyFilters()"
              (click)="clearAllFilters()">
              <span class="btn-text">{{ 'FILTER.CLEAR_ALL' | translate }}</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Filter Conditions -->
      <div class="filter-body" [class.filter-body-empty]="filterGroup.conditions.length === 0">

        <!-- Active Filter Conditions -->
        <div *ngIf="filterGroup.conditions.length > 0" class="filter-conditions">
          <div *ngFor="let condition of filterGroup.conditions; let i = index; trackBy: trackByFilterId"
               class="filter-condition-row"
               [class.filter-condition-first]="i === 0">

            <!-- Logical Operator (for conditions after the first) -->
            <div *ngIf="i > 0" class="logical-operator-connector">
              <span class="logical-operator-badge"
                    [class.logical-operator-and]="filterGroup.logicalOperator === 'AND'"
                    [class.logical-operator-or]="filterGroup.logicalOperator === 'OR'">
                {{ filterGroup.logicalOperator }}
              </span>
            </div>

            <!-- Filter Condition Card -->
            <div class="filter-condition-card">
              <div class="filter-condition-inputs">

                <!-- Column Selection -->
                <div class="filter-input-group">
                  <label class="filter-label">{{ 'FILTER.COLUMN' | translate }}</label>
                  <select
                    [(ngModel)]="condition.field"
                    (ngModelChange)="onColumnChange(condition, $event)"
                    class="filter-select filter-column-select"
                    [class.filter-select-error]="!condition.field && showValidation">
                    <option value="">{{ 'FILTER.SELECT_COLUMN' | translate }}</option>
                    <option *ngFor="let field of filterableFields" [value]="field.key">
                      {{ field.label | translate }}
                    </option>
                  </select>
                </div>

                <!-- Operator Selection -->
                <div class="filter-input-group">
                  <label class="filter-label">{{ 'FILTER.OPERATOR' | translate }}</label>
                  <select
                    [(ngModel)]="condition.operator"
                    (ngModelChange)="onOperatorChange(condition, $event)"
                    class="filter-select filter-operator-select"
                    [class.filter-select-error]="!condition.operator && showValidation"
                    [disabled]="!condition.field">
                    <option value="">{{ 'FILTER.SELECT_OPERATOR' | translate }}</option>
                    <option *ngFor="let op of getOperatorsForColumn(condition.field)" [value]="op.value">
                      {{ op.label | translate }}
                    </option>
                  </select>
                </div>

                <!-- Value Input -->
                <div class="filter-input-group filter-value-group">
                  <label class="filter-label">{{ 'FILTER.VALUE' | translate }}</label>

                  <!-- Text Input -->
                  <input
                    *ngIf="getInputType(condition) === 'text'"
                    type="text"
                    [(ngModel)]="condition.value"
                    (ngModelChange)="onValueChange(condition, $event)"
                    class="filter-input filter-value-input"
                    [class.filter-input-error]="isValueRequired(condition) && !condition.value && showValidation"
                    [placeholder]="getPlaceholderForCondition(condition)"
                    [disabled]="!condition.operator || isValueDisabled(condition)" />

                  <!-- Number Input -->
                  <input
                    *ngIf="getInputType(condition) === 'number'"
                    type="number"
                    [(ngModel)]="condition.value"
                    (ngModelChange)="onValueChange(condition, $event)"
                    class="filter-input filter-value-input"
                    [class.filter-input-error]="isValueRequired(condition) && !condition.value && showValidation"
                    [placeholder]="getPlaceholderForCondition(condition)"
                    [disabled]="!condition.operator || isValueDisabled(condition)" />

                  <!-- Date Input -->
                  <input
                    *ngIf="getInputType(condition) === 'date'"
                    type="date"
                    [(ngModel)]="condition.value"
                    (ngModelChange)="onValueChange(condition, $event)"
                    class="filter-input filter-value-input"
                    [class.filter-input-error]="isValueRequired(condition) && !condition.value && showValidation"
                    [disabled]="!condition.operator || isValueDisabled(condition)" />

                  <!-- Select Input -->
                  <select
                    *ngIf="getInputType(condition) === 'select'"
                    [(ngModel)]="condition.value"
                    (ngModelChange)="onValueChange(condition, $event)"
                    class="filter-select filter-value-input"
                    [class.filter-select-error]="isValueRequired(condition) && !condition.value && showValidation"
                    [disabled]="!condition.operator || isValueDisabled(condition)">
                    <option value="">{{ 'FILTER.SELECT_VALUE' | translate }}</option>
                    <option *ngFor="let option of getOptionsForColumn(condition.field)" [value]="option.value">
                      {{ option.label | translate }}
                    </option>
                  </select>

                  <!-- Boolean/Checkbox Input -->
                  <div *ngIf="getInputType(condition) === 'boolean'" class="filter-boolean-input">
                    <label class="filter-checkbox-label">
                      <input
                        type="checkbox"
                        [(ngModel)]="condition.value"
                        (ngModelChange)="onValueChange(condition, $event)"
                        class="filter-checkbox"
                        [disabled]="!condition.operator || isValueDisabled(condition)" />
                      <span class="filter-checkbox-text">{{ condition.value ? ('COMMON.YES' | translate) : ('COMMON.NO' | translate) }}</span>
                    </label>
                  </div>

                  <!-- Value Not Required Message -->
                  <div *ngIf="isValueDisabled(condition)" class="filter-value-disabled">
                    <span class="disabled-message">{{ 'FILTER.NO_VALUE_REQUIRED' | translate }}</span>
                  </div>
                </div>

                <!-- Remove Filter Button -->
                <div class="filter-remove-group">
                  <button
                    type="button"
                    class="btn-remove-filter"
                    (click)="removeFilterCondition(condition.id)"
                    [title]="'FILTER.REMOVE_FILTER' | translate">
                    <span class="remove-icon">×</span>
                  </button>
                </div>
              </div>

              <!-- Condition Preview -->
              <div *ngIf="isConditionComplete(condition)" class="filter-condition-preview">
                <span class="preview-text">{{ getConditionPreview(condition) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- No Filters State -->
        <div
          *ngIf="filterGroup.conditions.length === 0"
          class="flex justify-center items-center p-4 bg-gray-100 rounded-md border border-dashed border-gray-300 w-full max-w-lg mx-auto"
        >
          <div class="text-center space-y-2 text-sm">
            <p class="text-gray-600">🔍 {{ 'FILTER.NO_FILTERS_APPLIED' | translate }}</p>
          </div>
        </div>
      </div>

      <div *ngIf="!autoApply" class="filter-apply-section">
        <button
          type="button"
          class="btn-apply-filters"
          (click)="applyFilters()">
          <span class="btn-text">{{ 'FILTER.APPLY' | translate }}</span>
        </button>
      </div>
    </div>
  `,
  styleUrls: ['./advanced-filter.component.css']
})
export class AdvancedFilterComponent implements OnInit, OnChanges {

  // Input Properties
  @Input() data: any[] = [];
  @Input() fields: FilterField[] = [];
  @Input() autoApply: boolean = true;
  @Input() showStats: boolean = true;
  @Input() showExport: boolean = false;
  @Input() initialFilters: FilterCondition[] = [];
  @Input() maxConditions: number = 10;
  @Input() enableValidation: boolean = true;

  // Output Events
  @Output() filterChange = new EventEmitter<FilterResult>();
  @Output() filtersApplied = new EventEmitter<FilterResult>();
  @Output() filterCleared = new EventEmitter<void>();
  @Output() exportRequested = new EventEmitter<any[]>();

  // Component State
  originalData: any[] = [];
  filteredData: any[] = [];
  filterableFields: FilterField[] = [];
  filterGroup: FilterGroup = {
    id: 'main',
    conditions: [],
    logicalOperator: 'AND'
  };
  showValidation: boolean = false;

  // Operators Configuration
  private operators = {
    text: [
      { value: 'like', label: 'FILTER.OPERATORS.CONTAINS' },
      { value: 'not like', label: 'FILTER.OPERATORS.NOT_CONTAINS' },
      { value: '=', label: 'FILTER.OPERATORS.EQUALS' },
      { value: '<>', label: 'FILTER.OPERATORS.NOT_EQUALS' },
      { value: 'like_start', label: 'FILTER.OPERATORS.STARTS_WITH' },
      { value: 'like_end', label: 'FILTER.OPERATORS.ENDS_WITH' },
      { value: 'is null', label: 'FILTER.OPERATORS.IS_EMPTY' },
      { value: 'is not null', label: 'FILTER.OPERATORS.IS_NOT_EMPTY' }
    ],
    number: [
      { value: '=', label: 'FILTER.OPERATORS.EQUALS' },
      { value: '<>', label: 'FILTER.OPERATORS.NOT_EQUALS' },
      { value: '>', label: 'FILTER.OPERATORS.GREATER_THAN' },
      { value: '>=', label: 'FILTER.OPERATORS.GREATER_EQUAL' },
      { value: '<', label: 'FILTER.OPERATORS.LESS_THAN' },
      { value: '<=', label: 'FILTER.OPERATORS.LESS_EQUAL' },
      { value: 'between', label: 'FILTER.OPERATORS.BETWEEN' },
      { value: 'is null', label: 'FILTER.OPERATORS.IS_EMPTY' },
      { value: 'is not null', label: 'FILTER.OPERATORS.IS_NOT_EMPTY' }
    ],
    select: [
      { value: '=', label: 'FILTER.OPERATORS.EQUALS' },
      { value: '<>', label: 'FILTER.OPERATORS.NOT_EQUALS' },
      { value: 'is null', label: 'FILTER.OPERATORS.IS_EMPTY' },
      { value: 'is not null', label: 'FILTER.OPERATORS.IS_NOT_EMPTY' }
    ],
    date: [
      { value: '=', label: 'FILTER.OPERATORS.ON' },
      { value: '<>', label: 'FILTER.OPERATORS.NOT_ON' },
      { value: '>', label: 'FILTER.OPERATORS.AFTER' },
      { value: '>=', label: 'FILTER.OPERATORS.ON_AFTER' },
      { value: '<', label: 'FILTER.OPERATORS.BEFORE' },
      { value: '<=', label: 'FILTER.OPERATORS.ON_BEFORE' },
      { value: 'between', label: 'FILTER.OPERATORS.BETWEEN' },
      { value: 'is null', label: 'FILTER.OPERATORS.IS_EMPTY' },
      { value: 'is not null', label: 'FILTER.OPERATORS.IS_NOT_EMPTY' }
    ],
    boolean: [
      { value: '=', label: 'FILTER.OPERATORS.IS' },
      { value: '<>', label: 'FILTER.OPERATORS.IS_NOT' }
    ]
  }

  constructor(private translate: TranslateService) { }

  ngOnInit(): void {
    this.initializeComponent();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.originalData = [...this.data];
      this.applyFilters();
    }

    if (changes['fields'] && this.fields) {
      this.filterableFields = this.fields.filter(field =>
        ['text', 'number', 'select', 'date', 'boolean'].includes(field.type ?? "text")
      );
    }

    if (changes['initialFilters'] && this.initialFilters) {
      this.loadInitialFilters();
    }
  }

  private initializeComponent(): void {
    this.originalData = [...this.data];
    this.filteredData = [...this.data];
    this.filterableFields = this.fields.filter(field =>
      ['text', 'number', 'select', 'date', 'boolean'].includes(field.type ?? "text")
    );

    if (this.initialFilters && this.initialFilters.length > 0) {
      this.loadInitialFilters();
    }
  }

  private loadInitialFilters(): void {
    this.filterGroup.conditions = this.initialFilters.map(filter => {
      return {
        ...filter,
        id: filter.id || this.generateFilterId()
      }
    }) || [];
    console.log("loadInitialFilters", this.filterGroup.conditions)

    if (this.autoApply) {
      this.applyFilters();
    }
  }

  // Filter Management Methods
  addFilterCondition(): void {
    if (this.filterGroup.conditions.length >= this.maxConditions) {
      alert(`Maximum ${this.maxConditions} filters allowed`);
      return;
    }

    const newCondition: FilterCondition = {
      id: this.generateFilterId(),
      field: '',
      operator: '',
      value: '',
      columnType: ''
    };

    this.filterGroup.conditions.push(newCondition);
    this.emitFilterChange();
  }

  removeFilterCondition(conditionId: string): void {
    this.filterGroup.conditions = this.filterGroup.conditions.filter(c => c.id !== conditionId);

    if (this.autoApply) {
      this.applyFilters();
    }

    this.emitFilterChange();
  }

  clearAllFilters(): void {
    this.filterGroup.conditions = [];
    this.showValidation = false;

    if (this.autoApply) {
      this.applyFilters();
    }

    this.filterCleared.emit();
    this.emitFilterChange();
  }

  toggleLogicalOperator(): void {
    this.filterGroup.logicalOperator = this.filterGroup.logicalOperator === 'AND' ? 'OR' : 'AND';

    if (this.autoApply) {
      this.applyFilters();
    }

    this.emitFilterChange();
  }

  // Event Handlers
  onColumnChange(condition: FilterCondition, column: string): void {
    condition.field = column;
    condition.operator = '';
    condition.value = '';
    condition.columnType = this.getColumnType(column);

    if (this.autoApply) {
      this.applyFilters();
    }

    this.emitFilterChange();
  }

  onOperatorChange(condition: FilterCondition, operator: string): void {
    condition.operator = operator;

    // Reset value for operators that don't need a value
    if (this.isValueDisabled(condition)) {
      condition.value = '';
    }

    if (this.autoApply) {
      this.applyFilters();
    }

    this.emitFilterChange();
  }

  onValueChange(condition: FilterCondition, value: any): void {
    condition.value = value;

    if (this.autoApply) {
      this.applyFilters();
    }

    this.emitFilterChange();
  }

  // Filter Application
  applyFilters(): void {
    if (!this.originalData || this.originalData.length === 0) {
      this.filteredData = [];
      this.emitFilterApplied();
      return;
    }

    const validConditions = this.getValidConditions();

    if (validConditions.length === 0) {
      this.filteredData = [...this.originalData];
      this.emitFilterApplied();
      return;
    }

    this.filteredData = this.originalData.filter(row => {
      const results = validConditions.map(condition => this.evaluateCondition(row, condition));

      return this.filterGroup.logicalOperator === 'AND'
        ? results.every(result => result)
        : results.some(result => result);
    });

    this.emitFilterApplied();
  }

  private evaluateCondition(row: any, condition: FilterCondition): boolean {
    const rowValue = row[condition.field];
    const filterValue = condition.value;
    const columnType = this.getColumnType(condition.field);

    // Handle empty/not empty checks
    if (condition.operator === 'is_empty') {
      return rowValue === null || rowValue === undefined || rowValue === '';
    }

    if (condition.operator === 'is_not_empty') {
      return rowValue !== null && rowValue !== undefined && rowValue !== '';
    }

    // Convert values for comparison
    const rowStr = (rowValue || '').toString().toLowerCase();
    const filterStr = (filterValue || '').toString().toLowerCase();

    switch (columnType) {
      case 'text':
        return this.evaluateTextCondition(rowStr, filterStr, condition.operator);

      case 'number':
        return this.evaluateNumberCondition(
          parseFloat(rowValue) || 0,
          parseFloat(filterValue) || 0,
          condition.operator
        );

      case 'select':
        return this.evaluateSelectCondition(rowValue, filterValue, condition.operator);

      case 'date':
        return this.evaluateDateCondition(rowValue, filterValue, condition.operator);

      case 'boolean':
        return this.evaluateBooleanCondition(rowValue, filterValue, condition.operator);

      default:
        return this.evaluateTextCondition(rowStr, filterStr, condition.operator);
    }
  }

  private evaluateTextCondition(rowValue: string, filterValue: string, operator: string): boolean {
    switch (operator) {
      case 'contains': return rowValue.includes(filterValue);
      case 'does_not_contain': return !rowValue.includes(filterValue);
      case 'equals': return rowValue === filterValue;
      case 'does_not_equal': return rowValue !== filterValue;
      case 'starts_with': return rowValue.startsWith(filterValue);
      case 'ends_with': return rowValue.endsWith(filterValue);
      default: return true;
    }
  }

  private evaluateNumberCondition(rowValue: number, filterValue: number, operator: string): boolean {
    switch (operator) {
      case 'equals': return rowValue === filterValue;
      case 'does_not_equal': return rowValue !== filterValue;
      case 'greater_than': return rowValue > filterValue;
      case 'greater_than_or_equal': return rowValue >= filterValue;
      case 'less_than': return rowValue < filterValue;
      case 'less_than_or_equal': return rowValue <= filterValue;
      case 'between':
        const range = filterValue.toString().split(/[,-]/).map(v => parseFloat(v.trim()));
        if (range.length === 2) {
          return rowValue >= range[0] && rowValue <= range[1];
        }
        return true;
      default: return true;
    }
  }

  private evaluateSelectCondition(rowValue: any, filterValue: any, operator: string): boolean {
    switch (operator) {
      case 'equals': return rowValue === filterValue;
      case 'does_not_equal': return rowValue !== filterValue;
      default: return true;
    }
  }

  private evaluateDateCondition(rowValue: any, filterValue: any, operator: string): boolean {
    if (!rowValue || !filterValue) return false;

    const rowDate = new Date(rowValue);
    const filterDate = new Date(filterValue);

    switch (operator) {
      case 'equals': return rowDate.getTime() === filterDate.getTime();
      case 'does_not_equal': return rowDate.getTime() !== filterDate.getTime();
      case 'greater_than': return rowDate.getTime() > filterDate.getTime();
      case 'greater_than_or_equal': return rowDate.getTime() >= filterDate.getTime();
      case 'less_than': return rowDate.getTime() < filterDate.getTime();
      case 'less_than_or_equal': return rowDate.getTime() <= filterDate.getTime();
      default: return true;
    }
  }

  private evaluateBooleanCondition(rowValue: any, filterValue: any, operator: string): boolean {
    const rowBool = Boolean(rowValue);
    const filterBool = Boolean(filterValue);

    switch (operator) {
      case 'equals': return rowBool === filterBool;
      case 'does_not_equal': return rowBool !== filterBool;
      default: return true;
    }
  }

  // Helper Methods
  getOperatorsForColumn(columnKey: string): any[] {
    const columnType = this.getColumnType(columnKey);
    return this.operators[columnType as keyof typeof this.operators] || this.operators.text;
  }

  getColumnType(columnKey: string): string {
    const field = this.filterableFields.find(f => f.key === columnKey);
    return field?.type || 'text';
  }

  getOptionsForColumn(columnKey: string): any[] {
    const field = this.filterableFields.find(f => f.key === columnKey);
    return field?.options || [];
  }

  getInputType(condition: FilterCondition): string {
    return this.getColumnType(condition.field);
  }

  getPlaceholderForCondition(condition: FilterCondition): string {
    if (this.isValueDisabled(condition)) {
      return '';
    }

    const columnType = this.getColumnType(condition.field);
    const field = this.filterableFields.find(f => f.key === condition.field);

    if (field?.placeholder) {
      return field.placeholder;
    }

    switch (columnType) {
      case 'number':
        if (condition.operator === 'between') {
          return 'e.g., 100-500 or 100,500';
        }
        return 'Enter number';
      case 'text':
        return 'Enter text';
      case 'date':
        return 'Select date';
      default:
        return 'Enter value';
    }
  }

  isValueDisabled(condition: FilterCondition): boolean {
    return ['is_empty', 'is_not_empty'].includes(condition.operator);
  }

  isValueRequired(condition: FilterCondition): boolean {
    return !!(condition.operator && !this.isValueDisabled(condition));
  }

  isConditionComplete(condition: FilterCondition): boolean {
    return !!(condition.field && condition.operator &&
      (this.isValueDisabled(condition) || condition.value !== '' && condition.value !== null && condition.value !== undefined));
  }

  getConditionPreview(condition: FilterCondition): string {
    const field = this.filterableFields.find(f => f.key === condition.field);
    const fieldLabel = field?.label || condition.field;
    const operator = this.getOperatorsForColumn(condition.field)
      .find(op => op.value === condition.operator)?.label || condition.operator;

    if (this.isValueDisabled(condition)) {
      return `${fieldLabel} ${operator.toLowerCase()}`;
    }

    let displayValue = condition.value;
    if (field?.type === 'select') {
      const option = field.options?.find(opt => opt.value === condition.value);
      displayValue = option?.label || condition.value;
    }

    return `${fieldLabel} ${operator.toLowerCase()} "${displayValue}"`;
  }

  // Validation and State Methods
  hasAnyFilters(): boolean {
    return this.filterGroup.conditions.length > 0;
  }

  hasValidFilters(): boolean {
    return this.getValidConditions().length > 0;
  }

  getValidConditions(): FilterCondition[] {
    return this.filterGroup.conditions.filter(c => this.isConditionComplete(c));
  }

  getValidFilterCount(): number {
    return this.getValidConditions().length;
  }

  getActiveFilterCount(): number {
    return this.getValidFilterCount();
  }

  getFilterSummary(): string {
    const validConditions = this.getValidConditions();

    if (validConditions.length === 0) {
      return 'No active filters';
    }

    const summaries = validConditions.map(condition => this.getConditionPreview(condition));
    return summaries.join(` ${this.filterGroup.logicalOperator} `);
  }

  getFilterPercentage(): number {
    if (this.originalData.length === 0) return 0;
    return Math.round((this.filteredData.length / this.originalData.length) * 100);
  }

  // Export Functionality
  exportFilteredData(): void {
    this.exportRequested.emit(this.filteredData);
  }

  // Event Emitters
  private emitFilterChange(): void {
    const result: FilterResult = {
      filteredData: this.filteredData,
      filterGroup: { ...this.filterGroup },
      summary: this.getFilterSummary(),
      activeFilterCount: this.getActiveFilterCount()
    };

    this.filterChange.emit(result);
  }

  private emitFilterApplied(): void {
    const result: FilterResult = {
      filteredData: this.filteredData,
      filterGroup: { ...this.filterGroup },
      summary: this.getFilterSummary(),
      activeFilterCount: this.getActiveFilterCount()
    };

    this.filtersApplied.emit(result);
  }

  // Utility Methods
  trackByFilterId(index: number, condition: FilterCondition): string {
    return condition.id;
  }

  private generateFilterId(): string {
    return 'filter_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
  }

  // Public API Methods for Parent Components
  public getFilteredData(): any[] {
    return this.filteredData;
  }

  public getCurrentFilters(): FilterCondition[] {
    return this.filterGroup.conditions;
  }

  public setFilters(filters: FilterCondition[]): void {
    this.filterGroup.conditions = filters.map(filter => ({
      ...filter,
      id: filter.id || this.generateFilterId()
    }));

    if (this.autoApply) {
      this.applyFilters();
    }
  }

  public validateFilters(): boolean {
    this.showValidation = this.enableValidation;
    return this.hasValidFilters();
  }

  public resetFilters(): void {
    this.clearAllFilters();
  }
}
