-- Drop DoctorName column from Appointment table
-- This column is no longer needed as we now use DoctorId foreign key

USE EHR2;
GO

-- Drop the DoctorName column
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Healthcare].[Appointment]') AND name = 'DoctorName')
BEGIN
    ALTER TABLE [Healthcare].[Appointment]
    DROP COLUMN [DoctorName];
    PRINT 'DoctorName column dropped successfully';
END
ELSE
BEGIN
    PRINT 'DoctorName column does not exist';
END
GO
