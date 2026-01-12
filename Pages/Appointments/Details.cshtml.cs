using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Appointments;

public class DetailsModel : PageModel
{
    private readonly AppointmentService _appointmentService;

    public DetailsModel(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public Appointment Appointment { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        Appointment = appointment;
        return Page();
    }
}
