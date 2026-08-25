# Database setup — read this before you say "it doesn't work on my laptop"

Nkanyezi Tours Information System · CMPG223 Group 6

---

## The one thing you need to understand first

**The database does not come with the code.**

When you clone the repo you get `.aspx` files, `.cs` files and the two `.sql`
files in this folder. You do **not** get a database. SQL Server keeps its
databases on your own machine, outside the repo, and nothing in Git can put
them there for you.

`Web.config` contains this line:

```xml
<add name="NkanyeziDb"
     connectionString="Server=(localdb)\MSSQLLocalDB;Database=NkanyeziTours;Trusted_Connection=True;TrustServerCertificate=True;"
     providerName="System.Data.SqlClient" />
```

Three parts of it are specific to whoever last edited it:

| Part | What it means | Why it breaks on another laptop |
|---|---|---|
| `Server=(localdb)\MSSQLLocalDB` | Which SQL Server to talk to | LocalDB is **per user**. If you installed SQL Server Express instead, yours is `.\SQLEXPRESS` and this name does not exist at all. |
| `Database=NkanyeziTours` | Which database on that server | Yours is empty. You never ran `01-schema.sql`, so the database is not there. |
| `Trusted_Connection=True` | Log in as the current Windows user | Your Windows account is a different account on a different machine. |

So there are exactly **three jobs** to do on a new machine: have a SQL Server,
create + fill the database, and point `Web.config` at it. The options below
are just different ways of doing those three jobs.

---

## Option 1 — Run the script (do this one)

From this folder:

**Double-click `setup-database.bat`.**

That is it. It will:

1. Find `sqlcmd.exe`, or tell you what to install if SQL Server is missing.
2. Find your SQL Server — it tries `(localdb)\MSSQLLocalDB`, then
   `.\SQLEXPRESS`, then `localhost`, then `.`. If LocalDB exists but is
   stopped or was never created, it starts/creates it for you.
3. Create the `NkanyeziTours` database if it is not there.
4. Run `01-schema.sql`, then `03-seed-data.sql`.
5. Rewrite the `Server=` value in `Web.config` to whatever it actually found
   (your old file is kept as `Web.config.bak`).
6. Print a table and foreign-key count so you can see it worked.

You should end up with **18 tables, 25 foreign keys** and the seed row counts.

Then open the solution in Visual Studio and press F5.

### If you prefer PowerShell

```powershell
powershell -ExecutionPolicy Bypass -File .\setup-database.ps1
```

### Switches

| Switch | What it does |
|---|---|
| `-Server ".\SQLEXPRESS"` | Skip auto-detection and use the instance you name |
| `-NoSeed` | Create the tables but leave them empty |
| `-NoConfigUpdate` | Do not touch `Web.config` |

Example:

```powershell
powershell -ExecutionPolicy Bypass -File .\setup-database.ps1 -Server ".\SQLEXPRESS"
```

**It is safe to run again**, and you should re-run it whenever someone changes
`01-schema.sql`. The scripts drop and recreate everything, so any test data you
typed in yourself is lost. That is deliberate — we all need identical data or
bugs and screenshots do not match.

---

## Option 2 — Do it by hand with sqlcmd

Same three jobs, typed out. Run these from the repo root.

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE NkanyeziTours"
```

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d NkanyeziTours -i "Project\database\01-schema.sql"
```

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d NkanyeziTours -i "Project\database\03-seed-data.sql"
```

Then open `Web.config` and make sure `Server=` matches the instance you just
used.

Two things people get wrong here:

- **`(localdb)\MSSQLLocalDB` uses a BACKSLASH.** If you type
  `(localdb)/MSSQLLocalDB` with a forward slash, SQL Server does not recognise
  it as LocalDB, falls back to Named Pipes, and you get error 53 and "Login
  timeout expired". This is not a permissions problem, it is a typo.
- **The path to the `.sql` files depends on where you cloned.** If your folder
  layout has no `Project` level, it is `database\01-schema.sql` instead.

---

## Option 3 — SQL Server Management Studio (SSMS)

If you would rather click than type:

1. Open SSMS, connect to your instance (`(localdb)\MSSQLLocalDB` or
   `.\SQLEXPRESS`), Windows Authentication.
2. Right-click **Databases → New Database**, name it `NkanyeziTours`, OK.
3. **File → Open → File**, open `01-schema.sql`.
4. In the database dropdown in the toolbar, **select `NkanyeziTours`**. If you
   leave it on `master` you will create all our tables in the wrong database.
5. Press **F5** (Execute).
6. Repeat steps 3–5 for `03-seed-data.sql`.
7. Edit `Web.config` so `Server=` matches the instance you connected to.

---

## Which SQL Server do I actually have?

```powershell
sqllocaldb info
```

- Prints `MSSQLLocalDB` → you have LocalDB, use `(localdb)\MSSQLLocalDB`.
- Command not found → you probably have Express. Open **services.msc** and
  look for a running service named `SQL Server (SQLEXPRESS)`; if it is there,
  use `.\SQLEXPRESS`.
- Neither → you have no SQL Server yet. Install **SQL Server Express** from
  <https://www.microsoft.com/en-us/sql-server/sql-server-downloads>, or add the
  **Data storage and processing** workload in the Visual Studio Installer
  (that gives you LocalDB).

---

## Errors we have actually hit, and what they mean

### "Login failed for user 'YOUR-PC\Name'"

Almost always means **the database does not exist**, not that you lack
permission. SQL Server reports a missing database this way, which is
misleading. Run Option 1.

### "A network-related or instance-specific error… Server is not found or not accessible" / error 53 / "Login timeout expired"

The **instance name is wrong or the service is not running**.

- Check for a forward slash instead of a backslash in `(localdb)\MSSQLLocalDB`.
- Run `sqllocaldb info` and `sqllocaldb start MSSQLLocalDB`.
- Check `services.msc` for a running `SQL Server (...)` service.
- If your instance really is called something else, put that name in
  `Web.config` and pass it as `-Server` to the script.

### "Invalid object name 'dbo.AppUser'"

You connected fine, but **the tables are not there**. You created the database
and stopped. Run `01-schema.sql` and `03-seed-data.sql` (Option 1 or 2).

### "Invalid object name" for a table that exists for everyone else

Someone added a table to `01-schema.sql` and you have not re-run it. Pull, then
run Option 1 again.

### `sqlcmd` is not recognised

The SQL command line tools are not installed or not on your PATH. Either use
Option 3 (SSMS), or install:

```powershell
winget install Microsoft.SQLServer.2022.CommandLineUtilities
```

### The app worked yesterday and is broken today

You pulled someone's schema change. Re-run Option 1.

---

## Team rules

- **Never change tables using the SSMS designer.** Change `01-schema.sql`,
  commit it, and tell everyone in the group chat to re-run the setup. If people
  edit tables by hand, every laptop drifts apart and nothing reproduces.
- **Never commit your own `Server=` value on top of someone else's.** The setup
  script rewrites `Web.config` for your machine — that change is local to you,
  do not push it unless the group agreed. If you already pushed one, it is not
  a disaster; the next person just re-runs the script.
- **Test data lives in `03-seed-data.sql`, not in your database.** If you need a
  new test record for a demo, add it to the seed file so everyone has it.
- `_retired-stored-procedures/` is old work kept for reference. It is not part
  of the setup and you do not need to run it.

---

## Files in this folder

| File | What it is |
|---|---|
| `setup-database.bat` | Double-click launcher for the setup script |
| `setup-database.ps1` | Does the whole setup: detect, create, run, patch config |
| `01-schema.sql` | Creates all 18 tables and their keys. Drops first, so re-runnable |
| `03-seed-data.sql` | Loads the shared test data. Run after the schema |
| `_retired-stored-procedures/` | Old stored procedures, not used |
