using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HCAMiniEHR.Pages.Appointments;

public class CreateModel : PageModel
{
    private readonly AppointmentService _appointmentService;
    private readonly PatientService _patientService;

    public CreateModel(AppointmentService appointmentService, PatientService patientService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
    }

    [BindProperty]
    public int PatientId { get; set; }

    [BindProperty]
    public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

    [BindProperty]
    public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0);

    [BindProperty]
    public string DoctorName { get; set; } = string.Empty;

    [BindProperty]
    public string? Reason { get; set; }

    [BindProperty]
    public bool UseStoredProcedure { get; set; } = true;

    public SelectList Patients { get; set; } = null!;

    public async Task OnGetAsync(int? patientId)
    {
        var patients = await _patientService.GetAllPatientsAsync();
        Patients = new SelectList(patients, "PatientId", "FullName");

        if (patientId.HasValue)
        {
            PatientId = patientId.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            Patients = new SelectList(patients, "PatientId", "FullName");
            return Page();
        }

        try
        {
            if (UseStoredProcedure)
            {
                // Use stored procedure to create appointment
                var newId = await _appointmentService.CreateAppointmentUsingStoredProcAsync(
                    PatientId, AppointmentDate, AppointmentTime, DoctorName, Reason);
                
                TempData["SuccessMessage"] = $"Appointment created successfully using stored procedure! (ID: {newId})";
            }
            else
            {
                // Use regular EF Core method
                var appointment = new Appointment
                {
                    PatientId = PatientId,
                    AppointmentDate = AppointmentDate,
                    AppointmentTime = AppointmentTime,
                    DoctorName = DoctorName,
                    Reason = Reason
                };
                await _appointmentService.CreateAppointmentAsync(appointment);
                TempData["SuccessMessage"] = "Appointment created successfully!";
            }

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var patients = await _patientService.GetAllPatientsAsync();
            Patients = new SelectList(patients, "PatientId", "FullName");
            return Page();
        }
    }
}
