# Nkanyezi Tours Information System

CMPG223 · Group 6 · Client: Nkanyezi Tours · Deadline **30 September 2026**

A tour-agency management system: master data, trip itineraries, bookings,
expenses, a client self-service portal and reports.

**ASP.NET Web Forms** (.NET Framework 4.8, C#) with **SQL Server** and ADO.NET.

---

## Getting it running — 10 minutes

You need **Visual Studio 2022 or 2026** with the *ASP.NET and web development*
workload. SQL Server LocalDB comes with it, so there is nothing else to install.

### 1. Clone

```bash
git clone <repo-url>
```

### 2. Create the database

Open **SQL Server Management Studio** (or the SQL Server Object Explorer inside
Visual Studio) and connect to `(localdb)\MSSQLLocalDB`.

Run these two files **in this order**, from the `database/` folder:

1. `01-schema.sql` — creates the 18 tables. Safe to run again; it drops and
   recreates everything.
2. `03-seed-data.sql` — loads the shared test data.

Both must run without errors. `03-seed-data.sql` prints row counts at the end —
check them rather than assuming.

> Everyone must run the same two scripts so we all see identical data. Never
> change tables with the SSMS designer: change `01-schema.sql`, commit it, and
> tell everyone to re-run it. Otherwise our databases drift apart and a bug on
> your machine cannot be reproduced on mine.

### 3. Open and run

Open `Program/Nkanyezi Tours 2.0/NkanyeziTours.sln` and press **F5**.

Sign in with any seeded account — the password for all of them is
`Password123`:

| Email | Role |
|---|---|
| `admin@nkanyezitours.co.za` | Administrator |
| `supervisor@nkanyezitours.co.za` | Supervisor |
| `precious@nkanyezitours.co.za` | Travel Agent |

The connection string in `Web.config` already points at LocalDB, so it should
just work. Only change it if SQL Server is installed differently on your
machine — and if you do, do not commit that change.

---

## Where everything lives

```
Project/
├── database/                       SQL scripts — run these first
│   ├── 01-schema.sql               18 tables, constraints, indexes
│   └── 03-seed-data.sql            shared test data
├── docs/
│   ├── 03-app-sections.md          every screen, who may open it
│   └── 05-data-layer-guide.md      READ THIS before writing data code
└── Program/Nkanyezi Tours 2.0/
    └── NkanyeziTours/
        ├── Models/                 data classes, one per table
        ├── Data/                   repositories — all SQL lives here
        ├── Services/               business rules, login
        ├── Maintain/ Account/      the .aspx screens
        └── Web.config              connection string, login, role rules
```

---

## The four rules

**1. No SQL outside `Data/`.** Not in a page, not in a service, not "just this
once". Every query lives in a repository so it can be reviewed and fixed in one
place.

**2. Every value is a parameter.** Never glue user input into a SQL string —
that is SQL injection and it is how systems get broken into. See the comment at
the top of `Data/Db.cs`; it explains the attack with examples.

**3. Do not use `SqlDataSource` or `ObjectDataSource`.** The Web Forms designer
makes it tempting to drag one on and type SQL into a wizard. That puts SQL back
into the markup and undoes rules 1 and 2. Use the designer for **layout**, then
bind the grid in code-behind:

```csharp
gvAttractions.DataSource = _attractions.GetAll();
gvAttractions.DataBind();
```

**4. Only edit files in your own module.** If a shared file needs changing —
`Site.Master`, `Web.config`, the `.csproj` — say so in the group chat first.

---

## Copy these two files

`Maintain/Attractions/Default.aspx` (list) and `Edit.aspx` (form) are the worked
examples. Every other maintenance screen is those two with different table and
column names. Read them before starting your own.

---

## Working together

- `main` always builds. Nobody pushes to it directly.
- Work on `feature/<your-module>`, then open a Pull Request.
- **`git pull` before you start work, every time.**
- Merge into `main` at least twice a week. Long-lived branches are how a group
  project dies: the `.csproj` lists every file by hand, so the longer you wait,
  the worse the conflict.
- A `.csproj` conflict is almost always "keep both lines" — you added a file,
  they added a file, both belong.
- When git scares you: **stop, do not force anything, ask in the group chat.**

---

## If something goes wrong

**"Could not load attractions…"** — SQL Server is not running, or you have not
run the two SQL scripts.

**A blank 500 error on a page with validators** — already fixed in
`Global.asax.cs`. If you see it again, that registration was removed.

**"Duplicate items" when building** — a `.csproj` merge kept the same line
twice. Delete the duplicate.

**Everyone's data looks different** — re-run `01-schema.sql` then
`03-seed-data.sql`.
