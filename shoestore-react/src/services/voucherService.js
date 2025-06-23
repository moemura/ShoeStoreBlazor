import { fetchApi } from './api';

class VoucherService {
    constructor() {
        this.baseURL = '/voucher';
    }

    // Public voucher operations (no auth required)
    async validateVoucher(voucherData) {
        try {
            const response = await fetchApi(`${this.baseURL}/validate`, {
                method: 'POST',
                body: JSON.stringify(voucherData)
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            return {
                success: false,
                error: error.message || 'Lỗi validate voucher'
            };
        }
    }

    async applyVoucher(voucherData) {
        try {
            const response = await fetchApi(`${this.baseURL}/apply`, {
                method: 'POST',
                body: JSON.stringify(voucherData)
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            return {
                success: false,
                error: error.message || 'Lỗi apply voucher'
            };
        }
    }

    async getActiveVouchers() {
        try {
            const response = await fetchApi(`${this.baseURL}/active`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            return {
                success: false,
                error: error.message || 'Lỗi tải danh sách voucher'
            };
        }
    }

    async canUseVoucher(code, guestId = null) {
        try {
            const queryParams = guestId ? `?guestId=${guestId}` : '';
            const response = await fetchApi(`${this.baseURL}/${code}/can-use${queryParams}`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            return {
                success: false,
                error: error.message || 'Lỗi kiểm tra voucher'
            };
        }
    }

    // User voucher operations (auth required)
    async getMyVoucherUsage(limit = 10) {
        try {
            const response = await fetchApi(`${this.baseURL}/my-usage?limit=${limit}`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            return {
                success: false,
                error: error.message || 'Lỗi tải lịch sử voucher'
            };
        }
    }

    // Helper methods for voucher validation
    validateVoucherCode(code) {
        if (!code || typeof code !== 'string') {
            return { isValid: false, message: 'Mã voucher không được để trống' };
        }

        const trimmedCode = code.trim().toUpperCase();
        if (trimmedCode.length < 3 || trimmedCode.length > 20) {
            return { isValid: false, message: 'Mã voucher phải từ 3-20 ký tự' };
        }

        if (!/^[A-Z0-9]+$/.test(trimmedCode)) {
            return { isValid: false, message: 'Mã voucher chỉ được chứa chữ cái và số' };
        }

        return { isValid: true, code: trimmedCode };
    }

    formatDiscountAmount(voucher, orderAmount) {
        if (!voucher) return 0;

        let discount = 0;
        if (voucher.type === 1) { // Percentage
            discount = orderAmount * (voucher.value / 100);
            if (voucher.maxDiscountAmount) {
                discount = Math.min(discount, voucher.maxDiscountAmount);
            }
        } else if (voucher.type === 2) { // Fixed amount
            discount = Math.min(voucher.value, orderAmount);
        }

        return Math.round(discount);
    }

    formatVoucherDisplay(voucher) {
        if (!voucher) return '';

        if (voucher.type === 1) { // Percentage
            let display = `Giảm ${voucher.value}%`;
            if (voucher.maxDiscountAmount) {
                display += ` (tối đa ${this.formatCurrency(voucher.maxDiscountAmount)})`;
            }
            return display;
        } else if (voucher.type === 2) { // Fixed amount
            return `Giảm ${this.formatCurrency(voucher.value)}`;
        }

        return '';
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    getVoucherErrorMessage(errorCode) {
        const errorMessages = {
            1: 'Mã voucher không tồn tại',
            2: 'Mã voucher đã hết hạn',
            3: 'Mã voucher chưa có hiệu lực',
            4: 'Mã voucher đã bị vô hiệu hóa',
            5: 'Mã voucher đã hết lượt sử dụng',
            6: 'Đơn hàng chưa đủ giá trị tối thiểu',
            7: 'Bạn đã sử dụng mã voucher này rồi',
            8: 'Mã voucher đã được áp dụng',
            9: 'Mã voucher không hợp lệ',
            10: 'Lỗi hệ thống'
        };

        return errorMessages[errorCode] || 'Lỗi không xác định';
    }

    isVoucherExpired(voucher) {
        if (!voucher) return true;
        const now = new Date();
        const endDate = new Date(voucher.endDate);
        return now > endDate;
    }

    isVoucherActive(voucher) {
        if (!voucher || !voucher.isActive) return false;
        
        const now = new Date();
        const startDate = new Date(voucher.startDate);
        const endDate = new Date(voucher.endDate);
        
        return now >= startDate && now <= endDate;
    }

    getVoucherStatus(voucher) {
        if (!voucher) return 'invalid';
        
        if (!voucher.isActive) return 'inactive';
        
        const now = new Date();
        const startDate = new Date(voucher.startDate);
        const endDate = new Date(voucher.endDate);
        
        if (now < startDate) return 'upcoming';
        if (now > endDate) return 'expired';
        if (voucher.usageLimit && voucher.usedCount >= voucher.usageLimit) return 'used_up';
        
        return 'active';
    }

    getStatusText(status) {
        const statusTexts = {
            active: 'Có hiệu lực',
            inactive: 'Ngừng hoạt động',
            upcoming: 'Chưa bắt đầu',
            expired: 'Hết hạn',
            used_up: 'Hết lượt sử dụng',
            invalid: 'Không hợp lệ'
        };

        return statusTexts[status] || 'Không xác định';
    }

    getStatusColor(status) {
        const statusColors = {
            active: 'text-green-600',
            inactive: 'text-gray-500',
            upcoming: 'text-yellow-600',
            expired: 'text-red-600',
            used_up: 'text-red-500',
            invalid: 'text-red-700'
        };

        return statusColors[status] || 'text-gray-500';
    }
}

export default new VoucherService(); 