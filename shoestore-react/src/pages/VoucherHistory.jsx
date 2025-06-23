import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import voucherService from '../services/voucherService';
import LoadingSpinner from '../components/LoadingSpinner';

const VoucherHistory = () => {
    const { user } = useAuth();
    const { addToast } = useToast();
    const [voucherHistory, setVoucherHistory] = useState([]);
    const [activeVouchers, setActiveVouchers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState('available'); // 'available' or 'used'

    useEffect(() => {
        if (user) {
            loadData();
        }
    }, [user]);

    const loadData = async () => {
        setLoading(true);
        try {
            // Load both active vouchers and user's usage history
            const [activeResult, historyResult] = await Promise.all([
                voucherService.getActiveVouchers(),
                voucherService.getMyVoucherUsage(20)
            ]);

            if (activeResult.success) {
                setActiveVouchers(activeResult.data || []);
            }

            if (historyResult.success) {
                setVoucherHistory(historyResult.data || []);
            } else if (historyResult.error) {
                addToast('Không thể tải lịch sử voucher: ' + historyResult.error, 'error');
            }
        } catch (error) {
            console.error('Error loading voucher data:', error);
            addToast('Lỗi khi tải dữ liệu voucher', 'error');
        } finally {
            setLoading(false);
        }
    };

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    };

    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const copyVoucherCode = (code) => {
        navigator.clipboard.writeText(code).then(() => {
            addToast(`Đã sao chép mã ${code}`, 'success');
        }).catch(() => {
            addToast('Không thể sao chép mã voucher', 'error');
        });
    };

    if (!user) {
        return (
            <div className="container mx-auto px-4 py-8">
                <div className="text-center py-12">
                    <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} 
                              d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    <h2 className="text-xl font-semibold text-gray-600 mb-2">Cần đăng nhập</h2>
                    <p className="text-gray-500">Vui lòng đăng nhập để xem lịch sử voucher</p>
                </div>
            </div>
        );
    }

    if (loading) {
        return (
            <div className="container mx-auto px-4 py-8">
                <LoadingSpinner />
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8 max-w-4xl">
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-gray-800 mb-2">Mã giảm giá</h1>
                <p className="text-gray-600">Quản lý và theo dõi mã giảm giá của bạn</p>
            </div>

            {/* Tab Navigation */}
            <div className="flex mb-6 bg-gray-100 rounded-lg p-1">
                <button
                    onClick={() => setActiveTab('available')}
                    className={`flex-1 py-2 px-4 rounded-md text-center font-medium transition-colors ${
                        activeTab === 'available'
                            ? 'bg-white text-blue-600 shadow-sm'
                            : 'text-gray-600 hover:text-gray-800'
                    }`}
                >
                    <div className="flex items-center justify-center gap-2">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                  d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.99 1.99 0 013 12V7a4 4 0 014-4z" />
                        </svg>
                        <span>Khả dụng ({activeVouchers.length})</span>
                    </div>
                </button>
                <button
                    onClick={() => setActiveTab('used')}
                    className={`flex-1 py-2 px-4 rounded-md text-center font-medium transition-colors ${
                        activeTab === 'used'
                            ? 'bg-white text-blue-600 shadow-sm'
                            : 'text-gray-600 hover:text-gray-800'
                    }`}
                >
                    <div className="flex items-center justify-center gap-2">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                  d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                        <span>Đã sử dụng ({voucherHistory.length})</span>
                    </div>
                </button>
            </div>

            {/* Available Vouchers Tab */}
            {activeTab === 'available' && (
                <div>
                    {activeVouchers.length > 0 ? (
                        <div className="space-y-4">
                            {activeVouchers.map((voucher) => {
                                const status = voucherService.getVoucherStatus(voucher);
                                const isUsable = status === 'active';
                                
                                return (
                                    <div
                                        key={voucher.code}
                                        className={`bg-white border rounded-lg p-6 transition-all hover:shadow-md ${
                                            isUsable ? 'border-green-200 hover:border-green-300' : 'border-gray-200 opacity-75'
                                        }`}
                                    >
                                        <div className="flex items-start justify-between">
                                            <div className="flex-1">
                                                <div className="flex items-center gap-3 mb-3">
                                                    <div
                                                        onClick={() => copyVoucherCode(voucher.code)}
                                                        className={`px-4 py-2 rounded-lg font-mono font-bold text-lg cursor-pointer transition-colors ${
                                                            isUsable 
                                                                ? 'bg-blue-100 text-blue-700 hover:bg-blue-200' 
                                                                : 'bg-gray-100 text-gray-500'
                                                        }`}
                                                        title="Click để sao chép"
                                                    >
                                                        {voucher.code}
                                                    </div>
                                                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                                                        isUsable 
                                                            ? 'bg-green-100 text-green-700' 
                                                            : 'bg-gray-100 text-gray-500'
                                                    }`}>
                                                        {voucherService.getStatusText(status)}
                                                    </span>
                                                </div>

                                                <h3 className="font-semibold text-lg text-gray-800 mb-2">
                                                    {voucherService.formatVoucherDisplay(voucher)}
                                                </h3>

                                                {voucher.description && (
                                                    <p className="text-gray-600 mb-3">{voucher.description}</p>
                                                )}

                                                <div className="grid grid-cols-2 gap-4 text-sm text-gray-600">
                                                    {voucher.minOrderAmount && (
                                                        <div className="flex items-center gap-2">
                                                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                                                      d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                                                            </svg>
                                                            <span>Đơn tối thiểu: {formatCurrency(voucher.minOrderAmount)}</span>
                                                        </div>
                                                    )}
                                                    
                                                    <div className="flex items-center gap-2">
                                                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                                                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                                        </svg>
                                                        <span>Hết hạn: {formatDate(voucher.endDate)}</span>
                                                    </div>

                                                    {voucher.usageLimit && (
                                                        <div className="flex items-center gap-2">
                                                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                                                      d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                                            </svg>
                                                            <span>Còn lại: {voucher.usageLimit - voucher.usedCount} lượt</span>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>

                                            <div className="ml-4">
                                                <button
                                                    onClick={() => copyVoucherCode(voucher.code)}
                                                    disabled={!isUsable}
                                                    className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                                                        isUsable
                                                            ? 'bg-blue-600 text-white hover:bg-blue-700'
                                                            : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                                                    }`}
                                                >
                                                    Sao chép
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    ) : (
                        <div className="text-center py-12">
                            <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} 
                                      d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.99 1.99 0 013 12V7a4 4 0 014-4z" />
                            </svg>
                            <h3 className="text-lg font-semibold text-gray-600 mb-2">Chưa có mã giảm giá</h3>
                            <p className="text-gray-500">Hiện tại không có mã giảm giá nào khả dụng</p>
                        </div>
                    )}
                </div>
            )}

            {/* Used Vouchers Tab */}
            {activeTab === 'used' && (
                <div>
                    {voucherHistory.length > 0 ? (
                        <div className="space-y-4">
                            {voucherHistory.map((usage) => (
                                <div key={usage.id} className="bg-white border border-gray-200 rounded-lg p-6">
                                    <div className="flex items-start justify-between">
                                        <div className="flex-1">
                                            <div className="flex items-center gap-3 mb-2">
                                                <span className="px-3 py-1 bg-gray-100 text-gray-700 rounded-lg font-mono font-bold">
                                                    {usage.voucherCode}
                                                </span>
                                                <span className="px-2 py-1 bg-green-100 text-green-700 rounded-full text-xs font-medium">
                                                    Đã sử dụng
                                                </span>
                                            </div>

                                            <div className="grid grid-cols-2 gap-4 text-sm text-gray-600">
                                                <div className="flex items-center gap-2">
                                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                                              d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                                                    </svg>
                                                    <span>Giảm: {formatCurrency(usage.discountAmount)}</span>
                                                </div>

                                                <div className="flex items-center gap-2">
                                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                                              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                                    </svg>
                                                    <span>Ngày sử dụng: {formatDate(usage.createdAt)}</span>
                                                </div>

                                                <div className="flex items-center gap-2">
                                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                                              d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                                                    </svg>
                                                    <span>Đơn hàng: {usage.orderId}</span>
                                                </div>

                                                <div className="flex items-center gap-2">
                                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                                              d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
                                                    </svg>
                                                    <span>Tổng bill: {formatCurrency(usage.finalAmount)}</span>
                                                </div>
                                            </div>
                                        </div>

                                        <div className="ml-4 text-right">
                                            <div className="text-lg font-bold text-green-600">
                                                -{formatCurrency(usage.discountAmount)}
                                            </div>
                                            <div className="text-sm text-gray-500">
                                                Tiết kiệm
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="text-center py-12">
                            <svg className="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} 
                                      d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                            </svg>
                            <h3 className="text-lg font-semibold text-gray-600 mb-2">Chưa sử dụng voucher nào</h3>
                            <p className="text-gray-500">Lịch sử sử dụng mã giảm giá sẽ hiển thị ở đây</p>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default VoucherHistory; 