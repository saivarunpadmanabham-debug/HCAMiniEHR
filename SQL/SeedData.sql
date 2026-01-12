-- Seed Data for HCAMiniEHR Database
-- This script inserts realistic mock data for testing

USE EHR2;
GO

-- Insert 10+ Patients
INSERT INTO [Healthcare].[Patient] (FirstName, LastName, DateOfBirth, Gender, Phone, Email, Address, CreatedDate)
VALUES
    ('John', 'Smith', '1985-03-15', 'Male', '555-0101', 'john.smith@email.com', '123 Main St, Springfield', GETDATE()),
    ('Sarah', 'Johnson', '1990-07-22', 'Female', '555-0102', 'sarah.j@email.com', '456 Oak Ave, Springfield', GETDATE()),
    ('Michael', 'Williams', '1978-11-08', 'Male', '555-0103', 'mwilliams@email.com', '789 Pine Rd, Springfield', GETDATE()),
    ('Emily', 'Brown', '1995-01-30', 'Female', '555-0104', 'emily.brown@email.com', '321 Elm St, Springfield', GETDATE()),
    ('David', 'Jones', '1982-05-17', 'Male', '555-0105', 'djones@email.com', '654 Maple Dr, Springfield', GETDATE()),
    ('Jennifer', 'Garcia', '1988-09-25', 'Female', '555-0106', 'jgarcia@email.com', '987 Cedar Ln, Springfield', GETDATE()),
    ('Robert', 'Martinez', '1975-12-03', 'Male', '555-0107', 'rmartinez@email.com', '147 Birch Ct, Springfield', GETDATE()),
    ('Lisa', 'Rodriguez', '1992-04-14', 'Female', '555-0108', 'lrodriguez@email.com', '258 Willow Way, Springfield', GETDATE()),
    ('James', 'Wilson', '1980-08-19', 'Male', '555-0109', 'jwilson@email.com', '369 Spruce St, Springfield', GETDATE()),
    ('Mary', 'Anderson', '1987-02-28', 'Female', '555-0110', 'manderson@email.com', '741 Ash Blvd, Springfield', GETDATE()),
    ('William', 'Taylor', '1993-06-11', 'Male', '555-0111', 'wtaylor@email.com', '852 Poplar Ave, Springfield', GETDATE()),
    ('Patricia', 'Thomas', '1979-10-07', 'Female', '555-0112', 'pthomas@email.com', '963 Hickory Rd, Springfield', GETDATE());

-- Insert 5+ Appointments (various statuses and dates)
INSERT INTO [Healthcare].[Appointment] (PatientId, AppointmentDate, AppointmentTime, DoctorName, Reason, Status, CreatedDate)
VALUES
    (1, '2026-01-15', '09:00:00', 'Dr. Sarah Mitchell', 'Annual Physical Exam', 'Scheduled', GETDATE()),
    (2, '2026-01-16', '10:30:00', 'Dr. James Chen', 'Follow-up consultation', 'Scheduled', GETDATE()),
    (3, '2025-12-20', '14:00:00', 'Dr. Sarah Mitchell', 'Flu symptoms', 'Completed', GETDATE()),
    (4, '2026-01-18', '11:00:00', 'Dr. Robert Kumar', 'Diabetes management', 'Scheduled', GETDATE()),
    (5, '2025-12-28', '15:30:00', 'Dr. James Chen', 'Back pain evaluation', 'Completed', GETDATE()),
    (6, '2026-01-20', '08:30:00', 'Dr. Emily Watson', 'Prenatal checkup', 'Scheduled', GETDATE()),
    (7, '2026-01-22', '13:00:00', 'Dr. Robert Kumar', 'Hypertension follow-up', 'Scheduled', GETDATE()),
    (8, '2026-01-10', '16:00:00', 'Dr. Sarah Mitchell', 'Skin rash consultation', 'Cancelled', GETDATE());

-- Insert 5+ Lab Orders (various statuses)
INSERT INTO [Healthcare].[LabOrder] (AppointmentId, TestName, OrderDate, Status, Results, CompletedDate)
VALUES
    (1, 'Complete Blood Count (CBC)', GETDATE(), 'Pending', NULL, NULL),
    (1, 'Lipid Panel', GETDATE(), 'Pending', NULL, NULL),
    (2, 'Thyroid Function Test', GETDATE(), 'Pending', NULL, NULL),
    (3, 'Rapid Flu Test', '2025-12-20', 'Completed', 'Positive for Influenza A', '2025-12-20'),
    (4, 'HbA1c Test', GETDATE(), 'Pending', NULL, NULL),
    (4, 'Fasting Blood Glucose', GETDATE(), 'Pending', NULL, NULL),
    (5, 'X-Ray - Lower Back', '2025-12-28', 'Completed', 'No abnormalities detected', '2025-12-29'),
    (6, 'Prenatal Panel', GETDATE(), 'In Progress', NULL, NULL);

PRINT 'Seed data inserted successfully!';
PRINT 'Patients: 12 records';
PRINT 'Appointments: 8 records';
PRINT 'Lab Orders: 8 records';
GO
