using delivery_website.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace delivery_website.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            // check if data exists
            if (context.Restaurants.Any())
            {
                Console.WriteLine("База даних вже містить дані. Seeding пропущено.");
                return;
            }

            Console.WriteLine("Починаємо заповнення бази даних...");

            var owner = new IdentityUser
            {
                UserName = "owner@restaurant.com",
                Email = "owner@restaurant.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(owner, "Owner123!");

            if (!result.Succeeded)
            {
                Console.WriteLine("❌ Помилка при створенні користувача");
                return;
            }

            Console.WriteLine("✓ Створено тестового власника ресторану");

            var restaurants = new List<Restaurant>
            {
                new Restaurant
                {
                    RestaurantId = Guid.NewGuid(),
                    OwnerId = owner.Id,
                    Name = "Pizza Paradise",
                    Description = "Найкраща піца в місті з традиційними італійськими рецептами. Використовуємо лише свіжі інгредієнти та секретний соус.",
                    CuisineType = "Італійська",
                    PhoneNumber = "+380671234567",
                    Email = "pizza@paradise.ua",
                    AddressLine1 = "вул. Хрещатик, 15",
                    City = "Київ",
                    PostalCode = "01001",
                    MinimumOrderAmount = 150,
                    DeliveryFee = 50,
                    EstimatedDeliveryTime = 30,
                    IsActive = true,
                    IsApproved = true,
                    AverageRating = 4.5m,
                    TotalReviews = 120,
                    ApprovedDate = DateTime.UtcNow
                },
                new Restaurant
                {
                    RestaurantId = Guid.NewGuid(),
                    OwnerId = owner.Id,
                    Name = "Burger House",
                    Description = "Соковиті бургери та картопля фрі. Готуємо з натуральної яловичини та подаємо з фірмовими соусами.",
                    CuisineType = "Американська",
                    PhoneNumber = "+380672345678",
                    Email = "info@burgerhouse.ua",
                    AddressLine1 = "вул. Велика Васильківська, 25",
                    City = "Київ",
                    PostalCode = "01004",
                    MinimumOrderAmount = 100,
                    DeliveryFee = 40,
                    EstimatedDeliveryTime = 25,
                    IsActive = true,
                    IsApproved = true,
                    AverageRating = 4.7m,
                    TotalReviews = 89,
                    ApprovedDate = DateTime.UtcNow
                },
                new Restaurant
                {
                    RestaurantId = Guid.NewGuid(),
                    OwnerId = owner.Id,
                    Name = "Sushi Master",
                    Description = "Автентичні японські суші та роли від досвідчених суші-майстрів. Щодня свіжа риба та морепродукти.",
                    CuisineType = "Японська",
                    PhoneNumber = "+380673456789",
                    Email = "order@sushimaster.ua",
                    AddressLine1 = "пр. Перемоги, 50",
                    City = "Київ",
                    PostalCode = "01135",
                    MinimumOrderAmount = 200,
                    DeliveryFee = 60,
                    EstimatedDeliveryTime = 40,
                    IsActive = true,
                    IsApproved = true,
                    AverageRating = 4.8m,
                    TotalReviews = 156,
                    ApprovedDate = DateTime.UtcNow
                },
                new Restaurant
                {
                    RestaurantId = Guid.NewGuid(),
                    OwnerId = owner.Id,
                    Name = "Pasta Bella",
                    Description = "Свіжа паста та автентичні італійські страви. Готуємо за традиційними рецептами Неаполя.",
                    CuisineType = "Італійська",
                    PhoneNumber = "+380674567890",
                    Email = "hello@pastabella.ua",
                    AddressLine1 = "вул. Саксаганського, 33",
                    City = "Київ",
                    PostalCode = "01033",
                    MinimumOrderAmount = 180,
                    DeliveryFee = 45,
                    EstimatedDeliveryTime = 35,
                    IsActive = true,
                    IsApproved = true,
                    AverageRating = 4.6m,
                    TotalReviews = 95,
                    ApprovedDate = DateTime.UtcNow
                },
                new Restaurant
                {
                    RestaurantId = Guid.NewGuid(),
                    OwnerId = owner.Id,
                    Name = "Українська хата",
                    Description = "Домашня українська кухня з душею. Вареники, борщ, деруни - все як у бабусі!",
                    CuisineType = "Українська",
                    PhoneNumber = "+380675678901",
                    Email = "info@ukrhata.ua",
                    AddressLine1 = "вул. Шевченка, 12",
                    City = "Київ",
                    PostalCode = "01030",
                    MinimumOrderAmount = 120,
                    DeliveryFee = 35,
                    EstimatedDeliveryTime = 40,
                    IsActive = true,
                    IsApproved = true,
                    AverageRating = 4.4m,
                    TotalReviews = 78,
                    ApprovedDate = DateTime.UtcNow
                },
                new Restaurant
                {
                    RestaurantId = Guid.NewGuid(),
                    OwnerId = owner.Id,
                    Name = "Taco Fiesta",
                    Description = "Мексиканська кухня: тако, буріто, начос. Гострі страви з оригінальними мексиканськими спеціями.",
                    CuisineType = "Мексиканська",
                    PhoneNumber = "+380676789012",
                    Email = "hola@tacofiesta.ua",
                    AddressLine1 = "вул. Льва Толстого, 5",
                    City = "Київ",
                    PostalCode = "01004",
                    MinimumOrderAmount = 150,
                    DeliveryFee = 50,
                    EstimatedDeliveryTime = 30,
                    IsActive = true,
                    IsApproved = true,
                    AverageRating = 4.3m,
                    TotalReviews = 64,
                    ApprovedDate = DateTime.UtcNow
                }
            };

            await context.Restaurants.AddRangeAsync(restaurants);
            await context.SaveChangesAsync();

            Console.WriteLine($"✓ Створено {restaurants.Count} ресторанів");

            var pizzaRestaurant = restaurants[0];

            var pizzaCategory = new Category
            {
                CategoryId = Guid.NewGuid(),
                RestaurantId = pizzaRestaurant.RestaurantId,
                Name = "Піца",
                Description = "Класична італійська піца на тонкому тісті",
                SortOrder = 1
            };

            await context.Categories.AddAsync(pizzaCategory);
            await context.SaveChangesAsync();

            var pizzaMenuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = pizzaRestaurant.RestaurantId,
                    CategoryId = pizzaCategory.CategoryId,
                    Name = "Маргарита",
                    Description = "Томатний соус, моцарела, свіжий базилік, оливкова олія",
                    Price = 180,
                    IsAvailable = true,
                    PreparationTime = 20
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = pizzaRestaurant.RestaurantId,
                    CategoryId = pizzaCategory.CategoryId,
                    Name = "Пепероні",
                    Description = "Томатний соус, моцарела, пепероні, орегано",
                    Price = 220,
                    IsAvailable = true,
                    PreparationTime = 20
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = pizzaRestaurant.RestaurantId,
                    CategoryId = pizzaCategory.CategoryId,
                    Name = "4 Сири",
                    Description = "Моцарела, горгонзола, пармезан, чеддер",
                    Price = 250,
                    IsAvailable = true,
                    PreparationTime = 22
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = pizzaRestaurant.RestaurantId,
                    CategoryId = pizzaCategory.CategoryId,
                    Name = "Капрічоза",
                    Description = "Томатний соус, моцарела, шинка, печериці, артишоки",
                    Price = 240,
                    IsAvailable = true,
                    PreparationTime = 25
                }
            };

            var burgerRestaurant = restaurants[1];

            var burgerCategory = new Category
            {
                CategoryId = Guid.NewGuid(),
                RestaurantId = burgerRestaurant.RestaurantId,
                Name = "Бургери",
                Description = "Соковиті бургери з натуральної яловичини",
                SortOrder = 1
            };

            await context.Categories.AddAsync(burgerCategory);
            await context.SaveChangesAsync();

            var burgerMenuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = burgerRestaurant.RestaurantId,
                    CategoryId = burgerCategory.CategoryId,
                    Name = "Класичний бургер",
                    Description = "Яловича котлета 200г, салат, помідор, цибуля, соус",
                    Price = 120,
                    IsAvailable = true,
                    PreparationTime = 15
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = burgerRestaurant.RestaurantId,
                    CategoryId = burgerCategory.CategoryId,
                    Name = "Чізбургер",
                    Description = "Яловича котлета 200г, подвійний чеддер, фірмовий соус",
                    Price = 140,
                    IsAvailable = true,
                    PreparationTime = 15
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = burgerRestaurant.RestaurantId,
                    CategoryId = burgerCategory.CategoryId,
                    Name = "Бекон бургер",
                    Description = "Яловича котлета 200г, хрусткий бекон, сир, соус BBQ",
                    Price = 160,
                    IsAvailable = true,
                    PreparationTime = 18
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = burgerRestaurant.RestaurantId,
                    CategoryId = burgerCategory.CategoryId,
                    Name = "Подвійний бургер",
                    Description = "Дві яловичі котлети по 150г, сир, овочі, спеціальний соус",
                    Price = 200,
                    IsAvailable = true,
                    PreparationTime = 20
                }
            };

            var sushiRestaurant = restaurants[2];

            var sushiCategory = new Category
            {
                CategoryId = Guid.NewGuid(),
                RestaurantId = sushiRestaurant.RestaurantId,
                Name = "Суші та роли",
                Description = "Свіжі суші та роли від майстрів",
                SortOrder = 1
            };

            await context.Categories.AddAsync(sushiCategory);
            await context.SaveChangesAsync();

            var sushiMenuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = sushiRestaurant.RestaurantId,
                    CategoryId = sushiCategory.CategoryId,
                    Name = "Філадельфія",
                    Description = "Лосось, вершковий сир, огірок, авокадо (8 шт)",
                    Price = 280,
                    IsAvailable = true,
                    PreparationTime = 25
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = sushiRestaurant.RestaurantId,
                    CategoryId = sushiCategory.CategoryId,
                    Name = "Каліфорнія",
                    Description = "Краб, авокадо, огірок, ікра тобіко (8 шт)",
                    Price = 250,
                    IsAvailable = true,
                    PreparationTime = 25
                },
                new MenuItem
                {
                    MenuItemId = Guid.NewGuid(),
                    RestaurantId = sushiRestaurant.RestaurantId,
                    CategoryId = sushiCategory.CategoryId,
                    Name = "Сет Асорті",
                    Description = "Мікс з 24 різних суші та ролів",
                    Price = 650,
                    IsAvailable = true,
                    PreparationTime = 35
                }
            };

            var allMenuItems = new List<MenuItem>();
            allMenuItems.AddRange(pizzaMenuItems);
            allMenuItems.AddRange(burgerMenuItems);
            allMenuItems.AddRange(sushiMenuItems);

            await context.MenuItems.AddRangeAsync(allMenuItems);
            await context.SaveChangesAsync();

            Console.WriteLine($"✓ Створено {allMenuItems.Count} страв у меню");
            Console.WriteLine("🎉 База даних успішно заповнена!");
        }
    }
}