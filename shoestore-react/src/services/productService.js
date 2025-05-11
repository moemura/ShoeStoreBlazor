import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

export const productService = {
  getAll: async (pageIndex = 1, pageSize = 12, filters = {}) => {
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
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  },

  getById: async (id) => {
    const response = await axios.get(`${API_URL}/Products/${id}`);
    return response.data;
  },

  getByCategory: async (categoryId, pageIndex = 1, pageSize = 12) => {
    const params = new URLSearchParams();
    params.append('pageIndex', pageIndex);
    params.append('pageSize', pageSize);
    params.append('categoryId', categoryId);
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  },

  getByBrand: async (brandId, pageIndex = 1, pageSize = 12) => {
    const params = new URLSearchParams();
    params.append('pageIndex', pageIndex);
    params.append('pageSize', pageSize);
    params.append('brandId', brandId);
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  },

  search: async (query, pageIndex = 1, pageSize = 12) => {
    const params = new URLSearchParams();
    params.append('pageIndex', pageIndex);
    params.append('pageSize', pageSize);
    params.append('search', query);
    
    const response = await axios.get(`${API_URL}/Products/filter?${params.toString()}`);
    return response.data;
  }
}; 