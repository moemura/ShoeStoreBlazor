import axios from 'axios';

const API_URL = 'https://localhost:5001/api';

export const promotionService = {
  // Get all active promotions
  getActivePromotions: async () => {
    const response = await axios.get(`${API_URL}/Promotion/active`);
    return response.data;
  },

  // Get promotion by ID
  getById: async (id) => {
    const response = await axios.get(`${API_URL}/Promotion/${id}`);
    return response.data;
  },

  // Get promotions for carousel (filtered for homepage)
  getCarouselPromotions: async () => {
    const response = await axios.get(`${API_URL}/Promotion/carousel`);
    return response.data;
  }
}; 