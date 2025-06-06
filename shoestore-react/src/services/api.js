const API_BASE_URL = 'https://localhost:5001/api';

// Token management
const getToken = () => localStorage.getItem('token');
const setToken = (token) => localStorage.setItem('token', token);
const removeToken = () => localStorage.removeItem('token');
const getRefreshToken = () => localStorage.getItem('refreshToken');
const setRefreshToken = (token) => localStorage.setItem('refreshToken', token);
const removeRefreshToken = () => localStorage.removeItem('refreshToken');

// Enhanced fetch with JWT support
export const fetchApi = async (endpoint, options = {}) => {
  const token = getToken();
  
  const config = {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers,
    },
  };

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);

    // Handle 401 Unauthorized - try to refresh token
    if (response.status === 401 && token) {
      const refreshed = await tryRefreshToken();
      if (refreshed) {
        // Retry the original request with new token
        const newToken = getToken();
        config.headers.Authorization = `Bearer ${newToken}`;
        const retryResponse = await fetch(`${API_BASE_URL}${endpoint}`, config);
        
        if (!retryResponse.ok) {
          throw new Error(`API call failed: ${retryResponse.statusText}`);
        }
        return retryResponse.json();
      } else {
        // Refresh failed, redirect to login
        clearAuthData();
        window.location.href = '/login';
        throw new Error('Session expired. Please login again.');
      }
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || `API call failed: ${response.statusText}`);
    }

    return response.json();
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
};

// Try to refresh token
const tryRefreshToken = async () => {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  try {
    const response = await fetch(`${API_BASE_URL}/auth/refresh-token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(refreshToken),
    });

    if (response.ok) {
      const data = await response.json();
      setToken(data.token);
      setRefreshToken(data.refreshToken);
      return true;
    }
  } catch (error) {
    console.error('Token refresh failed:', error);
  }
  
  return false;
};

// Auth utilities
export const clearAuthData = () => {
  removeToken();
  removeRefreshToken();
};

export const isAuthenticated = () => {
  const token = getToken();
  if (!token) return false;

  try {
    // Basic check: decode JWT payload to check expiration
    const payload = JSON.parse(atob(token.split('.')[1]));
    const currentTime = Date.now() / 1000;
    
    // If token is expired, return false
    if (payload.exp && payload.exp < currentTime) {
      clearAuthData();
      return false;
    }
    
    return true;
  } catch (error) {
    // If token is malformed, clear it and return false
    clearAuthData();
    return false;
  }
};

export { setToken, setRefreshToken, getToken, getRefreshToken }; 