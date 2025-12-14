using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace delivery_website.Tests
{
    // Fake Services для ізоляції тестів
    public class RestaurantServiceFake
    {
        private List<Restaurant> _restaurants = new();

        public async Task Add(Restaurant restaurant)
        {
            _restaurants.Add(restaurant);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsByCuisineAsync(string cuisineType)
        {
            return await Task.FromResult(_restaurants.Where(r => r.CuisineType == cuisineType));
        }
    }

    public class CartServiceFake
    {
        private List<Cart> _carts = new();

        public async Task<Cart> AddToCartAsync(string userId, Guid menuItemId, Guid restaurantId, int quantity, decimal price)
        {
            var cart = _carts.FirstOrDefault(c => c.UserId == userId && c.RestaurantId == restaurantId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    RestaurantId = restaurantId,
                    Items = new List<CartItem>()
                };
                _carts.Add(cart);
            }

            var item = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                MenuItemId = menuItemId,
                Quantity = quantity,
                UnitPrice = price
            };

            cart.Items.Add(item);

            return await Task.FromResult(cart);
        }
    }

    public class OrderServiceFake
    {
        private List<Order> _orders = new();

        public async Task<Order> CreateOrderAsync(string userId, Guid restaurantId, List<OrderItem> items, decimal totalAmount)
        {
            // Валідація: перевірка мінімальної суми замовлення
            if (totalAmount < 50)
            {
                throw new InvalidOperationException("Мінімальна сума замовлення 50 грн");
            }

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = $"ORD-{DateTime.UtcNow.Year}-{_orders.Count + 1:000000}",
                UserId = userId,
                RestaurantId = restaurantId,
                OrderItems = items,
                TotalAmount = totalAmount,
                OrderStatus = "Pending",
                OrderDate = DateTime.UtcNow
            };

            _orders.Add(order);
            return await Task.FromResult(order);
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var order = _orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return false;
            }

            // Валідація переходів статусів
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "Confirmed", "Cancelled" } },
                { "Confirmed", new List<string> { "Preparing", "Cancelled" } },
                { "Preparing", new List<string> { "Ready", "Cancelled" } },
                { "Ready", new List<string> { "OutForDelivery", "Cancelled" } },
                { "OutForDelivery", new List<string> { "Delivered", "Cancelled" } },
                { "Delivered", new List<string>() },
                { "Cancelled", new List<string>() }
            };

            if (!validTransitions[order.OrderStatus].Contains(newStatus))
            {
                throw new InvalidOperationException($"Неможливо змінити статус з {order.OrderStatus} на {newStatus}");
            }

            order.OrderStatus = newStatus;

            if (newStatus == "Confirmed")
                order.ConfirmedDate = DateTime.UtcNow;
            else if (newStatus == "Delivered")
                order.DeliveredDate = DateTime.UtcNow;
            else if (newStatus == "Cancelled")
                order.CancelledDate = DateTime.UtcNow;

            return await Task.FromResult(true);
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId)
        {
            return await Task.FromResult(_orders.FirstOrDefault(o => o.OrderId == orderId));
        }
    }

    // Моделі для тестів
    public class Restaurant
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; }
        public string CuisineType { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public bool IsActive { get; set; }
    }

    public class Cart
    {
        public Guid CartId { get; set; }
        public string UserId { get; set; }
        public Guid RestaurantId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }

    public class CartItem
    {
        public Guid CartItemId { get; set; }
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class Order
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public Guid RestaurantId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
    }

    public class OrderItem
    {
        public Guid MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // ТЕСТИ - Reduced to essential business logic tests only
    public class RestaurantTests
    {
        [Fact]
        public async Task GetRestaurantsByCuisine_ShouldFilterCorrectly()
        {
            // Arrange
            var service = new RestaurantServiceFake();

            await service.Add(new Restaurant { RestaurantId = Guid.NewGuid(), Name = "Pizza Place", CuisineType = "Італійська", MinimumOrderAmount = 100, IsActive = true });
            await service.Add(new Restaurant { RestaurantId = Guid.NewGuid(), Name = "Sushi Bar", CuisineType = "Японська", MinimumOrderAmount = 150, IsActive = true });
            await service.Add(new Restaurant { RestaurantId = Guid.NewGuid(), Name = "Pasta House", CuisineType = "Італійська", MinimumOrderAmount = 120, IsActive = true });

            // Act
            var italianRestaurants = await service.GetRestaurantsByCuisineAsync("Італійська");

            // Assert
            Assert.Equal(2, italianRestaurants.Count());
            Assert.All(italianRestaurants, r => Assert.Equal("Італійська", r.CuisineType));
        }

        [Fact]
        public async Task GetRestaurantsByCuisine_WithNonExistentCuisine_ShouldReturnEmpty()
        {
            // Arrange
            var service = new RestaurantServiceFake();

            await service.Add(new Restaurant { RestaurantId = Guid.NewGuid(), Name = "Pizza Place", CuisineType = "Італійська", MinimumOrderAmount = 100, IsActive = true });
            await service.Add(new Restaurant { RestaurantId = Guid.NewGuid(), Name = "Sushi Bar", CuisineType = "Японська", MinimumOrderAmount = 150, IsActive = true });

            // Act
            var mexicanRestaurants = await service.GetRestaurantsByCuisineAsync("Мексиканська");

            // Assert
            Assert.Empty(mexicanRestaurants);
        }
    }

    public class CartTests
    {
        [Fact]
        public async Task AddToCart_ShouldCreateCartAndAddItem()
        {
            // Arrange
            var service = new CartServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();
            var menuItemId = Guid.NewGuid();

            // Act
            var cart = await service.AddToCartAsync(userId, menuItemId, restaurantId, 2, 50.00m);

            // Assert
            Assert.NotNull(cart);
            Assert.Single(cart.Items);
            Assert.Equal(2, cart.Items.First().Quantity);
            Assert.Equal(100.00m, cart.TotalAmount);
        }

        [Fact]
        public async Task AddToCart_MultipleItems_ShouldCalculateTotalCorrectly()
        {
            // Arrange
            var service = new CartServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();

            // Act - Add multiple items
            await service.AddToCartAsync(userId, Guid.NewGuid(), restaurantId, 2, 50.00m);
            await service.AddToCartAsync(userId, Guid.NewGuid(), restaurantId, 1, 75.00m);
            var cart = await service.AddToCartAsync(userId, Guid.NewGuid(), restaurantId, 3, 30.00m);

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(4, cart.Items.Count);
            Assert.Equal(265.00m, cart.TotalAmount); // (2*50) + (1*75) + (3*30) = 100 + 75 + 90 = 265
        }
    }

    public class OrderTests
    {
        [Fact]
        public async Task CreateOrder_BelowMinimumAmount_ShouldThrowException()
        {
            // Arrange
            var service = new OrderServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new OrderItem { MenuItemId = Guid.NewGuid(), MenuItemName = "Напій", Quantity = 1, UnitPrice = 30, TotalPrice = 30 }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateOrderAsync(userId, restaurantId, items, 30));

            Assert.Equal("Мінімальна сума замовлення 50 грн", exception.Message);
        }

        [Fact]
        public async Task UpdateOrderStatus_ValidTransition_ShouldSucceed()
        {
            // Arrange
            var service = new OrderServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new OrderItem { MenuItemId = Guid.NewGuid(), MenuItemName = "Бургер", Quantity = 1, UnitPrice = 120, TotalPrice = 120 }
            };

            var order = await service.CreateOrderAsync(userId, restaurantId, items, 120);

            // Act
            var result = await service.UpdateOrderStatusAsync(order.OrderId, "Confirmed");
            var updatedOrder = await service.GetOrderByIdAsync(order.OrderId);

            // Assert
            Assert.True(result);
            Assert.Equal("Confirmed", updatedOrder.OrderStatus);
            Assert.NotNull(updatedOrder.ConfirmedDate);
        }

        [Fact]
        public async Task UpdateOrderStatus_InvalidTransition_ShouldThrowException()
        {
            // Arrange
            var service = new OrderServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new OrderItem { MenuItemId = Guid.NewGuid(), MenuItemName = "Салат", Quantity = 1, UnitPrice = 80, TotalPrice = 80 }
            };

            var order = await service.CreateOrderAsync(userId, restaurantId, items, 80);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateOrderStatusAsync(order.OrderId, "Delivered"));

            Assert.Contains("Неможливо змінити статус", exception.Message);
        }

        [Fact]
        public async Task UpdateOrderStatus_FromCancelledState_ShouldThrowException()
        {
            // Arrange
            var service = new OrderServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new OrderItem { MenuItemId = Guid.NewGuid(), MenuItemName = "Десерт", Quantity = 1, UnitPrice = 60, TotalPrice = 60 }
            };

            var order = await service.CreateOrderAsync(userId, restaurantId, items, 60);
            await service.UpdateOrderStatusAsync(order.OrderId, "Cancelled");

            // Act & Assert - Cancelled is a terminal state, cannot transition to any other status
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateOrderStatusAsync(order.OrderId, "Confirmed"));

            Assert.Contains("Неможливо змінити статус", exception.Message);
        }
    }
}
