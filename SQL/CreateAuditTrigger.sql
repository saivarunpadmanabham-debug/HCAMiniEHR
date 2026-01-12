-- Create Audit Trigger for Appointment Table
-- This trigger logs INSERT, UPDATE, and DELETE operations on the Appointment table

CREATE TRIGGER trg_Appointment_Audit
ON [Healthcare].[Appointment]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Operation VARCHAR(10);
    DECLARE @RecordId INT;
    DECLARE @OldValue NVARCHAR(MAX);
    DECLARE @NewValue NVARCHAR(MAX);
    
    -- Determine the operation type
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        -- UPDATE operation
        SET @Operation = 'UPDATE';
        
        SELECT @RecordId = i.AppointmentId,
               @OldValue = CONCAT('PatientId:', d.PatientId, ', Date:', CONVERT(VARCHAR, d.AppointmentDate, 120), 
                                  ', Time:', CONVERT(VARCHAR, d.AppointmentTime, 108), ', Doctor:', d.DoctorName, 
                                  ', Status:', d.Status, ', Reason:', d.Reason),
               @NewValue = CONCAT('PatientId:', i.PatientId, ', Date:', CONVERT(VARCHAR, i.AppointmentDate, 120), 
                                  ', Time:', CONVERT(VARCHAR, i.AppointmentTime, 108), ', Doctor:', i.DoctorName, 
                                  ', Status:', i.Status, ', Reason:', i.Reason)
        FROM inserted i
        INNER JOIN deleted d ON i.AppointmentId = d.AppointmentId;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Operation, RecordId, OldValue, NewValue, ChangedDate, ChangedBy)
        VALUES ('Appointment', @Operation, @RecordId, @OldValue, @NewValue, GETDATE(), SYSTEM_USER);
    END
    ELSE IF EXISTS (SELECT * FROM inserted)
    BEGIN
        -- INSERT operation
        SET @Operation = 'INSERT';
        
        SELECT @RecordId = i.AppointmentId,
               @NewValue = CONCAT('PatientId:', i.PatientId, ', Date:', CONVERT(VARCHAR, i.AppointmentDate, 120), 
                                  ', Time:', CONVERT(VARCHAR, i.AppointmentTime, 108), ', Doctor:', i.DoctorName, 
                                  ', Status:', i.Status, ', Reason:', i.Reason)
        FROM inserted i;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Operation, RecordId, OldValue, NewValue, ChangedDate, ChangedBy)
        VALUES ('Appointment', @Operation, @RecordId, NULL, @NewValue, GETDATE(), SYSTEM_USER);
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        -- DELETE operation
        SET @Operation = 'DELETE';
        
        SELECT @RecordId = d.AppointmentId,
               @OldValue = CONCAT('PatientId:', d.PatientId, ', Date:', CONVERT(VARCHAR, d.AppointmentDate, 120), 
                                  ', Time:', CONVERT(VARCHAR, d.AppointmentTime, 108), ', Doctor:', d.DoctorName, 
                                  ', Status:', d.Status, ', Reason:', d.Reason)
        FROM deleted d;
        
        INSERT INTO [Healthcare].[AuditLog] (TableName, Operation, RecordId, OldValue, NewValue, ChangedDate, ChangedBy)
        VALUES ('Appointment', @Operation, @RecordId, @OldValue, NULL, GETDATE(), SYSTEM_USER);
    END
END;
GO
