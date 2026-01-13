-- =============================================
-- FIX AUDIT TRIGGER - Remove DoctorName reference
-- This trigger was causing the "Invalid column name 'DoctorName'" error
-- =============================================

USE EHR2;
GO

PRINT 'Dropping old audit trigger...';
DROP TRIGGER IF EXISTS [Healthcare].[trg_Appointment_Audit];
GO

PRINT 'Creating new audit trigger with DoctorId...';
GO

CREATE TRIGGER [Healthcare].[trg_Appointment_Audit]
ON [Healthcare].[Appointment]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Action VARCHAR(10);
    DECLARE @TableName VARCHAR(100) = 'Appointment';
    DECLARE @RecordId INT;
    DECLARE @Details NVARCHAR(MAX);
    
    -- Determine the action type
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        SET @Action = 'UPDATE';
    ELSE IF EXISTS (SELECT * FROM inserted)
        SET @Action = 'INSERT';
    ELSE
        SET @Action = 'DELETE';
    
    -- For INSERT and UPDATE, log from inserted table
    IF @Action IN ('INSERT', 'UPDATE')
    BEGIN
        SELECT @RecordId = AppointmentId,
               @Details = 'PatientId: ' + CAST(PatientId AS NVARCHAR(10)) + 
                         ', DoctorId: ' + CAST(DoctorId AS NVARCHAR(10)) +  -- Using DoctorId now
                         ', Date: ' + CONVERT(NVARCHAR(20), AppointmentDate, 120) +
                         ', Time: ' + CAST(AppointmentTime AS NVARCHAR(10)) +
                         ', Status: ' + Status
        FROM inserted;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Action, RecordId, Details, CreatedDate)
        VALUES (@TableName, @Action, @RecordId, @Details, GETDATE());
    END
    
    -- For DELETE, log from deleted table
    IF @Action = 'DELETE'
    BEGIN
        SELECT @RecordId = AppointmentId,
               @Details = 'PatientId: ' + CAST(PatientId AS NVARCHAR(10)) + 
                         ', DoctorId: ' + CAST(DoctorId AS NVARCHAR(10)) +  -- Using DoctorId now
                         ', Date: ' + CONVERT(NVARCHAR(20), AppointmentDate, 120) +
                         ', Time: ' + CAST(AppointmentTime AS NVARCHAR(10)) +
                         ', Status: ' + Status
        FROM deleted;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Action, RecordId, Details, CreatedDate)
        VALUES (@TableName, @Action, @RecordId, @Details, GETDATE());
    END
END
GO

PRINT 'Audit trigger recreated successfully!';
PRINT 'The trigger now uses DoctorId instead of DoctorName.';
GO
