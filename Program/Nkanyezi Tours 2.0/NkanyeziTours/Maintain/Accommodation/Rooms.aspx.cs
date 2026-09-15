using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Accommodation;

public partial class Rooms : Page
{
    private AccommodationRepository _accommodation => Repositories.Accommodation;
    private RoomRepository _rooms => Repositories.Rooms;

    private int? AccommodationId =>
        int.TryParse(Request.QueryString["accommodationId"], out var id) ? id : (int?)null;

    private int? EditingRoomId
    {
        get => ViewState["EditingRoomId"] as int?;
        set => ViewState["EditingRoomId"] = value;
    }

    private Nkanyezi.Models.Accommodation? _property;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (AccommodationId == null)
        {
            Response.Redirect("~/Maintain/Accommodation/AccommodationList.aspx");
            return;
        }

        _property = _accommodation.GetById(AccommodationId.Value);

        if (_property == null)
        {
            Response.Redirect("~/Maintain/Accommodation/AccommodationList.aspx");
            return;
        }

        litSubtitle.Text = Server.HtmlEncode($"{_property.Name} - {_property.Location}");

        if (!IsPostBack)
            BindRooms();
    }

    private void BindRooms()
    {
        var rooms = _rooms.GetByAccommodation(AccommodationId!.Value);

        lblCount.Text = rooms.Count.ToString();

        gvRooms.DataSource = rooms;
        gvRooms.DataBind();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var room = new Room
        {
            AccommodationId    = AccommodationId!.Value,
            RoomNumber         = txtNumber.Text.Trim(),
            RoomType           = ddlType.SelectedValue,
            Beds               = int.TryParse(txtBeds.Text.Trim(), out var beds) ? beds : 1,
            Size               = txtSize.Text.Trim(),
            Bathroom           = chkBathroom.Checked,
            AvailabilityStatus = chkAvailable.Checked,
            PricePerNight      = decimal.Parse(txtPrice.Text.Trim(), CultureInfo.InvariantCulture),
            Capacity           = int.TryParse(txtCapacity.Text.Trim(), out var cap) ? cap : 1
        };

        if (RoomNumberTaken(room.RoomNumber))
        {
            ShowMessage($"This property already has a room {room.RoomNumber}.", "alert-danger");
            return;
        }

        if (EditingRoomId == null)
        {
            _rooms.Create(room);
            ShowMessage($"Room {room.RoomNumber} added.", "alert-success");
        }
        else
        {
            room.RoomId = EditingRoomId.Value;
            _rooms.Update(room);
            ShowMessage($"Room {room.RoomNumber} saved.", "alert-success");
        }

        ClearForm();
        BindRooms();
    }

    private bool RoomNumberTaken(string roomNumber)
        => _rooms.GetByAccommodation(AccommodationId!.Value)
                 .Any(r => r.RoomId != (EditingRoomId ?? 0)
                        && string.Equals(r.RoomNumber, roomNumber,
                                         StringComparison.OrdinalIgnoreCase));

    protected void gvRooms_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        var roomId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "EditRoom")
        {
            LoadIntoForm(roomId);
        }
        else if (e.CommandName == "DeleteRoom")
        {
            var takenOutOfService = _rooms.Delete(roomId);

            ShowMessage(takenOutOfService
                ? "That room has bookings against it, so it was taken out of service rather than deleted."
                : "Room deleted.",
                takenOutOfService ? "alert-warning" : "alert-success");

            ClearForm();
            BindRooms();
        }
    }

    private void LoadIntoForm(int roomId)
    {
        var room = _rooms.GetById(roomId);

        if (room == null)
        {
            ShowMessage("That room no longer exists.", "alert-warning");
            BindRooms();
            return;
        }

        EditingRoomId = room.RoomId;

        txtNumber.Text        = room.RoomNumber;
        ddlType.SelectedValue = room.RoomType ?? "";
        txtBeds.Text          = room.Beds.ToString();
        txtSize.Text          = room.Size;
        chkBathroom.Checked   = room.Bathroom;
        chkAvailable.Checked  = room.AvailabilityStatus;
        txtPrice.Text         = room.PricePerNight.ToString("0.00", CultureInfo.InvariantCulture);
        txtCapacity.Text      = room.Capacity.ToString();

        litFormHeading.Text   = "Edit room " + Server.HtmlEncode(room.RoomNumber);
        btnSave.Text          = "Save room";
        btnCancelEdit.Visible = true;

        BindRooms();
    }

    protected void btnCancelEdit_Click(object sender, EventArgs e)
    {
        ClearForm();
        BindRooms();
    }

    private void ClearForm()
    {
        EditingRoomId = null;

        txtNumber.Text       = "";
        ddlType.SelectedIndex = 0;
        txtBeds.Text         = "2";
        txtSize.Text         = "";
        chkBathroom.Checked  = true;
        chkAvailable.Checked = true;
        txtPrice.Text        = "";
        txtCapacity.Text     = "2";

        litFormHeading.Text   = "Add a room";
        btnSave.Text          = "Add room";
        btnCancelEdit.Visible = false;
    }

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
