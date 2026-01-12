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

        if (appointmentId.HasValue)
        {
            LabOrder.AppointmentId = appointmentId.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
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
            return Page();
        }
    }
}
