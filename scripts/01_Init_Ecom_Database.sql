-- =============================================
-- E-Commerce Solution Database Setup Script
-- Database: saksham
-- =============================================

USE [saksham];
GO

-- 1. Ensure Users Table Columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Email')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [Email] NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'RefreshToken')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [RefreshToken] NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'RefreshTokenExpiryTime')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [RefreshTokenExpiryTime] DATETIME NULL;
END
GO

-- 2. Create Products Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(250) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [Stock] INT NOT NULL DEFAULT 0,
        [Category] NVARCHAR(100) NOT NULL,
        [Brand] NVARCHAR(100) NOT NULL,
        [Rating] DECIMAL(3,2) NOT NULL DEFAULT 0.0,
        [ReviewCount] INT NOT NULL DEFAULT 0,
        [Image] NVARCHAR(500) NULL,
        [Colors] NVARCHAR(200) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 3. Create Orders and OrderItems Tables
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Orders] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [OrderDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [TotalAmount] DECIMAL(18,2) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [ShippingAddress] NVARCHAR(500) NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems' AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[OrderItems] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [ProductName] NVARCHAR(250) NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        [Quantity] INT NOT NULL,
        [TotalPrice] DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE
    );
END
GO

-- =============================================
-- STORED PROCEDURES: USER & AUTH
-- =============================================

IF OBJECT_ID(N'[dbo].[USP_User_GetByUsernameOrEmail]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_User_GetByUsernameOrEmail];
GO
CREATE PROCEDURE [dbo].[USP_User_GetByUsernameOrEmail]
    @Identifier NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Username], [Email], [Password], [Role], [RefreshToken], [RefreshTokenExpiryTime]
    FROM [dbo].[Users]
    WHERE [Username] = @Identifier OR [Email] = @Identifier;
END
GO

IF OBJECT_ID(N'[dbo].[USP_User_Register]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_User_Register];
GO
CREATE PROCEDURE [dbo].[USP_User_Register]
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @Password NVARCHAR(255),
    @Role NVARCHAR(50) = 'Customer'
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = @Username OR [Email] = @Email)
    BEGIN
        SELECT -1 AS Result;
        RETURN;
    END

    INSERT INTO [dbo].[Users] ([Username], [Email], [Password], [Role])
    VALUES (@Username, @Email, @Password, @Role);

    SELECT SCOPE_IDENTITY() AS Result;
END
GO

IF OBJECT_ID(N'[dbo].[USP_User_UpdateRefreshToken]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_User_UpdateRefreshToken];
GO
CREATE PROCEDURE [dbo].[USP_User_UpdateRefreshToken]
    @UserId INT,
    @RefreshToken NVARCHAR(500),
    @RefreshTokenExpiryTime DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Users]
    SET [RefreshToken] = @RefreshToken,
        [RefreshTokenExpiryTime] = @RefreshTokenExpiryTime
    WHERE [Id] = @UserId;
END
GO

IF OBJECT_ID(N'[dbo].[USP_User_GetByRefreshToken]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_User_GetByRefreshToken];
GO
CREATE PROCEDURE [dbo].[USP_User_GetByRefreshToken]
    @RefreshToken NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Username], [Email], [Password], [Role], [RefreshToken], [RefreshTokenExpiryTime]
    FROM [dbo].[Users]
    WHERE [RefreshToken] = @RefreshToken;
END
GO

-- =============================================
-- STORED PROCEDURES: PRODUCTS
-- =============================================

IF OBJECT_ID(N'[dbo].[USP_Product_GetAll]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Product_GetAll];
GO
CREATE PROCEDURE [dbo].[USP_Product_GetAll]
    @Search NVARCHAR(100) = NULL,
    @Category NVARCHAR(100) = NULL,
    @Brand NVARCHAR(100) = NULL,
    @MinPrice DECIMAL(18,2) = NULL,
    @MaxPrice DECIMAL(18,2) = NULL,
    @InStockOnly BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        [Id],
        [Name],
        [Description],
        [Price],
        [Stock],
        [Category],
        [Brand],
        [Rating],
        [ReviewCount],
        [Image],
        [Colors],
        [CreatedAt]
    FROM [dbo].[Products]
    WHERE 
        (@Search IS NULL OR [Name] LIKE '%' + @Search + '%' OR [Description] LIKE '%' + @Search + '%')
        AND (@Category IS NULL OR [Category] = @Category)
        AND (@Brand IS NULL OR [Brand] = @Brand)
        AND (@MinPrice IS NULL OR [Price] >= @MinPrice)
        AND (@MaxPrice IS NULL OR [Price] <= @MaxPrice)
        AND (@InStockOnly = 0 OR [Stock] > 0)
    ORDER BY [Id] ASC;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Product_GetById]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Product_GetById];
GO
CREATE PROCEDURE [dbo].[USP_Product_GetById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        [Id],
        [Name],
        [Description],
        [Price],
        [Stock],
        [Category],
        [Brand],
        [Rating],
        [ReviewCount],
        [Image],
        [Colors],
        [CreatedAt]
    FROM [dbo].[Products]
    WHERE [Id] = @Id;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Product_Insert]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Product_Insert];
GO
CREATE PROCEDURE [dbo].[USP_Product_Insert]
    @Name NVARCHAR(250),
    @Description NVARCHAR(MAX) = NULL,
    @Price DECIMAL(18,2),
    @Stock INT = 0,
    @Category NVARCHAR(100),
    @Brand NVARCHAR(100),
    @Rating DECIMAL(3,2) = 0.0,
    @ReviewCount INT = 0,
    @Image NVARCHAR(500) = NULL,
    @Colors NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Products] (
        [Name], [Description], [Price], [Stock], [Category], [Brand], [Rating], [ReviewCount], [Image], [Colors], [CreatedAt]
    )
    VALUES (
        @Name, @Description, @Price, @Stock, @Category, @Brand, @Rating, @ReviewCount, @Image, @Colors, GETDATE()
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewProductId;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Product_Update]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Product_Update];
GO
CREATE PROCEDURE [dbo].[USP_Product_Update]
    @Id INT,
    @Name NVARCHAR(250),
    @Description NVARCHAR(MAX) = NULL,
    @Price DECIMAL(18,2),
    @Stock INT,
    @Category NVARCHAR(100),
    @Brand NVARCHAR(100),
    @Rating DECIMAL(3,2) = 0.0,
    @ReviewCount INT = 0,
    @Image NVARCHAR(500) = NULL,
    @Colors NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Products]
    SET 
        [Name] = @Name,
        [Description] = @Description,
        [Price] = @Price,
        [Stock] = @Stock,
        [Category] = @Category,
        [Brand] = @Brand,
        [Rating] = @Rating,
        [ReviewCount] = @ReviewCount,
        [Image] = @Image,
        [Colors] = @Colors
    WHERE [Id] = @Id;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Product_Delete]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Product_Delete];
GO
CREATE PROCEDURE [dbo].[USP_Product_Delete]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Products]
    WHERE [Id] = @Id;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- STORED PROCEDURES: ORDERS
-- =============================================

IF OBJECT_ID(N'[dbo].[USP_Order_GetByUserId]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Order_GetByUserId];
GO
CREATE PROCEDURE [dbo].[USP_Order_GetByUserId]
    @UserId INT
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
        oi.[Id] AS OrderItemId,
        oi.[ProductId],
        oi.[ProductName],
        oi.[UnitPrice],
        oi.[Quantity],
        oi.[TotalPrice]
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[OrderItems] oi ON o.[Id] = oi.[OrderId]
    WHERE o.[UserId] = @UserId
    ORDER BY o.[OrderDate] DESC;
END
GO

IF OBJECT_ID(N'[dbo].[USP_Order_GetById]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[USP_Order_GetById];
GO
CREATE PROCEDURE [dbo].[USP_Order_GetById]
    @Id INT
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
        oi.[Id] AS OrderItemId,
        oi.[ProductId],
        oi.[ProductName],
        oi.[UnitPrice],
        oi.[Quantity],
        oi.[TotalPrice]
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[OrderItems] oi ON o.[Id] = oi.[OrderId]
    WHERE o.[Id] = @Id;
END
GO

-- =============================================
-- SEED DATA: PRODUCTS
-- =============================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Products])
BEGIN
    SET IDENTITY_INSERT [dbo].[Products] ON;

    INSERT INTO [dbo].[Products] ([Id], [Name], [Description], [Price], [Stock], [Category], [Brand], [Rating], [ReviewCount], [Image], [Colors], [CreatedAt])
    VALUES
    (1, 'Aura Wireless Headphones', 'Over-ear ANC headphones with 40-hour battery life.', 129.99, 24, 'Electronics', 'Nimbus', 4.6, 1284, 'https://picsum.photos/seed/headphones/640/640', 'Black,Silver', '2026-06-12'),
    (2, 'Pulse Smartwatch S3', 'AMOLED fitness watch with GPS and heart-rate tracking.', 189.00, 18, 'Electronics', 'Pulse', 4.4, 860, 'https://picsum.photos/seed/watch/640/640', 'Black,Blue', '2026-07-02'),
    (3, 'Canvas Everyday Sneakers', 'Lightweight canvas sneakers for all-day wear.', 59.50, 42, 'Fashion', 'Stride', 4.3, 2104, 'https://picsum.photos/seed/sneakers/640/640', 'White,Navy,Olive', '2026-05-20'),
    (4, 'Linen Summer Shirt', 'Breathable linen shirt with a relaxed fit.', 44.00, 0, 'Fashion', 'Harbor', 4.1, 332, 'https://picsum.photos/seed/shirt/640/640', 'Beige,Sky', '2026-04-18'),
    (5, 'Nordic Ceramic Mug Set', 'Set of 4 stoneware mugs for coffee and tea.', 32.99, 57, 'Home', 'Hearth', 4.8, 941, 'https://picsum.photos/seed/mugs/640/640', 'Cream,Sage', '2026-03-09'),
    (6, 'CloudDown Throw Pillow', 'Soft down-alternative pillow with removable cover.', 28.00, 33, 'Home', 'Hearth', 4.5, 412, 'https://picsum.photos/seed/pillow/640/640', 'Ivory,Blush', '2026-08-01'),
    (7, 'TrailFlex Running Shoes', 'Cushioned trail runners with grippy outsole.', 98.00, 21, 'Sports', 'Stride', 4.7, 1502, 'https://picsum.photos/seed/run/640/640', 'Red,Gray', '2026-07-22'),
    (8, 'Forge Resistance Bands', '5-level resistance set with door anchor.', 24.99, 80, 'Sports', 'Forge', 4.2, 678, 'https://picsum.photos/seed/bands/640/640', 'Black', '2026-02-14'),
    (9, 'Glow Vitamin C Serum', 'Brightening serum with hyaluronic acid.', 36.50, 46, 'Beauty', 'Lumina', 4.6, 2201, 'https://picsum.photos/seed/serum/640/640', '', '2026-06-30'),
    (10, 'Silk Sleep Mask', 'Mulberry silk mask for travel and rest.', 19.99, 12, 'Beauty', 'Lumina', 4.0, 190, 'https://picsum.photos/seed/mask/640/640', 'Blush,Black', '2026-01-11'),
    (11, 'Pocket Bluetooth Speaker', 'Waterproof mini speaker with 12-hour playtime.', 49.00, 29, 'Electronics', 'Nimbus', 4.3, 754, 'https://picsum.photos/seed/speaker/640/640', 'Teal,Black', '2026-08-11'),
    (12, 'Oak Serving Board', 'Hand-finished oak board for cheese and charcuterie.', 41.00, 15, 'Home', 'Harbor', 4.9, 288, 'https://picsum.photos/seed/board/640/640', 'Natural', '2026-05-05'),
    (13, 'City Pack Backpack', '15\" laptop backpack with USB pass-through.', 72.00, 8, 'Fashion', 'Nimbus', 4.4, 509, 'https://picsum.photos/seed/backpack/640/640', 'Charcoal,Olive', '2026-07-28'),
    (14, 'Yoga Flow Mat', 'Non-slip 6mm mat with alignment marks.', 39.00, 0, 'Sports', 'Forge', 4.5, 833, 'https://picsum.photos/seed/yoga/640/640', 'Purple,Mint', '2026-04-02'),
    (15, 'Ceramic Hair Dryer', 'Ionic dryer with concentrator and diffuser.', 64.99, 19, 'Beauty', 'Pulse', 4.2, 421, 'https://picsum.photos/seed/dryer/640/640', 'White', '2026-08-18'),
    (16, 'Studio USB Microphone', 'Cardioid condenser mic for streaming and calls.', 89.00, 11, 'Electronics', 'Pulse', 4.7, 990, 'https://picsum.photos/seed/mic/640/640', 'Black', '2026-03-27');

    SET IDENTITY_INSERT [dbo].[Products] OFF;
END
GO
