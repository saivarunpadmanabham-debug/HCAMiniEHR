-- =============================================
-- FINAL FIX: Audit Trigger with CORRECT column names
-- Columns: TableName, Operation, RecordId, OldValue, NewValue, ChangedDate, ChangedBy
-- =============================================

USE EHR2;
GO

DROP TRIGGER IF EXISTS [Healthcare].[trg_Appointment_Audit];
GO

CREATE TRIGGER [Healthcare].[trg_Appointment_Audit]
ON [Healthcare].[Appointment]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Operation VARCHAR(10);
    DECLARE @TableName VARCHAR(100) = 'Appointment';
    DECLARE @RecordId INT;
    DECLARE @OldValue NVARCHAR(MAX) = NULL;
    DECLARE @NewValue NVARCHAR(MAX) = NULL;
    DECLARE @ChangedBy NVARCHAR(100) = SYSTEM_USER;
    
    -- Determine the operation type
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        SET @Operation = 'UPDATE';
    ELSE IF EXISTS (SELECT * FROM inserted)
        SET @Operation = 'INSERT';
    ELSE
        SET @Operation = 'DELETE';
    
    -- For INSERT, log new values
    IF @Operation = 'INSERT'
    BEGIN
        SELECT @RecordId = AppointmentId,
               @NewValue = 'PatientId: ' + CAST(PatientId AS NVARCHAR(10)) + 
                          ', DoctorId: ' + CAST(DoctorId AS NVARCHAR(10)) +
                          ', Date: ' + CONVERT(NVARCHAR(20), AppointmentDate, 120) +
                          ', Time: ' + CAST(AppointmentTime AS NVARCHAR(10)) +
                          ', Status: ' + Status
        FROM inserted;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Operation, RecordId, OldValue, NewValue, ChangedDate, ChangedBy)
        VALUES (@TableName, @Operation, @RecordId, NULL, @NewValue, GETDATE(), @ChangedBy);
    END
    
    -- For UPDATE, log old and new values
    IF @Operation = 'UPDATE'
    BEGIN
        SELECT @RecordId = i.AppointmentId,
               @OldValue = 'PatientId: ' + CAST(d.PatientId AS NVARCHAR(10)) + 
                          ', DoctorId: ' + CAST(d.DoctorId AS NVARCHAR(10)) +
                          ', Date: ' + CONVERT(NVARCHAR(20), d.AppointmentDate, 120) +
                          ', Status: ' + d.Status,
               @NewValue = 'PatientId: ' + CAST(i.PatientId AS NVARCHAR(10)) + 
                          ', DoctorId: ' + CAST(i.DoctorId AS NVARCHAR(10)) +
                          ', Date: ' + CONVERT(NVARCHAR(20), i.AppointmentDate, 120) +
                          ', Status: ' + i.Status
        FROM inserted i
        INNER JOIN deleted d ON i.AppointmentId = d.AppointmentId;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Operation, RecordId, OldValue, NewValue, ChangedDate, ChangedBy)
        VALUES (@TableName, @Operation, @RecordId, @OldValue, @NewValue, GETDATE(), @ChangedBy);
    END
    
    -- For DELETE, log old values
    IF @Operation = 'DELETE'
    BEGIN
        SELECT @RecordId = AppointmentId,
               @OldValue = 'PatientId: ' + CAST(PatientId AS NVARCHAR(10)) + 
                          ', DoctorId: ' + CAST(DoctorId AS NVARCHAR(10)) +
                          ', Date: ' + CONVERT(NVARCHAR(20), AppointmentDate, 120) +
                          ', Status: ' + Status
        FROM deleted;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Operation, RecordId, OldValue, NewValue, ChangedDate, ChangedBy)
        VALUES (@TableName, @Operation, @RecordId, @OldValue, NULL, GETDATE(), @ChangedBy);
    END
END
GO

PRINT 'Audit trigger created successfully with correct column names!';
GO

-- Test it
PRINT 'Testing trigger with appointment creation...';
DECLARE @TestId INT;
EXEC [Healthcare].[usp_CreateAppointment] 
    @PatientId = 1, 
    @DoctorId = 1, 
    @AppointmentDate = '2026-01-30', 
    @AppointmentTime = '16:00', 
    @Reason = 'Final test with audit logging', 
    @NewAppointmentId = @TestId OUTPUT;
    
SELECT @TestId AS 'Appointment Created';
SELECT TOP 1 * FROM [Healthcare].[AuditLog] ORDER BY ChangedDate DESC;
GO
