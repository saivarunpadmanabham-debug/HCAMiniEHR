-- Create Appointment Stored Procedure
-- This procedure creates a new appointment with validation

CREATE PROCEDURE [Healthcare].[usp_CreateAppointment]
    @PatientId INT,
    @AppointmentDate DATETIME,
    @AppointmentTime TIME,
    @DoctorName NVARCHAR(100),
    @Reason NVARCHAR(500),
    @NewAppointmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate that patient exists
    IF NOT EXISTS (SELECT 1 FROM [Healthcare].[Patient] WHERE PatientId = @PatientId)
    BEGIN
        RAISERROR('Patient with ID %d does not exist.', 16, 1, @PatientId);
        RETURN -1;
    END
    
    -- Validate that appointment date is not in the past
    IF @AppointmentDate < CAST(GETDATE() AS DATE)
    BEGIN
        RAISERROR('Appointment date cannot be in the past.', 16, 1);
        RETURN -2;
    END
    
    -- Insert the appointment
    INSERT INTO [Healthcare].[Appointment] 
        (PatientId, AppointmentDate, AppointmentTime, DoctorName, Reason, Status, CreatedDate)
    VALUES 
        (@PatientId, @AppointmentDate, @AppointmentTime, @DoctorName, @Reason, 'Scheduled', GETDATE());
    
    -- Get the newly created AppointmentId
    SET @NewAppointmentId = SCOPE_IDENTITY();
    
    -- Return success
    RETURN 0;
END;
GO
