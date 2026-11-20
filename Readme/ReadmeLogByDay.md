# 20.11
## Arbeid
Leste gjennom oppgavetekst. 
Så gjennom et Dometrain kurs som handler om JWT da man ikke har jobbet med JWT før.
Opprettet et API prisjekt og satte opp domene klassene.
Satte opp Readme filer med ulikt formål.
Opprettet domeneklasser.
Fikk hjelp av KI med oppsett av DbContext fil.

Totalt: Rundt 9 timer arbeid.


## Utfordringer 
Installerte Pomelo men fikk beskjed om at den innholdt en sårbarhet. Løsningen ble å oppdater MySqlConnector.
Navnekonvensjonen i C# samavarer ikke med databasen. Løsningen ble å installere pakken EFCore.NamingConventions.

Og legge den til i Program.cs
```C#
builder.Services.AddDbContext<KursplassenDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .UseSnakeCaseNamingConvention());
``