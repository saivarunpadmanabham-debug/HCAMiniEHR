using HCAMiniEHR.Data;
using HCAMiniEHR.DTOs;
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
    public IEnumerable<PendingLabOrderReportDTO> PendingLabOrders { get; set; } = new List<PendingLabOrderReportDTO>();

    // Report 2: Patients Without Follow-Up
    public IEnumerable<PatientWithoutFollowUpReportDTO> PatientsWithoutFollowUp { get; set; } = new List<PatientWithoutFollowUpReportDTO>();

    // Report 3: Appointments by Month
    public IEnumerable<AppointmentsByMonthReportDTO> AppointmentsByMonth { get; set; } = new List<AppointmentsByMonthReportDTO>();

    public async Task OnGetAsync()
    {
        // Report 1: Pending Lab Orders using LINQ (Where, Include, OrderBy)
        PendingLabOrders = await _context.LabOrders
            .Include(lo => lo.Appointment)
                .ThenInclude(a => a.Patient)
            .Include(lo => lo.Appointment)
                .ThenInclude(a => a.Doctor)
            .OrderBy(lo => lo.OrderDate)
            //.Where(lo => lo.Status)
            .Select(lo => new PendingLabOrderReportDTO
            {
                LabOrderId = lo.LabOrderId,
                TestName = lo.TestName,
                PatientName = lo.Appointment.Patient.FirstName + " " + lo.Appointment.Patient.LastName,
                OrderDate = lo.OrderDate,
                AppointmentDate = lo.Appointment.AppointmentDate,
                DoctorName = lo.Appointment.Doctor.FullName
            })
            .ToListAsync();

        // Report 2: Patients Without Follow-Up using LINQ (Where with Any, Select, OrderBy)
        var today = DateTime.Now;
        PatientsWithoutFollowUp = await _context.Patients
            .Include(p => p.Appointments)
            .Where(p => !p.Appointments.Any(a => a.AppointmentDate > today))
            .Select(p => new PatientWithoutFollowUpReportDTO
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
            .Select(g => new AppointmentsByMonthReportDTO
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
