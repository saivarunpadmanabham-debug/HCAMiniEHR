using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Patients;

public class DetailsModel : PageModel
{
    private readonly PatientService _patientService;

    public DetailsModel(PatientService patientService)
    {
        _patientService = patientService;
    }

    public Patient Patient { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        Patient = patient;
        return Page();
    }
}
