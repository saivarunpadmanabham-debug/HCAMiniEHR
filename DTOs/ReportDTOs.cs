namespace HCAMiniEHR.DTOs;

/// <summary>
/// DTO for Pending Lab Orders Report
/// </summary>
public class PendingLabOrderReportDTO
{
    public int LabOrderId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string DoctorName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for Patients Without Follow-Up Report
/// </summary>
public class PatientWithoutFollowUpReportDTO
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? LastAppointmentDate { get; set; }
    public int TotalAppointments { get; set; }
}

/// <summary>
/// DTO for Appointments by Month Report
/// </summary>
public class AppointmentsByMonthReportDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalAppointments { get; set; }
    public int ScheduledCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }

    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    public double CompletionRate => TotalAppointments > 0 
        ? Math.Round((CompletedCount * 100.0 / TotalAppointments), 1) 
        : 0;
}

/// <summary>
/// DTO for Doctor Statistics Report
/// </summary>
public class DoctorStatisticsReportDTO
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int ScheduledAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public double CompletionRate => TotalAppointments > 0 
        ? Math.Round((CompletedAppointments * 100.0 / TotalAppointments), 1) 
        : 0;
}
