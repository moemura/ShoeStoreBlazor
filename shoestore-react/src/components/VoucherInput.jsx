import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import voucherService from '../services/voucherService';

const VoucherInput = ({ 
    orderAmount, 
    onVoucherApplied, 
    onVoucherRemoved, 
    appliedVoucher,
    disabled = false 
}) => {
    const { user } = useAuth();
    const [voucherCode, setVoucherCode] = useState('');
    const [isValidating, setIsValidating] = useState(false);
    const [validationResult, setValidationResult] = useState(null);
    const [error, setError] = useState('');
    const [activeVouchers, setActiveVouchers] = useState([]);
    const [showSuggestions, setShowSuggestions] = useState(false);

    // Load active vouchers on component mount
    useEffect(() => {
        loadActiveVouchers();
    }, []);

    const loadActiveVouchers = async () => {
        const result = await voucherService.getActiveVouchers();
        if (result.success) {
            setActiveVouchers(result.data || []);
        }
    };

    const handleVoucherCodeChange = (e) => {
        const value = e.target.value.toUpperCase();
        setVoucherCode(value);
        setError('');
        setValidationResult(null);
        
        // Clear applied voucher if user changes code
        if (appliedVoucher && value !== appliedVoucher.code) {
            onVoucherRemoved?.();
        }
    };

    const validateAndApplyVoucher = async () => {
        if (!voucherCode.trim()) {
            setError('Vui lòng nhập mã voucher');
            return;
        }

        // Client-side validation
        const codeValidation = voucherService.validateVoucherCode(voucherCode);
        if (!codeValidation.isValid) {
            setError(codeValidation.message);
            return;
        }

        setIsValidating(true);
        setError('');

        try {
            // Get guest ID from localStorage if not logged in
            const guestId = user ? null : localStorage.getItem('guestId');
            
            const voucherData = {
                code: codeValidation.code,
                orderAmount: orderAmount,
                userId: user?.id || null,
                guestId: guestId
            };

            const result = await voucherService.validateVoucher(voucherData);

            if (result.success && result.data.isValid) {
                setValidationResult(result.data);
                onVoucherApplied?.(result.data);
                setShowSuggestions(false);
            } else {
                const errorMessage = result.data?.errorCode 
                    ? voucherService.getVoucherErrorMessage(result.data.errorCode)
                    : result.error || 'Mã voucher không hợp lệ';
                setError(errorMessage);
                setValidationResult(null);
            }
        } catch (err) {
            setError('Lỗi khi kiểm tra mã voucher. Vui lòng thử lại.');
            setValidationResult(null);
        } finally {
            setIsValidating(false);
        }
    };

    const handleRemoveVoucher = () => {
        setVoucherCode('');
        setValidationResult(null);
        setError('');
        onVoucherRemoved?.();
    };

    const handleKeyPress = (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            validateAndApplyVoucher();
        }
    };

    const selectSuggestedVoucher = async (voucher) => {
        // Prevent multiple clicks
        if (isValidating) return;
        
        setVoucherCode(voucher.code);
        setShowSuggestions(false);
        setError('');
        setValidationResult(null);
        
        // Auto-validate immediately when selecting from suggestions
        setIsValidating(true);

        try {
            // Get guest ID from localStorage if not logged in
            const guestId = user ? null : localStorage.getItem('guestId');
            
            const voucherData = {
                code: voucher.code,
                orderAmount: orderAmount,
                userId: user?.id || null,
                guestId: guestId
            };

            const result = await voucherService.validateVoucher(voucherData);

            if (result.success && result.data.isValid) {
                setValidationResult(result.data);
                onVoucherApplied?.(result.data);
            } else {
                const errorMessage = result.data?.errorCode 
                    ? voucherService.getVoucherErrorMessage(result.data.errorCode)
                    : result.error || 'Mã voucher không hợp lệ';
                setError(errorMessage);
                setValidationResult(null);
            }
        } catch (err) {
            setError('Lỗi khi kiểm tra mã voucher. Vui lòng thử lại.');
            setValidationResult(null);
        } finally {
            setIsValidating(false);
        }
    };

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    };

    return (
        <div className="voucher-input-container">
            {/* Voucher Input Section */}
            <div className="bg-gray-50 rounded-lg p-4 mb-4">
                <div className="flex items-center gap-2 mb-3">
                    <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                              d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.99 1.99 0 013 12V7a4 4 0 014-4z" />
                    </svg>
                    <h3 className="text-lg font-semibold text-gray-800">Mã giảm giá</h3>
                </div>

                {/* Applied Voucher Display */}
                {appliedVoucher && validationResult ? (
                    <div className="bg-green-50 border border-green-200 rounded-lg p-4 mb-3">
                        <div className="flex items-center justify-between">
                            <div className="flex-1">
                                <div className="flex items-center gap-2 mb-2">
                                    <span className="bg-green-600 text-white px-3 py-1 rounded-full text-sm font-medium">
                                        {appliedVoucher.code}
                                    </span>
                                    <span className="text-green-700 font-medium">
                                        {voucherService.formatVoucherDisplay(appliedVoucher)}
                                    </span>
                                </div>
                                {appliedVoucher.description && (
                                    <p className="text-gray-600 text-sm mb-2">{appliedVoucher.description}</p>
                                )}
                                <div className="text-sm text-gray-600">
                                    <span>Giảm: </span>
                                    <span className="font-bold text-green-600">
                                        {formatCurrency(validationResult.discountAmount)}
                                    </span>
                                    <span className="ml-2">Tổng sau giảm: </span>
                                    <span className="font-bold text-green-600">
                                        {formatCurrency(validationResult.finalAmount)}
                                    </span>
                                </div>
                            </div>
                            <button
                                onClick={handleRemoveVoucher}
                                className="ml-4 text-red-600 hover:text-red-800 transition-colors"
                                title="Bỏ mã giảm giá"
                            >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>
                    </div>
                ) : (
                    /* Voucher Input Form */
                    <div className="space-y-3">
                        <div className="flex gap-2">
                            <div className="flex-1 relative">
                                <input
                                    type="text"
                                    value={voucherCode}
                                    onChange={handleVoucherCodeChange}
                                    onKeyPress={handleKeyPress}
                                    onFocus={() => setShowSuggestions(true)}
                                    placeholder="Nhập mã giảm giá"
                                    className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${
                                        error ? 'border-red-500' : 'border-gray-300'
                                    } ${disabled ? 'bg-gray-100 cursor-not-allowed' : ''}`}
                                    disabled={disabled || isValidating}
                                    maxLength={20}
                                />
                                
                                {/* Suggestions Dropdown */}
                                {showSuggestions && activeVouchers.length > 0 && !appliedVoucher && (
                                    <div className="absolute top-full left-0 right-0 bg-white border border-gray-200 rounded-lg shadow-lg z-10 mt-1 max-h-60 overflow-y-auto">
                                        <div className="p-2 border-b bg-gray-50">
                                            <p className="text-sm text-gray-600 font-medium">Mã giảm giá khả dụng:</p>
                                        </div>
                                        {activeVouchers.map((voucher) => {
                                            const status = voucherService.getVoucherStatus(voucher);
                                            const isUsable = status === 'active';
                                            
                                            return (
                                                <div
                                                    key={voucher.code}
                                                    onClick={() => isUsable && !isValidating && selectSuggestedVoucher(voucher)}
                                                    className={`p-3 border-b border-gray-100 last:border-b-0 transition-colors ${
                                                        isUsable && !isValidating 
                                                            ? 'hover:bg-blue-50 cursor-pointer active:bg-blue-100' 
                                                            : 'opacity-50 cursor-not-allowed'
                                                    } ${isValidating && voucherCode === voucher.code ? 'bg-blue-50' : ''}`}
                                                >
                                                    <div className="flex items-center justify-between">
                                                        <div className="flex-1">
                                                            <div className="flex items-center gap-2 mb-1">
                                                                <span className={`px-2 py-1 rounded text-xs font-medium ${
                                                                    isUsable ? 'bg-blue-100 text-blue-800' : 'bg-gray-100 text-gray-600'
                                                                }`}>
                                                                    {voucher.code}
                                                                </span>
                                                                <span className={`text-sm ${voucherService.getStatusColor(status)}`}>
                                                                    {voucherService.getStatusText(status)}
                                                                </span>
                                                            </div>
                                                            <p className="text-sm text-gray-600">
                                                                {voucherService.formatVoucherDisplay(voucher)}
                                                            </p>
                                                            {voucher.minOrderAmount && (
                                                                <p className="text-xs text-gray-500">
                                                                    Đơn tối thiểu: {formatCurrency(voucher.minOrderAmount)}
                                                                </p>
                                                            )}
                                                        </div>
                                                        {isValidating && voucherCode === voucher.code ? (
                                                            <div className="w-4 h-4 border-2 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
                                                        ) : isUsable ? (
                                                            <svg className="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                                            </svg>
                                                        ) : null}
                                                    </div>
                                                </div>
                                            );
                                        })}
                                    </div>
                                )}
                            </div>
                            
                            <button
                                onClick={validateAndApplyVoucher}
                                disabled={disabled || isValidating || !voucherCode.trim()}
                                className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
                            >
                                {isValidating ? (
                                    <>
                                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                                        <span>Kiểm tra</span>
                                    </>
                                ) : (
                                    <>
                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                                        </svg>
                                        <span>Áp dụng</span>
                                    </>
                                )}
                            </button>
                        </div>

                        {/* Error Message */}
                        {error && (
                            <div className="flex items-center gap-2 text-red-600 text-sm">
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                          d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                <span>{error}</span>
                            </div>
                        )}

                        {/* Hide suggestions when clicking outside */}
                        {showSuggestions && (
                            <div 
                                className="fixed inset-0 z-0" 
                                onClick={() => setShowSuggestions(false)}
                            ></div>
                        )}
                    </div>
                )}
            </div>

            {/* Active Vouchers Preview (when no voucher applied) */}
            {!appliedVoucher && activeVouchers.length > 0 && (
                <div className="mb-4">
                    <p className="text-sm text-gray-600 mb-2">Mã giảm giá khả dụng:</p>
                    <div className="flex flex-wrap gap-2">
                        {activeVouchers.slice(0, 3).map((voucher) => {
                            const status = voucherService.getVoucherStatus(voucher);
                            const isUsable = status === 'active';
                            
                            return (
                                <button
                                    key={voucher.code}
                                    onClick={() => isUsable && !isValidating && selectSuggestedVoucher(voucher)}
                                    disabled={!isUsable || isValidating}
                                    className={`px-3 py-1 text-xs rounded-full border transition-colors relative ${
                                        isUsable && !isValidating
                                            ? 'border-blue-200 text-blue-700 hover:bg-blue-50 cursor-pointer active:bg-blue-100' 
                                            : 'border-gray-200 text-gray-500 cursor-not-allowed opacity-50'
                                    }`}
                                >
                                    {isValidating && voucherCode === voucher.code ? (
                                        <div className="flex items-center gap-1">
                                            <div className="w-3 h-3 border border-blue-600 border-t-transparent rounded-full animate-spin"></div>
                                            <span>{voucher.code}</span>
                                        </div>
                                    ) : (
                                        voucher.code
                                    )}
                                </button>
                            );
                        })}
                        {activeVouchers.length > 3 && (
                            <span className="px-3 py-1 text-xs text-gray-500 border border-gray-200 rounded-full">
                                +{activeVouchers.length - 3} khác
                            </span>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};

export default VoucherInput; 