-- Complete diagnostic script for appointment creation
-- Run this to verify everything is set up correctly

USE EHR2;
GO

PRINT '=== DIAGNOSTIC REPORT FOR APPOINTMENT CREATION ===';
PRINT '';

-- 1. Check Appointment table structure
PRINT '1. APPOINTMENT TABLE STRUCTURE:';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Appointment' AND TABLE_SCHEMA = 'Healthcare'
ORDER BY ORDINAL_POSITION;
PRINT '';

-- 2. Check for DoctorName column (should NOT exist)
PRINT '2. CHECKING FOR OLD DOCTORNAME COLUMN:';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Appointment' AND COLUMN_NAME = 'DoctorName')
    PRINT '   ERROR: DoctorName column still exists!';
ELSE
    PRINT '   OK: DoctorName column does not exist';
PRINT '';

-- 3. Check constraints
PRINT '3. CONSTRAINTS ON APPOINTMENT TABLE:';
SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE TABLE_NAME = 'Appointment' AND TABLE_SCHEMA = 'Healthcare';
PRINT '';

-- 4. Check triggers
PRINT '4. TRIGGERS ON APPOINTMENT TABLE:';
SELECT name, type_desc
FROM sys.triggers
WHERE parent_id = OBJECT_ID('Healthcare.Appointment');
PRINT '';

-- 5. Check stored procedure
PRINT '5. STORED PROCEDURE STATUS:';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_CreateAppointment]'))
    PRINT '   OK: Stored procedure exists';
ELSE
    PRINT '   WARNING: Stored procedure does not exist';
PRINT '';

-- 6. Test direct INSERT
PRINT '6. TESTING DIRECT INSERT:';
BEGIN TRY
    INSERT INTO [Healthcare].[Appointment] 
    (PatientId, DoctorId, AppointmentDate, AppointmentTime, Reason, Status, CreatedDate)
    VALUES (1, 1, '2026-02-05', '11:00', 'Diagnostic test', 'Scheduled', GETDATE());
    
    DECLARE @NewId INT = SCOPE_IDENTITY();
    PRINT '   SUCCESS: Created appointment ID ' + CAST(@NewId AS NVARCHAR(10));
    
    -- Clean up test record
    DELETE FROM [Healthcare].[Appointment] WHERE AppointmentId = @NewId;
    PRINT '   Test record cleaned up';
END TRY
BEGIN CATCH
    PRINT '   ERROR: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

-- 7. Test stored procedure
PRINT '7. TESTING STORED PROCEDURE:';
BEGIN TRY
    DECLARE @TestId INT;
    EXEC [Healthcare].[usp_CreateAppointment]
        @PatientId = 1,
        @DoctorId = 1,
        @AppointmentDate = '2026-02-06',
        @AppointmentTime = '12:00',
        @Reason = 'SP diagnostic test',
        @NewAppointmentId = @TestId OUTPUT;
    
    PRINT '   SUCCESS: Created appointment ID ' + CAST(@TestId AS NVARCHAR(10));
    
    -- Clean up test record
    DELETE FROM [Healthcare].[Appointment] WHERE AppointmentId = @TestId;
    PRINT '   Test record cleaned up';
END TRY
BEGIN CATCH
    PRINT '   ERROR: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

PRINT '=== END OF DIAGNOSTIC REPORT ===';
