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
    private readonly DoctorService _doctorService;

    public CreateModel(AppointmentService appointmentService, PatientService patientService, DoctorService doctorService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
    }

    [BindProperty]
    public int PatientId { get; set; }

    [BindProperty]
    public int DoctorId { get; set; }

    [BindProperty]
    public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

    [BindProperty]
    public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0);

    [BindProperty]
    public string? Reason { get; set; }

    [BindProperty]
    public bool UseStoredProcedure { get; set; } = false;

    public SelectList Patients { get; set; } = null!;
    public SelectList Doctors { get; set; } = null!;

    public async Task OnGetAsync(int? patientId)
    {
        try
        {
            var patients = await _patientService.GetAllPatientsAsync();
            Patients = new SelectList(patients, "PatientId", "FullName");

            var doctors = await _doctorService.GetAllDoctorsAsync();
            Doctors = new SelectList(doctors, "DoctorId", "FullNameWithSpecialization");

            if (patientId.HasValue)
            {
                PatientId = patientId.Value;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error loading data: {ex.Message}");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return Page();
        }

        try
        {
            if (UseStoredProcedure)
            {
                // Use stored procedure to create appointment
                var newId = await _appointmentService.CreateAppointmentUsingStoredProcAsync(
                    PatientId, DoctorId, AppointmentDate, AppointmentTime, Reason);
                
                TempData["SuccessMessage"] = $"Appointment created successfully using stored procedure! (ID: {newId})";
            }
            else
            {
                // Use regular EF Core method
                var appointment = new Appointment
                {
                    PatientId = PatientId,
                    DoctorId = DoctorId,
                    AppointmentDate = AppointmentDate,
                    AppointmentTime = AppointmentTime,
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
            await LoadSelectLists();
            return Page();
        }
    }

    private async Task LoadSelectLists()
    {
        try
        {
            var patients = await _patientService.GetAllPatientsAsync();
            Patients = new SelectList(patients, "PatientId", "FullName");

            var doctors = await _doctorService.GetAllDoctorsAsync();
            Doctors = new SelectList(doctors, "DoctorId", "FullNameWithSpecialization");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error loading data: {ex.Message}");
        }
    }
}
