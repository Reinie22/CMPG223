# Draft Requirement Tables — Transport, Itineraries, Trip Expenses

Written in the same Input / Processing / Output format as the Phase 2 tables for
Attractions and Accommodation, so these can drop straight into the document.

**Decisions taken by the group:**

| # | Question | Decision |
|---|---|---|
| 1 | Does Transport cover flights? | **Yes. Cars, flights and bus shuttles.** Flights and shuttles are handled together as routes, since both are a journey between two cities priced per seat. |
| 2 | How is transport priced? | **Cars:** set price per day covering 200km per day, then a rate per extra km, measured from the odometer at collection and return. **Routes:** a fare per seat. |
| 3 | Vehicle stock location and maintenance? | **Out of scope.** |
| 4 | Deactivate instead of delete? | **Yes**, across all maintenance modules. |
| 5 | Do itineraries have dates? | **Yes, but bookings are not restricted to them.** |
| 6 | Do itineraries have a status? | **Yes:** Draft, Confirmed, Cancelled, Completed. |
| 7 | Can clients capture expenses? | **Yes, and there is no verification step.** |
| 8 | Do booking costs appear as expenses? | **No.** They are added together only on the trip summary. |
| 9 | Does every trip have an agent? | **No.** A client can build a trip in the portal before an agent picks it up. |
| 10 | Where do flight and shuttle fares come from? | **A fare table kept by the Administrator.** The agent overrides the estimate with the fare actually paid. No airline pricing service is used. |

---

# 1. Maintenance of Transport

Transport covers three things and they do not behave alike. A car is hired by
the day and its final cost depends on how far it is driven. A flight and a bus
shuttle are both a seat on a journey between two cities at a set time. The first
is handled as a **vehicle**, the other two together as a **route**.

## 1a. Vehicles (car rental)

## Table 13: Maintenance of Vehicles – Create

| Input Data | Processing | Output |
|---|---|---|
| • VehicleId<br>• VehicleDescription<br>• Make<br>• Model<br>• Colour<br>• Registration number<br>• Capacity (number of passengers)<br>• Location (collection branch)<br>• PricePerDay<br>• IncludedKmPerDay (defaults to 200)<br>• RatePerExtraKm | • Validate all required fields are completed before saving.<br>• Validate that the VehicleId does not already exist in the Vehicle table.<br>• Validate that the registration number is not already assigned to another vehicle.<br>• Validate that PricePerDay, IncludedKmPerDay and Capacity are numeric and greater than zero.<br>• Validate that RatePerExtraKm is numeric and not negative.<br>• After the user clicks the Save button, the new vehicle record is inserted into the Vehicle database table. | • Warning message displayed if any required field is left empty.<br>• Warning message displayed if the VehicleId already exists.<br>• Warning message displayed if the registration number is already in use.<br>• Warning message displayed if any of the pricing or capacity values are invalid.<br>• Success message displayed to confirm the vehicle has been successfully added. |

## Table 14: Maintenance of Vehicles – Update

| Input Data | Processing | Output |
|---|---|---|
| • VehicleId (used to locate the record)<br>• Updated fields as needed: VehicleDescription, Make, Model, Colour, Registration number, Capacity, Location, PricePerDay, IncludedKmPerDay, RatePerExtraKm | • Display the current details of the selected vehicle.<br>• Allow the user to modify any of the editable fields.<br>• Validate that all required fields remain completed and that the pricing values remain valid.<br>• Where a rate is changed, the change applies to new bookings only. Bookings already made keep the rates they were quoted at, so that historic invoices and revenue figures do not change after the fact.<br>• After the user clicks the Save button, the updated record is written to the Vehicle database table. | • Warning message displayed if the VehicleId is not found.<br>• Warning message displayed if any required field is cleared or contains an invalid value.<br>• Message displayed confirming that existing bookings keep their original rates.<br>• Success message displayed to confirm the vehicle has been successfully updated. |

## Table 15: Maintenance of Vehicles – Delete

| Input Data | Processing | Output |
|---|---|---|
| • VehicleId (used to identify the record) | • Display the details of the selected vehicle for the user to review.<br>• Check whether the vehicle appears on any existing booking.<br>• Where bookings exist, the vehicle is deactivated rather than deleted, so that historic bookings and reports remain intact. A deactivated vehicle no longer appears in the booking screens.<br>• Prompt the user with a confirmation dialog before proceeding.<br>• After the user confirms, the record is removed or deactivated. | • Warning message displayed if the VehicleId is not found.<br>• Warning message displayed if the vehicle is linked to existing bookings, explaining that it will be deactivated instead.<br>• Confirmation prompt displayed before deletion is executed.<br>• Success message displayed to confirm the vehicle has been deleted or deactivated. |

> The deactivate rule applies to attractions and accommodation too. The Phase 2
> tables for those modules say the record is simply removed, which will fail once
> bookings point at it. Those tables need the same amendment.

## Table 16: Vehicle Booking – Collection and return

Car rental is the only module where the final cost is not known when the booking
is made, because it depends on how far the client drives.

| Input Data | Processing | Output |
|---|---|---|
| **At booking:**<br>• VehicleId<br>• ItineraryId<br>• StartDate<br>• EndDate<br><br>**At collection:**<br>• OdometerOut<br><br>**At return:**<br>• OdometerIn | • Validate that the vehicle is not already booked for any part of the requested period.<br>• Copy PricePerDay, IncludedKmPerDay and RatePerExtraKm from the vehicle onto the booking, freezing the rates at the time of hire.<br>• Calculate TotalDays as the number of days from StartDate to EndDate inclusive, and the base cost as TotalDays multiplied by the daily rate.<br>• On collection, record the opening odometer reading and set the status to Collected.<br>• On return, record the closing reading. Validate that it is not lower than the opening reading.<br>• Calculate the kilometres driven, subtract the included allowance of IncludedKmPerDay for each day of hire, and charge any excess at RatePerExtraKm.<br>• Add the base cost and the excess kilometre cost to give the final total, and set the status to Returned. | • Warning message displayed if the vehicle is already booked for those dates, showing the conflicting booking.<br>• Warning message displayed if the closing odometer reading is lower than the opening reading.<br>• Booking summary displayed showing the vehicle, dates, number of days, base cost and the kilometre allowance included.<br>• On return, a final summary showing kilometres driven, kilometres included, excess kilometres, the excess charge and the final total.<br>• Success message displayed confirming the rental has been closed. |

---

## 1b. Routes (flights and bus shuttles)

A route is a journey from one city to another, run by an airline or a bus
company, priced per seat. Flights and shuttles share these screens because the
only real difference between them is the carrier. The type is chosen from a
dropdown.

The fare captured against a route is an **indicative** price. It is what the
agency believes a seat on that journey costs, and it is what a client sees when
browsing the portal, so that someone considering Johannesburg to New York has a
figure to work with before phoning. The agency does not hold airline seats. When
a trip is confirmed, the agent books with the airline or the bus company in the
usual way and captures the fare actually paid against the booking.

## Table 17: Maintenance of Routes – Create

| Input Data | Processing | Output |
|---|---|---|
| • RouteId<br>• Route type (selected from list: flight or shuttle)<br>• Carrier (airline or bus company)<br>• Route code (flight number or service number)<br>• Departure city<br>• Arrival city<br>• Departure time<br>• Arrival time<br>• IndicativeFarePerSeat | • Validate all required fields are completed before saving.<br>• Validate that the RouteId does not already exist in the Route table.<br>• Validate that the departure and arrival cities are not the same.<br>• Validate that IndicativeFarePerSeat is numeric and greater than zero.<br>• Calculate the journey duration from the departure and arrival times.<br>• After the user clicks the Save button, the new route record is inserted into the Route database table. | • Warning message displayed if any required field is left empty.<br>• Warning message displayed if the RouteId already exists.<br>• Warning message displayed if the departure and arrival cities are the same.<br>• Warning message displayed if the fare is not a valid amount.<br>• Success message displayed to confirm the route has been successfully added. |

## Table 18: Maintenance of Routes – Update

| Input Data | Processing | Output |
|---|---|---|
| • RouteId (used to locate the record)<br>• Updated fields as needed: Route type, Carrier, Route code, Departure city, Arrival city, Departure time, Arrival time, IndicativeFarePerSeat | • Display the current details of the selected route.<br>• Allow the user to modify any of the editable fields.<br>• Validate that all required fields remain completed and that the fare remains valid.<br>• Where the indicative fare is changed, the change affects new bookings only. Bookings already made keep the fare that was actually paid at the time.<br>• After the user clicks the Save button, the updated record is written to the Route database table. | • Warning message displayed if the RouteId is not found.<br>• Warning message displayed if any required field is cleared or contains an invalid value.<br>• Message displayed confirming that existing bookings are unaffected.<br>• Success message displayed to confirm the route has been successfully updated. |

## Table 19: Maintenance of Routes – Delete

| Input Data | Processing | Output |
|---|---|---|
| • RouteId (used to identify the record) | • Display the details of the selected route for the user to review.<br>• Check whether the route appears on any existing booking.<br>• Where bookings exist, the route is deactivated rather than deleted, so that historic bookings and reports remain intact. A deactivated route no longer appears in the booking screens or the portal.<br>• Prompt the user with a confirmation dialog before proceeding.<br>• After the user confirms, the record is removed or deactivated. | • Warning message displayed if the RouteId is not found.<br>• Warning message displayed if the route is linked to existing bookings, explaining that it will be deactivated instead.<br>• Confirmation prompt displayed before deletion is executed.<br>• Success message displayed to confirm the route has been deleted or deactivated. |

## Table 20: Route Booking – Booking a flight or shuttle

| Input Data | Processing | Output |
|---|---|---|
| • RouteId<br>• ItineraryId<br>• Travel date<br>• Number of seats<br>• Fare per seat (pre-filled with the indicative fare, and changed by the agent to the fare actually paid)<br>• Booking reference from the airline or bus company | • Display the selected route with the carrier, route code, cities, times and indicative fare.<br>• Validate that the number of seats is numeric and greater than zero.<br>• Validate that the fare per seat is numeric and greater than zero.<br>• Calculate the total cost as the number of seats multiplied by the fare per seat.<br>• Compare the running itinerary total against the client's budget.<br>• No availability check is performed. The agency does not hold seats, it records what was booked with the carrier.<br>• After the user clicks the Save button, the booking is inserted into the RouteBooking database table and linked to the itinerary. | • Warning message displayed if the seats or fare contain invalid values.<br>• Warning message displayed if the booking takes the itinerary over the client's budget, showing the overrun. The user may continue.<br>• Reminder displayed where the fare has been left at the indicative price, since that figure is an estimate rather than a fare that was paid.<br>• Booking summary displayed showing the carrier, route, travel date, seats, fare per seat and total cost.<br>• Success message displayed to confirm the booking has been captured. |

---

# 2. Manage Itineraries

## Table 21: Manage Itineraries – Create

| Input Data | Processing | Output |
|---|---|---|
| • ItineraryId<br>• ClientId (selected from the client list, or the logged-in client in the portal)<br>• AgentId (defaults to the logged-in agent, and is left empty where a client creates the trip themselves)<br>• Trip name<br>• StartDate<br>• EndDate<br>• TotalBudget<br>• TripComments | • Validate all required fields are completed before saving.<br>• Validate that the selected client exists and is active.<br>• Where both dates are supplied, validate that EndDate is not earlier than StartDate.<br>• Validate that TotalBudget is numeric and greater than zero.<br>• Set the itinerary status to Draft.<br>• After the user clicks the Save button, the new itinerary record is inserted into the Itineraries database table. | • Warning message displayed if any required field is left empty.<br>• Warning message displayed if the end date falls before the start date.<br>• Warning message displayed if TotalBudget is not a valid amount.<br>• Success message displayed to confirm the itinerary has been created.<br>• The user is taken to the itinerary detail screen so that bookings can be added. |

## Table 22: Manage Itineraries – Add items to an itinerary

| Input Data | Processing | Output |
|---|---|---|
| • ItineraryId<br>• Item type (accommodation, attraction, car rental, or flight or shuttle)<br>• The relevant booking details for the selected type, as set out in the Bookings tables | • Display the itinerary with everything already added, and the running total against the budget.<br>• Pass the booking to the relevant booking process, where availability, capacity and date clashes are validated.<br>• Recalculate the running total for the itinerary once the booking is accepted.<br>• Compare the running total against TotalBudget.<br>• Insert the booking record and link it to the itinerary. | • Warning message displayed if the item is unavailable for the selected dates.<br>• Warning message displayed if adding the item takes the itinerary over the client's budget, showing the amount of the overrun. The user may continue.<br>• Success message displayed and the itinerary refreshed with the new item and the updated total. |

> Booking dates are deliberately not checked against the trip dates. The group
> agreed that what a client can book does not depend on the itinerary dates.

## Table 23: Manage Itineraries – Assign or reassign an agent

| Input Data | Processing | Output |
|---|---|---|
| • ItineraryId<br>• AgentId | • Display the itineraries that currently have no agent assigned, so that trips submitted through the client portal can be picked up.<br>• Validate that the selected user holds the Travel Agent role and is active.<br>• Record the change against the itinerary.<br>• Where an itinerary is reassigned from one agent to another, both agents' figures in the employee performance report follow the current assignment. | • Warning message displayed if the selected user is not an active travel agent.<br>• Success message displayed confirming which agent the trip is now assigned to.<br>• The itinerary is removed from the unassigned list. |

## Table 24: Manage Itineraries – Update

| Input Data | Processing | Output |
|---|---|---|
| • ItineraryId (used to locate the record)<br>• Updated fields as needed: Trip name, StartDate, EndDate, TotalBudget, TripComments, Status<br>• Items to be removed from the itinerary | • Display the current itinerary with all linked bookings and expenses.<br>• Allow the user to modify the editable fields and remove individual items.<br>• Where an item is removed, cancel the linked booking and recalculate the total.<br>• Where the budget is lowered, recheck it against the amount already committed.<br>• Where the status is changed to Confirmed, validate that the itinerary has at least one booking on it.<br>• Record the date and user against any change to TripComments so that the note history is preserved.<br>• After the user clicks the Save button, the updated records are written to the database. | • Warning message displayed if the revised budget is below the amount already committed.<br>• Warning message displayed if an empty itinerary is confirmed.<br>• Confirmation prompt displayed before an item is removed.<br>• Success message displayed to confirm the itinerary has been updated. |

## Table 25: Manage Itineraries – Delete

| Input Data | Processing | Output |
|---|---|---|
| • ItineraryId (used to identify the record) | • Display the itinerary and all linked bookings and expenses for the user to review.<br>• Check the itinerary status. Confirmed and completed itineraries are cancelled rather than deleted, so that revenue reporting is not affected.<br>• Where the itinerary is deleted, all linked bookings and expenses are removed with it.<br>• Prompt the user with a confirmation dialog listing what will be removed.<br>• After the user confirms, the records are removed or the itinerary is marked as cancelled. | • Warning message displayed if the ItineraryId is not found.<br>• Warning message displayed if the itinerary is confirmed, explaining that it will be cancelled rather than deleted.<br>• Confirmation prompt showing the number of bookings and expenses affected.<br>• Success message displayed to confirm the itinerary has been deleted or cancelled. |

---

# 3. Manage Trip Expenses

Both travel agents and clients may capture expenses, and there is no approval
step. The system records who captured each line so that the origin of a figure
can always be seen.

## Table 26: Manage Trip Expenses – Create

| Input Data | Processing | Output |
|---|---|---|
| • ExpenseId<br>• ItineraryId (the trip the expense belongs to)<br>• Expense category (selected from list: meals, fuel, entrance fees, shopping, other)<br>• Description<br>• Amount<br>• Currency<br>• ExpenseDate<br>• Receipt number (optional)<br>• CapturedBy (defaults to the logged-in user) | • Validate all required fields are completed before saving.<br>• Validate that the selected itinerary exists and is not cancelled.<br>• Validate that Amount is numeric and greater than zero.<br>• Where the currency is not Rand, convert the amount using the currency conversion function set out in the project scope, and store both the original and the converted amount.<br>• Add the amount to the running total for the trip and compare it against TotalBudget.<br>• After the user clicks the Save button, the expense is inserted into the TripExpenses database table. | • Warning message displayed if any required field is left empty.<br>• Warning message displayed if the amount is not a valid value.<br>• Warning message displayed if the expense takes the trip over budget, showing the overrun. The user may continue.<br>• Success message displayed to confirm the expense has been captured.<br>• Updated trip total and remaining budget displayed. |

## Table 27: Manage Trip Expenses – Update

| Input Data | Processing | Output |
|---|---|---|
| • ExpenseId (used to locate the record)<br>• Updated fields as needed: Category, Description, Amount, Currency, ExpenseDate, Receipt number | • Display the current details of the selected expense.<br>• Validate that the user is permitted to edit the record. Clients may only edit expenses on their own trips.<br>• Validate that Amount remains numeric and greater than zero.<br>• Recalculate the trip total and recheck it against the budget.<br>• After the user clicks the Save button, the updated record is written to the TripExpenses table. | • Warning message displayed if the ExpenseId is not found.<br>• Warning message displayed if the user is not permitted to edit the record.<br>• Warning message displayed if the revised amount takes the trip over budget.<br>• Success message displayed to confirm the expense has been updated. |

## Table 28: Manage Trip Expenses – Delete

| Input Data | Processing | Output |
|---|---|---|
| • ExpenseId (used to identify the record to be deleted) | • Display the details of the selected expense for the user to review.<br>• Validate that the user is permitted to delete the record.<br>• Prompt the user with a confirmation dialog before proceeding.<br>• After the user confirms, the expense record is removed from the TripExpenses database table.<br>• Recalculate the trip total. | • Warning message displayed if the ExpenseId is not found.<br>• Warning message displayed if the user is not permitted to delete the record.<br>• Confirmation prompt displayed before deletion is executed.<br>• Success message displayed to confirm the expense has been deleted.<br>• Updated trip total displayed. |

## Table 29: Manage Trip Expenses – Trip expense summary

| Input Data | Processing | Output |
|---|---|---|
| • ItineraryId<br>• Optional filter by expense category or date range | • Retrieve all expenses linked to the itinerary.<br>• Retrieve the total cost of all bookings linked to the itinerary, across accommodation, attractions, car rental and flights or shuttles.<br>• Group the expenses by category and calculate subtotals.<br>• Calculate the combined trip cost as bookings plus expenses.<br>• Compare the combined total against TotalBudget and calculate the variance. | • Summary displayed showing booking costs, expenses grouped by category, the combined total, the budget and the amount over or under.<br>• Message displayed where the trip is over budget.<br>• Summary can be exported to PDF as required by the export function in the scope. |
