using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Repositories;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

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

    // =============================================
    // STORED PROCEDURE METHODS
    // =============================================

    /// <summary>
    /// Creates a lab order using stored procedure
    /// </summary>
    public async Task<LabOrder> CreateLabOrderUsingStoredProcAsync(LabOrder labOrder)
    {
        try
        {
            if (labOrder == null)
                throw new ArgumentNullException(nameof(labOrder), "Lab order cannot be null");

            var newLabOrderIdParam = new SqlParameter
            {
                ParameterName = "@NewLabOrderId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [Healthcare].[usp_CreateLabOrder] @AppointmentId, @TestName, @NewLabOrderId OUTPUT",
                new SqlParameter("@AppointmentId", labOrder.AppointmentId),
                new SqlParameter("@TestName", labOrder.TestName),
                newLabOrderIdParam
            );

            var newLabOrderId = (int)newLabOrderIdParam.Value;
            var result = await _repository.GetByIdAsync(newLabOrderId);

            if (result == null)
                throw new ApplicationException("Lab order was created but could not be retrieved.");

            _logger.LogInformation("Lab order created via stored procedure: {LabOrderId}", result.LabOrderId);

            return result;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while creating lab order via stored procedure");
            throw new ApplicationException($"Database error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lab order via stored procedure");
            throw new ApplicationException("An unexpected error occurred while creating the lab order.", ex);
        }
    }

    /// <summary>
    /// Updates lab order status using stored procedure
    /// </summary>
    public async Task UpdateLabOrderStatusUsingStoredProcAsync(int labOrderId, string status, string? results)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [Healthcare].[usp_UpdateLabOrderStatus] @LabOrderId, @Status, @Results",
                new SqlParameter("@LabOrderId", labOrderId),
                new SqlParameter("@Status", status),
                new SqlParameter("@Results", (object?)results ?? DBNull.Value)
            );

            _logger.LogInformation("Lab order status updated via stored procedure: {LabOrderId}", labOrderId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while updating lab order status via stored procedure");
            throw new ApplicationException($"Database error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab order status via stored procedure");
            throw new ApplicationException("An unexpected error occurred while updating the lab order status.", ex);
        }
    }

    /// <summary>
    /// Deletes a lab order using stored procedure
    /// </summary>
    public async Task DeleteLabOrderUsingStoredProcAsync(int id)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [Healthcare].[usp_DeleteLabOrder] @LabOrderId",
                new SqlParameter("@LabOrderId", id)
            );

            _logger.LogInformation("Lab order deleted via stored procedure: {LabOrderId}", id);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error while deleting lab order via stored procedure");
            throw new ApplicationException($"Database error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lab order via stored procedure");
            throw new ApplicationException("An unexpected error occurred while deleting the lab order.", ex);
        }
    }

    // =============================================
    // EXISTING EF CORE METHODS (Keep for compatibility)
    // =============================================

    public async Task<IEnumerable<LabOrder>> GetAllLabOrdersAsync()
    {
        try
        {
            return await _context.LabOrders
                .Include(lo => lo.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(lo => lo.Appointment)
                    .ThenInclude(a => a.Doctor)
                .OrderByDescending(lo => lo.OrderDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all lab orders");
            throw new ApplicationException("An error occurred while retrieving lab orders.", ex);
        }
    }

    public async Task<LabOrder?> GetLabOrderByIdAsync(int id)
    {
        try
        {
            return await _context.LabOrders
                .Include(lo => lo.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(lo => lo.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(lo => lo.LabOrderId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lab order {LabOrderId}", id);
            throw new ApplicationException($"An error occurred while retrieving lab order {id}.", ex);
        }
    }

    public async Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder)
    {
        try
        {
            if (labOrder == null)
                throw new ArgumentNullException(nameof(labOrder), "Lab order cannot be null");

            if (labOrder.AppointmentId <= 0)
                throw new ArgumentException("Valid appointment is required.");

            if (string.IsNullOrWhiteSpace(labOrder.TestName))
                throw new ArgumentException("Test name is required.");

            labOrder.OrderDate = DateTime.Now;
            labOrder.Status = "Pending";

            var result = await _repository.AddAsync(labOrder);

            _logger.LogInformation("Lab order created: {LabOrderId}", result.LabOrderId);

            return result;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating lab order");
            throw new ApplicationException("An error occurred while saving the lab order.", ex);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lab order");
            throw new ApplicationException("An unexpected error occurred while creating the lab order.", ex);
        }
    }

    public async Task UpdateLabOrderStatusAsync(int id, string status, string? results)
    {
        try
        {
            var labOrder = await _repository.GetByIdAsync(id);
            if (labOrder == null)
                throw new InvalidOperationException($"Lab order with ID {id} not found.");

            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required.");

            if ((status == "Completed" || status == "Cancelled") &&
                (string.IsNullOrWhiteSpace(results) || results.Length < 10))
            {
                throw new ArgumentException("Results field is required and must be at least 10 characters when status is Completed or Cancelled.");
            }

            labOrder.Status = status;
            labOrder.Results = results;

            if (status == "Completed")
            {
                labOrder.CompletedDate = DateTime.Now;
            }

            await _repository.UpdateAsync(labOrder);

            _logger.LogInformation("Lab order status updated: {LabOrderId} - {Status}", id, status);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating lab order {LabOrderId}", id);
            throw new ApplicationException("An error occurred while updating the lab order.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab order {LabOrderId}", id);
            throw new ApplicationException("An unexpected error occurred while updating the lab order.", ex);
        }
    }

    public async Task DeleteLabOrderAsync(int id)
    {
        try
        {
            var labOrder = await _repository.GetByIdAsync(id);
            if (labOrder == null)
                throw new InvalidOperationException($"Lab order with ID {id} not found.");

            if (labOrder.Status == "Completed")
                throw new InvalidOperationException("Cannot delete a completed lab order.");

            await _repository.DeleteAsync(id);

            _logger.LogInformation("Lab order deleted: {LabOrderId}", id);
        }
        catch (InvalidOperationException)
        {
            throw;
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

    public async Task<IEnumerable<LabOrder>> GetPendingLabOrdersAsync()
    {
        try
        {
            return await _context.LabOrders
                .Include(lo => lo.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(lo => lo.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Where(lo => lo.Status == "Pending" || lo.Status == "In Progress")
                .OrderBy(lo => lo.OrderDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending lab orders");
            throw new ApplicationException("An error occurred while retrieving pending lab orders.", ex);
        }
    }
}
