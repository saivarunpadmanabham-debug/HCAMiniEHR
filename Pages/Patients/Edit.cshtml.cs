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

    // Hidden fields to store original values
    [BindProperty]
    public string OriginalFirstName { get; set; } = string.Empty;
    
    [BindProperty]
    public string OriginalLastName { get; set; } = string.Empty;
    
    [BindProperty]
    public string OriginalDateOfBirth { get; set; } = string.Empty;
    
    [BindProperty]
    public string? OriginalGender { get; set; }
    
    [BindProperty]
    public string? OriginalPhone { get; set; }
    
    [BindProperty]
    public string? OriginalEmail { get; set; }
    
    [BindProperty]
    public string? OriginalAddress { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var patient = await _patientService.GetPatientByIdForEditAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        Patient = patient;
        
        // Store original values in properties (will be rendered as hidden fields)
        OriginalFirstName = patient.FirstName;
        OriginalLastName = patient.LastName;
        OriginalDateOfBirth = patient.DateOfBirth.ToString("yyyy-MM-dd");
        OriginalGender = patient.Gender;
        OriginalPhone = patient.Phone;
        OriginalEmail = patient.Email;
        OriginalAddress = patient.Address;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check if anything changed
        bool hasChanges = Patient.FirstName != OriginalFirstName ||
                         Patient.LastName != OriginalLastName ||
                         Patient.DateOfBirth.ToString("yyyy-MM-dd") != OriginalDateOfBirth ||
                         Patient.Gender != OriginalGender ||
                         Patient.Phone != OriginalPhone ||
                         Patient.Email != OriginalEmail ||
                         Patient.Address != OriginalAddress;

        if (!hasChanges)
        {
            ModelState.AddModelError(string.Empty, "No changes were made. Please update at least one field before saving.");
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
