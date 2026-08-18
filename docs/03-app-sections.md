# App Sections and Screens

Every screen in the system, what it does, who may open it, and where the file
lives in the project. This is the map the whole team works from, so if a page is
not on this list it does not get built.

The five sections come from the interface hierarchy in Phase 2 (Figure 3). Three
things have been added since that diagram was drawn: a dashboard, the client
portal, and exchange rates. Those are marked **NEW** and are listed with the
reason at the end.

**Roles:** AD = Administrator · SU = Supervisor · AG = Travel Agent · CL = Client

---

## 0. Public and account pages

Everything outside the login wall. Four pages.

| Page | What it does | Who | File |
|---|---|---|---|
| Login | Email and password, sends the user to their dashboard | everyone | `/Pages/Account/Login.cshtml` |
| Register | Client self-registration, creates a Client account only | public | `/Pages/Account/Register.cshtml` |
| Logout | Ends the session | everyone | handled in code, no page |
| Access denied | Shown when a user opens something their role may not see | everyone | `/Pages/Account/Denied.cshtml` |

Registration only ever creates Clients. Staff accounts are made by the
Administrator under Users and Access, otherwise anyone could sign themselves up
as an admin.

---

## 1. Dashboard **NEW**

| Page | What it does | Who | File |
|---|---|---|---|
| Dashboard | Landing page after login. Different content per role | everyone | `/Pages/Index.cshtml` |

What each role sees:

- **Client** — my trips, and a button to plan a new one
- **Agent** — my assigned trips, plus trips submitted from the portal with no agent yet
- **Supervisor / Administrator** — counts and totals, with links into the reports

Not in the Phase 2 diagram, which sends the user from the login form straight
into the menu. A landing page is needed because the four roles have almost
nothing in common, and because the unassigned trips queue has to live somewhere
or client-submitted trips will never get picked up.

---

## 2. Maintain

Master data. Four sub-sections, each the same shape: a list page with search, an
add/edit page, and a delete confirmation. Anyone who learns one can build the
others, which is why this section is where the team starts.

### 2.1 Users and access — AD only

| Page | What it does | File |
|---|---|---|
| User list | All users, filter by role, active and inactive | `/Pages/Maintain/Users/Index.cshtml` |
| Add / edit user | Details and role. Password is set here | `/Pages/Maintain/Users/Edit.cshtml` |
| Deactivate user | Confirmation | `/Pages/Maintain/Users/Delete.cshtml` |

### 2.2 Attractions — AD, SU, AG

| Page | What it does | File |
|---|---|---|
| Attraction list | Search by name, filter by category and province | `/Pages/Maintain/Attractions/Index.cshtml` |
| Add / edit attraction | All fields from Phase 2 Table 2 | `/Pages/Maintain/Attractions/Edit.cshtml` |
| Time slots | Manage the slots for one attraction | `/Pages/Maintain/Attractions/TimeSlots.cshtml` |
| Delete attraction | Confirmation, deactivates where bookings exist | `/Pages/Maintain/Attractions/Delete.cshtml` |

### 2.3 Accommodation — AD, SU, AG

| Page | What it does | File |
|---|---|---|
| Accommodation list | Search by name, filter by province and star rating | `/Pages/Maintain/Accommodation/Index.cshtml` |
| Add / edit accommodation | All fields from Phase 2 Table 5 | `/Pages/Maintain/Accommodation/Edit.cshtml` |
| Rooms | Manage the rooms belonging to one accommodation | `/Pages/Maintain/Accommodation/Rooms.cshtml` |
| Delete accommodation | Confirmation, deactivates where bookings exist | `/Pages/Maintain/Accommodation/Delete.cshtml` |

### 2.4 Transport — AD, SU, AG

Transport is two modules, because a car and a flight have nothing in common
beyond getting the client somewhere. Cars are hired by the day and come back
with an odometer reading. Flights and shuttles are a seat on a journey between
two cities.

**Vehicles (car rental)**

| Page | What it does | File |
|---|---|---|
| Vehicle list | Search by make, model and registration | `/Pages/Maintain/Vehicles/Index.cshtml` |
| Add / edit vehicle | Details plus the three rate fields | `/Pages/Maintain/Vehicles/Edit.cshtml` |
| Delete vehicle | Confirmation, deactivates where bookings exist | `/Pages/Maintain/Vehicles/Delete.cshtml` |

**Routes (flights and bus shuttles)**

| Page | What it does | File |
|---|---|---|
| Route list | Filter by type, departure and arrival city | `/Pages/Maintain/Routes/Index.cshtml` |
| Add / edit route | Type, carrier, route code, cities, times, indicative fare | `/Pages/Maintain/Routes/Edit.cshtml` |
| Delete route | Confirmation, deactivates where bookings exist | `/Pages/Maintain/Routes/Delete.cshtml` |

The same three screens serve flights and shuttles. The type is a dropdown, so
whoever builds flights gets shuttles for nothing.

### 2.5 Exchange rates **NEW** — AD only

| Page | What it does | File |
|---|---|---|
| Rate list | Current and historic rates per currency | `/Pages/Maintain/Rates/Index.cshtml` |
| Capture rate | New rate with the date it applies from | `/Pages/Maintain/Rates/Edit.cshtml` |

Not in the Phase 2 diagram. The scope promises international clients a currency
converter, and the group decided rates are captured by hand rather than fetched
online, so somebody needs a screen to capture them on.

---

## 3. Manage Itineraries

The centre of the system. This is where trips are built and where the bookings
live, so it is the largest section and the one to build after the CRUD modules
are proven.

| Page | What it does | Who | File |
|---|---|---|---|
| Itinerary list | All trips, filter by client, agent and status. Agents see their own by default | AD SU AG | `/Pages/Itineraries/Index.cshtml` |
| Unassigned trips | Trips submitted from the portal with no agent. Agent claims one | AD SU AG | `/Pages/Itineraries/Unassigned.cshtml` |
| Create itinerary | Client, trip name, dates, budget | AD SU AG | `/Pages/Itineraries/Create.cshtml` |
| Itinerary detail | The trip: every booking, running total against budget, comments, status | AD SU AG CL | `/Pages/Itineraries/Detail.cshtml` |
| Edit itinerary | Change details, status, budget and comments | AD SU AG | `/Pages/Itineraries/Edit.cshtml` |
| Delete itinerary | Confirmation, cancels rather than deletes where confirmed | AD SU AG | `/Pages/Itineraries/Delete.cshtml` |

### Booking screens, reached from the itinerary detail page

| Page | What it does | File |
|---|---|---|
| Book accommodation | Search available rooms for a date range, one row per room booked | `/Pages/Itineraries/BookRoom.cshtml` |
| Book attraction | Pick a date and time slot, checks people booked against capacity | `/Pages/Itineraries/BookAttraction.cshtml` |
| Book a car | Pick dates, shows the daily rate and the km included | `/Pages/Itineraries/BookVehicle.cshtml` |
| Collect and return vehicle | Capture the odometer out and in, settles the final cost | `/Pages/Itineraries/VehicleReturn.cshtml` |
| Book a flight or shuttle | Pick a route and date, seats, fare per seat pre-filled from the indicative fare and overridden with what was actually paid, airline reference captured | `/Pages/Itineraries/BookRoute.cshtml` |

The vehicle return page is easy to forget and nothing else works without it. A
car rental's cost is not final until the odometer comes back.

---

## 4. Manage Trip Expenses

| Page | What it does | Who | File |
|---|---|---|---|
| Expense list | Expenses for one trip, grouped by category | AD SU AG CL | `/Pages/Expenses/Index.cshtml` |
| Add / edit expense | Category, description, amount, currency, date, receipt | AD SU AG CL | `/Pages/Expenses/Edit.cshtml` |
| Delete expense | Confirmation | AD SU AG CL | `/Pages/Expenses/Delete.cshtml` |
| Trip cost summary | Bookings plus expenses against budget, exportable | AD SU AG CL | `/Pages/Expenses/Summary.cshtml` |

Clients may capture and edit expenses on their own trips only. There is no
approval step, so the page shows who captured each line.

---

## 5. Client Portal **NEW**

| Page | What it does | Who | File |
|---|---|---|---|
| Browse attractions | Public catalogue with prices, no login needed to look | public | `/Pages/Portal/Attractions.cshtml` |
| Browse accommodation | Same for accommodation | public | `/Pages/Portal/Accommodation.cshtml` |
| Browse cars | Same for vehicles, showing rate and km included | public | `/Pages/Portal/Vehicles.cshtml` |
| Browse flights and shuttles | Routes with indicative fares, so a client can see roughly what Johannesburg to New York costs before phoning | public | `/Pages/Portal/Routes.cshtml` |
| My trips | The client's own itineraries | CL | `/Pages/Portal/MyTrips.cshtml` |
| Plan a trip | Client builds a trip and submits it to the agency | CL | `/Pages/Portal/PlanTrip.cshtml` |
| Currency converter | Converts a trip total using the captured rates | CL | `/Pages/Portal/Convert.cshtml` |

Not in the Phase 2 hierarchy, but the portal is in the scope, in the use case
diagram, and it is the answer to the main problem in the project: agents
drowning in telephone enquiries. It should not be left out of the interface
diagram when that gets redrawn.

Clients reuse the itinerary and expense pages rather than getting their own
copies. The same page shows less when a client opens it.

---

## 6. Reports — AD and SU only

| Page | What it does | File |
|---|---|---|
| Report menu | Pick a report, then a date range or filter | `/Pages/Reports/Index.cshtml` |
| Revenue and profit | Revenue by period, average spend per client | `/Pages/Reports/Revenue.cshtml` |
| Seasonal trends and hotspots | Bookings by month, by province, most visited attractions | `/Pages/Reports/Seasonal.cshtml` |
| Trip cost | Cost breakdown across trips | `/Pages/Reports/TripCost.cshtml` |
| Agent performance | Trips and revenue per agent, ignoring unassigned trips | `/Pages/Reports/AgentPerformance.cshtml` |

---

## 7. Export

Not a section with its own menu. Export is a button on the pages that produce
something worth exporting, which is how it actually gets used.

| Where | What comes out |
|---|---|
| Itinerary detail | The full itinerary as a PDF, for clients with no signal |
| Trip cost summary | The summary as a PDF |
| Any report | PDF or Excel |
| Any list page | Excel |

Libraries: QuestPDF for PDF, ClosedXML for Excel. Both free.

---

## Totals

| Section | Pages |
|---|---|
| Public and account | 3 |
| Dashboard | 1 |
| Maintain | 18 |
| Itineraries and bookings | 11 |
| Trip expenses | 4 |
| Client portal | 7 |
| Reports | 5 |
| **Total** | **49** |

Forty-nine pages across eight people is manageable, but only because the
eighteen Maintain pages are the same three screens repeated six times. Build one
set properly and the rest is copying.

## Suggested ownership

| Pair | Section |
|---|---|
| A | Account pages, dashboard, users and access |
| B | Attractions, accommodation and their rooms and time slots |
| C | Vehicles, routes, vehicle return, exchange rates |
| D | Itineraries and the four booking screens |
| A + B (later) | Client portal |
| C + D (later) | Reports and export |

The itineraries and bookings section is the hardest and everything else feeds
into it, so pair D should be the strongest coders.

## Three things to take back to the documents

1. The Phase 2 interface hierarchy has no client portal, no dashboard and no
   exchange rate screen. It needs redrawing before the next submission.
2. The hierarchy shows Export Data as a menu item. In practice it is a button on
   several pages. Worth correcting so the document matches what gets built.
3. Nothing in the documents describes the vehicle collection and return screen,
   because the kilometre pricing was only agreed after Phase 2 was written.
