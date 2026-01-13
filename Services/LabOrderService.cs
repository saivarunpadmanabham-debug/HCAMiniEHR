using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Repositories;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Services;

public class LabOrderService
{
    private readonly Repository<LabOrder> _repository;
    private readonly EhrDbContext _context;
    private readonly ILogger<LabOrderService> _logger;

    public LabOrderService(EhrDbContext context, ILogger<LabOrderService> logger)
    {
        _context = context;
        _repository = new Repository<LabOrder>(context);
        _logger = logger;
    }

    public async Task<IEnumerable<LabOrder>> GetAllLabOrdersAsync()
    {
        try
        {
            return await _context.LabOrders
                .Include(l => l.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(l => l.Appointment)
                    .ThenInclude(a => a.Doctor)
                .OrderByDescending(l => l.OrderDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all lab orders");
            throw new ApplicationException("An error occurred while retrieving lab orders. Please try again later.", ex);
        }
    }

    public async Task<LabOrder?> GetLabOrderByIdAsync(int id)
    {
        try
        {
            return await _context.LabOrders
                .Include(l => l.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(l => l.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(l => l.LabOrderId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab order with ID {LabOrderId}", id);
            throw new ApplicationException($"An error occurred while retrieving lab order with ID {id}.", ex);
        }
    }

    public async Task<IEnumerable<LabOrder>> GetLabOrdersByAppointmentIdAsync(int appointmentId)
    {
        try
        {
            return await _context.LabOrders
                .Where(l => l.AppointmentId == appointmentId)
                .OrderBy(l => l.OrderDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab orders for appointment {AppointmentId}", appointmentId);
            throw new ApplicationException($"An error occurred while retrieving lab orders for appointment {appointmentId}.", ex);
        }
    }

    public async Task<IEnumerable<LabOrder>> GetPendingLabOrdersAsync()
    {
        try
        {
            return await _context.LabOrders
                .Include(l => l.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(l => l.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Where(l => l.Status == "Pending")
                .OrderBy(l => l.OrderDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending lab orders");
            throw new ApplicationException("An error occurred while retrieving pending lab orders.", ex);
        }
    }

    public async Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder)
    {
        try
        {
            if (labOrder == null)
                throw new ArgumentNullException(nameof(labOrder), "Lab order cannot be null");

            // Validation
            if (string.IsNullOrWhiteSpace(labOrder.TestName))
                throw new ArgumentException("Test name is required.");

            if (labOrder.AppointmentId <= 0)
                throw new ArgumentException("Appointment is required.");

            labOrder.OrderDate = DateTime.Now;
            labOrder.Status = "Pending";
            
            var result = await _repository.AddAsync(labOrder);

            _logger.LogInformation("Lab order created: {LabOrderId} for Appointment {AppointmentId}", 
                result.LabOrderId, result.AppointmentId);

            return result;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating lab order");
            throw new ApplicationException("An error occurred while saving the lab order to the database.", ex);
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lab order");
            throw new ApplicationException("An unexpected error occurred while creating the lab order.", ex);
        }
    }

    public async Task UpdateLabOrderAsync(LabOrder labOrder)
    {
        try
        {
            if (labOrder == null)
                throw new ArgumentNullException(nameof(labOrder), "Lab order cannot be null");

            var existing = await _repository.GetByIdAsync(labOrder.LabOrderId);
            if (existing == null)
                throw new InvalidOperationException($"Lab order with ID {labOrder.LabOrderId} not found.");

            await _repository.UpdateAsync(labOrder);

            _logger.LogInformation("Lab order updated: {LabOrderId}", labOrder.LabOrderId);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error while updating lab order {LabOrderId}", labOrder.LabOrderId);
            throw new ApplicationException("The lab order was modified by another user. Please refresh and try again.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating lab order {LabOrderId}", labOrder.LabOrderId);
            throw new ApplicationException("An error occurred while updating the lab order in the database.", ex);
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw not found exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab order {LabOrderId}", labOrder.LabOrderId);
            throw new ApplicationException("An unexpected error occurred while updating the lab order.", ex);
        }
    }

    public async Task UpdateLabOrderStatusAsync(int labOrderId, string status, string? results = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required.");

            var labOrder = await _repository.GetByIdAsync(labOrderId);
            if (labOrder == null)
                throw new InvalidOperationException($"Lab order with ID {labOrderId} not found.");

            labOrder.Status = status;
            labOrder.Results = results;

            if (status == "Completed")
            {
                labOrder.CompletedDate = DateTime.Now;
            }

            await _repository.UpdateAsync(labOrder);

            _logger.LogInformation("Lab order status updated: {LabOrderId} to {Status}", labOrderId, status);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating lab order status {LabOrderId}", labOrderId);
            throw new ApplicationException("An error occurred while updating the lab order status.", ex);
        }
        catch (ArgumentException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw not found exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab order status {LabOrderId}", labOrderId);
            throw new ApplicationException("An unexpected error occurred while updating the lab order status.", ex);
        }
    }

    public async Task DeleteLabOrderAsync(int id)
    {
        try
        {
            await _repository.DeleteAsync(id);
            _logger.LogInformation("Lab order deleted: {LabOrderId}", id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting lab order {LabOrderId}", id);
            throw new ApplicationException("An error occurred while deleting the lab order.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lab order {LabOrderId}", id);
            throw new ApplicationException("An unexpected error occurred while deleting the lab order.", ex);
        }
    }
}
