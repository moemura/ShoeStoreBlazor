import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

export const categoryService = {
  async getAll() {
    const response = await axios.get(`${API_URL}/categories`);
    return response.data;
  },

  async getById(id) {
    const response = await axios.get(`${API_URL}/categories/${id}`);
    return response.data;
  }
}; 