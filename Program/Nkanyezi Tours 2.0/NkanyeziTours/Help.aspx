<%@ Page Title="Help" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>

<script runat="server">
    private string Role
    {
        get
        {
            var role = Nkanyezi.Services.AuthService.IsSignedIn ? Nkanyezi.Services.AuthService.CurrentRole : "";
            return string.IsNullOrEmpty(role) ? "Guest" : role;
        }
    }

    private bool ShowFor(params string[] roles)
    {
        return Array.IndexOf(roles, Role) >= 0;
    }
</script>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="h3 mb-1">Help</h1>
    <p class="text-muted mb-1">
        Answers to common questions. Type a word in the box, for example "trip" or "photo", to find a topic.
    </p>
    <p class="small text-muted mb-4">
        Showing help for: <strong><%: Role == "Guest" ? "Visitors" : Role %></strong>
    </p>

    <div class="card mb-4">
        <div class="card-body">
            <label for="helpSearch" class="form-label">Search help</label>
            <div class="d-flex gap-2">
                <input type="search" id="helpSearch" class="form-control" placeholder="e.g. trip, photo, flight"
                       title="Type a keyword. The topics below are filtered as you type." autocomplete="off" />
                <button type="button" id="helpClear" class="btn btn-outline-secondary" title="Clears the search and shows every topic.">Clear</button>
            </div>
            <div id="helpCount" class="form-text"></div>
        </div>
    </div>

    <div class="accordion" id="helpTopics">

        <% if (ShowFor("Guest", "Client")) { %>
        <div class="accordion-item help-topic" data-keywords="plan trip book booking leg start new client create">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h1">
                    How do I plan a trip?
                </button>
            </h2>
            <div id="h1" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    <ol class="mb-0">
                        <li>Sign in as a client and choose <strong>Create a new trip</strong>. Give the trip a name and a budget.</li>
                        <li>On <strong>My trip</strong>, add a leg: choose the region, the start date (first leg only), the nights and the guests.</li>
                        <li>Choose a resort for the leg, then choose one or more rooms.</li>
                        <li>Add activities and transport for the leg.</li>
                        <li>Add more legs if you are visiting more places. Each leg starts the day the one before it ends.</li>
                        <li>When you are happy, press <strong>Confirm booking</strong>. After that, only your agent can make changes.</li>
                    </ol>
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Client")) { %>
        <div class="accordion-item help-topic" data-keywords="leg region nights guests remove leg dates">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h2">
                    What is a leg, and how do I remove one?
                </button>
            </h2>
            <div id="h2" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    A leg is one stop on your trip: where you stay and for how long. The region you choose decides which
                    resorts, activities and cars you are offered. Press <strong>Remove leg</strong> on the leg to take it off;
                    the legs after it move up to close the gap.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Client")) { %>
        <div class="accordion-item help-topic" data-keywords="resort accommodation room rooms capacity change">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h3">
                    How do I choose a resort and rooms?
                </button>
            </h2>
            <div id="h3" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Press <strong>Choose a resort</strong> on the leg. Only resorts in the leg's region are shown. Then press
                    <strong>Choose rooms</strong> and select as many rooms as you need. If the rooms sleep fewer people than
                    your guests, the card turns red as a warning. Prices are per night for the whole room.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Client")) { %>
        <div class="accordion-item help-topic" data-keywords="activity activities transport car flight bus ticket tickets remove add">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h4">
                    How do I add or remove activities and transport?
                </button>
            </h2>
            <div id="h4" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Use <strong>Add activities</strong> or <strong>Add transport</strong> on the leg. Transport has three tabs:
                    Car rentals, Flights and Bus tickets. You get one flight or bus ticket for every guest on the leg.
                    To take something off, press <strong>Remove</strong> on its card. You can only remove items your agent has
                    not dealt with yet.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Client", "Travel Agent", "Supervisor", "Administrator")) { %>
        <div class="accordion-item help-topic" data-keywords="status requested approved declined confirmed completed cancelled meaning">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h5">
                    What do Requested, Approved and Declined mean?
                </button>
            </h2>
            <div id="h5" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    <ul class="mb-0">
                        <li><strong>Requested</strong> - you chose it and your agent has not looked at it yet.</li>
                        <li><strong>Approved</strong> - your agent accepted it and its cost is added to the trip amount.</li>
                        <li><strong>Declined</strong> - your agent turned it down. It is not counted.</li>
                    </ul>
                    A trip itself is <strong>Draft</strong> while you plan it, <strong>Confirmed</strong> once booked,
                    and <strong>Completed</strong> or <strong>Cancelled</strong> at the end.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Client")) { %>
        <div class="accordion-item help-topic" data-keywords="change edit submitted locked confirm after contact agent">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h6">
                    Can I change my trip after confirming it?
                </button>
            </h2>
            <div id="h6" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    No. Once you press Confirm booking the trip is locked, so the agent books exactly what you chose.
                    Contact your agent if something needs to change.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Travel Agent", "Supervisor", "Administrator")) { %>
        <div class="accordion-item help-topic" data-keywords="agent approve decline items accommodation amount budget itineraries">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h7">
                    Agents: how do I approve what a client asked for?
                </button>
            </h2>
            <div id="h7" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Open <strong>Itineraries</strong> and choose the trip. Under <strong>Asked for by the client</strong>,
                    press <strong>Approve</strong> or <strong>Decline</strong> for the accommodation, activities and transport.
                    Each approval adds its cost to the trip amount at the top. Approved accommodation also shows as booked on
                    the bookings chart.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Supervisor", "Administrator")) { %>
        <div class="accordion-item help-topic" data-keywords="supervisor assign agent pending waiting queue">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h8">
                    Supervisors: how do I give a trip to an agent?
                </button>
            </h2>
            <div id="h8" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Open <strong>Itineraries</strong>. Trips waiting for an agent are listed under
                    <strong>Pending assignment</strong>. Press <strong>Assign to an agent</strong>, choose the agent and save.
                    Agents cannot pick up trips themselves.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Travel Agent", "Supervisor", "Administrator")) { %>
        <div class="accordion-item help-topic" data-keywords="bookings chart gantt calendar month booked maintenance free double">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h9">
                    How do I read the bookings chart?
                </button>
            </h2>
            <div id="h9" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    The chart shows one month at a time. Each row is a room or a car. Solid bars are booked, lighter bars are
                    requests still waiting, and workshop bars show a car in for maintenance. Use <strong>Previous</strong> and <strong>Next</strong> to move between months.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Supervisor", "Administrator")) { %>
        <div class="accordion-item help-topic" data-keywords="settings add vehicle car resort accommodation attraction activity route room photo upload image">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h10">
                    How do I add a resort, activity, vehicle or route, with a photo?
                </button>
            </h2>
            <div id="h10" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Open <strong>Settings</strong> and choose the section. Press add, fill in the fields marked with a red
                    <span class="text-danger">*</span> and save. Accommodation, attractions and vehicles have a
                    <strong>Photo</strong> field: choose a JPG, PNG, GIF or WEBP file up to 5 MB. Tick
                    <strong>Remove this photo</strong> to take the current one away.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Supervisor", "Administrator")) { %>
        <div class="accordion-item help-topic" data-keywords="delete deactivate reactivate inactive remove record">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h11">
                    Why was something deactivated instead of deleted?
                </button>
            </h2>
            <div id="h11" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    If a resort, activity, car or route has been booked or chosen on a client's trip, deleting it would break that
                    trip. It is deactivated instead, so clients can no longer choose it. Use <strong>Reactivate</strong> to bring it back.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Supervisor", "Administrator")) { %>
        <div class="accordion-item help-topic" data-keywords="reports revenue seasonal trends trip cost export excel pdf sort">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h12">
                    How do I create and export a report?
                </button>
            </h2>
            <div id="h12" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Open <strong>Reports</strong>, choose the report and the dates, and press <strong>Preview Report</strong>. Use the tabs
                    to show one part at a time and the sort options to order a table. <strong>Export to Excel</strong> and
                    <strong>Export to PDF</strong> download the part you are looking at.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Guest", "Client")) { %>
        <div class="accordion-item help-topic" data-keywords="sign in login register account password forgot email">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h13">
                    How do I create an account or sign in?
                </button>
            </h2>
            <div id="h13" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Choose <strong>Register</strong>, enter your details and a password of at least 8 characters, and type it again
                    to confirm. Each email address can only be used once. Then sign in with your email and password.
                </div>
            </div>
        </div>
        <% } %>

        <% if (ShowFor("Guest", "Client")) { %>
        <div class="accordion-item help-topic" data-keywords="discover browse search province filter explore without account">
            <h2 class="accordion-header">
                <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#h14">
                    Can I look around before signing up?
                </button>
            </h2>
            <div id="h14" class="accordion-collapse collapse" data-bs-parent="#helpTopics">
                <div class="accordion-body">
                    Yes. <strong>Discover</strong> shows accommodations, activities and transport to everyone. Search by name,
                    filter by province, and press <strong>View rooms</strong> to see a resort's rooms and prices.
                </div>
            </div>
        </div>
        <% } %>

    </div>

    <div id="helpNone" class="alert alert-light border mt-3" hidden>
        No help topics match that word. Try a shorter word, such as "trip" or "car".
    </div>

    <script>
        (function () {
            var box = document.getElementById('helpSearch');
            var clear = document.getElementById('helpClear');
            var count = document.getElementById('helpCount');
            var none = document.getElementById('helpNone');
            var topics = Array.prototype.slice.call(document.querySelectorAll('.help-topic'));

            function filter() {
                var words = box.value.toLowerCase().trim().split(/\s+/).filter(function (w) { return w.length > 0; });
                var shown = 0;

                topics.forEach(function (t) {
                    var text = (t.getAttribute('data-keywords') + ' ' + t.textContent).toLowerCase();
                    var match = words.every(function (w) { return text.indexOf(w) >= 0; });
                    t.hidden = !match;
                    if (match) shown++;
                });

                none.hidden = shown > 0;
                count.textContent = words.length === 0 ? topics.length + ' topics' : shown + ' of ' + topics.length + ' topics match';
            }

            box.addEventListener('input', filter);
            clear.addEventListener('click', function () { box.value = ''; filter(); box.focus(); });

            var q = new URLSearchParams(window.location.search).get('q');
            if (q) box.value = q;
            filter();
        })();
    </script>

</asp:Content>
