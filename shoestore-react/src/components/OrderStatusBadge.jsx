import orderService from '../services/orderService';

const OrderStatusBadge = ({ status, className = "" }) => {
  const statusClass = orderService.getStatusBadgeClass(status);
  const statusText = orderService.formatOrderStatus(status);

  return (
    <span className={`px-3 py-1 rounded-full text-sm font-medium ${statusClass} ${className}`}>
      {statusText}
    </span>
  );
};

export default OrderStatusBadge; 