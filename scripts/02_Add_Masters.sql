-- =============================================
-- Script: 02_Add_Masters.sql
-- Database: saksham
-- Adds Category & Brand Master tables, procedures, and seeds
-- =============================================

USE [saksham];
GO

-- 1. Create Categories Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL UNIQUE,
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 2. Create Brands Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Brands' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Brands] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL UNIQUE,
        [Description] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 3. Stored Procedures: Category Master
IF OBJECT_ID(N'[dbo].[USP_Category_GetAll]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Category_GetAll];
GO
CREATE PROCEDURE [dbo].[USP_Category_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Name], [Description], [CreatedAt]
    FROM [dbo].[Categories]
    ORDER BY [Name] ASC;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Category_Insert]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Category_Insert];
GO
CREATE PROCEDURE [dbo].[USP_Category_Insert]
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM [dbo].[Categories] WHERE [Name] = @Name)
    BEGIN
        SELECT -1 AS Result;
        RETURN;
    END

    INSERT INTO [dbo].[Categories] ([Name], [Description], [CreatedAt])
    VALUES (@Name, @Description, GETDATE());

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Result;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Category_Delete]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Category_Delete];
GO
CREATE PROCEDURE [dbo].[USP_Category_Delete]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Categories] WHERE [Id] = @Id;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 4. Stored Procedures: Brand Master
IF OBJECT_ID(N'[dbo].[USP_Brand_GetAll]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Brand_GetAll];
GO
CREATE PROCEDURE [dbo].[USP_Brand_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Name], [Description], [CreatedAt]
    FROM [dbo].[Brands]
    ORDER BY [Name] ASC;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Brand_Insert]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Brand_Insert];
GO
CREATE PROCEDURE [dbo].[USP_Brand_Insert]
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM [dbo].[Brands] WHERE [Name] = @Name)
    BEGIN
        SELECT -1 AS Result;
        RETURN;
    END

    INSERT INTO [dbo].[Brands] ([Name], [Description], [CreatedAt])
    VALUES (@Name, @Description, GETDATE());

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Result;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Brand_Delete]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Brand_Delete];
GO
CREATE PROCEDURE [dbo].[USP_Brand_Delete]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Brands] WHERE [Id] = @Id;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 5. Stored Procedures: Order Admin Master
IF OBJECT_ID(N'[dbo].[USP_Order_GetAll]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Order_GetAll];
GO
CREATE PROCEDURE [dbo].[USP_Order_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        o.[Id] AS OrderId,
        o.[UserId],
        o.[OrderDate],
        o.[TotalAmount],
        o.[Status],
        o.[ShippingAddress],
        o.[PaymentMethod],
        o.[PaymentStatus],
        o.[TransactionId],
        oi.[Id] AS OrderItemId,
        oi.[ProductId],
        oi.[ProductName],
        oi.[UnitPrice],
        oi.[Quantity],
        oi.[TotalPrice]
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[OrderItems] oi ON o.[Id] = oi.[OrderId]
    ORDER BY o.[OrderDate] DESC;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Order_UpdateStatus]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Order_UpdateStatus];
GO
CREATE PROCEDURE [dbo].[USP_Order_UpdateStatus]
    @OrderId INT,
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Orders]
    SET [Status] = @Status
    WHERE [Id] = @OrderId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 6. Seed Categories & Brands
IF NOT EXISTS (SELECT 1 FROM [dbo].[Categories])
BEGIN
    INSERT INTO [dbo].[Categories] ([Name], [Description]) VALUES
    ('Electronics', 'Gadgets, audio, wearables, and electronic devices'),
    ('Fashion', 'Apparel, footwear, bags, and contemporary wear'),
    ('Home', 'Kitchenware, stoneware, decor, and living essentials'),
    ('Sports', 'Fitness gear, yoga equipment, and athletic apparel'),
    ('Beauty', 'Skincare, serums, haircare, and personal beauty products');
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Brands])
BEGIN
    INSERT INTO [dbo].[Brands] ([Name], [Description]) VALUES
    ('Nimbus', 'Premium electronics and audio equipment'),
    ('Pulse', 'Smart wearables and tech accessories'),
    ('Stride', 'High-performance footwear and sneakers'),
    ('Harbor', 'Relaxed lifestyle and artisanal goods'),
    ('Hearth', 'Modern stoneware and home essentials'),
    ('Forge', 'Heavy-duty athletic gear and fitness tools'),
    ('Lumina', 'Clean wellness and radiance skincare');
END
GO
