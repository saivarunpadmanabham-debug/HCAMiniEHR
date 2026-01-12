using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.LabOrders;

public class UpdateStatusModel : PageModel
{
    private readonly LabOrderService _labOrderService;

    public UpdateStatusModel(LabOrderService labOrderService)
    {
        _labOrderService = labOrderService;
    }

    [BindProperty]
    public int LabOrderId { get; set; }

    [BindProperty]
    public string TestName { get; set; } = string.Empty;

    [BindProperty]
    public string Status { get; set; } = string.Empty;

    [BindProperty]
    public string? Results { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var labOrder = await _labOrderService.GetLabOrderByIdAsync(id);
        if (labOrder == null)
        {
            return NotFound();
        }

        LabOrderId = labOrder.LabOrderId;
        TestName = labOrder.TestName;
        Status = labOrder.Status;
        Results = labOrder.Results;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _labOrderService.UpdateLabOrderStatusAsync(LabOrderId, Status, Results);
            TempData["SuccessMessage"] = "Lab order status updated successfully!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
