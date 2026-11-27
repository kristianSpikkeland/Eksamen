namespace CourseAPI.Services.Validation
{
    // Kastes av service-laget når en request ikke består valideringen.
    // Errors grupperer feilmeldingene per felt, slik Results.ValidationProblem forventer.
    public sealed class InvalidRequestException(IDictionary<string, string[]> errors): Exception("Ugyldige data")
    {
        public IDictionary<string, string[]> Errors { get; } = errors;
    } 
}
