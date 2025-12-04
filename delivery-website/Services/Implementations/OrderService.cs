using delivery_website.Data;
using delivery_website.Models.Configuration;
using delivery_website.Models.Entities;
using delivery_website.Repositories.Interfaces;
using delivery_website.Services.Interfaces;
using delivery_website.ViewModels.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace delivery_website.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartRepository _cartRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<OrderService> _logger;

        private const decimal TAX_RATE = 0.10m; // 10% tax
        private const decimal DEFAULT_DELIVERY_FEE = 50.00m; // 50 UAH

        public OrderService(
            ApplicationDbContext context,
            ICartRepository cartRepository,
            IAddressRepository addressRepository,
            IRestaurantRepository restaurantRepository,
            IOrderRepository orderRepository,
            IOptions<StripeSettings> stripeSettings,
            ILogger<OrderService> logger)
        {
            _context = context;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _restaurantRepository = restaurantRepository;
            _orderRepository = orderRepository;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
        }

        public async Task<CheckoutViewModel> PrepareCheckoutAsync(Guid visitorId, Guid restaurantId)
        {
            var userId = visitorId.ToString();

            // Get user's cart
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return new CheckoutViewModel
                {
                    CartItems = new List<CartItemViewModel>(),
                    RestaurantId = restaurantId
                };
            }

            // Get restaurant details
            var restaurant = await _restaurantRepository.GetRestaurantByIdAsync(cart.RestaurantId, includeMenuItems: false);

            // Get user's saved addresses
            var addresses = await _addressRepository.GetUserAddressesAsync(userId);

            // Calculate totals
            var subtotal = cart.CartItems.Sum(ci => ci.TotalPrice);
            var deliveryFee = restaurant?.DeliveryFee ?? DEFAULT_DELIVERY_FEE;
            var tax = subtotal * TAX_RATE;
            var total = subtotal + tax + deliveryFee;

            var model = new CheckoutViewModel
            {
                RestaurantId = cart.RestaurantId,
                RestaurantName = restaurant?.Name ?? "Ресторан",
                CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    MenuItemId = ci.MenuItemId,
                    Name = ci.MenuItem.Name,
                    ImageUrl = ci.MenuItem.ImageUrl,
                    Price = ci.UnitPrice,
                    Quantity = ci.Quantity,
                    Customizations = ci.Customizations
                }).ToList(),
                SavedAddresses = addresses.Select(a => new AddressViewModel
                {
                    AddressId = a.AddressId,
                    FullAddress = a.FullAddress,
                    StreetAddress = a.AddressLine1,
                    City = a.City,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    IsDefault = a.IsDefault
                }).ToList(),
                Subtotal = subtotal,
                Tax = tax,
                DeliveryFee = deliveryFee,
                Total = total,
                TaxRate = TAX_RATE,
                FixedDeliveryFee = deliveryFee
            };

            // Set default address if exists
            var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault);
            if (defaultAddress != null)
            {
                model.SelectedAddressId = defaultAddress.AddressId;
            }

            return model;
        }

        public async Task<Order> CreateOrderAsync(CheckoutViewModel model, Guid userId)
        {
            var userIdString = userId.ToString();

            // Get cart
            var cart = await _cartRepository.GetActiveCartByUserIdAsync(userIdString);
            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Кошик порожній");
            }

            // Get or create delivery address
            Guid? deliveryAddressId = null;
            string deliveryAddressString = string.Empty;

            if (model.UseExistingAddress && model.SelectedAddressId.HasValue)
            {
                var address = await _addressRepository.GetByIdAsync(model.SelectedAddressId.Value);
                if (address != null)
                {
                    deliveryAddressId = address.AddressId;
                    deliveryAddressString = address.FullAddress;
                }
            }
            else if (!string.IsNullOrEmpty(model.NewStreetAddress))
            {
                // Create new address
                var newAddress = new Models.Entities.Address
                {
                    AddressId = Guid.NewGuid(),
                    UserId = userIdString,
                    AddressLine1 = model.NewStreetAddress,
                    City = model.NewCity ?? "Київ",
                    PostalCode = model.NewPostalCode ?? "01001",
                    Country = model.NewCountry ?? "Ukraine",
                    IsDefault = false,
                    CreatedDate = DateTime.UtcNow
                };

                await _addressRepository.CreateAsync(newAddress);
                deliveryAddressId = newAddress.AddressId;
                deliveryAddressString = newAddress.FullAddress;
            }

            // Get restaurant
            var restaurant = await _restaurantRepository.GetRestaurantByIdAsync(cart.RestaurantId, includeMenuItems: false);
            var deliveryFee = restaurant?.DeliveryFee ?? DEFAULT_DELIVERY_FEE;

            // Calculate totals
            var subtotal = cart.CartItems.Sum(ci => ci.TotalPrice);
            var tax = subtotal * TAX_RATE;
            var total = subtotal + tax + deliveryFee;

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            // Create order
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userIdString,
                RestaurantId = cart.RestaurantId,
                OrderStatus = "Pending",
                DeliveryAddressId = deliveryAddressId,
                DeliveryInstructions = model.SpecialInstructions,
                SubtotalAmount = subtotal,
                TaxAmount = tax,
                DeliveryFee = deliveryFee,
                DiscountAmount = 0,
                TotalAmount = total,
                OrderDate = DateTime.UtcNow,
                EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(restaurant?.EstimatedDeliveryTime ?? 45),
                CreatedDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            // Create order items
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    MenuItemId = cartItem.MenuItemId,
                    MenuItemName = cartItem.MenuItem.Name,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice,
                    Customizations = cartItem.Customizations
                };
                _context.OrderItems.Add(orderItem);
            }

            // Create payment record
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                OrderId = order.OrderId,
                PaymentMethod = model.PaymentMethod == "CreditCard" ? "CreditCard" : "CashOnDelivery",
                PaymentStatus = model.PaymentMethod == "CashOnDelivery" ? "Pending" : "Processing",
                Amount = total,
                PaymentGateway = model.PaymentMethod == "CreditCard" ? "Stripe" : "Cash",
                PaymentDate = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            // Clear cart after successful order creation
            await _cartRepository.ClearCartAsync(cart.CartId);
            await _cartRepository.DeleteCartAsync(cart.CartId);

            _logger.LogInformation($"Order {orderNumber} created successfully for user {userId}");

            return order;
        }

        public async Task<OrderConfirmationViewModel> GetOrderConfirmationAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Restaurant)
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Include(o => o.DeliveryAddress)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return null!;
            }

            return new OrderConfirmationViewModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                RestaurantName = order.Restaurant?.Name ?? "Ресторан",
                RestaurantPhone = order.Restaurant?.PhoneNumber ?? "",
                OrderItems = order.OrderItems.Select(oi => new OrderItemDetailViewModel
                {
                    ItemName = oi.MenuItemName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Total = oi.TotalPrice,
                    Customizations = oi.Customizations
                }).ToList(),
                Subtotal = order.SubtotalAmount,
                Tax = order.TaxAmount,
                DeliveryFee = order.DeliveryFee,
                Total = order.TotalAmount,
                DeliveryAddress = order.DeliveryAddress?.FullAddress ?? "Адресу не вказано",
                ContactPhone = "", // Could be fetched from UserProfile
                SpecialInstructions = order.DeliveryInstructions,
                PaymentMethod = order.Payment?.PaymentMethod ?? "Не вказано",
                PaymentStatus = order.Payment?.PaymentStatus ?? "Pending",
                EstimatedDeliveryTime = order.EstimatedDeliveryTime
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            order.OrderStatus = newStatus;
            order.UpdatedDate = DateTime.UtcNow;

            // Set specific timestamp based on status
            switch (newStatus)
            {
                case "Confirmed":
                    order.ConfirmedDate = DateTime.UtcNow;
                    break;
                case "Preparing":
                    order.PreparingDate = DateTime.UtcNow;
                    break;
                case "Ready":
                    order.ReadyDate = DateTime.UtcNow;
                    break;
                case "OutForDelivery":
                    order.OutForDeliveryDate = DateTime.UtcNow;
                    break;
                case "Delivered":
                    order.DeliveredDate = DateTime.UtcNow;
                    break;
                case "Cancelled":
                    order.CancelledDate = DateTime.UtcNow;
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Order {order.OrderNumber} status updated to {newStatus}");

            return true;
        }

        public async Task<string> CreateStripeCheckoutSessionAsync(Order order)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var orderWithItems = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            if (orderWithItems == null)
            {
                throw new InvalidOperationException("Замовлення не знайдено");
            }

            var lineItems = new List<SessionLineItemOptions>();

            // Add menu items
            foreach (var item in orderWithItems.OrderItems)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = _stripeSettings.Currency,
                        UnitAmount = (long)(item.UnitPrice * 100), // Convert to cents
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.MenuItemName,
                            Description = item.Customizations
                        }
                    },
                    Quantity = item.Quantity
                });
            }

            // Add delivery fee
            if (orderWithItems.DeliveryFee > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = _stripeSettings.Currency,
                        UnitAmount = (long)(orderWithItems.DeliveryFee * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Доставка",
                            Description = $"Доставка з {orderWithItems.Restaurant?.Name}"
                        }
                    },
                    Quantity = 1
                });
            }

            // Add tax
            if (orderWithItems.TaxAmount > 0)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = _stripeSettings.Currency,
                        UnitAmount = (long)(orderWithItems.TaxAmount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "ПДВ (10%)",
                            Description = "Податок на додану вартість"
                        }
                    },
                    Quantity = 1
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"https://localhost:7224/Customer/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}&orderId={order.OrderId}",
                CancelUrl = $"https://localhost:7224/Customer/Checkout/Cancel?orderId={order.OrderId}",
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", order.OrderId.ToString() },
                    { "OrderNumber", order.OrderNumber }
                },
                CustomerEmail = null, // Could be fetched from user
                Locale = "uk"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // Update payment with session ID
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.OrderId);
            if (payment != null)
            {
                payment.TransactionId = session.Id;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Stripe session {session.Id} created for order {order.OrderNumber}");

            return session.Url; // Return the Stripe Checkout URL
        }

        public async Task<bool> ProcessPaymentSuccessAsync(string sessionId, Guid orderId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                if (session.PaymentStatus == "paid")
                {
                    // Update payment status
                    var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                    if (payment != null)
                    {
                        payment.PaymentStatus = "Completed";
                        payment.CompletedDate = DateTime.UtcNow;
                        payment.TransactionId = session.PaymentIntentId ?? session.Id;
                        payment.GatewayResponse = $"Payment completed via Stripe. Session: {sessionId}";
                    }

                    // Update order status
                    await UpdateOrderStatusAsync(orderId, "Confirmed");

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Payment for order {orderId} processed successfully");
                    return true;
                }

                _logger.LogWarning($"Payment for order {orderId} not completed. Status: {session.PaymentStatus}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing payment for order {orderId}: {ex.Message}");
                return false;
            }
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.UtcNow;
            var year = today.Year;

            // Get count of orders today for sequence number
            var todayStart = today.Date;
            var todayEnd = todayStart.AddDays(1);

            var todayOrderCount = await _context.Orders
                .Where(o => o.OrderDate >= todayStart && o.OrderDate < todayEnd)
                .CountAsync();

            var sequenceNumber = (todayOrderCount + 1).ToString("D6");

            return $"ORD-{year}-{sequenceNumber}";
        }
    }
}