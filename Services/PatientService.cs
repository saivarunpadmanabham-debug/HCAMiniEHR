using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Repositories;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Services;

public class PatientService
{
    private readonly Repository<Patient> _repository;
    private readonly EhrDbContext _context;
    private readonly ILogger<PatientService> _logger;

    public PatientService(EhrDbContext context, ILogger<PatientService> logger)
    {
        _context = context;
        _repository = new Repository<Patient>(context);
        _logger = logger;
    }

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        try
        {
            return await _context.Patients
                .Include(p => p.Appointments)
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all patients");
            throw new ApplicationException("An error occurred while retrieving patients. Please try again later.", ex);
        }
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        try
        {
            return await _context.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p => p.PatientId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient with ID {PatientId}", id);
            throw new ApplicationException($"An error occurred while retrieving patient with ID {id}.", ex);
        }
    }

    public async Task<Patient?> GetPatientByIdForEditAsync(int id)
    {
        try
        {
            return await _context.Patients
                .AsNoTracking()  // Don't track this entity to avoid conflicts
                .FirstOrDefaultAsync(p => p.PatientId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving patient {PatientId} for edit", id);
            throw new ApplicationException($"An error occurred while retrieving patient {id}.", ex);
        }
    }

    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        try
        {
            // Validation
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null");

            if (string.IsNullOrWhiteSpace(patient.FirstName))
                throw new ArgumentException("First name is required.");
            
            if (string.IsNullOrWhiteSpace(patient.LastName))
                throw new ArgumentException("Last name is required.");

            if (patient.DateOfBirth > DateTime.Now)
                throw new ArgumentException("Date of birth cannot be in the future.");

            // Check for duplicate patient (same first name, last name, and date of birth)
            var duplicateExists = await _context.Patients
                .AnyAsync(p => p.FirstName.ToLower() == patient.FirstName.ToLower()
                    && p.LastName.ToLower() == patient.LastName.ToLower()
                    && p.DateOfBirth.Date == patient.DateOfBirth.Date);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    $"A patient with the name {patient.FirstName} {patient.LastName} and date of birth {patient.DateOfBirth:MM/dd/yyyy} already exists in the system. Please verify the patient details.");
            }

            patient.CreatedDate = DateTime.Now;
            var result = await _repository.AddAsync(patient);

            _logger.LogInformation("Patient created successfully: {PatientId} - {PatientName}", 
                result.PatientId, result.FullName);

            return result;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating patient");
            throw new ApplicationException("An error occurred while saving the patient to the database.", ex);
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw duplicate detection exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            throw new ApplicationException("An unexpected error occurred while creating the patient.", ex);
        }
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        try
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null");

            // Don't load the entity again - it causes tracking conflicts
            // The patient object passed in already has the ID
            await _repository.UpdateAsync(patient);

            _logger.LogInformation("Patient updated successfully: {PatientId} - {PatientName}", 
                patient.PatientId, patient.FullName);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error while updating patient {PatientId}", patient.PatientId);
            throw new ApplicationException("The patient was modified by another user. Please refresh and try again.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating patient {PatientId}", patient.PatientId);
            throw new ApplicationException("An error occurred while updating the patient in the database.", ex);
        }
        catch (ArgumentNullException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient {PatientId}", patient.PatientId);
            throw new ApplicationException("An unexpected error occurred while updating the patient.", ex);
        }
    }

    public async Task DeletePatientAsync(int id)
    {
        try
        {
            var patient = await _repository.GetByIdAsync(id);
            if (patient == null)
                throw new InvalidOperationException($"Patient with ID {id} not found.");

            // Check for upcoming appointments (scheduled or confirmed, not cancelled)
            var upcomingAppointments = await _context.Appointments
                .Where(a => a.PatientId == id 
                    && a.AppointmentDate >= DateTime.Today
                    && a.Status != "Cancelled"
                    && a.Status != "Completed")
                .ToListAsync();

            if (upcomingAppointments.Any())
            {
                var appointmentDates = string.Join(", ", upcomingAppointments
                    .Select(a => a.AppointmentDate.ToString("MM/dd/yyyy")));
                throw new InvalidOperationException(
                    $"Cannot delete patient {patient.FullName}. They have upcoming appointments on: {appointmentDates}. Please cancel or complete these appointments first.");
            }

            // Check for pending or in-progress lab orders
            var pendingLabOrders = await _context.LabOrders
                .Where(lo => lo.Appointment.PatientId == id 
                    && (lo.Status == "Pending" || lo.Status == "In Progress"))
                .Include(lo => lo.Appointment)
                .ToListAsync();

            if (pendingLabOrders.Any())
            {
                var labOrderDetails = string.Join(", ", pendingLabOrders
                    .Select(lo => $"{lo.TestName} ({lo.Status})"));
                throw new InvalidOperationException(
                    $"Cannot delete patient {patient.FullName}. They have pending or in-progress lab orders: {labOrderDetails}. Please complete or cancel these lab orders first.");
            }

            await _repository.DeleteAsync(id);

            _logger.LogInformation("Patient deleted: {PatientId} - {PatientName}", id, patient.FullName);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting patient {PatientId}", id);
            throw new ApplicationException("An error occurred while deleting the patient. The patient may have related records.", ex);
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw not found exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient {PatientId}", id);
            throw new ApplicationException("An unexpected error occurred while deleting the patient.", ex);
        }
    }

    public async Task<bool> PatientExistsAsync(int id)
    {
        try
        {
            return await _repository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if patient exists {PatientId}", id);
            throw new ApplicationException("An error occurred while checking patient existence.", ex);
        }
    }
}
