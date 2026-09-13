using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nkanyezi.Data;
using Nkanyezi.Models;

namespace NkanyeziTours.Maintain.Rates;

public partial class _Default : Page
{
    private ExchangeRateRepository _rates => Repositories.ExchangeRates;
    private AuthService _auth => Repositories.Auth;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        txtFrom.Text = DateTime.Today.ToString("yyyy-MM-dd");
        BindGrid();
    }

    private void BindGrid()
    {
        gvRates.DataSource = chkCurrentOnly.Checked
            ? _rates.GetCurrentRates()
            : _rates.GetAll();

        gvRates.DataBind();
    }

    protected void chkCurrentOnly_Changed(object sender, EventArgs e) => BindGrid();

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
            return;

        var effectiveFrom = ParseDate(txtFrom.Text);

        if (effectiveFrom == null)
        {
            ShowMessage("That effective date could not be read.", "alert-danger");
            return;
        }

        var rate = new ExchangeRate
        {
            CurrencyCode  = txtCode.Text.Trim().ToUpperInvariant(),
            CurrencyName  = txtName.Text.Trim(),
            RateToZar     = decimal.Parse(txtRate.Text.Trim(), CultureInfo.InvariantCulture),
            EffectiveFrom = effectiveFrom.Value,
            CapturedBy    = AuthService.CurrentUserId
        };

        if (_rates.GetAll().Any(r => r.CurrencyCode == rate.CurrencyCode
                                  && r.EffectiveFrom.Date == rate.EffectiveFrom.Date))
        {
            ShowMessage($"A {rate.CurrencyCode} rate is already captured for that date.",
                        "alert-danger");
            return;
        }

        _rates.Capture(rate);

        ShowMessage($"{rate.CurrencyCode} captured at R{rate.RateToZar:N6}.", "alert-success");

        txtCode.Text = "";
        txtName.Text = "";
        txtRate.Text = "";
        txtFrom.Text = DateTime.Today.ToString("yyyy-MM-dd");

        BindGrid();
    }

    protected void gvRates_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "DeleteRate")
            return;

        _rates.Delete(Convert.ToInt32(e.CommandArgument));
        ShowMessage("Rate deleted.", "alert-success");
        BindGrid();
    }

    private static DateTime? ParseDate(string value)
        => DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                  DateTimeStyles.None, out var result)
               ? result
               : (DateTime?)null;

    private void ShowMessage(string text, string style)
    {
        litMessage.Text = text;
        pnlMessage.CssClass = "alert py-2 " + style;
        pnlMessage.Visible = true;
    }
}
