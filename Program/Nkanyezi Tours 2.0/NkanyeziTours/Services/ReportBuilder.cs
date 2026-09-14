using Nkanyezi.Data;
using Nkanyezi.Models;

namespace Nkanyezi.Services;

public class ReportBuilder
{
    private readonly ReportRepository _reports;

    public ReportBuilder(ReportRepository reports)
    {
        _reports = reports;
    }

    public static readonly ReportLegendItem RandLegend =
        new ReportLegendItem("R", "South African Rand (ZAR). All amounts are in rand.");

    public static readonly ReportLegendItem BookedLegend =
        new ReportLegendItem("Booked", "The value comes from the bookings an agent has made for the trip.");

    public static readonly ReportLegendItem QuotedLegend =
        new ReportLegendItem("Quoted", "Nothing is booked yet, so the value is estimated from the client's trip plan: room price x nights, activity rate x guests, car price x days and fare x guests.");

    public ReportDocument Build(string reportType, DateTime fromDate, DateTime toDate,
                                int? clientId, string? clientName, string generatedBy)
    {
        var document = reportType switch
        {
            ReportTypes.Seasonal => BuildSeasonal(fromDate, toDate),
            ReportTypes.TripCost => BuildTripCost(fromDate, toDate, clientId, clientName),
            _                    => BuildRevenue(fromDate, toDate)
        };

        document.FromDate    = fromDate.Date;
        document.ToDate      = toDate.Date;
        document.GeneratedAt = DateTime.Now;
        document.GeneratedBy = generatedBy;

        return document;
    }

    private ReportDocument BuildRevenue(DateTime fromDate, DateTime toDate)
    {
        var trips = _reports.GetTripValues(fromDate, toDate);

        var document = new ReportDocument
        {
            Title       = "Revenue report",
            Description = "How much the business earned from trips, by month, by booking type and by client."
        };

        var total      = trips.Sum(t => t.Revenue);
        var clients    = trips.Select(t => t.ClientId).Distinct().Count();
        var perClient  = clients == 0 ? 0 : total / clients;
        var perTrip    = trips.Count == 0 ? 0 : total / trips.Count;

        document.Stats.Add(new ReportStat("Total revenue",            Money(total)));
        document.Stats.Add(new ReportStat("Trips counted",            trips.Count.ToString("N0")));
        document.Stats.Add(new ReportStat("Average spend per client", Money(perClient)));
        document.Stats.Add(new ReportStat("Average trip value",       Money(perTrip)));

        var byMonth = new ReportSection
        {
            Key         = "revenue-by-month",
            Heading     = "Revenue by month",
            Description = "Revenue for each month, split by booking type. Trips are placed in the month they start."
        };
        byMonth.Columns.Add(new ReportColumn("Month",                   ReportColumnKind.Text,   3.0));
        byMonth.Columns.Add(new ReportColumn("Trips",                   ReportColumnKind.Number, 1.4));
        byMonth.Columns.Add(new ReportColumn("Accommodation",           ReportColumnKind.Money,  2.6));
        byMonth.Columns.Add(new ReportColumn("Activities",              ReportColumnKind.Money,  2.4));
        byMonth.Columns.Add(new ReportColumn("Car rental",              ReportColumnKind.Money,  2.4));
        byMonth.Columns.Add(new ReportColumn("Flights and buses",       ReportColumnKind.Money,  2.6));
        byMonth.Columns.Add(new ReportColumn("Total revenue",           ReportColumnKind.Money,  2.6));

        foreach (var month in Months(fromDate, toDate))
        {
            var inMonth = trips.Where(t => t.TripDate.Year == month.Year && t.TripDate.Month == month.Month).ToList();

            byMonth.Rows.Add(new object?[]
            {
                month.ToString("MMMM yyyy"),
                inMonth.Count,
                inMonth.Sum(t => t.Accommodation),
                inMonth.Sum(t => t.Activities),
                inMonth.Sum(t => t.CarRental),
                inMonth.Sum(t => t.FlightsAndBuses),
                inMonth.Sum(t => t.Revenue)
            });
        }

        byMonth.TotalRow = new object?[]
        {
            "Total",
            trips.Count,
            trips.Sum(t => t.Accommodation),
            trips.Sum(t => t.Activities),
            trips.Sum(t => t.CarRental),
            trips.Sum(t => t.FlightsAndBuses),
            total
        };

        var byType = new ReportSection
        {
            Key         = "revenue-by-type",
            Heading     = "Revenue by booking type",
            Description = "Which kinds of booking bring in the most money."
        };
        byType.Columns.Add(new ReportColumn("Booking type",     ReportColumnKind.Text,    6.0));
        byType.Columns.Add(new ReportColumn("Revenue",          ReportColumnKind.Money,   5.5));
        byType.Columns.Add(new ReportColumn("Share of revenue", ReportColumnKind.Percent, 5.5));

        var types = new List<KeyValuePair<string, decimal>>
        {
            new KeyValuePair<string, decimal>("Accommodation",     trips.Sum(t => t.Accommodation)),
            new KeyValuePair<string, decimal>("Activities",        trips.Sum(t => t.Activities)),
            new KeyValuePair<string, decimal>("Car rental",        trips.Sum(t => t.CarRental)),
            new KeyValuePair<string, decimal>("Flights and buses", trips.Sum(t => t.FlightsAndBuses))
        };

        foreach (var type in types.OrderByDescending(t => t.Value))
            byType.Rows.Add(new object?[] { type.Key, type.Value, Share(type.Value, total) });

        byType.TotalRow = new object?[] { "Total", total, total == 0 ? 0m : 100m };

        var byClient = new ReportSection
        {
            Key         = "revenue-by-client",
            Heading     = "Average spend per client",
            Description = "What each client spent in the period, highest first.",
            EmptyText   = "No clients had trips in the dates chosen."
        };
        byClient.Columns.Add(new ReportColumn("Client",               ReportColumnKind.Text,   6.2));
        byClient.Columns.Add(new ReportColumn("Trips",                ReportColumnKind.Number, 2.0));
        byClient.Columns.Add(new ReportColumn("Total spent",          ReportColumnKind.Money,  4.4));
        byClient.Columns.Add(new ReportColumn("Average per trip",     ReportColumnKind.Money,  4.4));

        foreach (var client in trips.GroupBy(t => new { t.ClientId, t.ClientName })
                                    .Select(g => new { g.Key.ClientName, Trips = g.Count(), Total = g.Sum(t => t.Revenue) })
                                    .OrderByDescending(c => c.Total)
                                    .ThenBy(c => c.ClientName))
        {
            byClient.Rows.Add(new object?[] { client.ClientName, client.Trips, client.Total, client.Total / client.Trips });
        }

        if (byClient.Rows.Count > 0)
            byClient.TotalRow = new object?[] { "All clients", trips.Count, total, perTrip };

        var tripList = TripListSection("revenue-trips", "Trips included",
            "Every trip counted in this report, with where its value comes from.");

        foreach (var trip in trips)
            tripList.Rows.Add(new object?[] { trip.TripName, trip.ClientName, trip.TripDate, trip.Status, trip.Source, trip.Revenue });

        if (trips.Count > 0)
            tripList.TotalRow = new object?[] { "Total", "", null, "", "", total };

        document.Sections.Add(byMonth);
        document.Sections.Add(byType);
        document.Sections.Add(byClient);
        document.Sections.Add(tripList);

        document.Legend.Add(RandLegend);
        document.Legend.Add(new ReportLegendItem("Revenue", "What clients are charged for accommodation, activities, car rental, flights and buses. Trip expenses are not included."));
        document.Legend.Add(new ReportLegendItem("Share of revenue", "The part of total revenue, as a percentage."));
        document.Legend.Add(BookedLegend);
        document.Legend.Add(QuotedLegend);

        return document;
    }

    private ReportDocument BuildSeasonal(DateTime fromDate, DateTime toDate)
    {
        var trips      = _reports.GetTripValues(fromDate, toDate);
        var provinces  = _reports.GetTripsByProvince(fromDate, toDate);
        var activities = _reports.GetTopActivities(fromDate, toDate, 10);

        var document = new ReportDocument
        {
            Title       = "Seasonal trends report",
            Description = "When clients travel, which provinces they visit and which activities are most popular."
        };

        var months = Months(fromDate, toDate)
            .Select(m => new
            {
                Month = m,
                Trips = trips.Count(t => t.TripDate.Year == m.Year && t.TripDate.Month == m.Month)
            })
            .ToList();

        var busiest     = months.Where(m => m.Trips > 0).OrderByDescending(m => m.Trips).ThenBy(m => m.Month).FirstOrDefault();
        var topProvince = provinces.FirstOrDefault(p => p.Trips > 0);
        var topActivity = activities.FirstOrDefault();

        document.Stats.Add(new ReportStat("Trips counted",          trips.Count.ToString("N0")));
        document.Stats.Add(new ReportStat("Busiest month",          busiest == null ? "None" : busiest.Month.ToString("MMMM yyyy")));
        document.Stats.Add(new ReportStat("Most visited province",  topProvince?.ProvinceName ?? "None"));
        document.Stats.Add(new ReportStat("Most booked activity",   topActivity?.ActivityName ?? "None"));

        var byMonth = new ReportSection
        {
            Key         = "seasonal-by-month",
            Heading     = "Trips by month",
            Description = "How many trips start in each month, to show busy and quiet seasons."
        };
        byMonth.Columns.Add(new ReportColumn("Month",          ReportColumnKind.Text,    6.0));
        byMonth.Columns.Add(new ReportColumn("Trips",          ReportColumnKind.Number,  5.5));
        byMonth.Columns.Add(new ReportColumn("Share of trips", ReportColumnKind.Percent, 5.5));

        foreach (var month in months)
            byMonth.Rows.Add(new object?[] { month.Month.ToString("MMMM yyyy"), month.Trips, Share(month.Trips, trips.Count) });

        byMonth.TotalRow = new object?[] { "Total", trips.Count, trips.Count == 0 ? 0m : 100m };

        var byProvince = new ReportSection
        {
            Key         = "seasonal-by-province",
            Heading     = "Trips by province",
            Description = "The provinces clients visit, busiest first. A trip that visits more than one province is counted in each."
        };
        byProvince.Columns.Add(new ReportColumn("Province",        ReportColumnKind.Text,    6.0));
        byProvince.Columns.Add(new ReportColumn("Trips",           ReportColumnKind.Number,  5.5));
        byProvince.Columns.Add(new ReportColumn("Share of trips",  ReportColumnKind.Percent, 5.5));

        foreach (var province in provinces)
            byProvince.Rows.Add(new object?[] { province.ProvinceName, province.Trips, Share(province.Trips, trips.Count) });

        var topTen = new ReportSection
        {
            Key         = "seasonal-top-activities",
            Heading     = "Top 10 most booked activities",
            Description = "The activities that appear on the most trips.",
            EmptyText   = "No activities were booked in the dates chosen."
        };
        topTen.Columns.Add(new ReportColumn("Rank",          ReportColumnKind.Number, 1.2));
        topTen.Columns.Add(new ReportColumn("Activity",      ReportColumnKind.Text,   5.0));
        topTen.Columns.Add(new ReportColumn("Activity type", ReportColumnKind.Text,   2.8));
        topTen.Columns.Add(new ReportColumn("Province",      ReportColumnKind.Text,   3.2));
        topTen.Columns.Add(new ReportColumn("Trips",         ReportColumnKind.Number, 2.2));
        topTen.Columns.Add(new ReportColumn("People",        ReportColumnKind.Number, 2.6));

        var rank = 1;
        foreach (var activity in activities)
            topTen.Rows.Add(new object?[] { rank++, activity.ActivityName, activity.ActivityType ?? "Not set", activity.ProvinceName ?? "Not set", activity.Trips, activity.People });

        document.Sections.Add(byMonth);
        document.Sections.Add(byProvince);
        document.Sections.Add(topTen);

        document.Legend.Add(new ReportLegendItem("Share of trips", "The part of all trips counted, as a percentage. Provinces can add up to more than 100% because one trip can visit several provinces."));
        document.Legend.Add(new ReportLegendItem("People", "The number of guests or tickets on the activity, across all trips."));
        document.Legend.Add(BookedLegend);
        document.Legend.Add(QuotedLegend);

        return document;
    }

    private ReportDocument BuildTripCost(DateTime fromDate, DateTime toDate, int? clientId, string? clientName)
    {
        var trips = _reports.GetTripValues(fromDate, toDate, clientId);

        var document = new ReportDocument
        {
            Title           = "Trip cost report",
            Description     = "What each trip costs, how that compares with the client's budget, and which trips went over.",
            HasClientFilter = true,
            ClientName      = clientName,
            Landscape       = true
        };

        var totalBudget = trips.Sum(t => t.TotalBudget);
        var totalCost   = trips.Sum(t => t.TotalCost);
        var over        = trips.Where(t => t.OverBudget).ToList();

        document.Stats.Add(new ReportStat("Trips counted",     trips.Count.ToString("N0")));
        document.Stats.Add(new ReportStat("Total budget",      Money(totalBudget)));
        document.Stats.Add(new ReportStat("Total cost",        Money(totalCost)));
        document.Stats.Add(new ReportStat("Trips over budget", over.Count.ToString("N0")));

        var breakdown = new ReportSection
        {
            Key         = "tripcost-breakdown",
            Heading     = "Cost breakdown per trip",
            Description = "What makes up the cost of each trip."
        };
        breakdown.Columns.Add(new ReportColumn("Trip",          ReportColumnKind.Text,  4.2));
        breakdown.Columns.Add(new ReportColumn("Client",        ReportColumnKind.Text,  3.6));
        breakdown.Columns.Add(new ReportColumn("Start date",    ReportColumnKind.Date,  2.2));
        breakdown.Columns.Add(new ReportColumn("Value from",    ReportColumnKind.Text,  1.9));
        breakdown.Columns.Add(new ReportColumn("Accommodation", ReportColumnKind.Money, 2.6));
        breakdown.Columns.Add(new ReportColumn("Activities",    ReportColumnKind.Money, 2.4));
        breakdown.Columns.Add(new ReportColumn("Transport",     ReportColumnKind.Money, 2.4));
        breakdown.Columns.Add(new ReportColumn("Expenses",      ReportColumnKind.Money, 2.4));
        breakdown.Columns.Add(new ReportColumn("Total cost",    ReportColumnKind.Money, 2.6));

        foreach (var trip in trips)
            breakdown.Rows.Add(new object?[] { trip.TripName, trip.ClientName, trip.TripDate, trip.Source, trip.Accommodation, trip.Activities, trip.Transport, trip.Expenses, trip.TotalCost });

        if (trips.Count > 0)
            breakdown.TotalRow = new object?[] { "Total", "", null, "", trips.Sum(t => t.Accommodation), trips.Sum(t => t.Activities), trips.Sum(t => t.Transport), trips.Sum(t => t.Expenses), totalCost };

        var budget = new ReportSection
        {
            Key         = "tripcost-budget",
            Heading     = "Budget versus actual per trip",
            Description = "Each trip's total cost against the budget the client gave."
        };
        budget.Columns.Add(new ReportColumn("Trip",            ReportColumnKind.Text,    4.6));
        budget.Columns.Add(new ReportColumn("Client",          ReportColumnKind.Text,    3.8));
        budget.Columns.Add(new ReportColumn("Budget",          ReportColumnKind.Money,   3.0));
        budget.Columns.Add(new ReportColumn("Total cost",      ReportColumnKind.Money,   3.0));
        budget.Columns.Add(new ReportColumn("Difference",      ReportColumnKind.Money,   3.0));
        budget.Columns.Add(new ReportColumn("Budget used",     ReportColumnKind.Percent, 2.4));
        budget.Columns.Add(new ReportColumn("Result",          ReportColumnKind.Text,    3.2));

        foreach (var trip in trips)
            budget.Rows.Add(new object?[] { trip.TripName, trip.ClientName, trip.TotalBudget, trip.TotalCost, trip.BudgetDifference, Share(trip.TotalCost, trip.TotalBudget), trip.OverBudget ? "Over budget" : "Within budget" });

        if (trips.Count > 0)
            budget.TotalRow = new object?[] { "Total", "", totalBudget, totalCost, totalBudget - totalCost, Share(totalCost, totalBudget), "" };

        var overBudget = new ReportSection
        {
            Key         = "tripcost-over-budget",
            Heading     = "Trips that went over budget",
            Description = "Trips whose total cost is more than the client's budget, biggest overspend first.",
            EmptyText   = "No trips went over budget in the dates chosen."
        };
        overBudget.Columns.Add(new ReportColumn("Trip",       ReportColumnKind.Text,  5.0));
        overBudget.Columns.Add(new ReportColumn("Client",     ReportColumnKind.Text,  4.2));
        overBudget.Columns.Add(new ReportColumn("Agent",      ReportColumnKind.Text,  4.2));
        overBudget.Columns.Add(new ReportColumn("Budget",     ReportColumnKind.Money, 3.8));
        overBudget.Columns.Add(new ReportColumn("Total cost", ReportColumnKind.Money, 3.8));
        overBudget.Columns.Add(new ReportColumn("Over by",    ReportColumnKind.Money, 3.8));

        foreach (var trip in over.OrderByDescending(t => t.TotalCost - t.TotalBudget))
            overBudget.Rows.Add(new object?[] { trip.TripName, trip.ClientName, trip.AgentName ?? "Not assigned", trip.TotalBudget, trip.TotalCost, trip.TotalCost - trip.TotalBudget });

        if (over.Count > 0)
            overBudget.TotalRow = new object?[] { "Total", "", "", over.Sum(t => t.TotalBudget), over.Sum(t => t.TotalCost), over.Sum(t => t.TotalCost - t.TotalBudget) };

        document.Sections.Add(breakdown);
        document.Sections.Add(budget);
        document.Sections.Add(overBudget);

        document.Legend.Add(RandLegend);
        document.Legend.Add(new ReportLegendItem("Transport", "Car rental plus flights and buses."));
        document.Legend.Add(new ReportLegendItem("Expenses", "Extra costs captured against the trip, such as fuel or tips."));
        document.Legend.Add(new ReportLegendItem("Total cost", "Accommodation, activities and transport, plus expenses."));
        document.Legend.Add(new ReportLegendItem("Difference", "Budget minus total cost. A negative amount means the trip is over budget."));
        document.Legend.Add(new ReportLegendItem("Budget used", "Total cost as a percentage of the budget."));
        document.Legend.Add(new ReportLegendItem("Value from", "Whether the trip's value is Booked or Quoted (see below)."));
        document.Legend.Add(BookedLegend);
        document.Legend.Add(QuotedLegend);

        return document;
    }

    private static ReportSection TripListSection(string key, string heading, string description)
    {
        var section = new ReportSection
        {
            Key         = key,
            Heading     = heading,
            Description = description,
            StartCollapsed = true
        };
        section.Columns.Add(new ReportColumn("Trip",       ReportColumnKind.Text,  4.2));
        section.Columns.Add(new ReportColumn("Client",     ReportColumnKind.Text,  3.6));
        section.Columns.Add(new ReportColumn("Start date", ReportColumnKind.Date,  2.3));
        section.Columns.Add(new ReportColumn("Status",     ReportColumnKind.Text,  2.2));
        section.Columns.Add(new ReportColumn("Value from", ReportColumnKind.Text,  2.1));
        section.Columns.Add(new ReportColumn("Revenue",    ReportColumnKind.Money, 2.6));
        return section;
    }

    private static IEnumerable<DateTime> Months(DateTime fromDate, DateTime toDate)
    {
        var month = new DateTime(fromDate.Year, fromDate.Month, 1);
        var last  = new DateTime(toDate.Year, toDate.Month, 1);

        while (month <= last)
        {
            yield return month;
            month = month.AddMonths(1);
        }
    }

    private static decimal Share(decimal part, decimal whole)
        => whole == 0 ? 0 : Math.Round(part / whole * 100, 1);

    private static string Money(decimal amount) => ReportDocument.FormatCell(amount, ReportColumnKind.Money);
}
