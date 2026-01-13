-- =============================================
-- Stored Procedures for Patient Operations
-- =============================================

USE EHR2;
GO

-- =============================================
-- 1. CREATE PATIENT
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_CreatePatient]'))
    DROP PROCEDURE [Healthcare].[usp_CreatePatient];
GO

CREATE PROCEDURE [Healthcare].[usp_CreatePatient]
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @DateOfBirth DATETIME2,
    @Gender NVARCHAR(10),
    @Phone NVARCHAR(20),
    @Email NVARCHAR(100) = NULL,
    @Address NVARCHAR(200) = NULL,
    @NewPatientId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation
        IF @FirstName IS NULL OR LTRIM(RTRIM(@FirstName)) = ''
            THROW 50001, 'First name is required', 1;
            
        IF @LastName IS NULL OR LTRIM(RTRIM(@LastName)) = ''
            THROW 50002, 'Last name is required', 1;
            
        IF @DateOfBirth > GETDATE()
            THROW 50003, 'Date of birth cannot be in the future', 1;
            
        IF @DateOfBirth < '1900-01-01'
            THROW 50004, 'Date of birth must be after 1900', 1;
            
        IF @Gender IS NULL OR LTRIM(RTRIM(@Gender)) = ''
            THROW 50005, 'Gender is required', 1;
            
        IF @Phone IS NULL OR LTRIM(RTRIM(@Phone)) = ''
            THROW 50006, 'Phone number is required', 1;
        
        -- Check for duplicate patient
        IF EXISTS (
            SELECT 1 FROM [Healthcare].[Patient]
            WHERE LOWER(FirstName) = LOWER(@FirstName)
                AND LOWER(LastName) = LOWER(@LastName)
                AND CAST(DateOfBirth AS DATE) = CAST(@DateOfBirth AS DATE)
        )
        BEGIN
            DECLARE @DuplicateError NVARCHAR(500) = 
                'A patient with the name ' + @FirstName + ' ' + @LastName + 
                ' and date of birth ' + CONVERT(VARCHAR(10), @DateOfBirth, 101) + 
                ' already exists in the system.';
            THROW 50007, @DuplicateError, 1;
        END
        
        -- Insert patient
        INSERT INTO [Healthcare].[Patient] 
        (FirstName, LastName, DateOfBirth, Gender, Phone, Email, Address, CreatedDate)
        VALUES 
        (@FirstName, @LastName, @DateOfBirth, @Gender, @Phone, @Email, @Address, GETDATE());
        
        SET @NewPatientId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        PRINT 'Patient created successfully with ID: ' + CAST(@NewPatientId AS NVARCHAR(10));
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- 2. UPDATE PATIENT
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_UpdatePatient]'))
    DROP PROCEDURE [Healthcare].[usp_UpdatePatient];
GO

CREATE PROCEDURE [Healthcare].[usp_UpdatePatient]
    @PatientId INT,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @DateOfBirth DATETIME2,
    @Gender NVARCHAR(10),
    @Phone NVARCHAR(20),
    @Email NVARCHAR(100) = NULL,
    @Address NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if patient exists
        IF NOT EXISTS (SELECT 1 FROM [Healthcare].[Patient] WHERE PatientId = @PatientId)
            THROW 50008, 'Patient not found', 1;
        
        -- Validation
        IF @FirstName IS NULL OR LTRIM(RTRIM(@FirstName)) = ''
            THROW 50001, 'First name is required', 1;
            
        IF @LastName IS NULL OR LTRIM(RTRIM(@LastName)) = ''
            THROW 50002, 'Last name is required', 1;
            
        IF @DateOfBirth > GETDATE()
            THROW 50003, 'Date of birth cannot be in the future', 1;
            
        IF @Gender IS NULL OR LTRIM(RTRIM(@Gender)) = ''
            THROW 50005, 'Gender is required', 1;
            
        IF @Phone IS NULL OR LTRIM(RTRIM(@Phone)) = ''
            THROW 50006, 'Phone number is required', 1;
        
        -- Update patient
        UPDATE [Healthcare].[Patient]
        SET FirstName = @FirstName,
            LastName = @LastName,
            DateOfBirth = @DateOfBirth,
            Gender = @Gender,
            Phone = @Phone,
            Email = @Email,
            Address = @Address
        WHERE PatientId = @PatientId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Patient updated successfully: ID ' + CAST(@PatientId AS NVARCHAR(10));
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- 3. DELETE PATIENT (with validation)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_DeletePatient]'))
    DROP PROCEDURE [Healthcare].[usp_DeletePatient];
GO

CREATE PROCEDURE [Healthcare].[usp_DeletePatient]
    @PatientId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if patient exists
        IF NOT EXISTS (SELECT 1 FROM [Healthcare].[Patient] WHERE PatientId = @PatientId)
            THROW 50008, 'Patient not found', 1;
        
        -- Check for upcoming appointments
        DECLARE @UpcomingCount INT;
        SELECT @UpcomingCount = COUNT(*)
        FROM [Healthcare].[Appointment]
        WHERE PatientId = @PatientId
            AND AppointmentDate >= CAST(GETDATE() AS DATE)
            AND Status NOT IN ('Cancelled', 'Completed');
        
        IF @UpcomingCount > 0
        BEGIN
            DECLARE @AppointmentError NVARCHAR(500) = 
                'Cannot delete patient. They have ' + CAST(@UpcomingCount AS NVARCHAR(10)) + 
                ' upcoming appointment(s). Please cancel or complete these appointments first.';
            THROW 50009, @AppointmentError, 1;
        END
        
        -- Check for pending lab orders
        DECLARE @PendingLabCount INT;
        SELECT @PendingLabCount = COUNT(*)
        FROM [Healthcare].[LabOrder] lo
        INNER JOIN [Healthcare].[Appointment] a ON lo.AppointmentId = a.AppointmentId
        WHERE a.PatientId = @PatientId
            AND lo.Status IN ('Pending', 'In Progress');
        
        IF @PendingLabCount > 0
        BEGIN
            DECLARE @LabOrderError NVARCHAR(500) = 
                'Cannot delete patient. They have ' + CAST(@PendingLabCount AS NVARCHAR(10)) + 
                ' pending or in-progress lab order(s). Please complete or cancel these lab orders first.';
            THROW 50010, @LabOrderError, 1;
        END
        
        -- Delete patient
        DELETE FROM [Healthcare].[Patient]
        WHERE PatientId = @PatientId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Patient deleted successfully: ID ' + CAST(@PatientId AS NVARCHAR(10));
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

PRINT '=== Patient Stored Procedures Created Successfully ===';
