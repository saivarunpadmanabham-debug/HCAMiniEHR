using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Repositories;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Services;

public class PatientService
{
    private readonly Repository<Patient> _repository;
    private readonly EhrDbContext _context;

    public PatientService(EhrDbContext context)
    {
        _context = context;
        _repository = new Repository<Patient>(context);
    }

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        return await _context.Patients
            .Include(p => p.Appointments)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Appointments)
                .ThenInclude(a => a.LabOrders)
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }

    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(patient.FirstName))
            throw new ArgumentException("First name is required.");
        
        if (string.IsNullOrWhiteSpace(patient.LastName))
            throw new ArgumentException("Last name is required.");

        if (patient.DateOfBirth > DateTime.Now)
            throw new ArgumentException("Date of birth cannot be in the future.");

        patient.CreatedDate = DateTime.Now;
        return await _repository.AddAsync(patient);
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        var existing = await _repository.GetByIdAsync(patient.PatientId);
        if (existing == null)
            throw new InvalidOperationException($"Patient with ID {patient.PatientId} not found.");

        await _repository.UpdateAsync(patient);
    }

    public async Task DeletePatientAsync(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient == null)
            throw new InvalidOperationException($"Patient with ID {id} not found.");

        await _repository.DeleteAsync(id);
    }

    public async Task<bool> PatientExistsAsync(int id)
    {
        return await _repository.ExistsAsync(id);
    }
}
