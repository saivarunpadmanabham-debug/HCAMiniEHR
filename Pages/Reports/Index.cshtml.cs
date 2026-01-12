using HCAMiniEHR.Data;
using HCAMiniEHR.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Pages.Reports;

public class IndexModel : PageModel
{
    private readonly EhrDbContext _context;

    public IndexModel(EhrDbContext context)
    {
        _context = context;
    }

    // Report 1: Pending Lab Orders
    public IEnumerable<PendingLabOrderReport> PendingLabOrders { get; set; } = new List<PendingLabOrderReport>();

    // Report 2: Patients Without Follow-Up
    public IEnumerable<PatientWithoutFollowUpReport> PatientsWithoutFollowUp { get; set; } = new List<PatientWithoutFollowUpReport>();

    // Report 3: Appointments by Month
    public IEnumerable<AppointmentsByMonthReport> AppointmentsByMonth { get; set; } = new List<AppointmentsByMonthReport>();

    public async Task OnGetAsync()
    {
        // Report 1: Pending Lab Orders using LINQ (Where, Include, OrderBy)
        PendingLabOrders = await _context.LabOrders
            .Include(lo => lo.Appointment)
                .ThenInclude(a => a.Patient)
            .Where(lo => lo.Status == "Pending")
            .OrderBy(lo => lo.OrderDate)
            .Select(lo => new PendingLabOrderReport
            {
                LabOrderId = lo.LabOrderId,
                TestName = lo.TestName,
                PatientName = lo.Appointment.Patient.FirstName + " " + lo.Appointment.Patient.LastName,
                OrderDate = lo.OrderDate,
                AppointmentDate = lo.Appointment.AppointmentDate,
                DoctorName = lo.Appointment.DoctorName
            })
            .ToListAsync();

        // Report 2: Patients Without Follow-Up using LINQ (Where with Any, Select, OrderBy)
        var today = DateTime.Now;
        PatientsWithoutFollowUp = await _context.Patients
            .Include(p => p.Appointments)
            .Where(p => !p.Appointments.Any(a => a.AppointmentDate > today))
            .Select(p => new PatientWithoutFollowUpReport
            {
                PatientId = p.PatientId,
                PatientName = p.FirstName + " " + p.LastName,
                Phone = p.Phone ?? "N/A",
                Email = p.Email ?? "N/A",
                LastAppointmentDate = p.Appointments
                    .OrderByDescending(a => a.AppointmentDate)
                    .Select(a => (DateTime?)a.AppointmentDate)
                    .FirstOrDefault(),
                TotalAppointments = p.Appointments.Count
            })
            .OrderBy(p => p.PatientName)
            .ToListAsync();

        // Report 3: Appointments by Month using LINQ (GroupBy, Select, OrderByDescending)
        AppointmentsByMonth = await _context.Appointments
            .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
            .Select(g => new AppointmentsByMonthReport
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAppointments = g.Count(),
                ScheduledCount = g.Count(a => a.Status == "Scheduled"),
                CompletedCount = g.Count(a => a.Status == "Completed"),
                CancelledCount = g.Count(a => a.Status == "Cancelled")
            })
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ToListAsync();
    }
}

// DTOs for Reports
public class PendingLabOrderReport
{
    public int LabOrderId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;
}

public class PatientWithoutFollowUpReport
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? LastAppointmentDate { get; set; }
    public int TotalAppointments { get; set; }
}

public class AppointmentsByMonthReport
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalAppointments { get; set; }
    public int ScheduledCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}
