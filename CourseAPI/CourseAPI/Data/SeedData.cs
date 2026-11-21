
using CourseAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CourseAPI.Data
{
    public static class SeedData
    {

        public static async Task SeedAsync(KursplassenDbContext db)
        {
            // Returnerer om data allerede er lagt til
            if (await db.Users.AnyAsync(u => u.Email == "ida@example.com")) return;

            // Hasher passordene til dummy brukerne
            var hasher = new PasswordHasher<User>();

            // Alle dummy brukere har samme passord for å gjøre testing enklere
            const string TestPassword = "Eksamen123!";

            // Lager brukere
            var idaHansen = new User()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Ida Hansen",
                Email = "ida@example.com",
                Role = UserRole.Instructor,
                CreatedAtUtc = DateTime.UtcNow,
            };
            idaHansen.PasswordHash = hasher.HashPassword(idaHansen, TestPassword);


            var omarAli = new User()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Omar Ali",
                Email = "omar@example.com",
                Role = UserRole.Instructor,
                CreatedAtUtc = DateTime.UtcNow
            };
            omarAli.PasswordHash = hasher.HashPassword(omarAli, TestPassword);


            var noraBerg = new User()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Nora Berg",
                Email = "nora@example.com",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            };
            noraBerg.PasswordHash = hasher.HashPassword(noraBerg, TestPassword);


            var emilOlsen = new User()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Emil Olsen",
                Email = "emil@example.com",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            };
            emilOlsen.PasswordHash = hasher.HashPassword(emilOlsen, TestPassword);


            var saraNilsen = new User()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Sara Nilsen",
                Email = "sara@example.com",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            };
            saraNilsen.PasswordHash = hasher.HashPassword(saraNilsen, TestPassword);


            var jonasLund = new User()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Jonas Lund",
                Email = "jonas@example.com",
                Role = UserRole.Participant,
                CreatedAtUtc = DateTime.UtcNow
            };
            jonasLund.PasswordHash = hasher.HashPassword(jonasLund, TestPassword);


            // Lager kurs

            // Lagrer tidspunktet som en variabel
            // SaveChangesAsync til slutt er transaksjon. Alt eller ingenting.
            var now = DateTime.UtcNow;

            var grunnleggendePython = new Course()
            {
                Id = Guid.NewGuid(),
                Title = "Grunnleggende Python",
                Owner = idaHansen,
                StartsAtUtc = new DateTime(2026, 12, 15, 9, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 12, 15, 14, 0, 0, DateTimeKind.Utc),
                Location = "Sandefjord, rom A",
                Capacity = 12,
                Status = CourseStatus.Published,
                Description = "Et Python kurs for nybegynnere uten kodeerfaring.",
                CreatedAtUtc = now,

                // Oppdatert settes med samme tid som opprettet. Kan senere overskrives.
                UpdatedAtUtc = now
            };

            var introduksjonSql = new Course()
            {
                Id = Guid.NewGuid(),
                Title = "Introduksjon til SQL",
                Owner = omarAli,
                StartsAtUtc = new DateTime(2026, 12, 16, 9, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 12, 16, 12, 0, 0, DateTimeKind.Utc),
                Location = "Nettbasert",
                Capacity = 2,
                Status = CourseStatus.Published,
                Description = "Kurset viser hvordan man kan komme igang med grunnleggende SQL spørringer.",
                CreatedAtUtc = now,

                // Oppdatert settes med samme tid som opprettet. Kan senere overskrives.
                UpdatedAtUtc = now
            };

            var restApi = new Course()
            {
                Id = Guid.NewGuid(),
                Title = "REST API med ASP.NET Core",
                Owner = idaHansen,
                StartsAtUtc = new DateTime(2026, 12, 17, 09, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 12, 17, 15, 0, 0, DateTimeKind.Utc),
                Location = "Sandefjord, rom B",
                Capacity = 10,
                Status = CourseStatus.Draft,
                Description = "Kurset går gjennom REST APIer med ASP.NET Core.",
                CreatedAtUtc = now,

                // Oppdatert settes med samme tid som opprettet. Kan senere overskrives.
                UpdatedAtUtc = now
            };

            var gitGithub = new Course()
            {
                Id = Guid.NewGuid(),
                Title = "Git og GitHub",
                Owner = omarAli,
                StartsAtUtc = new DateTime(2026, 12, 18, 9, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 12, 18, 12, 0, 0, DateTimeKind.Utc),
                Location = "Nettbasert",
                Capacity = 8,
                Status = CourseStatus.Cancelled,
                Description = "Kurset viser hvordan man installerer Git og forklarer hva versjonskontroll er. Intro til Github.",
                CreatedAtUtc = now,

                // Oppdatert settes med samme tid som opprettet. Kan senere overskrives.
                UpdatedAtUtc = now
            };

            var docker = new Course()
            {
                Id = Guid.NewGuid(),
                Title = "Docker for utviklere",
                Owner = omarAli,
                StartsAtUtc = new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 11, 10, 14, 0, 0, DateTimeKind.Utc),
                Location = "Sandefjord, rom A",
                Capacity = 10,
                Status = CourseStatus.Published,
                Description = "Docker from zero til hero kurs. Alt du trenger å vite om Docker.",
                CreatedAtUtc = now,

                // Oppdatert settes med samme tid som opprettet. Kan senere overskrives.
                UpdatedAtUtc = now
            };

            var sikkerhetWebApp = new Course()
            {
                Id = Guid.NewGuid(),
                Title = "Sikkerhet i webapplikasjoner",
                Owner = idaHansen,
                StartsAtUtc = new DateTime(2026, 12, 21, 9, 0, 0, DateTimeKind.Utc),
                EndsAtUtc = new DateTime(2026, 12, 21, 13, 0, 0, DateTimeKind.Utc),
                Location = "Nettbasert",
                Capacity = 6,
                Status = CourseStatus.Published,
                Description = "Datasikkerhets kurs for webapplikasjoner. Kurs uavhengig av programmeringsspråk.",
                CreatedAtUtc = now,

                // Oppdatert settes med samme tid som opprettet. Kan senere overskrives.
                UpdatedAtUtc = now
            };


            // Lager registreringer
            var p1 = new Registration()
            {
                Id = Guid.NewGuid(),
                Course = grunnleggendePython,
                Participant = noraBerg,
                ActiveParticipantUserId = noraBerg.Id,

                // Fant på en dato før kursstart 
                RegisteredAtUtc = new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc)
            };

            var p2 = new Registration()
            {
                Id = Guid.NewGuid(),
                Course = grunnleggendePython,
                Participant = emilOlsen,
                ActiveParticipantUserId = emilOlsen.Id,

                // Fant på en dato før kursstart 
                RegisteredAtUtc = new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc)
            };

            var p3 = new Registration()
            {
                Id = Guid.NewGuid(),
                Course = introduksjonSql,
                Participant = noraBerg,
                ActiveParticipantUserId = noraBerg.Id,

                // Fant på en dato før kursstart
                RegisteredAtUtc = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc)
            };

            var p4 = new Registration()
            {
                Id = Guid.NewGuid(),
                Course = introduksjonSql,
                Participant = saraNilsen,
                ActiveParticipantUserId = saraNilsen.Id,

                // Fant på en dato før kursstart 
                RegisteredAtUtc = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc)
            };

            //  ActiveParticipantUserId er fjernet siden Jonas har avbestilt kurset
            var p5 = new Registration()
            {
                Id = Guid.NewGuid(),
                Course = sikkerhetWebApp,
                Participant = jonasLund,

                // Fant på en dato før kursstart 
                RegisteredAtUtc = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc),

                // Fant på en dato etter registrering
                CancelledAtUtc = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc)

            };

            // Legger dataene til konteksten
            db.Users.AddRange(idaHansen, omarAli, noraBerg, emilOlsen, saraNilsen, jonasLund);
            db.Courses.AddRange(grunnleggendePython, introduksjonSql, restApi, gitGithub, docker, sikkerhetWebApp);
            db.Registrations.AddRange(p1, p2, p3, p4, p5);

            // Lagrer dummy dataene i databasen
            await db.SaveChangesAsync();


        }


    }
}