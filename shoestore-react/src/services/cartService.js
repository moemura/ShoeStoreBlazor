import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

// Helper function to get or generate guest ID
const getGuestId = () => {
  let guestId = localStorage.getItem('guestId');
  if (!guestId) {
    guestId = 'guest_' + Math.random().toString(36).substr(2, 9);
    localStorage.setItem('guestId', guestId);
  }
  return guestId;
};

// Helper function to get headers with guest ID
const getHeaders = () => {
  const token = localStorage.getItem('token');
  const headers = {
    'Content-Type': 'application/json',
  };
  
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  } else {
    headers.GuestId = getGuestId();
  }
  
  return headers;
};

export const cartService = {
  // Get cart items
  getCart: async () => {
    try {
      const response = await axios.get(`${API_URL}/Cart`, {
        headers: getHeaders()
      });
      return response.data;
    } catch (error) {
      console.error('Error getting cart:', error);
      return { items: [], createdAt: new Date(), updatedAt: new Date() };
    }
  },

  // Add or update item in cart
  addToCart: async (inventoryId, quantity = 1) => {
    try {
      const response = await axios.post(
        `${API_URL}/Cart/item`,
        { inventoryId, quantity },
        { headers: getHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error('Error adding to cart:', error);
      throw error;
    }
  },

  // Update item quantity
  updateQuantity: async (inventoryId, quantity) => {
    try {
      const response = await axios.post(
        `${API_URL}/Cart/item`,
        { inventoryId, quantity },
        { headers: getHeaders() }
      );
      return response.data;
    } catch (error) {
      console.error('Error updating quantity:', error);
      throw error;
    }
  },

  // Remove item from cart
  removeFromCart: async (inventoryId) => {
    try {
      await axios.delete(`${API_URL}/Cart/item/${inventoryId}`, {
        headers: getHeaders()
      });
    } catch (error) {
      console.error('Error removing from cart:', error);
      throw error;
    }
  },

  // Clear entire cart
  clearCart: async () => {
    try {
      await axios.delete(`${API_URL}/Cart`, {
        headers: getHeaders()
      });
    } catch (error) {
      console.error('Error clearing cart:', error);
      throw error;
    }
  },

  // Get cart item count
  getCartItemCount: async () => {
    try {
      const response = await axios.get(`${API_URL}/Cart/count`, {
        headers: getHeaders()
      });
      return response.data;
    } catch (error) {
      console.error('Error getting cart count:', error);
      return 0;
    }
  },

  // Merge guest cart to user cart on login
  mergeCart: async () => {
    try {
      const guestId = localStorage.getItem('guestId');
      if (guestId) {
        const token = localStorage.getItem('token');
        await axios.post(`${API_URL}/Cart/merge`, {}, {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            'GuestId': guestId
          }
        });
        // Remove guest ID after successful merge
        localStorage.removeItem('guestId');
      }
    } catch (error) {
      console.error('Error merging cart:', error);
      throw error;
    }
  }
}; 