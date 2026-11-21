# EF Core-spørringer for repository-klasser

*Teorien som trengs for å skrive lese- og skrivespørringer med Entity Framework Core, forklart med et fiktivt bibliotek og med Dapper som sammenligning.*

Dokumentet går gjennom:

1. Hvordan EF Core gjør LINQ om til SQL, og når spørringen faktisk kjøres.
2. De vanligste lesespørringene: filtrering, enkeltrader, sortering, telling og relaterte data.
3. Projeksjon til DTO-er, tracking og `AsNoTracking`.
4. Skriving: legge til, endre og slette.
5. Transaksjoner, unike indekser og samtidighet.
6. Hvordan en repository-klasse ser ut med EF Core.
7. De vanligste fallgruvene.

## Innhold

- [Bildet først](#bildet-først)
- [Eksempelet: Biblioteket](#eksempelet-biblioteket)
- [Del 1: Fra LINQ til SQL](#del-1-fra-linq-til-sql)
- [Del 2: Lesespørringer](#del-2-lesespørringer)
- [Del 3: Relaterte data og projeksjon](#del-3-relaterte-data-og-projeksjon)
- [Del 4: Tracking](#del-4-tracking)
- [Del 5: Skriving](#del-5-skriving)
- [Del 6: Transaksjoner og samtidighet](#del-6-transaksjoner-og-samtidighet)
- [Del 7: Repository-klassen](#del-7-repository-klassen)
- [Del 8: Fallgruver](#del-8-fallgruver)
- [Dapper og EF Core side om side](#dapper-og-ef-core-side-om-side)
- [Hele flyten på én side](#hele-flyten-på-én-side)

## Bildet først

Med Dapper skrives SQL-en for hånd, og Dapper gjør bare radene om til objekter. Med EF Core skrives **LINQ**, og EF Core lager SQL-en. Det meste av det som føles «særegent» med EF Core, handler om tre ting:

| Spørsmål | Kort svar |
|---|---|
| **Når kjøres spørringen?** | Først når en metode som `ToListAsync()` eller `FirstOrDefaultAsync()` kalles. Før det bygges bare en beskrivelse. |
| **Hva blir til SQL?** | Det som står *før* kjøringen. Kan ikke EF oversette et uttrykk, kastes en feil. |
| **Hva husker EF?** | Objekter som hentes, blir som standard *sporet* (tracking). Endres de, lagres endringen ved `SaveChangesAsync()`. |

## Eksempelet: Biblioteket

Et lite bibliotek-API med medlemmer, bøker og utlån. En bok har et antall eksemplarer, og et utlån gjelder ett eksemplar til det leveres tilbake.

| Tabell | Viktige kolonner |
|---|---|
| `members` | `id`, `name`, `email` |
| `books` | `id`, `title`, `author`, `published_year`, `total_copies`, `available_copies` |
| `loans` | `id`, `book_id`, `member_id`, `loaned_at_utc`, `due_at_utc`, `returned_at_utc` (null = ikke levert) |

<details>
<summary>Vis koden: LibraryAPI/Entities (eksempel, 29 linjer)</summary>

```csharp
public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int PublishedYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public ICollection<Loan> Loans { get; set; } = [];
}

public class Loan
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateTime LoanedAtUtc { get; set; }
    public DateTime DueAtUtc { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }
}
```

</details>

> **Merk:** Alle async-metodene i dokumentet (`ToListAsync`, `AnyAsync` osv.) krever `using Microsoft.EntityFrameworkCore;`. Uten den finnes bare de synkrone LINQ-metodene.

## Del 1: Fra LINQ til SQL

### 1. `IQueryable` er en oppskrift, ikke et resultat

`db.Books.Where(...)` henter ingenting. Det lager et `IQueryable<Book>`, som er en beskrivelse av en spørring. Flere `Where`, `OrderBy` og `Select` kan legges på, og alt blir til **én** SQL-setning.

<details>
<summary>Vis koden: Spørringen bygges i flere steg (eksempel)</summary>

```csharp
var query = db.Books.Where(b => b.AvailableCopies > 0);   // ingen SQL ennå

if (author is not null)
    query = query.Where(b => b.Author == author);         // fortsatt ingen SQL

var books = await query.OrderBy(b => b.Title).ToListAsync();   // NÅ kjøres SQL
```

</details>

<details>
<summary>Vis koden: SQL-en EF lager (eksempel)</summary>

```sql
SELECT b.id, b.title, b.author, ...
FROM books AS b
WHERE b.available_copies > 0 AND b.author = @author
ORDER BY b.title
```

</details>

- **Hvorfor det er nyttig:** valgfrie filtre kan legges på med vanlige `if`-setninger, uten å bygge SQL-strenger.

### 2. Metodene som faktisk kjører spørringen

| Metode | Returnerer | Merk |
|---|---|---|
| `ToListAsync()` | Liste, kan være tom | Den vanligste |
| `FirstOrDefaultAsync(...)` | Første treff eller `null` | Bruk `OrderBy` først hvis rekkefølgen betyr noe |
| `SingleOrDefaultAsync(...)` | Eneste treff eller `null` | Kaster feil hvis det finnes **flere** treff |
| `FindAsync(id)` | Rad med primærnøkkel eller `null` | Sjekker først om objektet allerede er sporet, og spør da ikke databasen |
| `AnyAsync(...)` | `bool` | Blir `SELECT EXISTS(...)`, stopper ved første treff |
| `CountAsync(...)` | `int` | Blir `SELECT COUNT(*)` |
| `SumAsync`, `MaxAsync` … | Tall | Aggregater i databasen |

### 3. Bare det som kan oversettes, blir SQL

EF kan oversette vanlige operatorer, sammenligninger, `Contains`, `StartsWith`, datoegenskaper og en del innebygde funksjoner. Egne C#-metoder kan ikke oversettes.

<details>
<summary>Vis koden: Feiler fordi IsClassic ikke finnes i SQL (eksempel)</summary>

```csharp
bool IsClassic(Book b) => b.PublishedYear < 1950;

var classics = await db.Books.Where(b => IsClassic(b)).ToListAsync();
// InvalidOperationException: The LINQ expression ... could not be translated
```

</details>

- **Løsning:** skriv uttrykket direkte i `Where`, `b => b.PublishedYear < 1950`.

### 4. Se SQL-en

Den beste måten å lære EF Core på er å se hva den lager.

<details>
<summary>Vis koden: ToQueryString (eksempel)</summary>

```csharp
var sql = db.Books.Where(b => b.AvailableCopies > 0).ToQueryString();
Console.WriteLine(sql);
```

</details>

Alternativt kan all SQL logges under utvikling med nivået `Information` på kategorien `Microsoft.EntityFrameworkCore.Database.Command` i `appsettings.Development.json`.

## Del 2: Lesespørringer

### 1. Filtrering og sortering

<details>
<summary>Vis koden: Tilgjengelige bøker av en forfatter, nyeste først (eksempel)</summary>

```csharp
var books = await db.Books
    .Where(b => b.Author == author && b.AvailableCopies > 0)
    .OrderByDescending(b => b.PublishedYear)
    .ThenBy(b => b.Title)
    .ToListAsync();
```

</details>

### 2. Én rad

<details>
<summary>Vis koden: Hent bok med id (eksempel)</summary>

```csharp
var book = await db.Books.FirstOrDefaultAsync(b => b.Id == id);
if (book is null)
{
    // finnes ikke: servicen avgjør hva det betyr (f.eks. 404)
}
```

</details>

- **`FirstOrDefaultAsync` eller `FindAsync`?** `FindAsync(id)` er kortest når det bare søkes på primærnøkkel. `FirstOrDefaultAsync` trengs når det filtreres på noe annet, eller når `Include`/`AsNoTracking` skal brukes.

### 3. Finnes, og hvor mange

<details>
<summary>Vis koden: Any og Count (eksempel)</summary>

```csharp
bool emailTaken = await db.Members.AnyAsync(m => m.Email == email);

int activeLoans = await db.Loans.CountAsync(l => l.MemberId == memberId && l.ReturnedAtUtc == null);
```

</details>

- **Bruk `AnyAsync` for ja/nei.** `CountAsync() > 0` teller alle radene, mens `AnyAsync` stopper ved første.

### 4. Tekstsøk

<details>
<summary>Vis koden: Søk i tittel (eksempel)</summary>

```csharp
var hits = await db.Books
    .Where(b => b.Title.Contains(search))
    .ToListAsync();
```

</details>

`Contains` blir `LIKE '%…%'` med søketeksten som parameter. Om søket skiller på store og små bokstaver, bestemmes av databasens collation. MySQL skiller som standard ikke.

> **Merk:** Unngå `b.Title.ToLower().Contains(...)`. Da må databasen kjøre en funksjon på hver rad og kan ikke bruke en indeks, og i MySQL er det som regel unødvendig.

### 5. Tid i spørringer

<details>
<summary>Vis koden: Forfalte utlån med tid som parameter (eksempel)</summary>

```csharp
public Task<List<Loan>> GetOverdueAsync(DateTime nowUtc) =>
    db.Loans
        .Where(l => l.ReturnedAtUtc == null && l.DueAtUtc < nowUtc)
        .ToListAsync();
```

</details>

- **Hvorfor `nowUtc` som parameter:** skrives `DateTime.UtcNow` rett i spørringen, oversetter EF den til databasens egen klokke (`UTC_TIMESTAMP()` i MySQL). Da brukes en annen klokke enn resten av appen, og det kan ikke styres i tester. Med parameter bestemmer servicen tidspunktet.

## Del 3: Relaterte data og projeksjon

Det finnes to måter å få med data fra andre tabeller. Valget mellom dem er en av de viktigste forskjellene fra Dapper.

### 1. `Include`: hent hele objekter med relasjoner

<details>
<summary>Vis koden: Utlån med bok og medlem (eksempel)</summary>

```csharp
var loans = await db.Loans
    .Include(l => l.Book)
    .Include(l => l.Member)
    .Where(l => l.ReturnedAtUtc == null)
    .ToListAsync();

// loans[0].Book.Title og loans[0].Member.Name er fylt
```

</details>

- **Uten `Include`** er navigasjonene `null` (eller tomme lister). EF henter aldri relaterte data av seg selv.
- **`ThenInclude`** går ett nivå til: `.Include(b => b.Loans).ThenInclude(l => l.Member)`.

### 2. `Select`: hent bare det som trengs

<details>
<summary>Vis koden: Projeksjon rett til DTO (eksempel)</summary>

```csharp
public record LoanResponse(Guid Id, string BookTitle, string MemberName, DateTime DueAtUtc);

var loans = await db.Loans
    .Where(l => l.ReturnedAtUtc == null)
    .Select(l => new LoanResponse(l.Id, l.Book.Title, l.Member.Name, l.DueAtUtc))
    .ToListAsync();
```

</details>

<details>
<summary>Vis koden: SQL-en EF lager (eksempel)</summary>

```sql
SELECT l.id, b.title, m.name, l.due_at_utc
FROM loans AS l
INNER JOIN books AS b ON l.book_id = b.id
INNER JOIN members AS m ON l.member_id = m.id
WHERE l.returned_at_utc IS NULL
```

</details>

- **Ingen `Include` trengs.** Brukes `l.Book.Title` inne i `Select`, lager EF JOIN-en selv.
- **Bare fire kolonner** hentes, ikke hele radene.
- **`Include` ignoreres** i en spørring som ender med `Select` til noe annet enn entiteten.

### 3. Telling av relaterte rader

<details>
<summary>Vis koden: Bøker med antall aktive utlån (eksempel)</summary>

```csharp
var stats = await db.Books
    .Select(b => new
    {
        b.Title,
        ActiveLoans = b.Loans.Count(l => l.ReturnedAtUtc == null)
    })
    .ToListAsync();
```

</details>

`b.Loans.Count(...)` blir en subquery i SQL. Uten navigasjonslisten skrives det samme som `db.Loans.Count(l => l.BookId == b.Id && l.ReturnedAtUtc == null)`, med identisk SQL.

### 4. Når velges hva?

| Situasjon | Velg |
|---|---|
| Data skal **vises** eller returneres som DTO | `Select` |
| Objektet skal **endres** og lagres | Hent entiteten (eventuelt med `Include`) |
| Forretningslogikk trenger hele objektet | Hent entiteten |

## Del 4: Tracking

EF husker objektene den henter, i en **change tracker**. Når `SaveChangesAsync()` kalles, sammenlignes objektene med slik de var, og EF lager `UPDATE` for det som er endret.

| | Standard (tracking) | `AsNoTracking()` |
|---|---|---|
| Endringer lagres ved `SaveChangesAsync` | Ja | Nei |
| Ytelse | Litt tregere, bruker mer minne | Raskere |
| Bruk | Når objektet skal endres | Rene lesespørringer |

<details>
<summary>Vis koden: Lesespørring uten tracking (eksempel)</summary>

```csharp
var books = await db.Books
    .AsNoTracking()
    .Where(b => b.AvailableCopies > 0)
    .ToListAsync();
```

</details>

- **`Select` til DTO spores ikke** uansett, fordi DTO-en ikke er en entitet. `AsNoTracking` er derfor mest aktuelt når hele entiteter hentes bare for å leses.

## Del 5: Skriving

### 1. Legge til

<details>
<summary>Vis koden: Nytt medlem (eksempel)</summary>

```csharp
var member = new Member { Id = Guid.NewGuid(), Name = name, Email = email };
db.Members.Add(member);
await db.SaveChangesAsync();   // INSERT INTO members ...
```

</details>

`Add` er synkron fordi den bare legger objektet i change trackeren. Det er `SaveChangesAsync` som snakker med databasen.

### 2. Endre: hent, endre, lagre

<details>
<summary>Vis koden: Lever tilbake et utlån (eksempel)</summary>

```csharp
var loan = await db.Loans.FirstOrDefaultAsync(l => l.Id == loanId);
if (loan is null) return false;

loan.ReturnedAtUtc = nowUtc;          // ingen SQL ennå
await db.SaveChangesAsync();          // UPDATE loans SET returned_at_utc = @p0 WHERE id = @p1
return true;
```

</details>

- **Ingen `Update(...)`-kall trengs.** Objektet er sporet, så EF ser endringen selv og oppdaterer bare kolonnen som er endret.
- **`db.Update(entity)`** brukes bare for objekter EF ikke kjenner fra før. Den markerer *alle* kolonner som endret.

### 3. Slette

<details>
<summary>Vis koden: Slett medlem (eksempel)</summary>

```csharp
var member = await db.Members.FindAsync(id);
if (member is null) return false;

db.Members.Remove(member);
await db.SaveChangesAsync();          // DELETE FROM members WHERE id = @p0
return true;
```

</details>

### 4. Endre eller slette direkte i databasen

EF Core 7 og nyere kan oppdatere eller slette uten å hente radene først. Det ligner mest på Dapper.

<details>
<summary>Vis koden: ExecuteUpdateAsync og ExecuteDeleteAsync (eksempel)</summary>

```csharp
int changed = await db.Loans
    .Where(l => l.ReturnedAtUtc == null && l.DueAtUtc < nowUtc)
    .ExecuteUpdateAsync(s => s.SetProperty(l => l.DueAtUtc, l => l.DueAtUtc.AddDays(7)));

int deleted = await db.Members
    .Where(m => !db.Loans.Any(l => l.MemberId == m.Id))
    .ExecuteDeleteAsync();
```

</details>

- **Kjøres med en gang**, uten `SaveChangesAsync`.
- **Går forbi change trackeren.** Objekter som allerede er hentet i samme forespørsel, oppdateres ikke i minnet.
- **Returnerer antall berørte rader**, som kan brukes til å se om noe faktisk ble endret.

## Del 6: Transaksjoner og samtidighet

### 1. `SaveChangesAsync` er én transaksjon

Alt som er lagt til, endret eller slettet før ett kall til `SaveChangesAsync`, lagres samlet. Feiler én rad, rulles alt tilbake.

### 2. Problemet: «sjekk, så gjør»

<details>
<summary>Vis koden: Lån med kappløpsfeil (eksempel)</summary>

```csharp
var book = await db.Books.FirstAsync(b => b.Id == bookId);
if (book.AvailableCopies == 0) return LoanResult.NoCopies;   // sjekk

book.AvailableCopies--;                                        // gjør
db.Loans.Add(new Loan { /* ... */ });
await db.SaveChangesAsync();
```

</details>

Er det ett eksemplar igjen, og to forespørsler kommer samtidig, kan begge lese `AvailableCopies == 1` før noen av dem har lagret. Begge låner, og antallet blir `-1`. Koden er riktig for én bruker, men feil for to samtidige.

### 3. Tre verktøy mot samtidighetsfeil

| Verktøy | Idé | Passer når |
|---|---|---|
| **Unik indeks** | Databasen nekter en rad som bryter regelen. Fanges som `DbUpdateException`. | Regelen er «det kan ikke finnes to like» |
| **Betinget oppdatering** | Sjekk og endring i *samme* SQL-setning, slik at databasen gjør dem atomisk | En teller skal ned, men ikke under en grense |
| **Transaksjon med lås** | Låser raden mens sjekk og endring gjøres, slik at andre må vente | Regelen krever flere spørringer |

<details>
<summary>Vis koden: Betinget oppdatering (eksempel)</summary>

```csharp
int updated = await db.Books
    .Where(b => b.Id == bookId && b.AvailableCopies > 0)
    .ExecuteUpdateAsync(s => s.SetProperty(b => b.AvailableCopies, b => b.AvailableCopies - 1));

if (updated == 0) return LoanResult.NoCopies;   // ingen ledige, eller boken finnes ikke
```

</details>

Setningen blir `UPDATE books SET available_copies = available_copies - 1 WHERE id = @id AND available_copies > 0`. Databasen behandler én slik setning om gangen per rad, så to samtidige forespørsler kan ikke begge få det siste eksemplaret.

<details>
<summary>Vis koden: Fang brudd på unik indeks (eksempel)</summary>

```csharp
try
{
    db.Members.Add(member);
    await db.SaveChangesAsync();
}
catch (DbUpdateException ex) when (ex.InnerException is MySqlException { ErrorCode: MySqlErrorCode.DuplicateKeyEntry })
{
    return RegisterResult.EmailTaken;
}
```

</details>

`MySqlException` og `MySqlErrorCode` ligger i `MySqlConnector`, som Pomelo bruker.

<details>
<summary>Vis koden: Egen transaksjon over flere steg (eksempel)</summary>

```csharp
await using var tx = await db.Database.BeginTransactionAsync();

// flere spørringer og endringer ...
await db.SaveChangesAsync();

await tx.CommitAsync();   // uten Commit rulles alt tilbake når tx lukkes
```

</details>

> **Merk:** En transaksjon alene hindrer ikke kappløp. Den sørger for «alt eller ingenting», men to transaksjoner kan fortsatt lese samme verdi samtidig. Mot kappløp trengs en lås (f.eks. `SELECT ... FOR UPDATE` med rå SQL inne i transaksjonen), en betinget oppdatering eller en unik indeks.

### 4. Rå SQL når LINQ ikke strekker til

<details>
<summary>Vis koden: FromSql og SqlQuery (eksempel)</summary>

```csharp
var books = await db.Books
    .FromSql($"SELECT * FROM books WHERE published_year < {year}")
    .ToListAsync();

var titles = await db.Database
    .SqlQuery<string>($"SELECT title AS Value FROM books")
    .ToListAsync();
```

</details>

Interpolerte verdier som `{year}` blir **parametere**, ikke tekst satt inn i SQL-en, så dette er trygt mot SQL-injeksjon. `FromSqlRaw` med strengsammenslåing er ikke trygt.

## Del 7: Repository-klassen

### 1. Ansvarsdeling

| Lag | Ansvar |
|---|---|
| **Repository** | *Hvordan* data hentes og lagres: LINQ, `Include`, `SaveChangesAsync` |
| **Service** | *Hva som er lov*: regler, tid, eierskap, feilsvar og mapping til DTO |

### 2. Et repository med konkrete metoder

Fil: `LibraryAPI/Data/Repositories/LoanRepository.cs`

<details>
<summary>Vis koden: ILoanRepository og LoanRepository (eksempel, 34 linjer)</summary>

```csharp
public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Loan>> GetActiveForMemberAsync(Guid memberId, CancellationToken ct = default);
    Task<bool> HasActiveLoanAsync(Guid memberId, Guid bookId, CancellationToken ct = default);
    Task AddAsync(Loan loan, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public class LoanRepository(LibraryDbContext db) : ILoanRepository
{
    public Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Loans.FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<List<Loan>> GetActiveForMemberAsync(Guid memberId, CancellationToken ct = default) =>
        db.Loans
            .AsNoTracking()
            .Include(l => l.Book)
            .Where(l => l.MemberId == memberId && l.ReturnedAtUtc == null)
            .OrderBy(l => l.DueAtUtc)
            .ToListAsync(ct);

    public Task<bool> HasActiveLoanAsync(Guid memberId, Guid bookId, CancellationToken ct = default) =>
        db.Loans.AnyAsync(l => l.MemberId == memberId && l.BookId == bookId && l.ReturnedAtUtc == null, ct);

    public async Task AddAsync(Loan loan, CancellationToken ct = default)
    {
        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
```

</details>

<details>
<summary>Vis koden: Registrering i Program.cs (eksempel)</summary>

```csharp
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
```

</details>

- **Konkrete navn** som `GetActiveForMemberAsync` forteller hva spørringen gjør. Filtreringen skjer i databasen, ikke i servicen.
- **`GetByIdAsync` er med tracking**, fordi servicen skal kunne endre utlånet og så kalle `SaveChangesAsync`.
- **`GetActiveForMemberAsync` er uten tracking**, fordi den bare leser.
- **`AddScoped`**, fordi `DbContext` lever én forespørsel, og repoet bruker den.
- **`CancellationToken`** sendes videre, slik at spørringen avbrytes hvis klienten kobler fra.
- **Interfacet** gjør at servicen kan testes med en falsk (mocket) versjon av repoet.

### 3. Entitet eller DTO fra repoet?

| Repoet returnerer | Fordel | Ulempe |
|---|---|---|
| **Entiteter** | Repoet vet ingenting om API-kontraktene | Henter hele rader |
| **DTO-er via `Select`** | Mest effektive spørringer | Repoet blir knyttet til API-kontraktene |

Begge er vanlige. Det viktigste er å velge én linje og følge den.

## Del 8: Fallgruver

| Fallgruve | Hva skjer | Løsning |
|---|---|---|
| **`ToList()` for tidlig** | `db.Books.ToList().Where(...)` henter hele tabellen og filtrerer i minnet | Legg `Where` før `ToListAsync()` |
| **Synkrone metoder** | `ToList()`, `First()` og `SaveChanges()` blokkerer tråden | Bruk `...Async`-variantene |
| **Glemt `Include`** | Navigasjonen er `null`, gir `NullReferenceException` | `Include`, eller `Select` med navigasjonen |
| **N+1-spørringer** | Én spørring for listen, deretter én per rad i en løkke | Hent alt i én spørring med `Include` eller `Select` |
| **Parallelle spørringer** | `Task.WhenAll` på samme `DbContext` kaster feil | Kjør spørringene etter hverandre med `await` |
| **`DateTime.UtcNow` i spørringen** | Databasens klokke brukes, og testen kan ikke styre tiden | Send tiden inn som parameter |
| **`Update(entity)` på sporet objekt** | Unødvendig, og alle kolonner skrives | Endre feltene og kall `SaveChangesAsync` |
| **Returnere entiteter som JSON** | Evige løkker og lekkede felter (f.eks. passordhash) | Map til DTO |

## Dapper og EF Core side om side

| Oppgave | Dapper | EF Core |
|---|---|---|
| Hent liste | `QueryAsync<Book>("SELECT * FROM books WHERE ...")` | `db.Books.Where(...).ToListAsync()` |
| Hent én | `QuerySingleOrDefaultAsync<Book>(...)` | `FirstOrDefaultAsync(...)` |
| Finnes | `ExecuteScalarAsync<bool>("SELECT EXISTS(...)")` | `AnyAsync(...)` |
| Tell | `ExecuteScalarAsync<int>("SELECT COUNT(*) ...")` | `CountAsync(...)` |
| JOIN | Skrives i SQL, multi-mapping med `splitOn` | `Include`, eller navigasjon i `Select` |
| Bare noen kolonner | `SELECT title, author ...` | `Select(b => new { b.Title, b.Author })` |
| Sett inn | `ExecuteAsync("INSERT ...", obj)` | `Add(obj)` + `SaveChangesAsync()` |
| Oppdater | `ExecuteAsync("UPDATE ...")` | Endre sporet objekt + `SaveChangesAsync()`, eller `ExecuteUpdateAsync` |
| Slett | `ExecuteAsync("DELETE ...")` | `Remove` + `SaveChangesAsync()`, eller `ExecuteDeleteAsync` |
| Transaksjon | `connection.BeginTransaction()` | `SaveChangesAsync()` alene, eller `Database.BeginTransactionAsync()` |
| Rå SQL | Standard | `FromSql`, `Database.SqlQuery<T>` |

## Hele flyten på én side

```text
Service                         Repository                         EF Core / MySQL
───────                         ──────────                         ───────────────
henter nowUtc
kaller repo.GetXxxAsync(…)  ──► bygger IQueryable
                                (Where, Include/Select, OrderBy)
                                ToListAsync / FirstOrDefaultAsync ─► SQL kjøres, rader tilbake
                            ◄── entiteter eller DTO-er
sjekker regler
endrer sporet entitet
kaller repo.SaveChangesAsync ─► db.SaveChangesAsync()  ─────────► INSERT/UPDATE/DELETE
                                                                   i én transaksjon
mapper til response-DTO
```

**Huskeliste for hver spørring:**

1. Skjer filtreringen **før** `ToListAsync()`?
2. Er det en ren lesing? Bruk `Select` til DTO eller `AsNoTracking()`.
3. Trengs relaterte data? `Include` for entiteter, navigasjon i `Select` for DTO-er.
4. Brukes `...Async` og `CancellationToken`?
5. Sendes tiden inn som parameter?
6. Kan to samtidige forespørsler bryte en regel? Vurder unik indeks, betinget oppdatering eller lås.
7. Hvordan ser SQL-en ut? Sjekk med `ToQueryString()` eller loggen.
