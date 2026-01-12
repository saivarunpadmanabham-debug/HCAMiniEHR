using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Repositories;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace HCAMiniEHR.Services;

public class AppointmentService
{
    private readonly Repository<Appointment> _repository;
    private readonly EhrDbContext _context;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(EhrDbContext context, ILogger<AppointmentService> logger)
    {
        _context = context;
        _repository = new Repository<Appointment>(context);
        _logger = logger;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        try
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.LabOrders)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all appointments");
            throw new ApplicationException("An error occurred while retrieving appointments. Please try again later.", ex);
        }
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        try
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.LabOrders)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment with ID {AppointmentId}", id);
            throw new ApplicationException($"An error occurred while retrieving appointment with ID {id}.", ex);
        }
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        try
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.LabOrders)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for patient {PatientId}", patientId);
            throw new ApplicationException($"An error occurred while retrieving appointments for patient {patientId}.", ex);
        }
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId)
    {
        try
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.LabOrders)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for doctor {DoctorId}", doctorId);
            throw new ApplicationException($"An error occurred while retrieving appointments for doctor {doctorId}.", ex);
        }
    }

    // Create appointment using STORED PROCEDURE (updated for DoctorId)
    public async Task<int> CreateAppointmentUsingStoredProcAsync(
        int patientId, 
        int doctorId,
        DateTime appointmentDate, 
        TimeSpan appointmentTime, 
        string? reason)
    {
        try
        {
            var patientIdParam = new SqlParameter("@PatientId", patientId);
            var doctorIdParam = new SqlParameter("@DoctorId", doctorId);
            var dateParam = new SqlParameter("@AppointmentDate", appointmentDate);
            var timeParam = new SqlParameter("@AppointmentTime", appointmentTime);
            var reasonParam = new SqlParameter("@Reason", (object?)reason ?? DBNull.Value);
            var newIdParam = new SqlParameter("@NewAppointmentId", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [Healthcare].[usp_CreateAppointment] @PatientId, @DoctorId, @AppointmentDate, @AppointmentTime, @Reason, @NewAppointmentId OUTPUT",
                patientIdParam, doctorIdParam, dateParam, timeParam, reasonParam, newIdParam);

            var newId = (int)newIdParam.Value;
            _logger.LogInformation("Appointment created via stored procedure: {AppointmentId}", newId);

            return newId;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while creating appointment via stored procedure");
            throw new ApplicationException("A database error occurred while creating the appointment.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment via stored procedure");
            throw new ApplicationException("An unexpected error occurred while creating the appointment.", ex);
        }
    }

    // Regular create method (without stored procedure)
    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        try
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment), "Appointment cannot be null");

            // Validation
            if (appointment.AppointmentDate.Date < DateTime.Now.Date)
                throw new ArgumentException("Appointment date cannot be in the past.");

            if (appointment.DoctorId <= 0)
                throw new ArgumentException("Doctor is required.");

            if (appointment.PatientId <= 0)
                throw new ArgumentException("Patient is required.");

            appointment.CreatedDate = DateTime.Now;
            appointment.Status = "Scheduled";
            
            // Clear navigation properties to avoid EF Core tracking issues
            appointment.Patient = null!;
            appointment.Doctor = null!;
            appointment.LabOrders = new List<LabOrder>();
            
            var result = await _repository.AddAsync(appointment);

            _logger.LogInformation("Appointment created: {AppointmentId} for Patient {PatientId} with Doctor {DoctorId}", 
                result.AppointmentId, result.PatientId, result.DoctorId);

            return result;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating appointment");
            throw new ApplicationException("An error occurred while saving the appointment to the database.", ex);
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            throw new ApplicationException("An unexpected error occurred while creating the appointment.", ex);
        }
    }

    public async Task UpdateAppointmentAsync(Appointment appointment)
    {
        try
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment), "Appointment cannot be null");

            var existing = await _repository.GetByIdAsync(appointment.AppointmentId);
            if (existing == null)
                throw new InvalidOperationException($"Appointment with ID {appointment.AppointmentId} not found.");

            await _repository.UpdateAsync(appointment);

            _logger.LogInformation("Appointment updated: {AppointmentId}", appointment.AppointmentId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error while updating appointment {AppointmentId}", appointment.AppointmentId);
            throw new ApplicationException("The appointment was modified by another user. Please refresh and try again.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating appointment {AppointmentId}", appointment.AppointmentId);
            throw new ApplicationException("An error occurred while updating the appointment in the database.", ex);
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw not found exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId}", appointment.AppointmentId);
            throw new ApplicationException("An unexpected error occurred while updating the appointment.", ex);
        }
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        try
        {
            await _repository.DeleteAsync(id);
            _logger.LogInformation("Appointment deleted: {AppointmentId}", id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting appointment {AppointmentId}", id);
            throw new ApplicationException("An error occurred while deleting the appointment.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment {AppointmentId}", id);
            throw new ApplicationException("An unexpected error occurred while deleting the appointment.", ex);
        }
    }

    public async Task<bool> AppointmentExistsAsync(int id)
    {
        try
        {
            return await _repository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if appointment exists {AppointmentId}", id);
            throw new ApplicationException("An error occurred while checking appointment existence.", ex);
        }
    }
}
