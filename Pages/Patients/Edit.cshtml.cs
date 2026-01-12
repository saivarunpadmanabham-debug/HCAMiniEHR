using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Patients;

public class EditModel : PageModel
{
    private readonly PatientService _patientService;

    public EditModel(PatientService patientService)
    {
        _patientService = patientService;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _patientService.UpdatePatientAsync(Patient);
            TempData["SuccessMessage"] = $"Patient {Patient.FullName} updated successfully!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
