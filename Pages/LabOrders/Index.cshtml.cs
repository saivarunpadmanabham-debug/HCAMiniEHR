using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.LabOrders;

public class IndexModel : PageModel
{
    private readonly LabOrderService _labOrderService;

    public IndexModel(LabOrderService labOrderService)
    {
        _labOrderService = labOrderService;
    }

    public IEnumerable<LabOrder> LabOrders { get; set; } = new List<LabOrder>();

    public async Task OnGetAsync()
    {
        LabOrders = await _labOrderService.GetAllLabOrdersAsync();
    }
}
