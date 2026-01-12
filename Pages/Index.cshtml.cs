using HCAMiniEHR.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Pages;

public class IndexModel : PageModel
{
    private readonly EhrDbContext _context;

    public IndexModel(EhrDbContext context)
    {
        _context = context;
    }

    public int TotalPatients { get; set; }
    public int UpcomingAppointments { get; set; }
    public int PendingLabOrders { get; set; }
    public int TotalAppointments { get; set; }

    public async Task OnGetAsync()
    {
        TotalPatients = await _context.Patients.CountAsync();
        
        var today = DateTime.Now;
        UpcomingAppointments = await _context.Appointments
            .Where(a => a.AppointmentDate >= today && a.Status == "Scheduled")
            .CountAsync();
        
        PendingLabOrders = await _context.LabOrders
            .Where(l => l.Status == "Pending")
            .CountAsync();
        
        TotalAppointments = await _context.Appointments.CountAsync();
    }
}
