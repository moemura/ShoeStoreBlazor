import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

export const productService = {
  getAll: async (pageIndex = 1, pageSize = 12, filters = {}, orderTotal = null) => {
    const params = new URLSearchParams();
    
    // Add pagination params
    params.append('pageIndex', pageIndex);
    params.append('pageSize', pageSize);
    
    // Add filter params if they exist
    if (filters.search) params.append('search', filters.search);
    if (filters.categoryId) params.append('categoryId', filters.categoryId);
    if (filters.brandId) params.append('brandId', filters.brandId);
    if (filters.minPrice) params.append('minPrice', filters.minPrice);
    if (filters.maxPrice) params.append('maxPrice', filters.maxPrice);
    
    // Add orderTotal for dynamic pricing
    if (orderTotal !== null && orderTotal > 0) {
      params.append('orderTotal', orderTotal);
    }
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  },

  getById: async (id, orderTotal = null) => {
    const params = new URLSearchParams();
    
    // Add orderTotal for dynamic pricing
    if (orderTotal !== null && orderTotal > 0) {
      params.append('orderTotal', orderTotal);
    }
    
    const url = params.toString() 
      ? `${API_URL}/Products/${id}?${params.toString()}`
      : `${API_URL}/Products/${id}`;
      
    const response = await axios.get(url);
    return response.data;
  },

  getByCategory: async (categoryId, pageIndex = 1, pageSize = 12, orderTotal = null) => {
    const params = new URLSearchParams();
    params.append('pageIndex', pageIndex);
    params.append('pageSize', pageSize);
    params.append('categoryId', categoryId);
    
    // Add orderTotal for dynamic pricing
    if (orderTotal !== null && orderTotal > 0) {
      params.append('orderTotal', orderTotal);
    }
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  },

  getByBrand: async (brandId, pageIndex = 1, pageSize = 12, orderTotal = null) => {
    const params = new URLSearchParams();
    params.append('pageIndex', pageIndex);
    params.append('pageSize', pageSize);
    params.append('brandId', brandId);
    
    // Add orderTotal for dynamic pricing
    if (orderTotal !== null && orderTotal > 0) {
      params.append('orderTotal', orderTotal);
    }
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  },

  search: async (query, pageIndex = 1, pageSize = 12, orderTotal = null) => {
    const params = new URLSearchParams();
    params.append('pageIndex', pageIndex);
    params.append('pageSize', pageSize);
    params.append('search', query);
    
    // Add orderTotal for dynamic pricing
    if (orderTotal !== null && orderTotal > 0) {
      params.append('orderTotal', orderTotal);
    }
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  },

  // Get promotions for a specific product with order validation
  getProductPromotions: async (productId, orderTotal = null) => {
    const params = new URLSearchParams();
    
    // Add orderTotal for MinOrderAmount validation
    if (orderTotal !== null && orderTotal > 0) {
      params.append('orderTotal', orderTotal);
    }
    
    const url = params.toString() 
      ? `${API_URL}/Products/${productId}/promotions?${params.toString()}`
      : `${API_URL}/Products/${productId}/promotions`;
      
    const response = await axios.get(url);
    return response.data;
  },

  // Backward compatibility methods (without orderTotal)
  getFeatured: async (count = 8) => {
    const response = await axios.get(`${API_URL}/Products/pagin?pageIndex=1&pageSize=${count}`);
    return response.data.data || [];
  },

  // Get featured products with dynamic pricing (sorted by popularity)
  getFeaturedProducts: async (count = 12, orderTotal = null) => {
    const params = new URLSearchParams();
    params.append('count', count);
    
    // Add orderTotal for dynamic pricing
    if (orderTotal !== null && orderTotal > 0) {
      params.append('orderTotal', orderTotal);
    }
    
    const response = await axios.get(`${API_URL}/Products/featured?${params.toString()}`);
    return response.data;
  }
}; 