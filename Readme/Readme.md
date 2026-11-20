# Forklaringer
## En til mange relasjoner

### Registrering
En registrering kun ha en bruker. En bruker kan ha flere registreringer, men ikke på samme kurs. Brukere må legges inn som fremmednøkkel.
En registrering kan kun ha et kurs. Et kurs kan ha flere registreringer. Kurs må legges inn som fremmednøkkel.

### Kurs
Et kurs kan ha flere brukere. Bruker må legges inn som fremmednøkkel.

### Arkitektur valg
Jeg valgte å holde domeneklassene relativt rene for valideringslogikk. På denne måten blir koden mer lettlest. Men det ligger "overskrifter" som [Column(TypeName = "text")] og [MaxLength] for at EF setter kolonnetypen med dem

### Validering
Nærmest databasen ligger modelbuilder sjekken. Et eksempel er om bruker registrer seg flere ganger veldig hurtig. Da kan valideringen slippe gjennom servicen og inn til databasen. Har ikke reflektert noe over dette tidligere når jeg har jobbet med Dapper. Under er et eksepel fra metoden OnModelCreating i KursplassenDbContext.cs:

```C#
modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
```


