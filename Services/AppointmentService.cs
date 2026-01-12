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

    public AppointmentService(EhrDbContext context)
    {
        _context = context;
        _repository = new Repository<Appointment>(context);
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.LabOrders)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.LabOrders)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.LabOrders)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    // Create appointment using STORED PROCEDURE
    public async Task<int> CreateAppointmentUsingStoredProcAsync(
        int patientId, 
        DateTime appointmentDate, 
        TimeSpan appointmentTime, 
        string doctorName, 
        string? reason)
    {
        var patientIdParam = new SqlParameter("@PatientId", patientId);
        var dateParam = new SqlParameter("@AppointmentDate", appointmentDate);
        var timeParam = new SqlParameter("@AppointmentTime", appointmentTime);
        var doctorParam = new SqlParameter("@DoctorName", doctorName);
        var reasonParam = new SqlParameter("@Reason", (object?)reason ?? DBNull.Value);
        var newIdParam = new SqlParameter("@NewAppointmentId", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC [Healthcare].[usp_CreateAppointment] @PatientId, @AppointmentDate, @AppointmentTime, @DoctorName, @Reason, @NewAppointmentId OUTPUT",
            patientIdParam, dateParam, timeParam, doctorParam, reasonParam, newIdParam);

        return (int)newIdParam.Value;
    }

    // Regular create method (without stored procedure)
    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        // Validation
        if (appointment.AppointmentDate.Date < DateTime.Now.Date)
            throw new ArgumentException("Appointment date cannot be in the past.");

        if (string.IsNullOrWhiteSpace(appointment.DoctorName))
            throw new ArgumentException("Doctor name is required.");

        appointment.CreatedDate = DateTime.Now;
        appointment.Status = "Scheduled";
        
        return await _repository.AddAsync(appointment);
    }

    public async Task UpdateAppointmentAsync(Appointment appointment)
    {
        var existing = await _repository.GetByIdAsync(appointment.AppointmentId);
        if (existing == null)
            throw new InvalidOperationException($"Appointment with ID {appointment.AppointmentId} not found.");

        await _repository.UpdateAsync(appointment);
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<bool> AppointmentExistsAsync(int id)
    {
        return await _repository.ExistsAsync(id);
    }
}
