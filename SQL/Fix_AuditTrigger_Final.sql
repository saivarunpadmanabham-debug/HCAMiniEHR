-- =============================================
-- FIX AUDIT TRIGGER - Correct column names for AuditLog table
-- =============================================

USE EHR2;
GO

PRINT 'Dropping old audit trigger...';
DROP TRIGGER IF EXISTS [Healthcare].[trg_Appointment_Audit];
GO

PRINT 'Creating new audit trigger with correct AuditLog columns...';
GO

CREATE TRIGGER [Healthcare].[trg_Appointment_Audit]
ON [Healthcare].[Appointment]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ActionType VARCHAR(10);
    DECLARE @TableName VARCHAR(100) = 'Appointment';
    DECLARE @RecordId INT;
    DECLARE @ChangeDetails NVARCHAR(MAX);
    DECLARE @UserName NVARCHAR(100) = SYSTEM_USER;
    
    -- Determine the action type
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
        SET @ActionType = 'UPDATE';
    ELSE IF EXISTS (SELECT * FROM inserted)
        SET @ActionType = 'INSERT';
    ELSE
        SET @ActionType = 'DELETE';
    
    -- For INSERT and UPDATE, log from inserted table
    IF @ActionType IN ('INSERT', 'UPDATE')
    BEGIN
        SELECT @RecordId = AppointmentId,
               @ChangeDetails = 'PatientId: ' + CAST(PatientId AS NVARCHAR(10)) + 
                         ', DoctorId: ' + CAST(DoctorId AS NVARCHAR(10)) +
                         ', Date: ' + CONVERT(NVARCHAR(20), AppointmentDate, 120) +
                         ', Time: ' + CAST(AppointmentTime AS NVARCHAR(10)) +
                         ', Status: ' + Status
        FROM inserted;
        
        -- Insert into AuditLog with correct column names
        INSERT INTO [Healthcare].[AuditLog] (TableName, ActionType, RecordId, ChangeDetails, ChangedBy, ChangedDate)
        VALUES (@TableName, @ActionType, @RecordId, @ChangeDetails, @UserName, GETDATE());
    END
    
    -- For DELETE, log from deleted table
    IF @ActionType = 'DELETE'
    BEGIN
        SELECT @RecordId = AppointmentId,
               @ChangeDetails = 'PatientId: ' + CAST(PatientId AS NVARCHAR(10)) + 
                         ', DoctorId: ' + CAST(DoctorId AS NVARCHAR(10)) +
                         ', Date: ' + CONVERT(NVARCHAR(20), AppointmentDate, 120) +
                         ', Time: ' + CAST(AppointmentTime AS NVARCHAR(10)) +
                         ', Status: ' + Status
        FROM deleted;
        
        -- Insert into AuditLog with correct column names
        INSERT INTO [Healthcare].[AuditLog] (TableName, ActionType, RecordId, ChangeDetails, ChangedBy, ChangedDate)
        VALUES (@TableName, @ActionType, @RecordId, @ChangeDetails, @UserName, GETDATE());
    END
END
GO

PRINT 'Audit trigger recreated successfully with correct column names!';
GO
