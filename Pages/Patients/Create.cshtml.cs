using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Patients;

public class CreateModel : PageModel
{
    private readonly PatientService _patientService;

    public CreateModel(PatientService patientService)
    {
        _patientService = patientService;
    }

    [BindProperty]
    public Patient Patient { get; set; } = new Patient();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Additional validation for date of birth
        if (Patient.DateOfBirth > DateTime.Today)
        {
            ModelState.AddModelError("Patient.DateOfBirth", "Date of birth cannot be in the future.");
        }

        if (Patient.DateOfBirth.Year < 1900)
        {
            ModelState.AddModelError("Patient.DateOfBirth", "Date of birth must be after 1900.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _patientService.CreatePatientAsync(Patient);
            TempData["SuccessMessage"] = $"Patient {Patient.FullName} created successfully!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
