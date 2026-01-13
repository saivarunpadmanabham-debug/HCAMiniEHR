-- =============================================
-- Comprehensive Database Diagnostics
-- Checks all tables, triggers, stored procedures, and constraints
-- =============================================

USE EHR2;
GO

PRINT '=== DATABASE DIAGNOSTICS FOR EHR2 ===';
PRINT '';

-- 1. List all tables in Healthcare schema
PRINT '1. TABLES IN [Healthcare] SCHEMA:';
PRINT '-----------------------------------';
SELECT 
    SCHEMA_NAME(schema_id) AS SchemaName,
    name AS TableName,
    create_date AS CreatedDate
FROM sys.tables
WHERE SCHEMA_NAME(schema_id) = 'Healthcare'
ORDER BY name;
GO

PRINT '';
PRINT '2. ALL TRIGGERS (Table-level):';
PRINT '-----------------------------------';
SELECT 
    t.name AS TriggerName,
    SCHEMA_NAME(o.schema_id) + '.' + OBJECT_NAME(t.parent_id) AS TableName,
    CASE WHEN t.is_disabled = 1 THEN 'DISABLED' ELSE 'ENABLED' END AS Status,
    CASE WHEN t.is_instead_of_trigger = 1 THEN 'INSTEAD OF' ELSE 'AFTER' END AS TriggerType,
    t.create_date AS CreatedDate
FROM sys.triggers t
INNER JOIN sys.objects o ON t.parent_id = o.object_id
WHERE t.parent_class = 1 -- Object or column triggers
ORDER BY TableName, TriggerName;
GO

PRINT '';
PRINT '3. STORED PROCEDURES IN [Healthcare] SCHEMA:';
PRINT '-----------------------------------';
SELECT 
    SCHEMA_NAME(schema_id) AS SchemaName,
    name AS ProcedureName,
    create_date AS CreatedDate,
    modify_date AS ModifiedDate
FROM sys.procedures
WHERE SCHEMA_NAME(schema_id) = 'Healthcare'
ORDER BY name;
GO

PRINT '';
PRINT '4. FOREIGN KEY CONSTRAINTS:';
PRINT '-----------------------------------';
SELECT 
    OBJECT_SCHEMA_NAME(f.parent_object_id) + '.' + OBJECT_NAME(f.parent_object_id) AS TableName,
    f.name AS ConstraintName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_SCHEMA_NAME(f.referenced_object_id) + '.' + OBJECT_NAME(f.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc ON f.object_id = fc.constraint_object_id
WHERE OBJECT_SCHEMA_NAME(f.parent_object_id) = 'Healthcare'
ORDER BY TableName, ConstraintName;
GO

PRINT '';
PRINT '5. AUDITLOG TABLE DETAILS:';
PRINT '-----------------------------------';
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('[Healthcare].[AuditLog]')
ORDER BY c.column_id;
GO

PRINT '';
PRINT '6. RECENT AUDIT LOG ENTRIES (Last 10):';
PRINT '-----------------------------------';
SELECT TOP 10
    AuditLogId,
    TableName,
    OperationType,
    RecordId,
    ChangedBy,
    ChangeDate
FROM [Healthcare].[AuditLog]
ORDER BY ChangeDate DESC;
GO

PRINT '';
PRINT '7. APPOINTMENT TABLE STRUCTURE:';
PRINT '-----------------------------------';
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('[Healthcare].[Appointment]')
ORDER BY c.column_id;
GO

PRINT '';
PRINT '=== DIAGNOSTICS COMPLETE ===';
