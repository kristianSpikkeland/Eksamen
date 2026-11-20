# 20.11
## Prosjektoppsett
### Prompt
Har fått som eksamens oppgave å lage en kursløsning med forskjellige roller admin, deltaker osv. Videre skal man bruke jwt token for blant annet registrering og innlogging. Planen min er å opprette et Blazor Web assembly prosjekt siden da har jeg en tydelig server og klient. Dette er mest siden oppgaven krever DTOer og Requester. Spørsmålet mitt er om jeg bør opprette et nytt prosjekt i løsningen for APIet. Tenker det hadde vært ryddig med tanke på at oppgaven krever jwt token og en av fordelene med det er at jwt er egnet for å ha flere klienter. Da vil jeg få et tydelig skille mellom APIet og Blazor serveren. Usikker på om det i det hele tatt er en fordel? Svar kort, legger ved oppgavetekst.

### Svar
Kort svar: dropp Blazor og lag et rent ASP.NET Core Web API-prosjekt. Da er spørsmålet om et eget API-prosjekt allerede løst.

Oppgaven ber bare om et REST API. Det står ingenting om frontend, og alt skal demonstreres med Swagger og en .http-fil. Et Blazor-grensesnitt gir deg mye ekstra arbeid uten at det teller i vurderingen. Det trekker også tid bort fra det som faktisk vurderes, som samtidighet ved påmelding, eierskapskontroll, rate limiting og tester.

Legg også merke til at rollene skal være Participant og Instructor, ikke admin.

Anbefalt oppsett i løsningen:

Kursplassen.sln
├── Kursplassen.Api      (Web API: controllere, DTO-er, tjenester, EF Core)
└── Kursplassen.Tests    (xUnit: enhetstester + integrasjonstest)

Integrasjonstesten blir enklest når API-et er et eget prosjekt, fordi du kan bruke WebApplicationFactory<Program> direkte.

Hvis du likevel vil ha Blazor som en bonus, har du rett i at API-et da bør være et eget prosjekt, med Blazor som en ren klient som bruker JWT. Men bygg API-et ferdig først, og ta eventuelt Blazor til slutt hvis du har tid igjen.

### Ny Prompt
Men hva med DTOer og Requester. Disse må ivaretas på en god måte slik at jeg viser forståelse. DTOene tenker jeg er greie å legge i API løsningen, men requestene hører jo hjemme på klientsiden.

### Svar
Her ligger det en misforståelse. Oppgaven sier «separate request- og response-DTO-er». Det betyr ikke klientkode som sender forespørsler. Det betyr to typer DTO-er, og begge hører hjemme i API-et:

Request-DTO: formen på dataene API-et tar imot i body-en.
Response-DTO: formen på dataene API-et sender tilbake.

```C#
// Request: det klienten får lov å sende inn
public record CreateCourseRequest(
    string Title, string Description,
    DateTime StartsAtUtc, DateTime EndsAtUtc,
    string Location, int Capacity);

// Response: det klienten får se
public record CourseDetailsResponse(
    Guid Id, string Title, string Description,
    DateTime StartsAtUtc, DateTime EndsAtUtc,
    string Location, int Capacity, int RegisteredCount, int AvailableSeats);

Forståelsen du skal vise, ligger i hva som er med og hva som mangler:

CreateCourseRequest har ikke OwnerUserId. Eieren hentes fra JWT-en.
RegisterRequest har ikke Role. Systemet setter den til Participant.
UserResponse har ikke PasswordHash.
CourseDetailsResponse har beregnede felter (AvailableSeats) og ingen deltakerinfo.

Forslag til mappestruktur i API-prosjektet:
Kursplassen.Api/
├── Contracts/
│   ├── Requests/   (RegisterRequest, LoginRequest, CreateCourseRequest, UpdateCourseRequest)
│   └── Responses/  (AuthResponse, CourseSummaryResponse, CourseDetailsResponse, RegistrationResponse)
├── Entities/       (User, Course, Registration: kun mot databasen)
├── Services/
├── Controllers/
└── Data/

De faktiske HTTP-kallene fra klientsiden leverer du som en .http-fil (eller Postman/curl). Den er påkrevd i oppgave 2 og erstatter behovet for en klient.
```
