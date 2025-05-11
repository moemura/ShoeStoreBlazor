import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

export const brandService = {
  async getAll() {
    const response = await axios.get(`${API_URL}/brands`);
    return response.data;
  },

  async getById(id) {
    const response = await axios.get(`${API_URL}/brands/${id}`);
    return response.data;
  }
}; 