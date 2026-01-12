-- Migration: Add Doctor Table and Update Appointments
-- This migration adds the Doctor table and updates the Appointment table to use DoctorId instead of DoctorName

-- Step 1: Create Doctor table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Doctor' AND schema_id = SCHEMA_ID('Healthcare'))
BEGIN
    CREATE TABLE [Healthcare].[Doctor] (
        [DoctorId] INT IDENTITY(1,1) PRIMARY KEY,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Specialization] NVARCHAR(100) NOT NULL,
        [LicenseNumber] NVARCHAR(50) NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [Address] NVARCHAR(200) NULL,
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [IsActive] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Doctor table created successfully';
END
ELSE
BEGIN
    PRINT 'Doctor table already exists';
END
GO

-- Step 2: Add sample doctors (optional - for testing)
IF NOT EXISTS (SELECT * FROM [Healthcare].[Doctor])
BEGIN
    INSERT INTO [Healthcare].[Doctor] ([FirstName], [LastName], [Specialization], [Phone], [Email], [IsActive])
    VALUES 
        ('John', 'Smith', 'Cardiology', '555-0101', 'john.smith@hospital.com', 1),
        ('Sarah', 'Johnson', 'Pediatrics', '555-0102', 'sarah.johnson@hospital.com', 1),
        ('Michael', 'Williams', 'Orthopedics', '555-0103', 'michael.williams@hospital.com', 1),
        ('Emily', 'Brown', 'General Practice', '555-0104', 'emily.brown@hospital.com', 1),
        ('David', 'Jones', 'Neurology', '555-0105', 'david.jones@hospital.com', 1);
    PRINT 'Sample doctors inserted';
END
GO

-- Step 3: Add DoctorId column to Appointment table (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Healthcare].[Appointment]') AND name = 'DoctorId')
BEGIN
    ALTER TABLE [Healthcare].[Appointment]
    ADD [DoctorId] INT NULL;
    PRINT 'DoctorId column added to Appointment table';
END
ELSE
BEGIN
    PRINT 'DoctorId column already exists in Appointment table';
END
GO

-- Step 4: Migrate existing DoctorName data to Doctor table and update DoctorId
-- This creates doctors from existing DoctorName values and updates appointments
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Healthcare].[Appointment]') AND name = 'DoctorName')
BEGIN
    -- Create doctors from unique DoctorName values
    INSERT INTO [Healthcare].[Doctor] ([FirstName], [LastName], [Specialization], [IsActive])
    SELECT DISTINCT
        CASE 
            WHEN CHARINDEX(' ', LTRIM(RTRIM(DoctorName))) > 0 
            THEN LEFT(LTRIM(RTRIM(DoctorName)), CHARINDEX(' ', LTRIM(RTRIM(DoctorName))) - 1)
            ELSE LTRIM(RTRIM(DoctorName))
        END AS FirstName,
        CASE 
            WHEN CHARINDEX(' ', LTRIM(RTRIM(DoctorName))) > 0 
            THEN SUBSTRING(LTRIM(RTRIM(DoctorName)), CHARINDEX(' ', LTRIM(RTRIM(DoctorName))) + 1, LEN(LTRIM(RTRIM(DoctorName))))
            ELSE ''
        END AS LastName,
        'General Practice' AS Specialization,
        1 AS IsActive
    FROM [Healthcare].[Appointment]
    WHERE DoctorName IS NOT NULL 
      AND DoctorName <> ''
      AND NOT EXISTS (
          SELECT 1 FROM [Healthcare].[Doctor] d
          WHERE d.FirstName + ' ' + d.LastName = LTRIM(RTRIM(DoctorName))
      );

    -- Update appointments with DoctorId based on DoctorName
    UPDATE a
    SET a.DoctorId = d.DoctorId
    FROM [Healthcare].[Appointment] a
    INNER JOIN [Healthcare].[Doctor] d 
        ON LTRIM(RTRIM(a.DoctorName)) = d.FirstName + ' ' + d.LastName
    WHERE a.DoctorId IS NULL;

    PRINT 'Existing appointments updated with DoctorId';
END
GO

-- Step 5: Create foreign key constraint
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Appointment_Doctor_DoctorId')
BEGIN
    ALTER TABLE [Healthcare].[Appointment]
    ADD CONSTRAINT [FK_Appointment_Doctor_DoctorId]
    FOREIGN KEY ([DoctorId]) REFERENCES [Healthcare].[Doctor]([DoctorId])
    ON DELETE NO ACTION;
    PRINT 'Foreign key constraint created';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint already exists';
END
GO

-- Step 6: Make DoctorId NOT NULL (after data migration)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Healthcare].[Appointment]') AND name = 'DoctorId' AND is_nullable = 1)
BEGIN
    -- First, ensure all appointments have a DoctorId
    -- If any appointments don't have a DoctorId, assign them to the first doctor
    UPDATE [Healthcare].[Appointment]
    SET DoctorId = (SELECT TOP 1 DoctorId FROM [Healthcare].[Doctor] WHERE IsActive = 1)
    WHERE DoctorId IS NULL;

    -- Now make the column NOT NULL
    ALTER TABLE [Healthcare].[Appointment]
    ALTER COLUMN [DoctorId] INT NOT NULL;
    PRINT 'DoctorId column set to NOT NULL';
END
GO

-- Step 7: Drop DoctorName column (optional - uncomment if you want to remove it)
-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Healthcare].[Appointment]') AND name = 'DoctorName')
-- BEGIN
--     ALTER TABLE [Healthcare].[Appointment]
--     DROP COLUMN [DoctorName];
--     PRINT 'DoctorName column dropped';
-- END
-- GO

PRINT 'Migration completed successfully!';
