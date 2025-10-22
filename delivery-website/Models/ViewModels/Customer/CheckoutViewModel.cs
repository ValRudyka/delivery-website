using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace delivery_website.ViewModels.Customer
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel>
    CartItems
        { get; set; } = new List<CartItemViewModel>
        ();

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }

        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; }

        // Step 1: Delivery Information
        public bool UseExistingAddress { get; set; } = true;

        public List<AddressViewModel>
            SavedAddresses
        { get; set; } = new List<AddressViewModel>
                ();

        public Guid? SelectedAddressId { get; set; }

        // New address fields
        [StringLength(500)]
        public string NewStreetAddress { get; set; }

        [StringLength(100)]
        public string NewCity { get; set; }

        [StringLength(20)]
        public string NewPostalCode { get; set; }

        [StringLength(100)]
        public string NewCountry { get; set; }

        // Contact information
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; }

        [Display(Name = "Special Instructions")]
        [StringLength(1000)]
        public string SpecialInstructions { get; set; }

        // Step 2: Payment Method
        [Required(ErrorMessage = "Please select a payment method")]
        public string PaymentMethod { get; set; } // "CreditCard" or "CashOnDelivery"

        // Configuration
        public decimal TaxRate { get; set; } = 0.10m; // 10% tax rate
        public decimal FixedDeliveryFee { get; set; } = 5.00m;
    }

    public class CartItemViewModel
    {
        public Guid MenuItemId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public string Customizations { get; set; }
    }

    public class AddressViewModel
    {
        public Guid AddressId { get; set; }
        public string FullAddress { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }
    }
}
