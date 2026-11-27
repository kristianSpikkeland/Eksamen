using System.ComponentModel.DataAnnotations;

namespace CourseAPI.Services.Validation
{
    public static class RequestValidator
    {
        // Validerer DataAnnotations og IValidatableObject på en request-DTO.
        // Kaster InvalidRequestException med feilene gruppert per felt hvis noe er ugyldig.
        public static void Validate(object request)
        {
            var results = new List<ValidationResult>();
            if (Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true))
            {
                return;
            }

            var errors = results
                .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty)
                    .Select(member => new { Member = member, Message = r.ErrorMessage ?? "Ugyldig verdi" }))
                .GroupBy(e => e.Member)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());

            throw new InvalidRequestException(errors);
        }
    }
}