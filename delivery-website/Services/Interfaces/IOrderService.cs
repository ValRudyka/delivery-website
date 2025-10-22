using delivery_website.Models.Entities;
using delivery_website.ViewModels.Customer;
using System;
using System.Threading.Tasks;

namespace delivery_website.Services.Interfaces
{
    public interface IOrderService
    {
        Task<CheckoutViewModel> PrepareCheckoutAsync(Guid userId, Guid restaurantId);
        Task<Order> CreateOrderAsync(CheckoutViewModel model, Guid userId);
        Task<OrderConfirmationViewModel> GetOrderConfirmationAsync(Guid orderId);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<string> CreateStripeCheckoutSessionAsync(Order order);
        Task<bool> ProcessPaymentSuccessAsync(string sessionId, Guid orderId);
    }
}