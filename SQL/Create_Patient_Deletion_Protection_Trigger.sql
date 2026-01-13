-- =============================================
-- Patient Deletion Protection Trigger
-- Prevents deletion of patients with upcoming appointments or pending lab orders
-- =============================================

USE EHR2;
GO

-- Drop the trigger if it exists
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Patient_PreventDeletion' AND parent_id = OBJECT_ID('[Healthcare].[Patient]'))
BEGIN
    DROP TRIGGER [Healthcare].[trg_Patient_PreventDeletion];
    PRINT 'Existing trigger dropped.';
END
GO

-- Create the trigger to prevent deletion of patients with upcoming appointments or pending lab orders
CREATE TRIGGER [Healthcare].[trg_Patient_PreventDeletion]
ON [Healthcare].[Patient]
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PatientId INT;
    DECLARE @PatientName NVARCHAR(200);
    DECLARE @UpcomingAppointmentCount INT;
    DECLARE @PendingLabOrderCount INT;
    DECLARE @AppointmentDates NVARCHAR(MAX);
    DECLARE @LabOrderDetails NVARCHAR(MAX);
    
    -- Get the patient being deleted
    SELECT @PatientId = PatientId,
           @PatientName = FirstName + ' ' + LastName
    FROM deleted;
    
    -- Check for upcoming appointments (not cancelled or completed)
    SELECT @UpcomingAppointmentCount = COUNT(*)
    FROM [Healthcare].[Appointment]
    WHERE PatientId = @PatientId
        AND AppointmentDate >= CAST(GETDATE() AS DATE)
        AND Status NOT IN ('Cancelled', 'Completed');
    
    IF @UpcomingAppointmentCount > 0
    BEGIN
        -- Get appointment dates for error message
        SELECT @AppointmentDates = STRING_AGG(
            CONVERT(VARCHAR(10), AppointmentDate, 101), ', ')
        FROM [Healthcare].[Appointment]
        WHERE PatientId = @PatientId
            AND AppointmentDate >= CAST(GETDATE() AS DATE)
            AND Status NOT IN ('Cancelled', 'Completed');
        
        DECLARE @AppointmentError NVARCHAR(500) = 
            'Cannot delete patient ' + @PatientName + 
            '. They have upcoming appointments on: ' + @AppointmentDates + 
            '. Please cancel or complete these appointments first.';
        
        RAISERROR(@AppointmentError, 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    -- Check for pending or in-progress lab orders
    SELECT @PendingLabOrderCount = COUNT(*)
    FROM [Healthcare].[LabOrder] lo
    INNER JOIN [Healthcare].[Appointment] a ON lo.AppointmentId = a.AppointmentId
    WHERE a.PatientId = @PatientId
        AND lo.Status IN ('Pending', 'In Progress');
    
    IF @PendingLabOrderCount > 0
    BEGIN
        -- Get lab order details for error message (FIXED: added table alias to Status)
        SELECT @LabOrderDetails = STRING_AGG(
            lo.TestName + ' (' + lo.Status + ')', ', ')
        FROM [Healthcare].[LabOrder] lo
        INNER JOIN [Healthcare].[Appointment] a ON lo.AppointmentId = a.AppointmentId
        WHERE a.PatientId = @PatientId
            AND lo.Status IN ('Pending', 'In Progress');
        
        DECLARE @LabOrderError NVARCHAR(500) = 
            'Cannot delete patient ' + @PatientName + 
            '. They have pending or in-progress lab orders: ' + @LabOrderDetails + 
            '. Please complete or cancel these lab orders first.';
        
        RAISERROR(@LabOrderError, 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    -- If all checks pass, allow the deletion
    DELETE FROM [Healthcare].[Patient]
    WHERE PatientId = @PatientId;
    
    PRINT 'Patient ' + @PatientName + ' deleted successfully.';
END
GO

PRINT 'Trigger [Healthcare].[trg_Patient_PreventDeletion] created successfully.';
GO

-- Verify the trigger was created
SELECT 
    t.name AS TriggerName,
    OBJECT_NAME(t.parent_id) AS TableName,
    t.is_disabled AS IsDisabled,
    t.is_instead_of_trigger AS IsInsteadOf
FROM sys.triggers t
WHERE t.parent_id = OBJECT_ID('[Healthcare].[Patient]');
GO

PRINT '';
PRINT '=== Patient Deletion Protection Trigger Setup Complete ===';
