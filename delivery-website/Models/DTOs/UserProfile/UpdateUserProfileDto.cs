using System.ComponentModel.DataAnnotations;

namespace delivery_website.Models.DTOs.UserProfile
{
    public class UpdateUserProfileDto
    {
        [Required(ErrorMessage = "Ім'я є обов'язковим")]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Прізвище є обов'язковим")]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Номер телефону є обов'язковим")]
        [Phone(ErrorMessage = "Невірний формат номера телефону")]
        public string PhoneNumber { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string PreferredLanguage { get; set; } = "uk";
    }
}