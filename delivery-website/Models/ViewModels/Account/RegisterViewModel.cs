using System.ComponentModel.DataAnnotations;

namespace delivery_website.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ім'я є обов'язковим")]
        [StringLength(100, ErrorMessage = "Ім'я не може перевищувати 100 символів")]
        [Display(Name = "Ім'я")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Прізвище є обов'язковим")]
        [StringLength(100, ErrorMessage = "Прізвище не може перевищувати 100 символів")]
        [Display(Name = "Прізвище")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Номер телефону є обов'язковим")]
        [Phone(ErrorMessage = "Невірний формат номера телефону")]
        [Display(Name = "Номер телефону")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [StringLength(100, ErrorMessage = "Пароль повинен містити мінімум {2} символів", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження паролю")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = null!;

        [Display(Name = "Я погоджуюсь з умовами використання")]
        public bool AgreeToTerms { get; set; }
    }
}