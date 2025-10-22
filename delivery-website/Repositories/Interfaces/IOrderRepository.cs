using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using delivery_website.Models.Entities;

namespace delivery_website.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid orderId);
        Task<Order> GetByIdWithDetailsAsync(Guid orderId);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetByRestaurantIdAsync(Guid restaurantId);
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
    }
}