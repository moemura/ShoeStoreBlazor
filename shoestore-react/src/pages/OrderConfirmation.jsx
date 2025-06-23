import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useOrder } from '../context/OrderContext';
import orderService from '../services/orderService';

const OrderConfirmation = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const { getOrderById, loading } = useOrder();
  const [order, setOrder] = useState(null);
  const [localLoading, setLocalLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrder = async () => {
      if (!orderId) {
        setError('Không tìm thấy mã đơn hàng');
        setLocalLoading(false);
        return;
      }

      try {
        setLocalLoading(true);
        const orderData = await getOrderById(orderId);
        if (orderData) {
          setOrder(orderData);
        } else {
          setError('Không tìm thấy đơn hàng');
        }
      } catch (err) {
        setError('Có lỗi xảy ra khi tải thông tin đơn hàng');
      } finally {
        setLocalLoading(false);
      }
    };

    fetchOrder();
  }, [orderId, getOrderById]);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (localLoading || loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-2xl mx-auto text-center">
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
            {error}
          </div>
          <Link
            to="/products"
            className="inline-block bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 transition-colors"
          >
            Tiếp tục mua sắm
          </Link>
        </div>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-2xl mx-auto text-center">
          <h1 className="text-2xl font-bold mb-4">Không tìm thấy đơn hàng</h1>
          <Link
            to="/products"
            className="inline-block bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 transition-colors"
          >
            Tiếp tục mua sắm
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        {/* Success Header */}
        <div className="text-center mb-8">
          <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg
              className="w-10 h-10 text-green-500"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Đặt hàng thành công!</h1>
          <p className="text-gray-600">
            Cảm ơn bạn đã đặt hàng. Chúng tôi sẽ xử lý đơn hàng của bạn trong thời gian sớm nhất.
          </p>
        </div>

        {/* Order Details */}
        <div className="bg-white rounded-lg border p-6 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div>
              <h2 className="text-lg font-semibold mb-4">Thông tin đơn hàng</h2>
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-gray-600">Mã đơn hàng:</span>
                  <span className="font-medium">#{order.id}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Ngày đặt:</span>
                  <span>{formatDate(order.createdAt)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Trạng thái:</span>
                  <span className={`px-2 py-1 rounded text-sm ${orderService.getStatusBadgeClass(order.status)}`}>
                    {orderService.formatOrderStatus(order.status)}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Thanh toán:</span>
                  <span>{orderService.formatPaymentMethod(order.paymentMethod)}</span>
                </div>
                {order.voucherCode && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Voucher:</span>
                    <div className="text-right">
                      <div className="text-sm bg-green-100 text-green-800 px-2 py-1 rounded inline-block">
                        {order.voucherCode}
                      </div>
                      {order.voucherName && (
                        <div className="text-xs text-gray-500 mt-1">{order.voucherName}</div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>

            <div>
              <h2 className="text-lg font-semibold mb-4">Thông tin giao hàng</h2>
              <div className="space-y-2">
                <div>
                  <span className="text-gray-600">Người nhận:</span>
                  <div className="font-medium">{order.customerName}</div>
                </div>
                <div>
                  <span className="text-gray-600">Số điện thoại:</span>
                  <div>{order.phone}</div>
                </div>
                {order.email && (
                  <div>
                    <span className="text-gray-600">Email:</span>
                    <div>{order.email}</div>
                  </div>
                )}
                <div>
                  <span className="text-gray-600">Địa chỉ:</span>
                  <div>{order.address}</div>
                </div>
              </div>
            </div>
          </div>

          {order.customerNote && (
            <div className="border-t pt-4">
              <h3 className="font-medium mb-2">Ghi chú:</h3>
              <p className="text-gray-600">{order.customerNote}</p>
            </div>
          )}
        </div>

        {/* Order Items */}
        <div className="bg-white rounded-lg border p-6 mb-6">
          <h2 className="text-lg font-semibold mb-4">Sản phẩm đã đặt</h2>
          <div className="space-y-4">
            {order.items?.map((item) => (
              <div key={item.id} className="flex gap-4 p-4 border rounded-lg">
                <img
                  src={item.mainImage || '/images/defaults/product-default.jpg'}
                  alt={item.productName}
                  className="w-20 h-20 object-cover rounded"
                />
                <div className="flex-1">
                  <h3 className="font-medium">{item.productName}</h3>
                  <p className="text-gray-500">Size: {item.size}</p>
                  <div className="flex justify-between items-center mt-2">
                    <span className="text-gray-600">Số lượng: {item.quantity}</span>
                    <div className="text-right">
                      <p className="font-medium">{formatPrice(item.subtotal)}</p>
                      <p className="text-sm text-gray-500">
                        {formatPrice(item.price)} x {item.quantity}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Order Total */}
          <div className="border-t mt-6 pt-4">
            {order.discountAmount > 0 ? (
              <div className="space-y-2">
                <div className="flex justify-between text-lg">
                  <span>Tạm tính:</span>
                  <span className="line-through text-gray-500">{formatPrice(order.originalAmount)}</span>
                </div>
                <div className="flex justify-between text-lg text-red-600">
                  <span>Giảm giá ({order.voucherCode}):</span>
                  <span>-{formatPrice(order.discountAmount)}</span>
                </div>
                <div className="flex justify-between text-lg font-bold text-green-600">
                  <span>Tổng cộng:</span>
                  <span>{formatPrice(order.totalAmount)}</span>
                </div>
                <div className="bg-green-50 p-3 rounded-lg mt-4">
                  <p className="text-green-800 text-sm font-medium text-center">
                    🎉 Bạn đã tiết kiệm được {formatPrice(order.discountAmount)} với voucher {order.voucherCode}!
                  </p>
                </div>
              </div>
            ) : (
              <div className="flex justify-between text-lg font-bold">
                <span>Tổng cộng:</span>
                <span>{formatPrice(order.totalAmount)}</span>
              </div>
            )}
          </div>
        </div>

        {/* Next Steps */}
        <div className="bg-blue-50 rounded-lg p-6 mb-6">
          <h2 className="text-lg font-semibold mb-4">Bước tiếp theo</h2>
          <div className="space-y-2">
            <p className="flex items-center">
              <span className="w-2 h-2 bg-blue-500 rounded-full mr-3"></span>
              Chúng tôi sẽ xác nhận đơn hàng trong vòng 24 giờ
            </p>
            <p className="flex items-center">
              <span className="w-2 h-2 bg-blue-500 rounded-full mr-3"></span>
              Đơn hàng sẽ được đóng gói và giao trong 2-3 ngày làm việc
            </p>
            <p className="flex items-center">
              <span className="w-2 h-2 bg-blue-500 rounded-full mr-3"></span>
              Bạn sẽ nhận được thông báo cập nhật trạng thái đơn hàng
            </p>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <Link
            to="/products"
            className="px-6 py-3 bg-black text-white rounded-lg hover:bg-gray-800 transition-colors text-center"
          >
            Tiếp tục mua sắm
          </Link>
          
          <Link
            to="/orders"
            className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors text-center"
          >
            Xem đơn hàng của tôi
          </Link>

          <button
            onClick={() => window.print()}
            className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            In đơn hàng
          </button>
        </div>
      </div>
    </div>
  );
};

export default OrderConfirmation; 