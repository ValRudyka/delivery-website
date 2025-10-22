using delivery_website.Models;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using delivery_website.Services.Interfaces;
using delivery_website.ViewModels.Customer;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace delivery_website.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretKey;

        public OrderService(
            IOrderRepository orderRepository,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _stripeSecretKey = _configuration["Stripe:SecretKey"];

            // Initialize Stripe
            if (!string.IsNullOrEmpty(_stripeSecretKey))
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;
            }
        }

        public async Task<CheckoutViewModel> PrepareCheckoutAsync(Guid userId, Guid restaurantId)
        {
            // This method would retrieve cart items from session/database
            // and prepare the checkout view model

            var model = new CheckoutViewModel
            {
                RestaurantId = restaurantId,
                // Cart items would be populated from session or database
                CartItems = new List<CartItemViewModel>(),
                TaxRate = 0.10m, // 10% tax
                FixedDeliveryFee = 5.00m
            };

            // Calculate totals
            model.Subtotal = model.CartItems.Sum(item => item.Total);
            model.Tax = model.Subtotal * model.TaxRate;
            model.DeliveryFee = model.FixedDeliveryFee;
            model.Total = model.Subtotal + model.Tax + model.DeliveryFee;

            return model;
        }

        public async Task<Order> CreateOrderAsync(CheckoutViewModel model, Guid userId)
        {
            // Create the order
            var order = new Order
            {
                UserId = userId,
                RestaurantId = model.RestaurantId,
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow,
                SubtotalAmount = model.Subtotal,
                TaxAmount = model.Tax,
                DeliveryFee = model.DeliveryFee,
                DiscountAmount = 0,
                FinalAmount = model.Total,
                ContactPhone = model.ContactPhone,
                SpecialInstructions = model.SpecialInstructions,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == "CashOnDelivery" ? "Pending" : "AwaitingPayment"
            };

            // Set delivery address
            if (model.UseExistingAddress && model.SelectedAddressId.HasValue)
            {
                var selectedAddress = model.SavedAddresses
                    .FirstOrDefault(a => a.AddressId == model.SelectedAddressId.Value);
                order.DeliveryAddress = selectedAddress?.FullAddress ?? "Address not found";
            }
            else
            {
                order.DeliveryAddress = $"{model.NewStreetAddress}, {model.NewCity}, {model.NewPostalCode}, {model.NewCountry}";
            }

            // Create order items
            foreach (var cartItem in model.CartItems)
            {
                var orderItem = new OrderItem
                {
                    MenuItemId = cartItem.MenuItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price,
                    TotalPrice = cartItem.Total,
                    Customizations = cartItem.Customizations
                };
                order.OrderItems.Add(orderItem);
            }

            // Create payment record
            var payment = new Payment
            {
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == "CashOnDelivery" ? "Pending" : "AwaitingPayment",
                Amount = order.FinalAmount,
                PaymentGateway = model.PaymentMethod == "CreditCard" ? "Stripe" : "CashOnDelivery"
            };
            order.Payment = payment;

            // Save to database
            var createdOrder = await _orderRepository.CreateAsync(order);

            return createdOrder;
        }

        public async Task<OrderConfirmationViewModel> GetOrderConfirmationAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

            if (order == null)
                return null;

            var model = new OrderConfirmationViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderId.ToString().Substring(0, 8).ToUpper(),
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                RestaurantName = order.Restaurant?.Name ?? "Unknown Restaurant",
                RestaurantPhone = order.Restaurant?.PhoneNumber,
                Subtotal = order.SubtotalAmount,
                Tax = order.TaxAmount,
                DeliveryFee = order.DeliveryFee,
                Total = order.FinalAmount,
                DeliveryAddress = order.DeliveryAddress,
                ContactPhone = order.ContactPhone,
                SpecialInstructions = order.SpecialInstructions,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                EstimatedDeliveryTime = order.OrderDate.AddMinutes(45) // 45 minutes estimate
            };

            // Map order items
            model.OrderItems = order.OrderItems.Select(oi => new OrderItemDetailViewModel
            {
                ItemName = oi.MenuItem?.Name ?? "Unknown Item",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Total = oi.TotalPrice,
                Customizations = oi.Customizations
            }).ToList();

            return model;
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            return await _orderRepository.UpdateOrderStatusAsync(orderId, newStatus);
        }

        public async Task<string> CreateStripeCheckoutSessionAsync(Order order)
        {
            if (string.IsNullOrEmpty(_stripeSecretKey))
            {
                throw new InvalidOperationException("Stripe API key is not configured. Please add it to appsettings.json");
            }

            var domain = _configuration["AppSettings:Domain"] ?? "https://localhost:5001";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{domain}/Customer/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}&orderId={order.OrderId}",
                CancelUrl = $"{domain}/Customer/Checkout/Cancel?orderId={order.OrderId}",
                ClientReferenceId = order.OrderId.ToString(),
                CustomerEmail = order.User?.Email,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", order.OrderId.ToString() },
                    { "UserId", order.UserId.ToString() }
                }
            };

            // Add line items
            foreach (var item in order.OrderItems)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.MenuItem?.Name ?? "Menu Item",
                            Description = item.Customizations
                        },
                        UnitAmount = (long)(item.UnitPrice * 100) // Convert to cents
                    },
                    Quantity = item.Quantity
                });
            }

            // Add tax as a line item
            if (order.TaxAmount > 0)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Tax"
                        },
                        UnitAmount = (long)(order.TaxAmount * 100)
                    },
                    Quantity = 1
                });
            }

            // Add delivery fee as a line item
            if (order.DeliveryFee > 0)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Delivery Fee"
                        },
                        UnitAmount = (long)(order.DeliveryFee * 100)
                    },
                    Quantity = 1
                });
            }

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Id;
        }

        public async Task<bool> ProcessPaymentSuccessAsync(string sessionId, Guid orderId)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                if (session.PaymentStatus == "paid")
                {
                    var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
                    if (order != null)
                    {
                        order.PaymentStatus = "Completed";
                        order.OrderStatus = "Confirmed";
                        order.ConfirmedDate = DateTime.UtcNow;

                        if (order.Payment != null)
                        {
                            order.Payment.PaymentStatus = "Completed";
                            order.Payment.TransactionId = session.PaymentIntentId;
                        }

                        await _orderRepository.UpdateAsync(order);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}