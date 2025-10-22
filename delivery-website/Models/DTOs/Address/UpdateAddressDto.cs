using System.ComponentModel.DataAnnotations;

namespace delivery_website.Models.DTOs.Address
{
    public class UpdateAddressDto
    {
        [Required]
        public Guid AddressId { get; set; }

        [Required(ErrorMessage = "Адреса є обов'язковою")]
        [StringLength(200)]
        public string AddressLine1 { get; set; } = null!;

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Місто є обов'язковим")]
        [StringLength(100)]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Поштовий індекс є обов'язковим")]
        [StringLength(20)]
        public string PostalCode { get; set; } = null!;

        public bool IsDefault { get; set; }

        [StringLength(50)]
        public string? Label { get; set; }
    }
}