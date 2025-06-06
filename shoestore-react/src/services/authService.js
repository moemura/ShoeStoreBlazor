import { fetchApi, setToken, setRefreshToken, clearAuthData } from './api';

export const authService = {
  // Authentication
  async register(userData) {
    const response = await fetchApi('/auth/register', {
      method: 'POST',
      body: JSON.stringify(userData),
    });
    
    if (response.success && response.token) {
      setToken(response.token);
      setRefreshToken(response.refreshToken);
    }
    
    return response;
  },

  async login(credentials) {
    const response = await fetchApi('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });
    
    if (response.success && response.token) {
      setToken(response.token);
      setRefreshToken(response.refreshToken);
    }
    
    return response;
  },

  async logout() {
    try {
      // Try to revoke the refresh token on server
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        await fetchApi('/auth/revoke-token', {
          method: 'POST',
          body: JSON.stringify(refreshToken),
        });
      }
    } catch (error) {
      console.error('Error revoking token:', error);
    } finally {
      // Always clear local storage
      clearAuthData();
    }
  },

  async refreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await fetchApi('/auth/refresh-token', {
      method: 'POST',
      body: JSON.stringify(refreshToken),
    });

    if (response.success && response.token) {
      setToken(response.token);
      setRefreshToken(response.refreshToken);
    }

    return response;
  },

  // Password Management
  async forgotPassword(email) {
    return await fetchApi('/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify({ email }),
    });
  },

  async resetPassword(resetData) {
    return await fetchApi('/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify(resetData),
    });
  },

  // Account Management (requires authentication)
  async getProfile() {
    return await fetchApi('/account/profile');
  },

  async updateProfile(profileData) {
    return await fetchApi('/account/profile', {
      method: 'PUT',
      body: JSON.stringify(profileData),
    });
  },

  async changePassword(passwordData) {
    return await fetchApi('/account/change-password', {
      method: 'POST',
      body: JSON.stringify(passwordData),
    });
  },

  async deleteAccount(password) {
    return await fetchApi('/account/account', {
      method: 'DELETE',
      body: JSON.stringify(password),
    });
  },
};

export default authService; 