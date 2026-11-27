# Fredag 20.11
Leste gjennom oppgavetekst. 
Så gjennom et Dometrain kurs som handler om JWT da man ikke har jobbet med JWT før.
Opprettet et API prisjekt og satte opp domene klassene.
Satte opp Readme filer med ulikt formål.
Opprettet domeneklasser.
Fikk hjelp av KI med oppsett av DbContext fil.

## Timer
Totalt: Rundt 9 timer arbeid.


## Utfordringer 
Installerte Pomelo men fikk beskjed om at den innholdt en sårbarhet. Løsningen ble å oppdater MySqlConnector.
Navnekonvensjonen i C# samavarer ikke med databasen. Løsningen ble å installere pakken EFCore.NamingConventions.

Og legge den til i Program.cs
```C#
builder.Services.AddDbContext<KursplassenDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .UseSnakeCaseNamingConvention());
```

# Lørdag 21.11
La til docker-compose.yml fil slik at sensor enkelt kan teste løsningen mot database.
Fikk kjørt migrasjonen og sjekket med MySql Workbench at databasen og kolonenne ble opprettet.
Brukte ganske mye tid på å fa settet opp en seed klasse med brukere, kurs og registreringer.
Startet APIet på nytt og sjekket at seed dataene ble opprettet i databasen. 
Fikk KI til å generere et dokument som omahandlet EF Core spørringer og leste meg opp på dette.
En del drodling med KI rundt JWT vs Cookie. Det viste seg at mye rundt det er helt likt.
Fikk såvidt begynt på repo spørringer. 

## Timer
Totalt rundt 9,5 timer brukt.

# Mandag 22.11
Jobbet ikke direkte med oppgaven. Leste gjennom ukens plan og deltok på undervisning-

# Tirsdag 23.11

Fikk skrevet ferdig all repository kode og laget interface for repone.
Laget request og update DTO for kurs.

## Utfordringer
Brukte mye tid og fikk god KI hjelp for å lage logikken rundt registrering. Det var mange sjekker som måtte settes opp for å unngå "ulovlige" registreringer.

## Timer
Totalt rundt 9 timer brukt.

# Onsdag 24.11
Laget request DTO for brukere. 

## Timer
Ca. 1. time

# Fredag 27.11
Produserte response DTOer.
Videre laget jeg ferdig CourseService klasse med CRUD metoder og implementerte samme DataAnotations validering som request DTOene har. 
Fikk KI til å lage InvalidRequestException, RequestValidator klasse og endepunktfilter BadRequestOnInvalidRequest.cs.
Klassene bygger videre på et gammelt KI prosjekt jeg hadde liggende, men nå ble det forbedret slik at klient kan vise feilmelding på riktig plass.  

## Timer
Ca. 9. timer
