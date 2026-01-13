-- Create or update stored procedure for creating appointments
-- This procedure uses DoctorId instead of DoctorName

USE EHR2;
GO

-- Drop the procedure if it exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_CreateAppointment]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [Healthcare].[usp_CreateAppointment];
    PRINT 'Existing procedure dropped';
END
GO

-- Create the new procedure
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
            THROW 50001, 'Invalid PatientId', 1;
            
        IF @DoctorId <= 0
            THROW 50002, 'Invalid DoctorId', 1;
            
        IF @AppointmentDate < CAST(GETDATE() AS DATE)
            THROW 50003, 'Appointment date cannot be in the past', 1;
        
        -- Insert the appointment
        INSERT INTO [Healthcare].[Appointment] 
        (
            PatientId, 
            DoctorId, 
            AppointmentDate, 
            AppointmentTime, 
            Reason, 
            Status, 
            CreatedDate
        )
        VALUES 
        (
            @PatientId, 
            @DoctorId, 
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

PRINT 'Stored procedure [Healthcare].[usp_CreateAppointment] created successfully';
GO
