using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;

    [BindProperty]
    public string? Results { get; set; }

    private string? OriginalStatus { get; set; }
    private string? OriginalResults { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var labOrder = await _labOrderService.GetLabOrderByIdAsync(id);
            if (labOrder == null)
            {
                TempData["ErrorMessage"] = "Lab order not found.";
                return RedirectToPage("./Index");
            }

            LabOrderId = labOrder.LabOrderId;
            TestName = labOrder.TestName;
            Status = labOrder.Status;
            Results = labOrder.Results;
            
            // Store original values in TempData for comparison
            TempData["OriginalStatus"] = labOrder.Status;
            TempData["OriginalResults"] = labOrder.Results;

            return Page();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading lab order: {ex.Message}";
            return RedirectToPage("./Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Conditional validation for Results based on Status
        if (Status == "Completed" || Status == "Cancelled")
        {
            if (string.IsNullOrWhiteSpace(Results))
            {
                ModelState.AddModelError(nameof(Results), "Results are required when status is Completed or Cancelled.");
            }
            else if (Results.Length < 10)
            {
                ModelState.AddModelError(nameof(Results), "Results must be at least 10 characters long.");
            }
        }

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please fill in all required fields.");
            return Page();
        }

        // Validate that status is not empty
        if (string.IsNullOrWhiteSpace(Status))
        {
            ModelState.AddModelError(nameof(Status), "Status cannot be empty.");
            return Page();
        }

        // Check if anything changed
        var originalStatus = TempData["OriginalStatus"]?.ToString();
        var originalResults = TempData["OriginalResults"]?.ToString();

        if (Status == originalStatus && Results == originalResults)
        {
            ModelState.AddModelError(string.Empty, "No changes were made. Please update at least one field.");
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
