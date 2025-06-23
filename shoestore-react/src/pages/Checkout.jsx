import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useOrder } from '../context/OrderContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import { paymentService } from '../services/paymentService';
import PaymentMethodSelector from '../components/PaymentMethodSelector';
import OrderSuccessModal from '../components/OrderSuccessModal';
import VoucherInput from '../components/VoucherInput';

const Checkout = () => {
  const navigate = useNavigate();
  const { cart, loading: cartLoading, clearCart } = useCart();
  const { createOrder, loading: orderLoading } = useOrder();
  const { user } = useAuth();
  const { addToast } = useToast();

  const [formData, setFormData] = useState({
    customerName: '',
    phone: '',
    email: '',
    address: '',
    paymentMethod: 0, // COD default
    customerNote: '',
  });

  // Voucher state
  const [appliedVoucher, setAppliedVoucher] = useState(null);
  const [voucherDiscount, setVoucherDiscount] = useState(0);

  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const hasRedirectedRef = useRef(false);
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [successOrder, setSuccessOrder] = useState(null);
  const orderPlacedRef = useRef(false);

  // Prefill form if user is logged in
  useEffect(() => {
    if (user) {
      setFormData(prev => ({
        ...prev,
        customerName: user.fullName || '',
        email: user.email || '',
        phone: user.phoneNumber || '',
      }));
    }
  }, [user]);

  const cartItems = cart?.items || [];
  
  // Check for empty cart and handle redirect (only if not just placed an order)
  useEffect(() => {
    // Only redirect if we've finished loading, cart is definitely empty, and haven't just placed an order
    if (!cartLoading && cart && cartItems.length === 0 && !hasRedirectedRef.current && !orderPlacedRef.current) {
      hasRedirectedRef.current = true;
      addToast('Giỏ hàng trống, vui lòng thêm sản phẩm trước khi thanh toán', 'warning');
      navigate('/cart');
    }
  }, [cartLoading, cart, cartItems.length, navigate]); // eslint-disable-line react-hooks/exhaustive-deps
  const subtotal = cartItems.reduce(
    (total, item) => {
      const effectivePrice = item.salePrice || item.price || 0;
      return total + effectivePrice * item.quantity;
    },
    0
  );

  const finalTotal = Math.max(0, subtotal - voucherDiscount);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  // Voucher handlers
  const handleVoucherApplied = (voucherResult) => {
    setAppliedVoucher(voucherResult.voucher);
    setVoucherDiscount(voucherResult.discountAmount);
    addToast(`Áp dụng mã giảm giá thành công! Giảm ${formatPrice(voucherResult.discountAmount)}`, 'success');
  };

  const handleVoucherRemoved = () => {
    setAppliedVoucher(null);
    setVoucherDiscount(0);
    addToast('Đã bỏ mã giảm giá', 'info');
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    // Convert paymentMethod to number for comparison
    const processedValue = name === 'paymentMethod' ? parseInt(value) : value;
    setFormData(prev => ({ ...prev, [name]: processedValue }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.customerName.trim()) {
      newErrors.customerName = 'Họ tên là bắt buộc';
    }

    if (!formData.phone.trim()) {
      newErrors.phone = 'Số điện thoại là bắt buộc';
    } else if (!/^[0-9]{10,11}$/.test(formData.phone)) {
      newErrors.phone = 'Số điện thoại không hợp lệ';
    }

    if (!formData.address.trim()) {
      newErrors.address = 'Địa chỉ là bắt buộc';
    }

    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Email không hợp lệ';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    if (cartItems.length === 0) {
      addToast('Giỏ hàng trống', 'error');
      return;
    }

    setIsSubmitting(true);

    try {
      const orderData = {
        customerName: formData.customerName,
        phone: formData.phone,
        email: formData.email || null,
        address: formData.address,
        paymentMethod: parseInt(formData.paymentMethod),
        customerNote: formData.customerNote || null,
        voucherCode: appliedVoucher?.code || null,
        items: cartItems.map(item => ({
          inventoryId: item.inventoryId,
          quantity: item.quantity
        }))
      };

      const result = await createOrder(orderData);
      
      if (result.success) {
        // Mark that order has been placed to prevent cart redirect
        orderPlacedRef.current = true;
        
        // Check if payment requires redirect (e-wallet payments)
        console.log('Full result:', result); // Debug log
        console.log('Payment result:', result.paymentResult); // Debug log
        
        // Check if payment requires redirect - VnPay/MoMo returns PaymentUrl when successful
        if (result.paymentResult && result.paymentResult.paymentUrl) {
          // For e-wallet payments, redirect to payment gateway
          console.log('Payment success:', result.paymentResult.success);
          console.log('Payment URL:', result.paymentResult.paymentUrl);
          console.log('Error message:', result.paymentResult.errorMessage);
          
          addToast('Đang chuyển đến trang thanh toán...', 'info');
          window.location.href = result.paymentResult.paymentUrl;
          return;
        } else if (result.paymentResult && result.paymentResult.errorMessage) {
          // Payment initiation failed
          const errorMsg = result.paymentResult.errorMessage || 'Khởi tạo thanh toán thất bại';
          console.error('Payment initiation failed:', errorMsg);
          addToast(errorMsg, 'error');
          return;
        }
        
        // For COD or completed payments, clear cart and show success
        await clearCart();
        
        // Show success modal with order details
        setSuccessOrder(result.order);
        setShowSuccessModal(true);
      } else {
        // Show error message
        addToast(result.error || 'Có lỗi xảy ra khi đặt hàng', 'error');
      }
    } catch (error) {
      console.error('Checkout error:', error);
      addToast('Có lỗi xảy ra khi đặt hàng', 'error');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Payment method change handler
  const handlePaymentMethodChange = (methodId) => {
    setFormData(prev => ({ ...prev, paymentMethod: methodId }));
  };

  if (cartLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold mb-8">Thanh toán</h1>

        <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Customer Information */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-lg border p-6 mb-6">
              <h2 className="text-xl font-semibold mb-4">Thông tin giao hàng</h2>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium mb-2">
                    Họ tên <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    name="customerName"
                    value={formData.customerName}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      errors.customerName ? 'border-red-500' : 'border-gray-300'
                    }`}
                    placeholder="Nhập họ tên"
                  />
                  {errors.customerName && (
                    <p className="text-red-500 text-sm mt-1">{errors.customerName}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium mb-2">
                    Số điện thoại <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="tel"
                    name="phone"
                    value={formData.phone}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      errors.phone ? 'border-red-500' : 'border-gray-300'
                    }`}
                    placeholder="Nhập số điện thoại"
                  />
                  {errors.phone && (
                    <p className="text-red-500 text-sm mt-1">{errors.phone}</p>
                  )}
                </div>

                <div className="md:col-span-2">
                  <label className="block text-sm font-medium mb-2">Email</label>
                  <input
                    type="email"
                    name="email"
                    value={formData.email}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      errors.email ? 'border-red-500' : 'border-gray-300'
                    }`}
                    placeholder="Nhập email (tùy chọn)"
                  />
                  {errors.email && (
                    <p className="text-red-500 text-sm mt-1">{errors.email}</p>
                  )}
                </div>

                <div className="md:col-span-2">
                  <label className="block text-sm font-medium mb-2">
                    Địa chỉ giao hàng <span className="text-red-500">*</span>
                  </label>
                  <textarea
                    name="address"
                    value={formData.address}
                    onChange={handleInputChange}
                    rows={3}
                    className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                      errors.address ? 'border-red-500' : 'border-gray-300'
                    }`}
                    placeholder="Nhập địa chỉ giao hàng chi tiết"
                  />
                  {errors.address && (
                    <p className="text-red-500 text-sm mt-1">{errors.address}</p>
                  )}
                </div>

                <div className="md:col-span-2">
                  <label className="block text-sm font-medium mb-2">Ghi chú</label>
                  <textarea
                    name="customerNote"
                    value={formData.customerNote}
                    onChange={handleInputChange}
                    rows={2}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="Ghi chú cho đơn hàng (tùy chọn)"
                  />
                </div>
              </div>
            </div>

            {/* Payment Methods */}
            <div className="bg-white rounded-lg border p-6">
              <PaymentMethodSelector
                selectedPaymentMethod={formData.paymentMethod}
                onPaymentMethodChange={handlePaymentMethodChange}
              />
            </div>
          </div>

          {/* Order Summary */}
          <div className="lg:col-span-1">
            <div className="bg-gray-50 rounded-lg p-6 sticky top-4">
              <h2 className="text-xl font-semibold mb-4">Tóm tắt đơn hàng</h2>
              
              {/* Voucher Input */}
              <VoucherInput
                orderAmount={subtotal}
                onVoucherApplied={handleVoucherApplied}
                onVoucherRemoved={handleVoucherRemoved}
                appliedVoucher={appliedVoucher}
                disabled={isSubmitting || orderLoading}
              />
              
              {/* Order Items */}
              <div className="space-y-4 mb-6">
                {cartItems.map((item) => (
                  <div key={item.inventoryId} className="flex gap-3">
                    <img
                      src={item.mainImage || '/images/defaults/product-default.jpg'}
                      alt={item.productName}
                      className="w-16 h-16 object-cover rounded"
                    />
                    <div className="flex-1">
                      <h3 className="font-medium text-sm">{item.productName}</h3>
                      <p className="text-gray-500 text-sm">Size: {item.size}</p>
                      <div className="flex justify-between items-center mt-1">
                        <span className="text-sm">x{item.quantity}</span>
                        <span className="font-medium">
                          {formatPrice((item.salePrice || item.price || 0) * item.quantity)}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              {/* Order Total */}
              <div className="border-t pt-4 space-y-2">
                <div className="flex justify-between">
                  <span>Tạm tính:</span>
                  <span>{formatPrice(subtotal)}</span>
                </div>
                {voucherDiscount > 0 && (
                  <div className="flex justify-between text-green-600">
                    <span>Giảm giá ({appliedVoucher?.code}):</span>
                    <span>-{formatPrice(voucherDiscount)}</span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span>Phí vận chuyển:</span>
                  <span className="text-green-600">Miễn phí</span>
                </div>
                <div className="flex justify-between text-lg font-bold border-t pt-2">
                  <span>Tổng cộng:</span>
                  <span className={voucherDiscount > 0 ? "text-green-600" : ""}>
                    {formatPrice(finalTotal)}
                  </span>
                </div>
                {voucherDiscount > 0 && subtotal !== finalTotal && (
                  <div className="text-sm text-gray-500 text-right">
                    <span className="line-through">{formatPrice(subtotal)}</span>
                    <span className="ml-2 text-green-600 font-medium">
                      Tiết kiệm {formatPrice(voucherDiscount)}
                    </span>
                  </div>
                )}
              </div>

              {/* Submit Button */}
              <button
                type="submit"
                disabled={isSubmitting || orderLoading || cartItems.length === 0}
                className="w-full mt-6 bg-black text-white py-3 rounded-lg font-medium hover:bg-gray-800 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSubmitting || orderLoading ? (
                  <div className="flex items-center justify-center">
                    <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
                    Đang xử lý...
                  </div>
                ) : (
                  'Đặt hàng'
                )}
              </button>
            </div>
          </div>
        </form>
      </div>

      {/* Success Modal */}
      <OrderSuccessModal
        isOpen={showSuccessModal}
        order={successOrder}
        onClose={() => {
          setShowSuccessModal(false);
          setSuccessOrder(null);
          navigate('/products'); // Navigate to products after closing modal
        }}
      />
    </div>
  );
};

export default Checkout; 