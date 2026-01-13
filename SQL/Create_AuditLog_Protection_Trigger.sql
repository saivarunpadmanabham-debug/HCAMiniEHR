-- =============================================
-- AuditLog Protection Trigger
-- Prevents any DELETE or UPDATE operations on the AuditLog table
-- =============================================

USE EHR2;
GO

-- Drop the trigger if it exists
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_AuditLog_PreventModification' AND parent_id = OBJECT_ID('[Healthcare].[AuditLog]'))
BEGIN
    DROP TRIGGER [Healthcare].[trg_AuditLog_PreventModification];
    PRINT 'Existing trigger dropped.';
END
GO

-- Create the trigger to prevent DELETE and UPDATE on AuditLog
CREATE TRIGGER [Healthcare].[trg_AuditLog_PreventModification]
ON [Healthcare].[AuditLog]
INSTEAD OF DELETE, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RowCount INT = (SELECT COUNT(*) FROM deleted);
    DECLARE @UpdateCount INT = (SELECT COUNT(*) FROM inserted);
    
    -- Check if this is a DELETE operation
    IF @RowCount > 0 AND @UpdateCount = 0
    BEGIN
        RAISERROR('DELETE operations are not allowed on the AuditLog table. Audit records must be preserved for compliance.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    -- Check if this is an UPDATE operation
    IF @UpdateCount > 0
    BEGIN
        RAISERROR('UPDATE operations are not allowed on the AuditLog table. Audit records must be immutable.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

PRINT 'Trigger [Healthcare].[trg_AuditLog_PreventModification] created successfully.';
GO

-- Verify the trigger was created
SELECT 
    t.name AS TriggerName,
    OBJECT_NAME(t.parent_id) AS TableName,
    t.is_disabled AS IsDisabled,
    t.is_instead_of_trigger AS IsInsteadOf
FROM sys.triggers t
WHERE t.parent_id = OBJECT_ID('[Healthcare].[AuditLog]');
GO

-- Test the trigger by attempting a DELETE (should fail)
PRINT '';
PRINT '=== Testing DELETE protection ===';
BEGIN TRY
    DELETE TOP (1) FROM [Healthcare].[AuditLog];
    PRINT 'ERROR: Delete was allowed! Trigger is not working.';
END TRY
BEGIN CATCH
    PRINT 'SUCCESS: Delete was blocked by trigger.';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
END CATCH
GO

-- Test the trigger by attempting an UPDATE (should fail)
PRINT '';
PRINT '=== Testing UPDATE protection ===';
BEGIN TRY
    UPDATE TOP (1) [Healthcare].[AuditLog] 
    SET Action = 'TEST' 
    WHERE 1=1;
    PRINT 'ERROR: Update was allowed! Trigger is not working.';
END TRY
BEGIN CATCH
    PRINT 'SUCCESS: Update was blocked by trigger.';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
END CATCH
GO

PRINT '';
PRINT '=== AuditLog Protection Trigger Setup Complete ===';
