-- =============================================
-- Script: 03_Add_Payment_And_OrderDelete.sql
-- Database: saksham
-- Adds payment columns to Orders table +
-- USP_Order_Delete stored procedure
-- Run AFTER 01_Init_Ecom_Database.sql and 02_Add_Masters.sql
-- =============================================

USE [saksham];
GO

-- 1. Add PaymentMethod column (idempotent)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]')
      AND name = 'PaymentMethod'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [PaymentMethod] NVARCHAR(100) NULL;
    PRINT 'Added column: Orders.PaymentMethod';
END
ELSE
    PRINT 'Column Orders.PaymentMethod already exists — skipped.';
GO

-- 2. Add PaymentStatus column (idempotent)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]')
      AND name = 'PaymentStatus'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [PaymentStatus] NVARCHAR(50) NULL;
    PRINT 'Added column: Orders.PaymentStatus';
END
ELSE
    PRINT 'Column Orders.PaymentStatus already exists — skipped.';
GO

-- 3. Add TransactionId column (idempotent)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[Orders]')
      AND name = 'TransactionId'
)
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [TransactionId] NVARCHAR(100) NULL;
    PRINT 'Added column: Orders.TransactionId';
END
ELSE
    PRINT 'Column Orders.TransactionId already exists — skipped.';
GO

-- 4. USP_Order_Delete — deletes OrderItems first (FK), then Order row
IF OBJECT_ID(N'[dbo].[USP_Order_Delete]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Order_Delete];
GO
CREATE PROCEDURE [dbo].[USP_Order_Delete]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Delete child records first (FK: FK_OrderItems_Orders)
        DELETE FROM [dbo].[OrderItems]
        WHERE [OrderId] = @OrderId;

        -- Delete the parent order
        DELETE FROM [dbo].[Orders]
        WHERE [Id] = @OrderId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Script 03_Add_Payment_And_OrderDelete.sql completed successfully.';
GO
