import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useOrder } from '../context/OrderContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import ConfirmDialog from '../components/ConfirmDialog';
import orderService from '../services/orderService';

const OrderDetail = () => {
  const { orderId } = useParams();
  const { getOrderById, cancelOrder, loading: orderLoading } = useOrder();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);

  useEffect(() => {
    const fetchOrder = async () => {
      if (!orderId) {
        setError('Không tìm thấy mã đơn hàng');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const orderData = await getOrderById(orderId);
        if (orderData) {
          setOrder(orderData);
        } else {
          setError('Không tìm thấy đơn hàng');
        }
      } catch (err) {
        setError('Có lỗi xảy ra khi tải thông tin đơn hàng');
      } finally {
        setLoading(false);
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

  const handleCancelOrderClick = () => {
    setShowConfirmDialog(true);
  };

  const handleConfirmCancel = async () => {
    setShowConfirmDialog(false);
    setIsProcessing(true);
    
    const success = await cancelOrder(orderId);
    
    if (success) {
      addToast('Hủy đơn hàng thành công', 'success');
      // Refresh order data
      const updatedOrder = await getOrderById(orderId);
      if (updatedOrder) {
        setOrder(updatedOrder);
      }
    } else {
      addToast('Có lỗi xảy ra khi hủy đơn hàng', 'error');
    }
    setIsProcessing(false);
  };

  const handleCancelDialog = () => {
    setShowConfirmDialog(false);
  };

  if (!user) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center py-12">
          <h1 className="text-2xl font-bold mb-4">Vui lòng đăng nhập</h1>
          <p className="text-gray-600 mb-6">Bạn cần đăng nhập để xem chi tiết đơn hàng</p>
          <Link
            to="/login"
            className="inline-block bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 transition-colors"
          >
            Đăng nhập
          </Link>
        </div>
      </div>
    );
  }

  if (loading || orderLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900"></div>
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-2xl mx-auto text-center">
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
            {error || 'Không tìm thấy đơn hàng'}
          </div>
          <Link
            to="/orders"
            className="inline-block bg-black text-white px-6 py-3 rounded-lg hover:bg-gray-800 transition-colors"
          >
            Quay lại danh sách đơn hàng
          </Link>
        </div>
      </div>
    );
  }

  const isCancellable = order.status === 1 || order.status === 2; // Pending or Preparing

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <Link
              to="/orders"
              className="text-gray-600 hover:text-gray-800 mb-2 inline-flex items-center"
            >
              <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7" />
              </svg>
              Quay lại
            </Link>
            <h1 className="text-3xl font-bold">Đơn hàng #{order.id}</h1>
            <p className="text-gray-600">{formatDate(order.createdAt)}</p>
          </div>
          <span className={`px-4 py-2 rounded-full text-sm font-medium ${orderService.getStatusBadgeClass(order.status)}`}>
            {orderService.formatOrderStatus(order.status)}
          </span>
        </div>

        {/* Order Information */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
          <div className="bg-white rounded-lg border p-6">
            <h2 className="text-lg font-semibold mb-4">Thông tin đơn hàng</h2>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600">Mã đơn hàng:</span>
                <span className="font-medium">#{order.id}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Ngày đặt:</span>
                <span>{formatDate(order.createdAt)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Thanh toán:</span>
                <span>{orderService.formatPaymentMethod(order.paymentMethod)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Tổng tiền:</span>
                <span className="font-semibold text-lg">{formatPrice(order.totalAmount)}</span>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg border p-6">
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

        {/* Customer Note */}
        {order.customerNote && (
          <div className="bg-white rounded-lg border p-6 mb-6">
            <h2 className="text-lg font-semibold mb-2">Ghi chú</h2>
            <p className="text-gray-600">{order.customerNote}</p>
          </div>
        )}

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
                  <h3 className="font-medium text-lg">{item.productName}</h3>
                  <p className="text-gray-500">Size: {item.size}</p>
                  <div className="flex justify-between items-center mt-2">
                    <span className="text-gray-600">Số lượng: {item.quantity}</span>
                    <div className="text-right">
                      <p className="font-semibold text-lg">{formatPrice(item.subtotal)}</p>
                      <p className="text-sm text-gray-500">
                        {formatPrice(item.price)} x {item.quantity}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="border-t mt-6 pt-4">
            <div className="flex justify-between text-xl font-bold">
              <span>Tổng cộng:</span>
              <span>{formatPrice(order.totalAmount)}</span>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex flex-wrap gap-4 justify-center">
          <Link
            to="/orders"
            className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            Quay lại danh sách
          </Link>

          {isCancellable && (
            <button
              onClick={handleCancelOrderClick}
              disabled={isProcessing}
              className="px-6 py-3 text-red-600 border border-red-300 rounded-lg hover:bg-red-50 transition-colors disabled:opacity-50"
            >
              {isProcessing ? 'Đang hủy...' : 'Hủy đơn hàng'}
            </button>
          )}

          <button
            onClick={() => window.print()}
            className="px-6 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            In đơn hàng
          </button>
        </div>
      </div>

      {/* Confirm Dialog */}
      <ConfirmDialog
        isOpen={showConfirmDialog}
        title="Xác nhận hủy đơn hàng"
        message="Bạn có chắc chắn muốn hủy đơn hàng này? Hành động này không thể hoàn tác."
        confirmText="Hủy đơn hàng"
        cancelText="Quay lại"
        confirmVariant="danger"
        onConfirm={handleConfirmCancel}
        onCancel={handleCancelDialog}
      />
    </div>
  );
};

export default OrderDetail; 