-- Tạo cơ sở dữ liệu
CREATE DATABASE ShoeStore;
GO

USE ShoeStore;
GO

-- Bảng Users
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(15),
    Address NVARCHAR(255),
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Saler', 'Customer')),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Bảng Categories
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Bảng Products
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(18, 2) NOT NULL,
    Quantity INT NOT NULL,
    ImageURL NVARCHAR(255),
    CategoryID INT NULL FOREIGN KEY REFERENCES Categories(CategoryID),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Bảng Carts
CREATE TABLE Carts (
    CartID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Bảng CartItems
CREATE TABLE CartItems (
    CartItemID INT IDENTITY(1,1) PRIMARY KEY,
    CartID INT NOT NULL FOREIGN KEY REFERENCES Carts(CartID),
    ProductID INT NOT NULL FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- Bảng Orders
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Pending', 'Processing', 'Shipped', 'Completed', 'Cancelled')),
    ShippingAddress NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Bảng OrderDetails
CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderID),
    ProductID INT NOT NULL FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL
);

-- Bảng Shipments
CREATE TABLE Shipments (
    ShipmentID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderID),
    ShippedDate DATETIME NULL,
    EstimatedDeliveryDate DATETIME NOT NULL,
    DeliveryStatus NVARCHAR(20) NOT NULL CHECK (DeliveryStatus IN ('In Transit', 'Delivered', 'Returned', 'Failed')),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);


ALTER TABLE Products
ADD SellerID INT NULL;

UPDATE Products
SET SellerID = (SELECT TOP 1 UserID FROM Users WHERE Role = 'Saler'); -- Gán một người bán hợp lệ

ALTER TABLE Products
ADD CONSTRAINT FK_Products_Seller FOREIGN KEY (SellerID) REFERENCES Users(UserID);
