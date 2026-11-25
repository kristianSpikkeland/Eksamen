
using System.ComponentModel.DataAnnotations;

namespace CourseAPI.DTOs.Requests
{
    public class CreateUserRequest
    {
        [EmailAddress(ErrorMessage = "Ugyldig epost")]
        [Required(ErrorMessage = "Epost må fylles ut")]
        [MaxLength(254, ErrorMessage = "For lang epost, maksimum {1} tegn")]
        public string Email { get; set; } = null!;


        [Required(ErrorMessage = "Passord må fylles ut")]
        [MaxLength(100, ErrorMessage = "For langt passord, maksimum {1} tegn")]
        [MinLength(8, ErrorMessage = "For kort passord, minimum {1} tegn")]
        [RegularExpression(@"^(?=.*[^\p{L}\p{N}]).+$", ErrorMessage = "Passordet må inneholde minst ett spesialtegn")]
        public string Password { get; set; } = null!;


        [Required(ErrorMessage = "Navn må fylles ut")]
        [MaxLength(120, ErrorMessage = "For langt navn, maks {1} tegn")]
        public string DisplayName { get; set; } = null!;

    }
}
