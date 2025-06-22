import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

// Helper function to get headers with authorization
const getHeaders = () => {
  const token = localStorage.getItem('token');
  const headers = {
    'Content-Type': 'application/json',
  };
  
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }
  
  return headers;
};

export const paymentService = {
  // Initiate payment
  initiatePayment: async (paymentRequest) => {
    try {
      const response = await axios.post(`${API_URL}/payment/initiate`, paymentRequest, {
        headers: getHeaders()
      });
      return response.data;
    } catch (error) {
      console.error('Error initiating payment:', error);
      throw error;
    }
  },

  // Get payment status by order ID
  getPaymentStatus: async (orderId) => {
    try {
      const response = await axios.get(`${API_URL}/payment/status/${orderId}`, {
        headers: getHeaders()
      });
      return response.data;
    } catch (error) {
      console.error('Error getting payment status:', error);
      throw error;
    }
  },

  // Get payment transaction by transaction ID
  getPaymentTransaction: async (transactionId) => {
    try {
      const response = await axios.get(`${API_URL}/payment/transaction/${transactionId}`, {
        headers: getHeaders()
      });
      return response.data;
    } catch (error) {
      console.error('Error getting payment transaction:', error);
      throw error;
    }
  },

  // Expire payment transaction
  expirePaymentTransaction: async (paymentTransactionId) => {
    try {
      const response = await axios.post(`${API_URL}/payment/expire/${paymentTransactionId}`, {}, {
        headers: getHeaders()
      });
      return response.data;
    } catch (error) {
      console.error('Error expiring payment transaction:', error);
      throw error;
    }
  },

  // Process payment result from callback URL
  processPaymentResult: (urlParams) => {
    const params = new URLSearchParams(urlParams);
    const transactionId = params.get('transactionId');
    const status = params.get('status');
    const orderId = params.get('orderId');

    // Determine payment result based on status codes
    let isSuccess = false;
    let message = 'Thanh toán không thành công';

    if (status === '0' || status === '00') {
      isSuccess = true;
      message = 'Thanh toán thành công';
    } else if (status === '24') {
      message = 'Người dùng đã hủy thanh toán';
    } else if (status === '11') {
      message = 'Đã hết hạn thanh toán';
    } else if (status === 'error') {
      message = 'Có lỗi xảy ra trong quá trình thanh toán';
    }

    return {
      success: isSuccess,
      transactionId,
      orderId,
      status,
      message
    };
  },

  // Get available payment methods
  getPaymentMethods: () => {
    return [
      {
        id: 0,
        name: 'COD',
        displayName: 'Thanh toán khi nhận hàng',
        icon: '💵',
        description: 'Thanh toán bằng tiền mặt khi nhận hàng'
      },
      {
        id: 3,
        name: 'MoMo',
        displayName: 'Ví MoMo',
        icon: '🟡',
        description: 'Thanh toán qua ví điện tử MoMo'
      },
      {
        id: 4,
        name: 'VnPay',
        displayName: 'VnPay',
        icon: '🔵',
        description: 'Thanh toán qua VnPay'
      }
    ];
  },

  // Format payment method display
  formatPaymentMethod: (paymentMethodId) => {
    const methods = paymentService.getPaymentMethods();
    const method = methods.find(m => m.id === paymentMethodId);
    return method ? method.displayName : 'Không xác định';
  },

  // Format payment status display
  formatPaymentStatus: (status) => {
    const statusMap = {
      1: { text: 'Chờ thanh toán', color: 'orange' },
      2: { text: 'Đang xử lý', color: 'blue' },
      3: { text: 'Thành công', color: 'green' },
      4: { text: 'Thất bại', color: 'red' },
      5: { text: 'Đã hủy', color: 'gray' },
      6: { text: 'Hết hạn', color: 'red' },
      7: { text: 'Đã hoàn tiền', color: 'purple' }
    };
    
    return statusMap[status] || { text: 'Không xác định', color: 'gray' };
  }
}; 