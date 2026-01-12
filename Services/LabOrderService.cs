using HCAMiniEHR.Data;
using HCAMiniEHR.Data.Repositories;
using HCAMiniEHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HCAMiniEHR.Services;

public class LabOrderService
{
    private readonly Repository<LabOrder> _repository;
    private readonly EhrDbContext _context;

    public LabOrderService(EhrDbContext context)
    {
        _context = context;
        _repository = new Repository<LabOrder>(context);
    }

    public async Task<IEnumerable<LabOrder>> GetAllLabOrdersAsync()
    {
        return await _context.LabOrders
            .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
            .OrderByDescending(l => l.OrderDate)
            .ToListAsync();
    }

    public async Task<LabOrder?> GetLabOrderByIdAsync(int id)
    {
        return await _context.LabOrders
            .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
            .FirstOrDefaultAsync(l => l.LabOrderId == id);
    }

    public async Task<IEnumerable<LabOrder>> GetLabOrdersByAppointmentIdAsync(int appointmentId)
    {
        return await _context.LabOrders
            .Where(l => l.AppointmentId == appointmentId)
            .OrderBy(l => l.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetPendingLabOrdersAsync()
    {
        return await _context.LabOrders
            .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
            .Where(l => l.Status == "Pending")
            .OrderBy(l => l.OrderDate)
            .ToListAsync();
    }

    public async Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(labOrder.TestName))
            throw new ArgumentException("Test name is required.");

        labOrder.OrderDate = DateTime.Now;
        labOrder.Status = "Pending";
        
        return await _repository.AddAsync(labOrder);
    }

    public async Task UpdateLabOrderAsync(LabOrder labOrder)
    {
        var existing = await _repository.GetByIdAsync(labOrder.LabOrderId);
        if (existing == null)
            throw new InvalidOperationException($"Lab order with ID {labOrder.LabOrderId} not found.");

        await _repository.UpdateAsync(labOrder);
    }

    public async Task UpdateLabOrderStatusAsync(int labOrderId, string status, string? results = null)
    {
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
    }

    public async Task DeleteLabOrderAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}
