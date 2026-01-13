-- =============================================
-- CLEAN DROP AND RECREATE STORED PROCEDURE
-- Run this script to fix the appointment creation issue
-- =============================================

USE EHR2;
GO

-- Step 1: Drop the old procedure completely
PRINT 'Step 1: Dropping old procedure...';
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_CreateAppointment]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [Healthcare].[usp_CreateAppointment];
    PRINT '  - Old procedure dropped successfully';
END
ELSE
BEGIN
    PRINT '  - No existing procedure found';
END
GO

-- Step 2: Create the new procedure with DoctorId
PRINT 'Step 2: Creating new procedure...';
GO

CREATE PROCEDURE [Healthcare].[usp_CreateAppointment]
    @PatientId INT,
    @DoctorId INT,
    @AppointmentDate DATETIME2,
    @AppointmentTime TIME,
    @Reason NVARCHAR(500) = NULL,
    @NewAppointmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF @PatientId <= 0
        BEGIN
            RAISERROR('Invalid PatientId', 16, 1);
            RETURN;
        END
            
        IF @DoctorId <= 0
        BEGIN
            RAISERROR('Invalid DoctorId', 16, 1);
            RETURN;
        END
            
        IF @AppointmentDate < CAST(GETDATE() AS DATE)
        BEGIN
            RAISERROR('Appointment date cannot be in the past', 16, 1);
            RETURN;
        END
        
        -- Insert the appointment with DoctorId (NOT DoctorName)
        INSERT INTO [Healthcare].[Appointment] 
        (
            PatientId, 
            DoctorId,           -- Using DoctorId column
            AppointmentDate, 
            AppointmentTime, 
            Reason, 
            Status, 
            CreatedDate
        )
        VALUES 
        (
            @PatientId, 
            @DoctorId,          -- Using DoctorId parameter
            @AppointmentDate, 
            @AppointmentTime, 
            @Reason, 
            'Scheduled', 
            GETDATE()
        );
        
        -- Get the new appointment ID
        SET @NewAppointmentId = SCOPE_IDENTITY();
        
        PRINT 'Appointment created successfully with ID: ' + CAST(@NewAppointmentId AS NVARCHAR(10));
    END TRY
    BEGIN CATCH
        -- Re-throw the error
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT 'Step 3: Procedure created successfully!';
GO

-- Step 4: Test the procedure
PRINT 'Step 4: Testing the procedure...';
DECLARE @TestNewId INT;
EXEC [Healthcare].[usp_CreateAppointment] 
    @PatientId = 1, 
    @DoctorId = 1, 
    @AppointmentDate = '2026-01-20', 
    @AppointmentTime = '14:00', 
    @Reason = 'Test appointment from stored procedure', 
    @NewAppointmentId = @TestNewId OUTPUT;

SELECT @TestNewId AS 'Test Appointment ID Created';
GO

PRINT '==============================================';
PRINT 'SCRIPT COMPLETED SUCCESSFULLY!';
PRINT 'The stored procedure is now ready to use.';
PRINT '==============================================';
