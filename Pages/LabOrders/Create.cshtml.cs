using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HCAMiniEHR.Pages.LabOrders;

public class CreateModel : PageModel
{
    private readonly LabOrderService _labOrderService;
    private readonly AppointmentService _appointmentService;

    public CreateModel(LabOrderService labOrderService, AppointmentService appointmentService)
    {
        _labOrderService = labOrderService;
        _appointmentService = appointmentService;
    }

    [BindProperty]
    public LabOrder LabOrder { get; set; } = new LabOrder();

    public SelectList Appointments { get; set; } = null!;
    public SelectList TestTypes { get; set; } = null!;

    // Common lab test types
    private static readonly List<string> CommonTestTypes = new()
    {
        "Complete Blood Count (CBC)",
        "Lipid Panel",
        "HbA1c",
        "Thyroid Function Test",
        "Liver Function Test",
        "Kidney Function Test",
        "X-Ray",
        "MRI",
        "CT Scan",
        "Ultrasound",
        "ECG",
        "Urinalysis"
    };

    public async Task OnGetAsync(int? appointmentId)
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        Appointments = new SelectList(
            appointments.Select(a => new
            {
                a.AppointmentId,
                DisplayText = $"Apt #{a.AppointmentId} - {a.Patient.FullName} - {a.AppointmentDate:MM/dd/yyyy}"
            }),
            "AppointmentId",
            "DisplayText"
        );

        TestTypes = new SelectList(CommonTestTypes);

        if (appointmentId.HasValue)
        {
            LabOrder.AppointmentId = appointmentId.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Explicit validation for appointment selection
        if (LabOrder.AppointmentId <= 0)
        {
            ModelState.AddModelError("LabOrder.AppointmentId", "Please select an appointment.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return Page();
        }

        try
        {
            await _labOrderService.CreateLabOrderAsync(LabOrder);
            TempData["SuccessMessage"] = $"Lab order for {LabOrder.TestName} created successfully!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadSelectLists();
            return Page();
        }
    }

    private async Task LoadSelectLists()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        Appointments = new SelectList(
            appointments.Select(a => new
            {
                a.AppointmentId,
                DisplayText = $"Apt #{a.AppointmentId} - {a.Patient.FullName} - {a.AppointmentDate:MM/dd/yyyy}"
            }),
            "AppointmentId",
            "DisplayText"
        );

        TestTypes = new SelectList(CommonTestTypes);
    }
}
