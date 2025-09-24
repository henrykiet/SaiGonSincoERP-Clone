import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms';

@Component({
    standalone: true,
    selector: 'app-taxcode-input',
    templateUrl: './taxcode-input.component.html',
    imports: [CommonModule, FormsModule]
})
export class TaxcodeInputComponent {
    @Input() value: string = '';
    @Input() required: boolean = false;
    errorMessage: string | null = null;
    @Output() valueChange = new EventEmitter<string>();

    showPopup = false;
    taxInfo: any = null;

    constructor(private http: HttpClient) { }

    onValueChange(newValue: string) {
        this.value = newValue;
        this.valueChange.emit(this.value);
    }

    validateTaxCode() {
        if (!this.value) {
            alert('Vui lòng nhập mã số thuế');
            return;
        }

        this.http.get(`https://api.vietqr.io/v2/business/${this.value}`).subscribe({
            next: (res: any) => {
                if (res && res.code === "00" && res.data) {
                    // ✅ Thành công
                    this.taxInfo = {
                        taxCode: res.data.id,
                        companyName: res.data.name,
                        internationalName: res.data.internationalName,
                        shortName: res.data.shortName,
                        address: res.data.address,
                    };
                    this.errorMessage = null;
                } else {
                    // ❌ Lỗi từ API (ví dụ code = 51)
                    this.taxInfo = null;
                    this.errorMessage = res?.desc || "Không tìm thấy thông tin MST";
                }
                this.showPopup = true;
            },
            error: () => {
                this.taxInfo = null;
                this.errorMessage = "Không thể kết nối đến dịch vụ tra cứu. Vui lòng thử lại sau.";
                this.showPopup = true;
            }
        });
    }

    closePopup() {
        this.showPopup = false;
    }
}
