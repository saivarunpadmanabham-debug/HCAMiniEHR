using HCAMiniEHR.Models;
using HCAMiniEHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCAMiniEHR.Pages.Patients;

public class IndexModel : PageModel
{
    private readonly PatientService _patientService;

    public IndexModel(PatientService patientService)
    {
        _patientService = patientService;
    }

    public IEnumerable<Patient> Patients { get; set; } = new List<Patient>();

    public async Task OnGetAsync()
    {
        Patients = await _patientService.GetAllPatientsAsync();
    }
}
