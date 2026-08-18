# Nkanyezi Tours System — Build Plan (Draft 1)

**Group 6 · CMPG213 · Client: Nkanyezi Tours · Deadline: 30 September 2026**

> This is a *planning* document for review, not the final repo docs.
> Read it, mark anything you disagree with, and we turn it into the real repo + detailed
> per-module docs afterwards.

---

## 1. Decisions already made

| Decision | Choice |
|---|---|
| Language / platform | C# on .NET 8 (LTS) |
| Application type | **ASP.NET Core Web Application** (browser-based) |
| Database | **Microsoft SQL Server Express** (free) |
| Source control | Git + GitHub, simple beginner workflow |
| Team size | 8 people (7 listed in the docs + you) |

ASP.NET Core is the right call — it satisfies the non-functional requirement of
"24/7 web browser accessibility" and it is the only realistic way to build the
**client self-help portal** the scope demands.

---

## 2. Open decisions (must close before we write code)

### 2.1 Razor Pages vs MVC

| | Razor Pages | MVC |
|---|---|---|
| Learning curve | **Easier** — one `.cshtml` page + one `.cshtml.cs` code-behind, feels like WinForms | Harder — controllers, actions, views spread across 3 folders |
| Good for | Page-driven CRUD apps (exactly what this is) | Large apps with lots of shared logic / APIs |
| Beginners on the team | Much friendlier | More things to explain |

**Recommendation: Razor Pages.** Each screen = one folder-mate pair of files, so a team
member can own "the Attractions pages" without touching anyone else's files. This
massively reduces merge conflicts with 8 people.

### 2.2 Data access — "cheapest option"

Cost-wise there is nothing to choose: **ADO.NET, Dapper and EF Core are all free and
open-source.** The real cost is *learning time* and *marks*.

| Option | Cost | Dev speed | Teaches SQL | Risk with beginners |
|---|---|---|---|---|
| Raw ADO.NET + stored procedures | Free | Slowest | Yes — heavily | Lots of repetitive code, easy to leak connections |
| **Dapper** | Free | Fast | Yes | Low — you still write real SQL |
| EF Core | Free | Fastest | No — hides SQL | Migrations confuse beginners; may lose marks if the module wants visible SQL |

**✅ DECIDED: ADO.NET with the SQL written inline in the repository classes**, wrapped in
a small shared helper class (`Data/Db.cs`) that is already written.

We first planned to use stored procedures and then changed to inline SQL. Reason: this
is a *database* module and the marks are in the SQL. With stored procedures the SQL sits
in a separate file a marker has to go and find; inline, the query sits directly next to
the C# that runs it, so the two are read together.

**This does not make anything less safe.** Stored procedures are not what prevents SQL
injection — *parameters* are. Every query passes its values as `SqlParameter` objects and
nothing the user typed is ever glued into a SQL string. See `docs/05-data-layer-guide.md`,
which explains the attack and the fix with examples.

The SQL still lives in exactly one place per entity (its repository), so it does not get
duplicated by 8 people. The old procedure files are archived under
`database/_retired-stored-procedures/` and are not run by anything.

### 2.3 Hosting

Development runs on everyone's own machine (`localhost` + SQL Server Express). We only
need real hosting if the client demo must be online. Free options exist (Azure free
tier, student credits) but **plan for a local demo** — cheapest and lowest risk.

### 2.4 Authentication

ASP.NET Core Identity is free and built in, but it generates a lot of tables and code
that beginners can't explain in a demo. Given the docs only require "secure login" with
4 roles and optional MFA, **recommendation: a hand-written login against our own `User`
table with hashed passwords (BCrypt) + cookie authentication + role checks.** Simpler,
fully explainable, and it demonstrates the "Maintain system users and access" requirement.

**❓ Decide as a group.**

---

## 3. Repository structure

One repo, **one Visual Studio project** with folders for each layer. The original plan
called for four separate projects; in practice one project with four folders gives the
same separation with far less for beginners to configure, and the namespaces are already
`Nkanyezi.Models` / `.Data` / `.Services` so splitting it later needs no renaming.

This is what is actually on disk. Items marked ⬜ are still to be written.

```
Project/
├── .gitignore                     ← ✅ keeps bin/obj/.vs and connection strings out
├── README.md                      ⬜ what this is, how to run it, in 5 minutes
├── PROJECT_PLAN.md                ← ✅ this file
├── docs/
│   ├── 00-START-HERE.md           ⬜ read this first if you're lost
│   ├── 01-setup-your-machine.md   ⬜ install VS, SQL Server, clone repo, run it
│   ├── 02-git-guide.md            ⬜ every git command you will ever need
│   ├── 03-app-sections.md         ← ✅ every screen, who may open it, where it lives
│   ├── 04-coding-standards.md     ⬜ naming, formatting, comments
│   ├── 05-data-layer-guide.md     ← ✅ how the data layer works + SQL injection
│   ├── 06-how-to-add-a-page.md    ⬜ step-by-step recipe, copy-paste-able
│   ├── 07-work-split.md           ⬜ who owns what
│   ├── 08-build-order.md          ⬜ what gets built in which week
│   └── modules/                   ⬜ one spec per module (see §5)
├── database/
│   ├── 01-schema.sql              ← ✅ 18 tables, constraints and indexes included
│   ├── 03-seed-data.sql           ← ✅ test data so everyone has the same DB
│   └── _retired-stored-procedures/ ← archived, not used, see §2.2
└── Program/Nkanyezi Tours/Nkanyezi Tours/
    ├── Models/                    ← ✅ shared C# classes (Attraction, Booking, ...)
    ├── Data/                      ← ✅ BACKEND: repositories, the SQL lives here
    ├── Services/                  ← BACKEND: business rules & validation
    ├── Pages/                     ← FRONTEND: Razor Pages, layout, forms
    └── wwwroot/                   ← CSS, JS, Bootstrap
```

There is no `02-constraints.sql`: the foreign keys, CHECK constraints and indexes are
written inline in `01-schema.sql`, next to the tables they belong to.

### The rule that makes this work for 8 people

> **Pages** may call **Services**. **Services** may call **Data**. **Data** may call SQL.
> Never skip a layer, never call backwards.

A Razor Page is never allowed to contain SQL. That one rule prevents 90% of the mess
that kills group projects.

---

## 4. Frontend / backend split — what each person actually touches

**Frontend work (`Pages/`)**
- Razor Pages (`.cshtml`) — the HTML forms, tables and buttons
- Page models (`.cshtml.cs`) — read the form, call a service, show the result
- Shared layout, navigation menu, CSS (Bootstrap — free, comes with the template)
- Client-side validation messages

**Backend work (`Services/` + `Data/` + `database/`)**
- The validation rules from the Phase 2 requirement tables (dates don't clash, quantity
  > 0, cost ≤ client budget, ID doesn't already exist…)
- The repository methods and the SQL inside them
- Report queries
- PDF / Excel export

Every module has a frontend half and a backend half, so **pair people up: one frontend,
one backend per module.** With 8 people that's 4 working pairs. Pairs are far better
than 8 individuals — nobody is stranded, and reviews happen naturally.

---

## 5. Module breakdown

Each module gets its own detailed markdown spec in `docs/modules/`. Every spec will
contain the same sections so they're predictable:

> Purpose · Screens · Database tables used · Stored procedures · Validation rules
> (copied from the Phase 2 tables) · Which roles may access it · Definition of done ·
> How to test it

Modules, in the order they must be built:

| # | Module | Depends on | Notes |
|---|---|---|---|
| 0 | **Database foundation** | — | Schema, seed data, connection helper. *Nothing else can start.* |
| 1 | **Auth & login** | 0 | Login page, logout, password hashing, role checks |
| 2 | **Users & access** | 1 | CRUD for the 4 roles: Admin, Supervisor, Agent, Client |
| 3 | **Attractions** | 1 | Fully specced in Phase 2 — the reference implementation |
| 4 | **Accommodation** | 1 | Fully specced in Phase 2, incl. rooms |
| 5 | **Transport** | 1 | ⚠️ *No detailed spec exists yet — we must write it* |
| 6 | **Bookings** | 3, 4, 5 | Date clashes, capacity, budget checks |
| 7 | **Itineraries** | 6 | ⚠️ *No detailed spec yet* |
| 8 | **Trip expenses** | 7 | ⚠️ *No detailed spec yet* |
| 9 | **Client portal** | 6 | Restricted view of 3–7 for the Client role |
| 10 | **Reports** | 6–8 | Revenue, seasonal trends, hotspots, agent performance |
| 11 | **Export (PDF/Excel)** | 7, 10 | QuestPDF + ClosedXML — both free |

**Gap to fix first:** modules 5, 7 and 8 are in the project scope but have no
input/processing/output tables in the Phase 2 document. We need to write those in the
same format before building them — otherwise three pairs have nothing concrete to code
against. This is a documentation task, not a coding task, and can happen in parallel
with module 0.

---

## 6. Build order (proposed schedule)

Your project plan gives: database 14–21 Aug, software 24 Aug–7 Sep, UI 8–17 Sep,
testing 18–30 Sep. That plan builds UI *after* software, which doesn't work for a web
app — the UI *is* the software. Suggested revision that keeps the same end dates:

**Stage 0 — Foundation (this week, ~14–21 Aug) — you + 1 other, everyone else reads docs**
1. Create GitHub repo, .gitignore, README, docs
2. Finalise the ERD into `01-schema.sql` and run it
3. Seed data
4. Create the empty 4-project solution, get "Hello World" running for everyone
5. Build the DB helper class + **one complete vertical slice: Attractions CRUD**
   — this becomes the template everyone copies

*Nobody else writes code until step 5 exists.* This is the single most important
decision in the plan. One worked example beats ten pages of instructions.

**Stage 1 — Everyone onboards (~22–25 Aug)**
Every member: install tools, clone repo, run the app, make one trivial change, open a
pull request, get it merged. Small and harmless, but it means nobody hits git for the
first time in week 6 under pressure.

**Stage 2 — Core CRUD in parallel (~24 Aug – 5 Sep)**
- Pair A: Auth + Users & access
- Pair B: Accommodation
- Pair C: Transport (writes the spec first)
- Pair D: finish Attractions + shared layout/navigation

**Stage 3 — Transactional features (~5–17 Sep)**
- Pair A: Bookings
- Pair B: Itineraries
- Pair C: Trip expenses
- Pair D: Client portal

**Stage 4 — Reporting & export (~15–22 Sep)**
- Pairs A+B: Reports · Pairs C+D: PDF/Excel export

**Stage 5 — Test, fix, document, demo (~22–30 Sep)**
Test plan, bug fixing, user manual, demo rehearsal. Reserve the last 4 days for
*nothing but fixing and rehearsing* — do not schedule new features here.

---

## 7. Git workflow for beginners

Deliberately minimal:

- `main` — always works. Nobody pushes to it directly (protected).
- `feature/<name>-<module>` — e.g. `feature/reinhardt-attractions`. One per person per task.
- Pull Request into `main`, reviewed by you or one other person, then merged.

Rules that prevent 90% of beginner pain:
1. `git pull` **before** you start work, every time.
2. Commit small and often, with a real message ("Add attraction validation", not "update").
3. Never commit `bin/`, `obj/`, or your `appsettings.json` connection string.
4. Only edit files inside your own module. If you need a shared file changed, ask.
5. When git scares you: **stop, don't force anything, ask in the group chat.**

`docs/02-git-guide.md` will spell out every command with expected output.

---

## 8. Risks

| Risk | Mitigation |
|---|---|
| 8 people editing the same files → merge hell | Razor Pages + one module per pair + rule 4 above |
| Members who can't code contribute nothing | Give them docs, test plans, seed data, report SQL, and pair them with a coder |
| Transport / itineraries / expenses have no spec | Write specs in Stage 0, in parallel |
| Scope too big for the time | Modules 9–11 are the cut candidates. Core CRUD + bookings must ship. |
| ERD in the Phase 3 doc is an image | Must be transcribed into SQL by hand before Stage 0 step 2 |
| Everyone's database drifts apart | All schema changes go through numbered `.sql` files in the repo — never through the SSMS GUI |

---

## 9. What I need from you next

1. ~~✅ / ❌ on the open decisions in §2~~ — **all settled.** Razor Pages, ADO.NET with
   inline parameterised SQL, custom cookie auth with BCrypt. All three are implemented.
2. The **ERD** — either describe the entities and relationships to me, or confirm I
   should propose a schema from the Phase 2 requirement tables
3. Confirm the pair assignments (who is actually reliable, who needs pairing)
4. Confirm GitHub as the host and whether the repo is private

Once those are settled I'll generate the full repo: all `docs/` files, the `.sql`
scripts, and the solution skeleton.
