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
        var patient = await _patientService.GetPatientByIdForEditAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        Patient = patient;
        
        // Store original values for change detection
        TempData["OriginalFirstName"] = patient.FirstName;
        TempData["OriginalLastName"] = patient.LastName;
        TempData["OriginalDateOfBirth"] = patient.DateOfBirth.ToString("yyyy-MM-dd");
        TempData["OriginalGender"] = patient.Gender;
        TempData["OriginalPhone"] = patient.Phone;
        TempData["OriginalEmail"] = patient.Email;
        TempData["OriginalAddress"] = patient.Address;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check if anything changed
        var originalFirstName = TempData["OriginalFirstName"]?.ToString();
        var originalLastName = TempData["OriginalLastName"]?.ToString();
        var originalDateOfBirth = TempData["OriginalDateOfBirth"]?.ToString();
        var originalGender = TempData["OriginalGender"]?.ToString();
        var originalPhone = TempData["OriginalPhone"]?.ToString();
        var originalEmail = TempData["OriginalEmail"]?.ToString();
        var originalAddress = TempData["OriginalAddress"]?.ToString();

        bool hasChanges = Patient.FirstName != originalFirstName ||
                         Patient.LastName != originalLastName ||
                         Patient.DateOfBirth.ToString("yyyy-MM-dd") != originalDateOfBirth ||
                         Patient.Gender != originalGender ||
                         Patient.Phone != originalPhone ||
                         Patient.Email != originalEmail ||
                         Patient.Address != originalAddress;

        if (!hasChanges)
        {
            ModelState.AddModelError(string.Empty, "No changes were made. Please update at least one field.");
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
