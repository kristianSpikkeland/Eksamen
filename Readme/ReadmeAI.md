# KI bruk

Hovedtrekkene i KI bruk:
* Kvalitetssjekking av egenskrevet kode. Dagens KI modeller er gode på kontrollering og det å ikke kontrollere koden er dårlig praksis.
* Drøfting rundt beste praksis knyttet til arkitektur og kode.
* Generering av kodeforslag. Føler ofte at man finner ut av de beste løsningene når man er litt kritisk og kjører flere iterasjoner med KI forslag.
* Fikk KI til å omskrive til camelCase i dummy klasse. Hadde skrevet PascalCase som er dårlig praksis for variabler.
* Tatt noen kode kommentarer fra KI. 


Prompt for Claude chat som omhandler hele prosjektet:
Lag en md fil med navn ReadmeCExtendedAI med bakgrunn i hele chatten vi har hatt fra prosjektoppstart. Formålet med denne er å gi sensor et komplett og ærlig bilde av AI bruk. Legg til en oversikt som viser alle klassene jeg har skrevet hovedsaklig selv og skill ut på dem du har gitt meg. De klassene der vi har samarbeidet mye og bearbeidet i flere omganger skiller du ut som en egen kategori. Fjern alle sårbarheter fra chatten som kan brukes av hackere da dette skal publiseres åpent på Github. Fjern de mest trivielle og ubetydelige promptene som eks. oversetting fra norsk til engelsk. Trekk spesielt frem de tilfellene der jeg har utfordret deg og kommet med bedre løsninger.

# Kode 100% generert av KI
## Data
KursplassenDbContext.cs

## Repo
Metoden GetWithActiveCountAsync(Guid courseId). 

## Service
InvalidRequestException, RequestValidator.cs og BadRequestOnInvalidRequest.cs er kode som KI har bearbeidet med utgangspunkt i tidligere KI generert kode. Problemet i den tidligere koden var at feilmeldinger ikke var koblet til felt. Det gjorde at klienten ikke ville kunne vise feilmeldingen på riktig plass.


# Plasser der jeg har utfordret KI (Claude)
## Passord
KI ville at jeg skulle dele root passord i koden min for MySql. Valgte å ikke gjøre dette og heller sette opp en bruker med passord.

## Passord hasher
Prompt:
Tenkte å lage en PasswordHasher i en egen security mappe. Denne vil jeg injecte i SeedData klassen min for å opprette PasswordHash til seed dataen. Videre må jeg bruke den i Servicen når brukere opprettes for å gjøre brukerens passord om til en hash før lagring i databasen. Utfordre meg gjerne på tankegangen. Har tidligere jobbet med slik en PasswordHasher, men finner ikke igjen denne koden. Kan du gi meg et eksempel som er så godt at det kunne blitt brukt i produksjon.

Svar:
Langt svar og komplisert kode.

Konklusjon
Løsningen ble å bruke Microsofts PasswordHasher<T> ved å installere pakken Microsoft.Extensions.Identity.Core
Da ligner koden mer på det jeg er vant med.

## Seeding av data
Claude ville i utgangspunktet ha Seed og SeedAsync metoder og kjøre migrasjon i Program.cs. Følte det ble en del duplisert logikk i Seed klassen og Program.cs. I tillegg måtte sensor kjøre migrasjon manuelt

Etter å ha utfordret Claude på dette fikk jeg kode som ungikk denne kompleksiteten og kjører ved oppstart. I tillegg tas det hensyn til at vi ikke skal berike produksjonsdatabasen kun utviklingsmiljøet. Koden er mer lik på kode man er vant med fra tidligere.

