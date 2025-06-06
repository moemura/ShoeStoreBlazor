import React, { createContext, useContext, useReducer, useEffect, useCallback, useMemo } from 'react';
import { cartService } from '../services/cartService';

// Cart action types
const CART_ACTIONS = {
  SET_CART: 'SET_CART',
  SET_LOADING: 'SET_LOADING',
  SET_ERROR: 'SET_ERROR',
  SET_CART_COUNT: 'SET_CART_COUNT',
  CLEAR_ERROR: 'CLEAR_ERROR',
  UPDATE_ITEM_QUANTITY: 'UPDATE_ITEM_QUANTITY',
  REMOVE_ITEM: 'REMOVE_ITEM',
  ADD_ITEM: 'ADD_ITEM'
};

// Initial state
const initialState = {
  cart: { items: [], createdAt: null, updatedAt: null },
  cartCount: 0,
  loading: false,
  error: null
};

// Cart reducer
const cartReducer = (state, action) => {
  switch (action.type) {
    case CART_ACTIONS.SET_CART:
      return {
        ...state,
        cart: action.payload,
        cartCount: action.payload.items?.reduce((total, item) => total + item.quantity, 0) || 0,
        loading: false,
        error: null
      };
    case CART_ACTIONS.SET_LOADING:
      return {
        ...state,
        loading: action.payload
      };
    case CART_ACTIONS.SET_ERROR:
      return {
        ...state,
        error: action.payload,
        loading: false
      };
    case CART_ACTIONS.SET_CART_COUNT:
      return {
        ...state,
        cartCount: action.payload
      };
    case CART_ACTIONS.CLEAR_ERROR:
      return {
        ...state,
        error: null
      };
    case CART_ACTIONS.UPDATE_ITEM_QUANTITY: {
      const { inventoryId, quantity } = action.payload;
      const updatedItems = state.cart.items.map(item =>
        item.inventoryId === inventoryId ? { ...item, quantity } : item
      );
      const newCartCount = updatedItems.reduce((total, item) => total + item.quantity, 0);
      return {
        ...state,
        cart: {
          ...state.cart,
          items: updatedItems
        },
        cartCount: newCartCount
      };
    }
    case CART_ACTIONS.REMOVE_ITEM: {
      const inventoryId = action.payload;
      const updatedItems = state.cart.items.filter(item => item.inventoryId !== inventoryId);
      const newCartCount = updatedItems.reduce((total, item) => total + item.quantity, 0);
      return {
        ...state,
        cart: {
          ...state.cart,
          items: updatedItems
        },
        cartCount: newCartCount
      };
    }
    case CART_ACTIONS.ADD_ITEM: {
      const newItem = action.payload;
      const existingItemIndex = state.cart.items.findIndex(item => item.inventoryId === newItem.inventoryId);
      let updatedItems;
      
      if (existingItemIndex >= 0) {
        updatedItems = state.cart.items.map((item, index) =>
          index === existingItemIndex 
            ? { ...item, quantity: item.quantity + newItem.quantity }
            : item
        );
      } else {
        updatedItems = [...state.cart.items, newItem];
      }
      
      const newCartCount = updatedItems.reduce((total, item) => total + item.quantity, 0);
      return {
        ...state,
        cart: {
          ...state.cart,
          items: updatedItems
        },
        cartCount: newCartCount
      };
    }
    default:
      return state;
  }
};

// Create context
const CartContext = createContext();

// Cart provider component
export const CartProvider = ({ children }) => {
  const [state, dispatch] = useReducer(cartReducer, initialState);

  // Load cart on component mount
  useEffect(() => {
    loadCart();
  }, []);

  // Load cart from API
  const loadCart = useCallback(async () => {
    try {
      dispatch({ type: CART_ACTIONS.SET_LOADING, payload: true });
      const cart = await cartService.getCart();
      dispatch({ type: CART_ACTIONS.SET_CART, payload: cart });
    } catch (error) {
      dispatch({ type: CART_ACTIONS.SET_ERROR, payload: error.message });
    }
  }, []);

  // Add item to cart with optimistic update
  const addToCart = useCallback(async (inventoryId, quantity = 1) => {
    try {
      const result = await cartService.addToCart(inventoryId, quantity);
      // Add the returned item to state immediately
      dispatch({ type: CART_ACTIONS.ADD_ITEM, payload: result });
      return true;
    } catch (error) {
      dispatch({ type: CART_ACTIONS.SET_ERROR, payload: error.message });
      return false;
    }
  }, []);

  // Update item quantity with optimistic update
  const updateQuantity = useCallback(async (inventoryId, quantity) => {
    // Store original quantity for rollback
    const originalItem = state.cart.items.find(item => item.inventoryId === inventoryId);
    const originalQuantity = originalItem?.quantity;

    try {
      // Optimistic update
      dispatch({ type: CART_ACTIONS.UPDATE_ITEM_QUANTITY, payload: { inventoryId, quantity } });
      
      // Call API
      await cartService.updateQuantity(inventoryId, quantity);
      return true;
    } catch (error) {
      // Rollback on error
      if (originalQuantity !== undefined) {
        dispatch({ type: CART_ACTIONS.UPDATE_ITEM_QUANTITY, payload: { inventoryId, quantity: originalQuantity } });
      }
      dispatch({ type: CART_ACTIONS.SET_ERROR, payload: error.message });
      return false;
    }
  }, [state.cart.items]);

  // Remove item from cart with optimistic update
  const removeFromCart = useCallback(async (inventoryId) => {
    // Store original item for rollback
    const originalItem = state.cart.items.find(item => item.inventoryId === inventoryId);

    try {
      // Optimistic update
      dispatch({ type: CART_ACTIONS.REMOVE_ITEM, payload: inventoryId });
      
      // Call API
      await cartService.removeFromCart(inventoryId);
      return true;
    } catch (error) {
      // Rollback on error
      if (originalItem) {
        dispatch({ type: CART_ACTIONS.ADD_ITEM, payload: originalItem });
      }
      dispatch({ type: CART_ACTIONS.SET_ERROR, payload: error.message });
      return false;
    }
  }, [state.cart.items]);

  // Clear entire cart
  const clearCart = useCallback(async () => {
    // Store original cart for rollback
    const originalItems = [...state.cart.items];

    try {
      // Optimistic update
      dispatch({ type: CART_ACTIONS.SET_CART, payload: { ...state.cart, items: [] } });
      
      // Call API
      await cartService.clearCart();
      return true;
    } catch (error) {
      // Rollback on error
      dispatch({ type: CART_ACTIONS.SET_CART, payload: { ...state.cart, items: originalItems } });
      dispatch({ type: CART_ACTIONS.SET_ERROR, payload: error.message });
      return false;
    }
  }, [state.cart]);

  // Merge guest cart to user cart (on login)
  const mergeCart = useCallback(async () => {
    try {
      await cartService.mergeCart();
      await loadCart();
      return true;
    } catch (error) {
      dispatch({ type: CART_ACTIONS.SET_ERROR, payload: error.message });
      return false;
    }
  }, [loadCart]);

  // Clear error
  const clearError = useCallback(() => {
    dispatch({ type: CART_ACTIONS.CLEAR_ERROR });
  }, []);

  // Context value with memoization
  const value = React.useMemo(() => ({
    cart: state.cart,
    cartCount: state.cartCount,
    loading: state.loading,
    error: state.error,
    addToCart,
    updateQuantity,
    removeFromCart,
    clearCart,
    mergeCart,
    loadCart,
    clearError
  }), [
    state.cart,
    state.cartCount,
    state.loading,
    state.error,
    addToCart,
    updateQuantity,
    removeFromCart,
    clearCart,
    mergeCart,
    loadCart,
    clearError
  ]);

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
};

// Custom hook to use cart context
export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
}; 