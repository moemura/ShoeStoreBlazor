import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { paymentService } from '../services/paymentService';
import orderService from '../services/orderService';
import LoadingSpinner from '../components/LoadingSpinner';

const PaymentResult = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [paymentResult, setPaymentResult] = useState(null);
  const [orderDetails, setOrderDetails] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    const processPaymentResult = async () => {
      try {
        setLoading(true);
        
        // Process payment result from URL parameters
        const result = paymentService.processPaymentResult(searchParams.toString());
        setPaymentResult(result);

        // Get order details if orderId is available
        if (result.orderId) {
          try {
            const order = await orderService.getOrderById(result.orderId);
            setOrderDetails(order);
          } catch (orderError) {
            console.error('Error getting order details:', orderError);
            // Continue even if order details fail
          }
        }

        // Get payment transaction details if transactionId is available
        if (result.transactionId) {
          try {
            const transaction = await paymentService.getPaymentTransaction(result.transactionId);
            console.log('Payment transaction:', transaction);
          } catch (transactionError) {
            console.error('Error getting payment transaction:', transactionError);
            // Continue even if transaction details fail
          }
        }

      } catch (err) {
        console.error('Error processing payment result:', err);
        setError('Không thể xử lý kết quả thanh toán. Vui lòng thử lại.');
      } finally {
        setLoading(false);
      }
    };

    processPaymentResult();
  }, [searchParams]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="max-w-md mx-auto text-center">
          <div className="text-6xl mb-4">❌</div>
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Có lỗi xảy ra</h1>
          <p className="text-gray-600 mb-6">{error}</p>
          <button
            onClick={() => navigate('/')}
            className="bg-blue-500 text-white px-6 py-2 rounded-lg hover:bg-blue-600 transition-colors"
          >
            Về trang chủ
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <div className="bg-white rounded-lg shadow-lg p-8">
          {/* Payment Result Header */}
          <div className="text-center mb-8">
            <div className="text-8xl mb-4">
              {paymentResult?.success ? '✅' : '❌'}
            </div>
            <h1 className={`text-3xl font-bold mb-2 ${
              paymentResult?.success ? 'text-green-600' : 'text-red-600'
            }`}>
              {paymentResult?.success ? 'Thanh toán thành công!' : 'Thanh toán không thành công'}
            </h1>
            <p className="text-gray-600 text-lg">
              {paymentResult?.message}
            </p>
          </div>

          {/* Payment Details */}
          <div className="bg-gray-50 rounded-lg p-6 mb-6">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">
              Chi tiết giao dịch
            </h3>
            <div className="space-y-3">
              {paymentResult?.transactionId && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Mã giao dịch:</span>
                  <span className="font-mono text-sm bg-gray-200 px-2 py-1 rounded">
                    {paymentResult.transactionId}
                  </span>
                </div>
              )}
              {paymentResult?.orderId && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Mã đơn hàng:</span>
                  <span className="font-mono text-sm bg-gray-200 px-2 py-1 rounded">
                    {paymentResult.orderId}
                  </span>
                </div>
              )}
              <div className="flex justify-between">
                <span className="text-gray-600">Thời gian:</span>
                <span>{new Date().toLocaleString('vi-VN')}</span>
              </div>
            </div>
          </div>

          {/* Order Details */}
          {orderDetails && (
            <div className="bg-gray-50 rounded-lg p-6 mb-6">
              <h3 className="text-lg font-semibold text-gray-800 mb-4">
                Thông tin đơn hàng
              </h3>
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-gray-600">Người nhận:</span>
                  <span>{orderDetails.customerName}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Số điện thoại:</span>
                  <span>{orderDetails.phone}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Địa chỉ:</span>
                  <span className="text-right max-w-xs">{orderDetails.address}</span>
                </div>
                <div className="flex justify-between font-semibold text-lg">
                  <span className="text-gray-600">Tổng tiền:</span>
                  <span className="text-red-600">
                    {orderDetails.totalAmount?.toLocaleString('vi-VN')}₫
                  </span>
                </div>
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            {paymentResult?.success ? (
              <>
                <Link
                  to="/orders"
                  className="bg-blue-500 text-white px-6 py-3 rounded-lg hover:bg-blue-600 transition-colors text-center"
                >
                  Xem đơn hàng của tôi
                </Link>
                <Link
                  to="/"
                  className="bg-gray-500 text-white px-6 py-3 rounded-lg hover:bg-gray-600 transition-colors text-center"
                >
                  Tiếp tục mua sắm
                </Link>
              </>
            ) : (
              <>
                <button
                  onClick={() => navigate('/cart')}
                  className="bg-blue-500 text-white px-6 py-3 rounded-lg hover:bg-blue-600 transition-colors"
                >
                  Thử lại
                </button>
                <button
                  onClick={() => navigate('/')}
                  className="bg-gray-500 text-white px-6 py-3 rounded-lg hover:bg-gray-600 transition-colors"
                >
                  Về trang chủ
                </button>
              </>
            )}
          </div>

          {/* Help Section */}
          <div className="mt-8 pt-6 border-t border-gray-200 text-center">
            <p className="text-sm text-gray-500 mb-2">
              Cần hỗ trợ? Liên hệ với chúng tôi
            </p>
            <div className="flex justify-center space-x-4 text-sm">
              <a href="tel:19001900" className="text-blue-500 hover:underline">
                📞 1900 1900
              </a>
              <a href="mailto:support@shoestore.com" className="text-blue-500 hover:underline">
                ✉️ support@shoestore.com
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PaymentResult; 