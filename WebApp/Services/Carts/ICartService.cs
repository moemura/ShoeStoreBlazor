using System.Threading.Tasks;
using WebApp.Models.DTOs;

namespace WebApp.Services.Carts
{
    public interface ICartService
    {
        Task<CartDto> GetCart(string userIdOrGuestId);
        Task AddOrUpdateItem(string userIdOrGuestId, CartItemAddOrUpdateRequest request);
        Task RemoveItem(string userIdOrGuestId, int inventoryId);
        Task ClearCart(string userIdOrGuestId);
        Task<int> GetCartItemCount(string userIdOrGuestId);
        Task MergeGuestCartToUser(string guestId, string userId);
    }
} 