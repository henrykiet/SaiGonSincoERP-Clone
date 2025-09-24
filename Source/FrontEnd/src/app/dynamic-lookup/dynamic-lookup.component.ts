import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LookupApiQuery, LookupApiResponse } from '../models';
import { environment } from '../../environments/environment';

@Component({
  standalone: true,
  selector: 'app-lookup',
  templateUrl: './dynamic-lookup.component.html',
  imports: [CommonModule, FormsModule],
})
export class DynamicLookupComponent implements OnInit, OnChanges {
  @Input() multiple: boolean = true;
  @Input() query!: LookupApiQuery;
  @Input() value: any;
  @Output() valueChange = new EventEmitter<any>();

  showPopup = false;
  response!: LookupApiResponse;

  selectedItem: any = null;
  selectedItems: any[] = [];

  searchText = '';
  searchName = '';
  filteredData: any[] = [];
  paginatedData: any[] = [];
  pageSize = 10;
  currentPage = 0;
  get totalPages() {
    return Math.ceil(this.filteredData.length / this.pageSize);
  }

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.fetchData();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['value'] && !changes['value'].firstChange) {
      // Update internal selected state without emitting event
      if (this.response) {
        if (this.multiple) {
          this.selectedItems = Array.isArray(changes['value'].currentValue)
            ? changes['value'].currentValue
            : [];
        } else {
          this.selectedItem = changes['value'].currentValue;
        }
      } else {
        console.log('Response not ready yet, will update when fetchData completes');
      }
    }
  }

  fetchData() {
    this.http
      .post<any>(`${environment.apiUrl}/api/Lookup`, this.query)
      .subscribe((res) => {

        this.response = res.data as LookupApiResponse;
        this.filteredData = this.response.datas;
        this.setPage(0);

        // Initialize selected state without emitting
        if (this.value) {
          if (this.multiple) {
            this.selectedItems = Array.isArray(this.value) ? this.value : [];
          } else {
            this.selectedItem = this.value;
          }
        }
      });
  }

  togglePopup() {
    this.showPopup = !this.showPopup;
  }

  filterData() {
    const searchText = this.searchText.toLowerCase();
    const searchName = this.searchName.toLowerCase();
    this.filteredData = this.response.datas.filter((data) => {
      const values = Object.values(data);
      let checkName = true
      if (values.length >= 2) {
        checkName = String(values[1]).toLowerCase().includes(searchName);
      }

      return values.some((val) =>
        String(val).toLowerCase().includes(searchText)
      ) && checkName;
    }

    );
    this.setPage(0);
  }

  setPage(page: number) {
    this.currentPage = page;
    const start = page * this.pageSize;
    this.paginatedData = this.filteredData.slice(start, start + this.pageSize);
  }

  nextPage() {
    if (this.currentPage + 1 < this.totalPages)
      this.setPage(this.currentPage + 1);
  }
  prevPage() {
    if (this.currentPage > 0) this.setPage(this.currentPage - 1);
  }

  selectData(prikey: string) {
    if (this.multiple) {
      const exists = this.selectedItems.some((item) => item === prikey);
      if (exists) {
        this.selectedItems = this.selectedItems.filter(
          (item) => item !== prikey
        );
      } else {
        this.selectedItems.push(prikey);
      }
      this.valueChange.emit(this.selectedItems);
    } else {
      this.selectedItem = prikey;

      this.valueChange.emit(this.selectedItem);
      this.showPopup = false;
    }
  }

  isSelected(data: any): boolean {
    return this.multiple
      ? this.selectedItems.includes(data)
      : this.selectedItem === data;
  }

  getDisplayText(): string {
    if (!this.response) {
      console.log('getDisplayText: No response data yet');
      return '';
    }


    if (this.multiple) {
      const displayText = this.response.datas
        .filter((data) =>
          this.selectedItems.includes(data[this.response.fields[0].field])
        )
        .map((item) =>
          this.response.fields.map((f) => item[f.field]).join(' - ')
        )
        .join(', ');

      return displayText;
    } else {
      const foundItem = this.response.datas.find(
        (data) => data[this.response.fields[0].field] == this.selectedItem
      );


      const displayText = Object.values(foundItem ?? []).join(' - ');
      return displayText;
    }
  }
}
