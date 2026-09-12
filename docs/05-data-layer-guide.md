# How the data layer works, and how to add your ownThis is one Visual Studio project. Everything is under
`Program/Nkanyezi Tours 2.0/NkanyeziTours/`:

| Folder | What goes in it | Who writes it |
|---|---|---|
| `Models/` | Data classes. Properties only, no logic, no SQL | Reinhardt |
| `Data/` | Repositories. The SQL lives here | Reinhardt |
| `Services/` | Business rules, login, the repository factory | Reinhardt |
| `Maintain/` `Account/` `Itineraries/` … | The .aspx screens | everyone else |

If you are building screens, you only ever touch the screen folders.
Read this before you write any code that touches the database. It is short, and
the section on SQL injection is not optional.

## The chain

```
Web Forms page  →  Repository  →  SQL Server
(.aspx.cs)        (Data/)        (the SQL is written in the repository)
```

**You never write SQL in a page.** You never open a connection. You never see a
connection string. You call a method on a repository and you get back a list of
model objects.

A page that lists attractions is this much code:

```csharp
public partial class Default : Page
{
    private readonly AttractionRepository _attractions = Repositories.Attractions;

    protected void Page_Load(object sender, EventArgs e)
    {
        // IsPostBack is false the first time the page opens and true after
        // every button click. Re-binding on a postback would wipe out what the
        // user had selected.
        if (IsPostBack) return;

        gvAttractions.DataSource = _attractions.GetAll();
        gvAttractions.DataBind();
    }
}
```

That is the whole point of the layer. Everything difficult already happened
somewhere else.

## Where the files live

This is one Visual Studio project. Everything is under
`Program/Nkanyezi Tours 2.0/NkanyeziTours/`:

| Folder | What goes in it | Who writes it |
|---|---|---|
| `Models/` | Data classes. Properties only, no logic, no SQL | Reinhardt |
| `Data/` | Repositories. The SQL lives here | Reinhardt |
| `Services/` | Business rules, login, the repository factory | Reinhardt |
| `Account/` `Maintain/` `Itineraries/` … | The .aspx screens | everyone else |

If you are building screens, you only ever touch the screen folders.

The namespaces are `Nkanyezi.Models`, `Nkanyezi.Data` and `Nkanyezi.Services`
even though there is only one project. That is deliberate: if we ever split it
into separate projects, nothing has to be renamed.

---

# The rule that matters most: SQL injection

We write our SQL as text. That means **you** are responsible for making it safe,
and there is exactly one rule.

> **Never put a value into a SQL string. Always pass it as a parameter.**

## What goes wrong

Wrong — and this is a real, exploitable bug, not a style opinion:

```csharp
var sql = "SELECT * FROM dbo.AppUser WHERE Email = '" + email + "'";
```

If somebody types this into the email box:

```
x' OR '1'='1
```

the database receives:

```sql
SELECT * FROM dbo.AppUser WHERE Email = 'x' OR '1'='1'
```

which matches **every user in the table**, and they are logged in as the first
one. Type something worse and they can empty a table:

```
x'; DROP TABLE dbo.Itinerary; --
```

That is SQL injection. It is the most common way real systems get broken into,
and it is caused entirely by gluing text together.

## What to do instead

```csharp
const string sql = "SELECT * FROM dbo.AppUser WHERE Email = @Email";
var user = _db.QuerySingle(sql, Map, Db.P("@Email", email));
```

Now SQL Server is handed the query and the value as two separate things. It
compiles the query first and then slots the value in, so whatever the user typed
is treated as *text to compare against*, never as commands to run. Typing
`x' OR '1'='1` simply looks for a user whose email address is literally
`x' OR '1'='1`, finds nobody, and fails the login. Exactly right.

It fixes apostrophes for free too. Without parameters, an attraction named
`Ludwig's Roses` breaks the query. With parameters it just works.

## The rule in one line

> If it came from a user, a form, a query string or a file, it is a
> **parameter**. Only column names and table names are ever typed into the SQL
> string itself, and those never come from the user.

## The one that catches people out

Search screens tempt you into building a `WHERE` clause out of pieces, because
some filters are empty. **Do not.** Use this pattern instead:

```sql
WHERE (@Name       IS NULL OR a.Name LIKE @Name ESCAPE '\')
  AND (@CategoryId IS NULL OR a.CategoryId = @CategoryId)
  AND (@ProvinceId IS NULL OR a.ProvinceId = @ProvinceId)
```

Pass `null` for a filter the user left blank and that whole line drops away. One
query serves every combination of filters, and no part of the SQL is ever
assembled from strings.

For the `LIKE` line, use `Db.Like()` rather than building `"%" + text + "%"`
yourself. It wraps the text in `%` signs *and* escapes any `%` or `_` the user
typed, so somebody searching for `50%` gets rows containing "50%" instead of
every row in the table.

---

## The helper: Db.cs

Five methods, and that is the entire contract.

| Method | Use it for |
|---|---|
| `Query(sql, map, ...)` | A `SELECT` returning many rows |
| `QuerySingle(sql, map, ...)` | A `SELECT` returning one row or none |
| `Scalar<T>(sql, ...)` | A `COUNT`, `SUM` or `MAX` — one single value |
| `Execute(sql, ...)` | `INSERT`, `UPDATE`, `DELETE`. Returns rows changed |
| `ExecuteInsert(sql, ...)` | An `INSERT` where you need the new id back |

Plus the parameter builders:

| Helper | Use it for |
|---|---|
| `Db.P(name, value)` | **Every** parameter |
| `Db.Like(name, text)` | Parameters used with `LIKE` |
| `Db.GetNullable<T>(r, "Col")` | Reading a column that allows NULL |
| `Db.GetNullableString(r, "Col")` | Reading a text column that allows NULL |

**Always use `Db.P()`.** Passing a C# `null` straight into a `SqlParameter` does
not do what you expect: ADO.NET reads it as "no value was supplied" and the call
fails with a confusing error. `Db.P()` converts it to a proper database NULL.
This is the single most common ADO.NET mistake and it will cost you an
afternoon.

## What already exists

Every table has a model and a repository, and they all compile:

- `Models/` — Attraction, Accommodation, Room, Vehicle, Route, Itinerary, the
  four booking classes, TripExpense, ExchangeRate, AppUser
- `Data/Db.cs` — the connection helper, written once
- `Data/AttractionRepository.cs` — **the pattern. Copy this one.**
- `Data/` — a repository for every entity, all handed out by `Services/Repositories.cs`

These are not just compiling — 21 of the queries, including the four-way
UNION on the itinerary page, the budget totals and all four availability
checks, have been run against a real database with real parameters. The
Attractions screens have been driven end to end in a browser.

That does not make your own new queries correct. Test each one in SSMS before
blaming your page.

## Adding a repository for your own entity

Four steps.

**1. Copy the model class.** Properties named exactly like the columns.

**2. Copy `AttractionRepository.cs`.** Change the table name, the column names,
and the two private helpers at the bottom (`Map` and `BuildParameters`). The
public methods barely change at all.

**3. Test each query in SSMS first**, with real values typed in:

```sql
SELECT * FROM dbo.Accommodation WHERE AccommodationId = 1;
```

If it does not work there, it will not work from C# either, and it is far easier
to debug in SSMS.

**4. Add it to `Services/Repositories.cs`** so pages can ask for it:

```csharp
public static AccommodationRepository Accommodation => new AccommodationRepository(_db);
```

Web Forms has no dependency injection container - a page is created by ASP.NET
itself and you cannot add constructor arguments to it. That small factory class
takes its place. Your page then reads:

```csharp
private readonly AccommodationRepository _accommodation = Repositories.Accommodation;
```

Forget step 4 and there is simply no way for the page to reach your repository.

## Rules that are not negotiable

**No SQL outside the `Data/` folder.** Not in a page, not in a service, not
"just this once for a quick report". If the SQL is in a repository it gets
reviewed, it gets marked, and it can be fixed by one person. If it is scattered
through the pages, it cannot.

**Every value is a parameter.** See above. There is no exception to this, not
even for a number you are sure is safe.

**Check for a `WHERE` clause on every `UPDATE` and `DELETE`.** Without one they
rewrite the entire table, and there is no undo.

**Return models, never readers.** Nothing outside `Data/` should know what a
`SqlDataReader` is. If a method returns one, the abstraction is broken and the
next person will write SQL in a page.

**One `Map` method per repository.** Do not write the column-reading code more
than once. When a column is added, there should be exactly one line to change.

**Never write to a computed column.** `VehicleBooking` (TotalDays, KmDriven,
ExtraKm, BaseCost, ExtraKmCost, TotalCost) and `RouteBooking` (TotalCost) have
columns SQL Server works out for itself. Leave them out of every `INSERT` and
`UPDATE`.

## Where the business rules live

Repositories do data. They do not decide whether a booking is allowed.

Rules like *does this room clash with an existing booking*, *is this trip over
budget*, *does this attraction have space in that slot* go in a **service** class
in `Services/`, which calls the repositories.

The reason is simple: if the rule lives in a page, then the next person who
writes a page that does the same thing has to remember to write the rule again.
With six people building screens, somebody will forget, and the bug will be
found by a marker rather than by us.

Simple screens with no rules can call a repository directly. Bookings never do.

**One exception, and it is deliberate.** The availability check for rooms, cars
and attractions is written *inside* the `INSERT` statement, not in a service.
That is because a check done beforehand leaves a gap: two agents can both see
the last room as free, both pass the check, and both save. Putting the test in
the same statement as the insert closes that gap — the second one inserts
nothing and `Create` returns 0, so the page can say "somebody just took it".
The service still checks first, so the user gets a clean message in the normal
case.

## When something goes wrong

**"Must declare the scalar variable '@Something'"** — the SQL mentions a
parameter you did not pass, or you misspelled one of the two names.

**"Invalid column name 'Something'"** — the column does not exist, or you
spelled it differently in the SQL and the `Map` method. Run the query in SSMS.

**"Cannot insert the value NULL into column..."** — the model property was empty
when the column does not allow NULL. Check the form actually bound the field.

**"The column cannot be modified because it is either a computed column or is
the result of a UNION operator"** — you put a computed column in an `INSERT` or
`UPDATE`. See the rule above.

**"error CS0104: 'Route' is an ambiguous reference"** — already fixed for
`Route` in `GlobalUsings.cs`. If it happens to another name, that is where to
fix it.

**Data looks different from someone else's** — re-run `01-schema.sql` and then
`03-seed-data.sql`. Both of you should be on identical data.
