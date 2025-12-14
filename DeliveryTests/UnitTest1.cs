using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DeliveryTestss
{
    public class RestaurantServiceFakes
    {
        private List<Restaurant> _restaurants = new();

        public async Task Add(Restaurant restaurant)
        {
            _restaurants.Add(restaurant);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsAsync()
        {
            return await Task.FromResult(_restaurants);
        }

        public async Task Update(Restaurant restaurant)
        {
            var existing = _restaurants.FirstOrDefault(r => r.RestaurantId == restaurant.RestaurantId);
            if (existing != null)
            {
                _restaurants.Remove(existing);
                _restaurants.Add(restaurant);
            }
            await Task.CompletedTask;
        }

        public async Task Remove(Guid id)
        {
            var restaurant = _restaurants.FirstOrDefault(r => r.RestaurantId == id);
            if (restaurant != null)
            {
                _restaurants.Remove(restaurant);
            }
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
        private List<CartItem> _cartItems = new();

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
            _cartItems.Add(item);

            return await Task.FromResult(cart);
        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            return await Task.FromResult(_carts.FirstOrDefault(c => c.UserId == userId));
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UserId == userId);
            if (cart != null)
            {
                cart.Items.Clear();
            }
            await Task.CompletedTask;
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

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId)
        {
            return await Task.FromResult(_orders.Where(o => o.UserId == userId));
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

    // ТЕСТИ
    public class RestaurantTests
    {
        [Fact]
        public async Task AddRestaurant_ShouldIncreaseRestaurantCount()
        {
            // Arrange
            var service = new RestaurantServiceFake();
            var restaurant = new Restaurant
            {
                RestaurantId = Guid.NewGuid(),
                Name = "Pizza Palace",
                CuisineType = "Італійська",
                MinimumOrderAmount = 100,
                IsActive = true
            };

            // Act
            await service.Add(restaurant);
            var restaurants = await service.GetRestaurantsAsync();

            // Assert
            Assert.Single(restaurants);
            Assert.Equal("Pizza Palace", restaurants.First().Name);
        }

        [Fact]
        public async Task UpdateRestaurant_ShouldModifyRestaurant()
        {
            // Arrange
            var service = new RestaurantServiceFake();
            var restaurant = new Restaurant
            {
                RestaurantId = Guid.NewGuid(),
                Name = "Old Name",
                CuisineType = "Українська",
                MinimumOrderAmount = 100,
                IsActive = true
            };

            await service.Add(restaurant);

            // Act
            restaurant.Name = "New Name";
            restaurant.MinimumOrderAmount = 150;
            await service.Update(restaurant);

            var restaurants = await service.GetRestaurantsAsync();
            var updated = restaurants.First();

            // Assert
            Assert.Equal("New Name", updated.Name);
            Assert.Equal(150, updated.MinimumOrderAmount);
        }

        [Fact]
        public async Task RemoveRestaurant_ShouldDeleteRestaurant()
        {
            // Arrange
            var service = new RestaurantServiceFake();
            var restaurant = new Restaurant
            {
                RestaurantId = Guid.NewGuid(),
                Name = "Test Restaurant",
                CuisineType = "Японська",
                MinimumOrderAmount = 200,
                IsActive = true
            };

            await service.Add(restaurant);

            // Act
            await service.Remove(restaurant.RestaurantId);
            var restaurants = await service.GetRestaurantsAsync();

            // Assert
            Assert.Empty(restaurants);
        }

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
        public async Task ClearCart_ShouldRemoveAllItems()
        {
            // Arrange
            var service = new CartServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();

            await service.AddToCartAsync(userId, Guid.NewGuid(), restaurantId, 1, 50.00m);
            await service.AddToCartAsync(userId, Guid.NewGuid(), restaurantId, 2, 30.00m);

            // Act
            await service.ClearCartAsync(userId);
            var cart = await service.GetCartAsync(userId);

            // Assert
            Assert.Empty(cart.Items);
            Assert.Equal(0, cart.TotalAmount);
        }
    }

    public class OrderTests
    {
        [Fact]
        public async Task CreateOrder_WithValidAmount_ShouldSucceed()
        {
            // Arrange
            var service = new OrderServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new OrderItem { MenuItemId = Guid.NewGuid(), MenuItemName = "Піца Маргарита", Quantity = 2, UnitPrice = 150, TotalPrice = 300 }
            };

            // Act
            var order = await service.CreateOrderAsync(userId, restaurantId, items, 300);

            // Assert
            Assert.NotNull(order);
            Assert.Equal("Pending", order.OrderStatus);
            Assert.Equal(300, order.TotalAmount);
            Assert.Contains("ORD-", order.OrderNumber);
        }

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
        public async Task UpdateOrderStatus_DeliveredOrder_ShouldSetDeliveryDate()
        {
            // Arrange
            var service = new OrderServiceFake();
            var userId = "user123";
            var restaurantId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new OrderItem { MenuItemId = Guid.NewGuid(), MenuItemName = "Суші сет", Quantity = 1, UnitPrice = 350, TotalPrice = 350 }
            };

            var order = await service.CreateOrderAsync(userId, restaurantId, items, 350);

            // Act - Проходимо всі етапи до доставки
            await service.UpdateOrderStatusAsync(order.OrderId, "Confirmed");
            await service.UpdateOrderStatusAsync(order.OrderId, "Preparing");
            await service.UpdateOrderStatusAsync(order.OrderId, "Ready");
            await service.UpdateOrderStatusAsync(order.OrderId, "OutForDelivery");
            await service.UpdateOrderStatusAsync(order.OrderId, "Delivered");

            var deliveredOrder = await service.GetOrderByIdAsync(order.OrderId);

            // Assert
            Assert.Equal("Delivered", deliveredOrder.OrderStatus);
            Assert.NotNull(deliveredOrder.DeliveredDate);
        }
    }
}
