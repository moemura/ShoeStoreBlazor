import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useOrder } from '../context/OrderContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../components/Toast';
import ConfirmDialog from '../components/ConfirmDialog';
import orderService from '../services/orderService';

const Orders = () => {
  const { orders, loading, error, getUserOrders, cancelOrder } = useOrder();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [statusFilter, setStatusFilter] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [orderToCancel, setOrderToCancel] = useState(null);

  useEffect(() => {
    if (user) {
      getUserOrders();
    }
  }, [user, getUserOrders]);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
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

  const handleCancelOrderClick = (orderId) => {
    setOrderToCancel(orderId);
    setShowConfirmDialog(true);
  };

  const handleConfirmCancel = async () => {
    if (!orderToCancel) return;

    setIsLoading(true);
    setShowConfirmDialog(false);
    
    const success = await cancelOrder(orderToCancel);
    setIsLoading(false);

    if (success) {
      // Show success message
      addToast('Hủy đơn hàng thành công', 'success');
      // Refresh orders
      getUserOrders();
    } else {
      addToast('Có lỗi xảy ra khi hủy đơn hàng', 'error');
    }

    setOrderToCancel(null);
  };

  const handleCancelDialog = () => {
    setShowConfirmDialog(false);
    setOrderToCancel(null);
  };

  // Filter orders
  const filteredOrders = orders.filter(order => {
    const matchesStatus = statusFilter === 'all' || order.status.toString() === statusFilter;
    const matchesSearch = !searchTerm || 
      order.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.phone.includes(searchTerm);
    return matchesStatus && matchesSearch;
  });

  const statusOptions = [
    { value: 'all', label: 'Tất cả' },
    { value: '1', label: 'Chờ xử lý' },
    { value: '2', label: 'Đang chuẩn bị' },
    { value: '3', label: 'Đang giao' },
    { value: '4', label: 'Hoàn thành' },
    { value: '5', label: 'Đã hủy' },
  ];

  if (!user) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center py-12">
          <h1 className="text-2xl font-bold mb-4">Vui lòng đăng nhập</h1>
          <p className="text-gray-600 mb-6">Bạn cần đăng nhập để xem danh sách đơn hàng</p>
          <Link
            to="/login"
            className="inline-block bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white px-6 py-3 rounded-lg transition-all duration-300 shadow-lg"
          >
            Đăng nhập
          </Link>
        </div>
      </div>
    );
  }

  if (loading) {
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
        <h1 className="text-3xl font-bold mb-8">Đơn hàng của tôi</h1>

        {/* Error Display */}
        {error && (
          <div className="mb-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg">
            <p>{error}</p>
          </div>
        )}

        {/* Filters */}
        <div className="bg-white rounded-lg border p-6 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">Tìm kiếm</label>
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Tìm theo mã đơn hàng, tên hoặc số điện thoại..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Trạng thái</label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {statusOptions.map(option => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* Orders List */}
        {filteredOrders.length === 0 ? (
          <div className="text-center py-12">
            <div className="mb-8">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                className="h-24 w-24 mx-auto text-gray-400 mb-4"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1}
                  d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                />
              </svg>
              <h2 className="text-2xl font-semibold text-gray-700 mb-2">
                {searchTerm || statusFilter !== 'all' ? 'Không tìm thấy đơn hàng' : 'Chưa có đơn hàng nào'}
              </h2>
              <p className="text-gray-500 mb-6">
                {searchTerm || statusFilter !== 'all' 
                  ? 'Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm'
                  : 'Bạn chưa đặt hàng lần nào'}
              </p>
            </div>
            <Link
              to="/products"
              className="inline-block bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white px-6 py-3 rounded-lg transition-all duration-300 shadow-lg"
            >
              Bắt đầu mua sắm
            </Link>
          </div>
        ) : (
          <div className="space-y-6">
            {filteredOrders.map((order) => (
              <div key={order.id} className="bg-white rounded-lg border p-6">
                <div className="flex flex-col lg:flex-row lg:items-center justify-between mb-4">
                  <div>
                    <h3 className="font-semibold text-lg">Đơn hàng #{order.id}</h3>
                    <p className="text-gray-600">{formatDate(order.createdAt)}</p>
                  </div>
                  <div className="flex items-center gap-3 mt-2 lg:mt-0">
                    <span className={`px-3 py-1 rounded-full text-sm ${orderService.getStatusBadgeClass(order.status)}`}>
                      {orderService.formatOrderStatus(order.status)}
                    </span>
                    <div className="text-right">
                      {order.discountAmount > 0 ? (
                        <div>
                          <div className="text-sm text-gray-500 line-through">
                            {formatPrice(order.originalAmount)}
                          </div>
                          <div className="font-semibold text-lg text-green-600">
                            {formatPrice(order.totalAmount)}
                          </div>
                          <div className="text-xs text-red-600">
                            Tiết kiệm {formatPrice(order.discountAmount)}
                          </div>
                        </div>
                      ) : (
                        <span className="font-semibold text-lg">{formatPrice(order.totalAmount)}</span>
                      )}
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                  <div>
                    <h4 className="font-medium mb-2">Thông tin giao hàng</h4>
                    <p className="text-gray-600">{order.customerName}</p>
                    <p className="text-gray-600">{order.phone}</p>
                    <p className="text-gray-600 text-sm">{order.address}</p>
                  </div>
                  <div>
                    <h4 className="font-medium mb-2">Thanh toán</h4>
                    <p className="text-gray-600">{orderService.formatPaymentMethod(order.paymentMethod)}</p>
                    {order.voucherCode && (
                      <div className="mt-2">
                        <span className="text-sm font-medium">Voucher: </span>
                        <span className="text-sm bg-green-100 text-green-800 px-2 py-1 rounded">
                          {order.voucherCode}
                        </span>
                        {order.voucherName && (
                          <div className="text-xs text-gray-600 mt-1">{order.voucherName}</div>
                        )}
                      </div>
                    )}
                    {order.customerNote && (
                      <div className="mt-2">
                        <span className="text-sm font-medium">Ghi chú: </span>
                        <span className="text-sm text-gray-600">{order.customerNote}</span>
                      </div>
                    )}
                  </div>
                </div>

                {/* Order Items Preview */}
                {order.items && order.items.length > 0 && (
                  <div className="mb-4">
                    <h4 className="font-medium mb-2">Sản phẩm ({order.items.length} sản phẩm)</h4>
                    <div className="flex gap-2 overflow-x-auto">
                      {order.items.slice(0, 4).map((item) => (
                        <div key={item.id} className="flex-shrink-0 w-16 h-16">
                          <img
                            src={item.mainImage || '/images/defaults/product-default.jpg'}
                            alt={item.productName}
                            className="w-full h-full object-cover rounded"
                          />
                        </div>
                      ))}
                      {order.items.length > 4 && (
                        <div className="flex-shrink-0 w-16 h-16 bg-gray-100 rounded flex items-center justify-center">
                          <span className="text-xs text-gray-600">+{order.items.length - 4}</span>
                        </div>
                      )}
                    </div>
                  </div>
                )}

                {/* Actions */}
                <div className="flex flex-wrap gap-3">
                  <Link
                    to={`/order-detail/${order.id}`}
                    className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    Xem chi tiết
                  </Link>
                  
                  {(order.status === 1 || order.status === 2) && ( // Pending or Preparing
                    <button
                      onClick={() => handleCancelOrderClick(order.id)}
                      disabled={isLoading}
                      className="px-4 py-2 text-red-600 border border-red-300 rounded-lg hover:bg-red-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {isLoading ? 'Đang hủy...' : 'Hủy đơn hàng'}
                    </button>
                  )}

                  {order.status === 4 && ( // Completed
                    <Link
                      to={`/products`}
                      className="px-4 py-2 bg-black text-white rounded-lg hover:bg-gray-800 transition-colors"
                    >
                      Mua lại
                    </Link>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
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

export default Orders; 