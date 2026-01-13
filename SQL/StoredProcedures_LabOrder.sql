-- =============================================
-- Stored Procedures for Lab Order Operations
-- =============================================

USE EHR2;
GO

-- =============================================
-- 1. CREATE LAB ORDER
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_CreateLabOrder]'))
    DROP PROCEDURE [Healthcare].[usp_CreateLabOrder];
GO

CREATE PROCEDURE [Healthcare].[usp_CreateLabOrder]
    @AppointmentId INT,
    @TestName NVARCHAR(200),
    @NewLabOrderId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validation
        IF @AppointmentId <= 0
            THROW 50011, 'Invalid AppointmentId', 1;
            
        IF @TestName IS NULL OR LTRIM(RTRIM(@TestName)) = ''
            THROW 50012, 'Test name is required', 1;
        
        -- Check if appointment exists
        IF NOT EXISTS (SELECT 1 FROM [Healthcare].[Appointment] WHERE AppointmentId = @AppointmentId)
            THROW 50013, 'Appointment not found', 1;
        
        -- Insert lab order
        INSERT INTO [Healthcare].[LabOrder] 
        (AppointmentId, TestName, OrderDate, Status)
        VALUES 
        (@AppointmentId, @TestName, GETDATE(), 'Pending');
        
        SET @NewLabOrderId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        
        PRINT 'Lab order created successfully with ID: ' + CAST(@NewLabOrderId AS NVARCHAR(10));
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
-- 2. UPDATE LAB ORDER STATUS
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_UpdateLabOrderStatus]'))
    DROP PROCEDURE [Healthcare].[usp_UpdateLabOrderStatus];
GO

CREATE PROCEDURE [Healthcare].[usp_UpdateLabOrderStatus]
    @LabOrderId INT,
    @Status NVARCHAR(50),
    @Results NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if lab order exists
        IF NOT EXISTS (SELECT 1 FROM [Healthcare].[LabOrder] WHERE LabOrderId = @LabOrderId)
            THROW 50014, 'Lab order not found', 1;
        
        -- Validation
        IF @Status IS NULL OR LTRIM(RTRIM(@Status)) = ''
            THROW 50015, 'Status is required', 1;
        
        -- Validate status values
        IF @Status NOT IN ('Pending', 'In Progress', 'Completed', 'Cancelled')
            THROW 50016, 'Invalid status. Must be: Pending, In Progress, Completed, or Cancelled', 1;
        
        -- If status is Completed or Cancelled, Results is required
        IF @Status IN ('Completed', 'Cancelled')
        BEGIN
            IF @Results IS NULL OR LTRIM(RTRIM(@Results)) = '' OR LEN(LTRIM(RTRIM(@Results))) < 10
                THROW 50017, 'Results field is required and must be at least 10 characters when status is Completed or Cancelled', 1;
        END
        
        -- Update lab order
        UPDATE [Healthcare].[LabOrder]
        SET Status = @Status,
            Results = @Results,
            CompletedDate = CASE WHEN @Status = 'Completed' THEN GETDATE() ELSE CompletedDate END
        WHERE LabOrderId = @LabOrderId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Lab order status updated successfully: ID ' + CAST(@LabOrderId AS NVARCHAR(10));
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
-- 3. DELETE LAB ORDER
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_DeleteLabOrder]'))
    DROP PROCEDURE [Healthcare].[usp_DeleteLabOrder];
GO

CREATE PROCEDURE [Healthcare].[usp_DeleteLabOrder]
    @LabOrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if lab order exists
        IF NOT EXISTS (SELECT 1 FROM [Healthcare].[LabOrder] WHERE LabOrderId = @LabOrderId)
            THROW 50014, 'Lab order not found', 1;
        
        -- Check if lab order is completed
        DECLARE @Status NVARCHAR(50);
        SELECT @Status = Status FROM [Healthcare].[LabOrder] WHERE LabOrderId = @LabOrderId;
        
        IF @Status = 'Completed'
            THROW 50018, 'Cannot delete a completed lab order', 1;
        
        -- Delete lab order
        DELETE FROM [Healthcare].[LabOrder]
        WHERE LabOrderId = @LabOrderId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Lab order deleted successfully: ID ' + CAST(@LabOrderId AS NVARCHAR(10));
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
-- 4. GET PENDING LAB ORDERS
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Healthcare].[usp_GetPendingLabOrders]'))
    DROP PROCEDURE [Healthcare].[usp_GetPendingLabOrders];
GO

CREATE PROCEDURE [Healthcare].[usp_GetPendingLabOrders]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lo.LabOrderId,
        lo.TestName,
        lo.OrderDate,
        lo.Status,
        p.FirstName + ' ' + p.LastName AS PatientName,
        d.FirstName + ' ' + d.LastName AS DoctorName,
        a.AppointmentDate
    FROM [Healthcare].[LabOrder] lo
    INNER JOIN [Healthcare].[Appointment] a ON lo.AppointmentId = a.AppointmentId
    INNER JOIN [Healthcare].[Patient] p ON a.PatientId = p.PatientId
    INNER JOIN [Healthcare].[Doctor] d ON a.DoctorId = d.DoctorId
    WHERE lo.Status IN ('Pending', 'In Progress')
    ORDER BY lo.OrderDate DESC;
END
GO

PRINT '=== Lab Order Stored Procedures Created Successfully ===';
