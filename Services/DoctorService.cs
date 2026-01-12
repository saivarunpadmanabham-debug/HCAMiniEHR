using HCAMiniEHR.Data;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Services;

public class DoctorService
{
    private readonly EhrDbContext _context;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(EhrDbContext context, ILogger<DoctorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
    {
        try
        {
            return await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all doctors");
            throw new ApplicationException("An error occurred while retrieving doctors. Please try again later.", ex);
        }
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        try
        {
            return await _context.Doctors
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(d => d.DoctorId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving doctor with ID {DoctorId}", id);
            throw new ApplicationException($"An error occurred while retrieving doctor with ID {id}.", ex);
        }
    }

    public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
    {
        try
        {
            if (doctor == null)
            {
                throw new ArgumentNullException(nameof(doctor), "Doctor cannot be null");
            }

            doctor.CreatedDate = DateTime.Now;
            doctor.IsActive = true;

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor created successfully: {DoctorId} - {DoctorName}", 
                doctor.DoctorId, doctor.FullName);

            return doctor;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating doctor");
            throw new ApplicationException("An error occurred while saving the doctor to the database.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor");
            throw new ApplicationException("An unexpected error occurred while creating the doctor.", ex);
        }
    }

    public async Task<bool> UpdateDoctorAsync(Doctor doctor)
    {
        try
        {
            if (doctor == null)
            {
                throw new ArgumentNullException(nameof(doctor), "Doctor cannot be null");
            }

            var existingDoctor = await _context.Doctors.FindAsync(doctor.DoctorId);
            if (existingDoctor == null)
            {
                _logger.LogWarning("Attempted to update non-existent doctor with ID {DoctorId}", doctor.DoctorId);
                return false;
            }

            _context.Entry(existingDoctor).CurrentValues.SetValues(doctor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor updated successfully: {DoctorId} - {DoctorName}", 
                doctor.DoctorId, doctor.FullName);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error while updating doctor {DoctorId}", doctor.DoctorId);
            throw new ApplicationException("The doctor was modified by another user. Please refresh and try again.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating doctor {DoctorId}", doctor.DoctorId);
            throw new ApplicationException("An error occurred while updating the doctor in the database.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating doctor {DoctorId}", doctor.DoctorId);
            throw new ApplicationException("An unexpected error occurred while updating the doctor.", ex);
        }
    }

    public async Task<bool> DeleteDoctorAsync(int id)
    {
        try
        {
            var doctor = await _context.Doctors
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                _logger.LogWarning("Attempted to delete non-existent doctor with ID {DoctorId}", id);
                return false;
            }

            // Check if doctor has appointments
            if (doctor.Appointments.Any())
            {
                // Soft delete - mark as inactive instead of deleting
                doctor.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Doctor soft-deleted (marked inactive): {DoctorId} - {DoctorName}", 
                    doctor.DoctorId, doctor.FullName);
            }
            else
            {
                // Hard delete if no appointments
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Doctor deleted: {DoctorId} - {DoctorName}", 
                    doctor.DoctorId, doctor.FullName);
            }

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting doctor {DoctorId}", id);
            throw new ApplicationException("An error occurred while deleting the doctor from the database.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor {DoctorId}", id);
            throw new ApplicationException("An unexpected error occurred while deleting the doctor.", ex);
        }
    }

    public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllDoctorsAsync();
            }

            return await _context.Doctors
                .Where(d => d.IsActive && 
                    (d.FirstName.Contains(searchTerm) || 
                     d.LastName.Contains(searchTerm) ||
                     d.Specialization.Contains(searchTerm)))
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching doctors with term: {SearchTerm}", searchTerm);
            throw new ApplicationException("An error occurred while searching for doctors.", ex);
        }
    }
}
