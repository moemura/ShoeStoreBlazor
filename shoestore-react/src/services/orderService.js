const API_BASE = 'https://localhost:5001/api';

class OrderService {
  async createOrder(orderData, token = null, guestId = null) {
    const headers = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    if (guestId) {
      headers['GuestId'] = guestId;
    }

    const response = await fetch(`${API_BASE}/order`, {
      method: 'POST',
      headers,
      body: JSON.stringify(orderData),
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Có lỗi xảy ra khi tạo đơn hàng');
    }

    return await response.json();
  }

  async getUserOrders(token) {
    const response = await fetch(`${API_BASE}/order`, {
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Có lỗi xảy ra khi tải danh sách đơn hàng');
    }

    return await response.json();
  }

  async getOrderById(orderId, token = null) {
    const headers = {};
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_BASE}/order/${orderId}`, {
      headers,
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Có lỗi xảy ra khi tải chi tiết đơn hàng');
    }

    return await response.json();
  }

  async cancelOrder(orderId, token) {
    const response = await fetch(`${API_BASE}/order/${orderId}/cancel`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Có lỗi xảy ra khi hủy đơn hàng');
    }

    // 204 No Content doesn't have body, just return success
    if (response.status === 204) {
      return { success: true };
    }

    return await response.json();
  }

  async syncGuestOrders(guestId, token) {
    const response = await fetch(`${API_BASE}/order/sync`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'GuestId': guestId,
      },
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Có lỗi xảy ra khi đồng bộ đơn hàng');
    }

    // 204 No Content doesn't have body, just return success
    if (response.status === 204) {
      return { success: true };
    }

    return await response.json();
  }

  // Utility methods
  formatOrderStatus(status) {
    const statusMap = {
      1: 'Chờ xử lý',
      2: 'Chờ thanh toán',
      3: 'Đã thanh toán',
      4: 'Đang chuẩn bị',
      5: 'Đang giao',
      6: 'Hoàn thành',
      7: 'Đã hủy',
      8: 'Từ chối'
    };
    return statusMap[status] || 'Không xác định';
  }

  formatPaymentMethod(method) {
    const methodMap = {
      0: 'Thanh toán khi nhận hàng (COD)',
      1: 'Chuyển khoản ngân hàng',
      2: 'Thẻ tín dụng',
      3: 'MoMo',
      4: 'VnPay',
      5: 'ZaloPay',
      6: 'PayPal'
    };
    return methodMap[method] || 'Không xác định';
  }

  getStatusBadgeClass(status) {
    const classes = {
      1: 'bg-yellow-100 text-yellow-800',   // Pending
      2: 'bg-orange-100 text-orange-800',   // PendingPayment
      3: 'bg-green-100 text-green-800',     // Paid
      4: 'bg-blue-100 text-blue-800',       // Preparing
      5: 'bg-purple-100 text-purple-800',   // Shipping
      6: 'bg-green-100 text-green-800',     // Completed
      7: 'bg-red-100 text-red-800',         // Cancelled
      8: 'bg-gray-100 text-gray-800'        // Rejected
    };
    return classes[status] || 'bg-gray-100 text-gray-800';
  }
}

export default new OrderService(); 