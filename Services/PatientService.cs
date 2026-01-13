using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Repositories;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HCAMiniEHR.Services;

public class PatientService
{
    private readonly Repository<Patient> _repository;
    private readonly EhrDbContext _context;
    private readonly ILogger<PatientService> _logger;

    public PatientService(Repository<Patient> repository, EhrDbContext context, ILogger<PatientService> logger)
    {
        _repository = repository;
        _context = context;
        _logger = logger;
    }

    // =============================================
    // STORED PROCEDURE METHODS
    // =============================================

    /// <summary>
    /// Creates a patient using stored procedure
    /// </summary>
    public async Task<Patient> CreatePatientUsingStoredProcAsync(Patient patient)
    {
        try
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null");

            var newPatientIdParam = new SqlParameter
            {
                ParameterName = "@NewPatientId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [Healthcare].[usp_CreatePatient] @FirstName, @LastName, @DateOfBirth, @Gender, @Phone, @Email, @Address, @NewPatientId OUTPUT",
                new SqlParameter("@FirstName", patient.FirstName),
                new SqlParameter("@LastName", patient.LastName),
                new SqlParameter("@DateOfBirth", patient.DateOfBirth),
                new SqlParameter("@Gender", (object?)patient.Gender ?? DBNull.Value),
                new SqlParameter("@Phone", (object?)patient.Phone ?? DBNull.Value),
                new SqlParameter("@Email", (object?)patient.Email ?? DBNull.Value),
                new SqlParameter("@Address", (object?)patient.Address ?? DBNull.Value),
                newPatientIdParam
            );

            var newPatientId = (int)newPatientIdParam.Value;
            var result = await _repository.GetByIdAsync(newPatientId);

            if (result == null)
                throw new ApplicationException("Patient was created but could not be retrieved.");

            _logger.LogInformation("Patient created via stored procedure: {PatientId} - {PatientName}",
                result.PatientId, result.FullName);

            return result;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while creating patient via stored procedure");
            throw new ApplicationException($"Database error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient via stored procedure");
            throw new ApplicationException("An unexpected error occurred while creating the patient.", ex);
        }
    }

    /// <summary>
    /// Updates a patient using stored procedure
    /// </summary>
    public async Task UpdatePatientUsingStoredProcAsync(Patient patient)
    {
        try
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null");

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [Healthcare].[usp_UpdatePatient] @PatientId, @FirstName, @LastName, @DateOfBirth, @Gender, @Phone, @Email, @Address",
                new SqlParameter("@PatientId", patient.PatientId),
                new SqlParameter("@FirstName", patient.FirstName),
                new SqlParameter("@LastName", patient.LastName),
                new SqlParameter("@DateOfBirth", patient.DateOfBirth),
                new SqlParameter("@Gender", (object?)patient.Gender ?? DBNull.Value),
                new SqlParameter("@Phone", (object?)patient.Phone ?? DBNull.Value),
                new SqlParameter("@Email", (object?)patient.Email ?? DBNull.Value),
                new SqlParameter("@Address", (object?)patient.Address ?? DBNull.Value)
            );

            _logger.LogInformation("Patient updated via stored procedure: {PatientId}", patient.PatientId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating patient via stored procedure");
            throw new ApplicationException($"Database error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient via stored procedure");
            throw new ApplicationException("An unexpected error occurred while updating the patient.", ex);
        }
    }

    /// <summary>
    /// Deletes a patient using stored procedure
    /// </summary>
    public async Task DeletePatientUsingStoredProcAsync(int id)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [Healthcare].[usp_DeletePatient] @PatientId",
                new SqlParameter("@PatientId", id)
            );

            _logger.LogInformation("Patient deleted via stored procedure: {PatientId}", id);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while deleting patient via stored procedure");
            throw new ApplicationException($"Database error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient via stored procedure");
            throw new ApplicationException("An unexpected error occurred while deleting the patient.", ex);
        }
    }

    // =============================================
    // EXISTING EF CORE METHODS (Keep for compatibility)
    // =============================================

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        try
        {
            return await _repository.GetAllAsync();
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
                .AsNoTracking()
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
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null");

            if (string.IsNullOrWhiteSpace(patient.FirstName))
                throw new ArgumentException("First name is required.");

            if (string.IsNullOrWhiteSpace(patient.LastName))
                throw new ArgumentException("Last name is required.");

            if (patient.DateOfBirth > DateTime.Now)
                throw new ArgumentException("Date of birth cannot be in the future.");

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
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
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

            await _repository.UpdateAsync(patient);

            _logger.LogInformation("Patient updated successfully: {PatientId}", patient.PatientId);
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
            throw;
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
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting patient {PatientId}", id);
            throw new ApplicationException("An error occurred while deleting the patient. The patient may have related records.", ex);
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
            return await _context.Patients.AnyAsync(p => p.PatientId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if patient exists: {PatientId}", id);
            throw new ApplicationException($"An error occurred while checking if patient {id} exists.", ex);
        }
    }

    public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllPatientsAsync();

            return await _context.Patients
                .Where(p => p.FirstName.Contains(searchTerm) ||
                           p.LastName.Contains(searchTerm) ||
                           (p.Phone != null && p.Phone.Contains(searchTerm)) ||
                           (p.Email != null && p.Email.Contains(searchTerm)))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients with term: {SearchTerm}", searchTerm);
            throw new ApplicationException("An error occurred while searching for patients.", ex);
        }
    }
}
