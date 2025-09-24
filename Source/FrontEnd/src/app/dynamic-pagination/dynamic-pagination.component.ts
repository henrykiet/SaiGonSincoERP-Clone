import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
@Component({
    selector: 'app-dynamic-pagination',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './dynamic-pagination.component.html',
})
export class DynamicPaginationComponent {
    @Input() totalItems = 0;
    @Input() itemsPerPage = 10;
    @Input() currentPage = 1;
    @Output() pageChange = new EventEmitter<number>();

    get totalPages(): number {
        return Math.ceil(this.totalItems / this.itemsPerPage);
    }

    getPaginationRange(): number[] {
        const range: number[] = [];
        const delta = 2;

        if (this.totalPages <= 1) {
            return [1];
        }

        if (this.totalPages <= 5 || this.currentPage < 4) {
            const end = Math.min(this.totalPages, 5);
            for (let i = 1; i <= end; i++) {
                range.push(i);
            }
        } else {
            for (
                let i = Math.max(1, this.currentPage - delta);
                i <= Math.min(this.totalPages, this.currentPage + delta);
                i++
            ) {
                range.push(i);
            }
        }

        return range;
    }

    handlePageClick(page: number): void {
        if (page !== this.currentPage) {
            this.pageChange.emit(page);
        }
    }
}
