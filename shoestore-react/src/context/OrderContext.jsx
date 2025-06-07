import { createContext, useContext, useReducer, useEffect, useCallback } from 'react';
import orderService from '../services/orderService';
import { useAuth } from './AuthContext';
import { useToast } from '../components/Toast';

const OrderContext = createContext();

// Order reducer
const orderReducer = (state, action) => {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, loading: action.payload };
    case 'SET_ERROR':
      return { ...state, error: action.payload, loading: false };
    case 'CLEAR_ERROR':
      return { ...state, error: null };
    case 'SET_ORDERS':
      return { ...state, orders: action.payload, loading: false, error: null };
    case 'SET_CURRENT_ORDER':
      return { ...state, currentOrder: action.payload, loading: false, error: null };
    case 'ADD_ORDER':
      return {
        ...state,
        orders: [action.payload, ...state.orders],
        currentOrder: action.payload,
        loading: false,
        error: null
      };
    case 'UPDATE_ORDER_STATUS':
      return {
        ...state,
        orders: state.orders.map(order =>
          order.id === action.payload.orderId
            ? { ...order, status: action.payload.status }
            : order
        ),
        currentOrder: state.currentOrder?.id === action.payload.orderId
          ? { ...state.currentOrder, status: action.payload.status }
          : state.currentOrder
      };
    default:
      return state;
  }
};

// Initial state
const initialState = {
  orders: [],
  currentOrder: null,
  loading: false,
  error: null,
};

export const OrderProvider = ({ children }) => {
  const [state, dispatch] = useReducer(orderReducer, initialState);
  const { user, getToken } = useAuth();
  const { addToast } = useToast();

  // Get guest ID from localStorage
  const getGuestId = useCallback(() => {
    return localStorage.getItem('guestId') || `guest_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }, []);

  // Ensure guest ID exists
  useEffect(() => {
    if (!user) {
      const guestId = getGuestId();
      localStorage.setItem('guestId', guestId);
    }
  }, [user]);

  // Create order
  const createOrder = useCallback(async (orderData) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      
      const token = getToken();
      const guestId = user ? null : getGuestId();
      const order = await orderService.createOrder(orderData, token, guestId);
      
      dispatch({ type: 'ADD_ORDER', payload: order });
      
      return { success: true, order };
    } catch (error) {
      const errorMessage = error.message || 'Có lỗi xảy ra khi đặt hàng';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      return { success: false, error: errorMessage };
    }
  }, [user, getToken]);

  // Get user orders
  const getUserOrders = useCallback(async () => {
    const token = getToken();
    if (!user || !token) return;

    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const orders = await orderService.getUserOrders(token);
      dispatch({ type: 'SET_ORDERS', payload: orders });
    } catch (error) {
      const errorMessage = error.message || 'Có lỗi xảy ra khi tải danh sách đơn hàng';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
    }
  }, [user, getToken]);

  // Get order by ID
  const getOrderById = useCallback(async (orderId) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const token = getToken();
      const order = await orderService.getOrderById(orderId, token);
      dispatch({ type: 'SET_CURRENT_ORDER', payload: order });
      return order;
    } catch (error) {
      const errorMessage = error.message || 'Có lỗi xảy ra khi tải chi tiết đơn hàng';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      return null;
    }
  }, [getToken]);

  // Cancel order
  const cancelOrder = useCallback(async (orderId) => {
    const token = getToken();
    if (!user || !token) {
      return false;
    }

    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      await orderService.cancelOrder(orderId, token);
      
      dispatch({ type: 'UPDATE_ORDER_STATUS', payload: { orderId, status: 5 } }); // Cancelled = 5
      
      return true;
    } catch (error) {
      const errorMessage = error.message || 'Có lỗi xảy ra khi hủy đơn hàng';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      return false;
    }
  }, [user, getToken]);

  // Sync guest orders when user logs in
  const syncGuestOrders = useCallback(async () => {
    const token = getToken();
    if (!user || !token) return;

    const guestId = localStorage.getItem('guestId');
    if (!guestId) return;

    try {
      await orderService.syncGuestOrders(guestId, token);
      
      // Clear guest ID after successful sync
      localStorage.removeItem('guestId');
      
      // Return true to indicate success, caller can refresh orders
      return true;
    } catch (error) {
      console.error('Failed to sync guest orders:', error);
      return false;
    }
  }, [user, getToken]);

  // Clear error
  const clearError = useCallback(() => {
    dispatch({ type: 'CLEAR_ERROR' });
  }, []);

  // Load user orders when user logs in
  useEffect(() => {
    if (user) {
      const loadData = async () => {
        await syncGuestOrders();
        await getUserOrders();
      };
      loadData();
    }
  }, [user, getUserOrders, syncGuestOrders]);

  const value = {
    // State
    orders: state.orders,
    currentOrder: state.currentOrder,
    loading: state.loading,
    error: state.error,
    
    // Actions
    createOrder,
    getUserOrders,
    getOrderById,
    cancelOrder,
    syncGuestOrders,
    clearError,
    
    // Utilities
    getGuestId,
  };

  return (
    <OrderContext.Provider value={value}>
      {children}
    </OrderContext.Provider>
  );
};

export const useOrder = () => {
  const context = useContext(OrderContext);
  if (!context) {
    throw new Error('useOrder must be used within an OrderProvider');
  }
  return context;
}; 