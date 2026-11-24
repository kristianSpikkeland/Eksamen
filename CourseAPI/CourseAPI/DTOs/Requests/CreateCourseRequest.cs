using CourseAPI.Entities;
using System.ComponentModel.DataAnnotations;


namespace CourseAPI.DTOs.Requests
{
    public class CreateCourseRequest : IValidatableObject
    {
       
        [MaxLength(180, ErrorMessage = "For lang tittel på kurs")]
        [Required(ErrorMessage = "Tittel må fylles ut for kurs")]
        public string Title { get; set; } = null!;


        [StringLength(400, ErrorMessage = "Beskrivelsen av kurset er for lang")]
        [Required(ErrorMessage = "Beskrivelse av kurs må fylles ut")]
        public string Description { get; set; } = null!;


        [Required(ErrorMessage = "Starttid for kurs må fylles ut")]
        public DateTime? StartsAtUtc { get; set; }


        [Required(ErrorMessage = "Sluttid for kurs må fylles ut for kurs")]
        public DateTime? EndsAtUtc { get; set; }


        [MaxLength(180, ErrorMessage = "For lang kurslokasjon")]
        [Required(ErrorMessage = "Lokasjon må fylles ut for kurs")]
        public string Location { get; set; } = null!;


        [Required(ErrorMessage = "Kapasitet for kurs må fylles ut")]
        [Range(1, 2000, ErrorMessage = "Ugyldig kapasitet for kurs")] 
        public int? Capacity { get; set; }


        [Required(ErrorMessage = "Status for kurs må fylles ut")]
        [EnumDataType(typeof(CourseStatus), ErrorMessage = "Ugyldig status for kurs")]
        public CourseStatus? Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndsAtUtc <= StartsAtUtc)
            {
                yield return new ValidationResult(
                    "Sluttid må være etter starttid",
                    new[] { nameof(EndsAtUtc) });
            }
        }
    }
}
