using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HCAMiniEHR.Pages.Appointments;

public class IndexModel : PageModel
{
    private readonly AppointmentService _appointmentService;

    public IndexModel(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public IEnumerable<Appointment> Appointments { get; set; } = new List<Appointment>();

    public async Task OnGetAsync()
    {
        Appointments = await _appointmentService.GetAllAppointmentsAsync();
    }
}
