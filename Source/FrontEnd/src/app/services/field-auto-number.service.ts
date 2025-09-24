import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface VoucherNumberResponse {
    success: boolean;
    data: string;
    message?: string;
    statusCode: number;
}

@Injectable({
    providedIn: 'root'
})
export class VoucherNumberService {
    constructor(private http: HttpClient) {}

    /**
     * Gọi API để lấy số voucher tiếp theo
     * @param controller Tên controller
     * @param field Tên field cần sinh số
     * @param formId Form ID (tùy chọn)
     * @returns Promise<string> Số voucher được sinh
     */
    async getNextVoucherNumber(controller: string, field: string, formId?: string): Promise<string> {
        try {
            const params = new HttpParams()
                .set('controller', controller)
                .set('field', field)
                .set('formId', formId || '');
            
            const response = await this.http.get<VoucherNumberResponse>(
                `${environment.apiUrl}/api/Dynamic/next-field-number`,
                { params }
            ).toPromise();
            
            if (response?.success && response?.data) {
                return response.data;
            }
            
            console.warn('API không trả về dữ liệu hợp lệ:', response);
            return '';
        } catch (error) {
            console.error('Lỗi khi gọi API sinh voucher number:', error);
            throw error;
        }
    }

    /**
     * Kiểm tra xem field có phải là voucher number field không
     * @param field Field configuration
     * @returns boolean
     */
    isVoucherNumberField(field: any): boolean {
        // Kiểm tra theo key name phổ biến
        const voucherFieldNames = [
            'voucherNumber', 'voucher_number', 'soChungTu', 'so_chung_tu',
            'documentNumber', 'document_number', 'maChungTu', 'ma_chung_tu',
            'billNumber', 'bill_number', 'orderNumber', 'order_number',
            'receiptNumber', 'receipt_number', 'invoiceNumber', 'invoice_number'
        ];
        
        const isVoucherByName = voucherFieldNames.includes(field.key?.toLowerCase());
        
        // Kiểm tra theo autoGenerate flag trong config
        const hasAutoGenerate = field.autoGenerate === true;
        
        // Kiểm tra theo type và pattern
        const isTextFieldWithNumberPattern = field.type === 'text' && 
            (field.pattern || '').includes('number');
        
        return isVoucherByName || hasAutoGenerate || isTextFieldWithNumberPattern;
    }

    /**
     * Tạo voucher number theo format tùy chỉnh
     * @param prefix Tiền tố (VD: BG, HD, PX)
     * @param date Ngày tạo
     * @param sequence Số thứ tự
     * @returns string Voucher number đã format
     */
    formatVoucherNumber(prefix: string, date: Date, sequence: number): string {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const seq = String(sequence).padStart(4, '0');
        
        return `${prefix}${year}${month}${day}${seq}`;
    }

    /**
     * Parse voucher number để lấy thông tin
     * @param voucherNumber Voucher number cần parse
     * @returns object Thông tin parsed
     */
    parseVoucherNumber(voucherNumber: string): {
        prefix: string;
        year: number;
        month: number;
        day: number;
        sequence: number;
    } | null {
        // Pattern: PREFIX + YYYYMMDD + SEQUENCE
        const match = voucherNumber.match(/^([A-Z]+)(\d{4})(\d{2})(\d{2})(\d+)$/);
        
        if (!match) {
            return null;
        }
        
        return {
            prefix: match[1],
            year: parseInt(match[2]),
            month: parseInt(match[3]),
            day: parseInt(match[4]),
            sequence: parseInt(match[5])
        };
    }
}
