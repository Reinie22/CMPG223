namespace Nkanyezi.Models;

/// <summary>
/// One thing a client has asked for on their own draft trip - a hotel or an
/// attraction - waiting for an agent to approve it.
///
/// This is deliberately NOT a booking. A booking holds real capacity and
/// carries the price the client is charged; a request holds neither. The agent
/// turns an approved request into a real booking through the normal booking
/// repository, which is where capacity and price are settled.
/// </summary>
public class TripRequestItem
{
    public int TripRequestItemId { get; set; }

    public int ItineraryId { get; set; }

    /// <summary>"Accommodation" or "Attraction". Use RequestItemTypes.</summary>
    public string ItemType { get; set; } = RequestItemTypes.Accommodation;

    /// <summary>Set when ItemType is Accommodation, otherwise null.</summary>
    public int? AccommodationId { get; set; }

    /// <summary>Set when ItemType is Attraction, otherwise null.</summary>
    public int? AttractionId { get; set; }

    /// <summary>Check-in for accommodation, or the visit date for an attraction.</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Check-out. Not used for attractions.</summary>
    public DateTime? EndDate { get; set; }

    public int Guests { get; set; } = 1;

    public string? ClientNote { get; set; }

    /// <summary>Requested, Approved or Declined. Use RequestStatus.</summary>
    public string Status { get; set; } = RequestStatus.Requested;

    /// <summary>The agent's reason, mostly for a decline.</summary>
    public string? AgentNote { get; set; }

    public DateTime CreatedDate { get; set; }

    // -------------------------------------------------------------------------
    // Joined in for display. Not columns on the table.
    // -------------------------------------------------------------------------

    /// <summary>The hotel or attraction name.</summary>
    public string ItemName { get; set; } = "";

    /// <summary>Town and province, so one line identifies the place.</summary>
    public string? ItemLocation { get; set; }

    /// <summary>
    /// The published guide price - per night for accommodation, per person for
    /// an attraction. A guide only: the agent captures the real price on the
    /// booking, exactly as with route fares.
    /// </summary>
    public decimal GuidePrice { get; set; }

    // -------------------------------------------------------------------------

    public bool IsAccommodation => ItemType == RequestItemTypes.Accommodation;
    public bool IsAttraction    => ItemType == RequestItemTypes.Attraction;

    public bool IsRequested => Status == RequestStatus.Requested;
    public bool IsApproved  => Status == RequestStatus.Approved;
    public bool IsDeclined  => Status == RequestStatus.Declined;

    /// <summary>Nights for accommodation; zero when the dates are not both set.</summary>
    public int Nights =>
        StartDate == null || EndDate == null ? 0 : (EndDate.Value - StartDate.Value).Days;

    /// <summary>"2026-12-05 to 2026-12-12", or just the one date for a visit.</summary>
    public string DateDisplay
    {
        get
        {
            if (StartDate == null) return "No date given";

            var start = StartDate.Value.ToString("yyyy-MM-dd");

            if (EndDate == null || IsAttraction) return start;

            return $"{start} to {EndDate.Value:yyyy-MM-dd}";
        }
    }

    /// <summary>
    /// Roughly what this would cost at the guide price, so the client can see
    /// their draft adding up against the budget they gave.
    /// </summary>
    public decimal EstimatedCost =>
        IsAccommodation ? GuidePrice * Math.Max(Nights, 1)
                        : GuidePrice * Guests;

    /// <summary>The badge colour, so the status reads at a glance.</summary>
    public string StatusCssClass => Status switch
    {
        RequestStatus.Approved => "text-bg-success",
        RequestStatus.Declined => "text-bg-danger",
        _                      => "text-bg-secondary"
    };
}

/// <summary>
/// The two kinds of request. The database has
/// CHECK (ItemType IN ('Accommodation','Attraction')) on the column.
/// </summary>
public static class RequestItemTypes
{
    public const string Accommodation = "Accommodation";
    public const string Attraction    = "Attraction";
}

/// <summary>
/// Where a request has got to. The database has
/// CHECK (Status IN ('Requested','Approved','Declined')).
/// </summary>
public static class RequestStatus
{
    public const string Requested = "Requested";
    public const string Approved  = "Approved";
    public const string Declined  = "Declined";
}
