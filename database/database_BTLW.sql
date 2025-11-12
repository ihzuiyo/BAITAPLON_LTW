
--CREATE DATABASE QLSV_TTTH

USE QLSV_TTTH
--GO

/* ===================================
   1. PHÂN QUYỀN NGƯỜI DÙNG
   =================================== */
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE -- Admin, Student
);

CREATE TABLE Permissions (
    PermissionId INT IDENTITY(1,1) PRIMARY KEY,
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);

CREATE TABLE RolePermissions (
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    PRIMARY KEY (RoleId, PermissionId),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber VARCHAR(20),
    Status NVARCHAR(20) NOT NULL DEFAULT N'Active',
    DateCreated DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

/* ===================================
   2. SINH VIÊN
   =================================== */
CREATE TABLE Students (
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE, -- liên kết tài khoản
    StudentCode NVARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE,
    Gender NVARCHAR(10),
    Address NVARCHAR(255),
    PhoneNumber VARCHAR(20),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Status NVARCHAR(20) NOT NULL DEFAULT N'Đang học', -- Đang học, Bảo lưu, Tốt nghiệp
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

/* ===================================
   3. KHÓA HỌC, LỚP HỌC, PHÒNG HỌC
   =================================== */
CREATE TABLE Courses (
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    CourseCode NVARCHAR(20) NOT NULL UNIQUE,
    CourseName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Duration NVARCHAR(50), -- '3 tháng', '45 giờ'
    TuitionFee DECIMAL(18, 2) NOT NULL DEFAULT 0
);

CREATE TABLE Rooms (
    RoomId INT IDENTITY(1,1) PRIMARY KEY,
    RoomName NVARCHAR(50) NOT NULL UNIQUE,
    Capacity INT NOT NULL -- Sức chứa tối đa sinh viên
);

CREATE TABLE Classes (
    ClassId INT IDENTITY(1,1) PRIMARY KEY,
    CourseId INT NOT NULL,
    RoomId INT NOT NULL,
    ClassCode NVARCHAR(20) NOT NULL UNIQUE,
    ClassName NVARCHAR(100) NOT NULL,
    MaxStudents INT,
    StartDate DATE,
    EndDate DATE,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE CASCADE,
    FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
);

CREATE TABLE ClassSchedules (
    ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
    ClassId INT NOT NULL,
    Weekday NVARCHAR(20) NOT NULL, -- 'Monday', 'Tuesday', ...
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId) ON DELETE CASCADE
);

/* ===================================
   4. GHI DANH
   =================================== */
CREATE TABLE Enrollments (
    EnrollmentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    ClassId INT NOT NULL,
    EnrollmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT N'Enrolled', -- Enrolled, Completed, Dropped
    CONSTRAINT UQ_Student_Class UNIQUE (StudentId, ClassId),
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    FOREIGN KEY (ClassId) REFERENCES Classes(ClassId)
);

/* ===================================
   5. QUẢN LÝ ĐIỂM SỐ
   =================================== */
CREATE TABLE ScoreTypes (
    ScoreTypeId INT IDENTITY(1,1) PRIMARY KEY,
    ScoreTypeName NVARCHAR(50) NOT NULL, -- Chuyên cần, Giữa kỳ, Cuối kỳ
    Weight DECIMAL(5, 2) -- Trọng số (0.3 = 30%)
);

CREATE TABLE Scores (
    ScoreId INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentId INT NOT NULL,
    ScoreTypeId INT NOT NULL,
    ScoreValue DECIMAL(5, 2) NOT NULL,
    CONSTRAINT CHK_ScoreValue CHECK (ScoreValue >= 0 AND ScoreValue <= 10),
    FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(EnrollmentId) ON DELETE CASCADE,
    FOREIGN KEY (ScoreTypeId) REFERENCES ScoreTypes(ScoreTypeId)
);

/* ===================================
   6. HỌC PHÍ & THANH TOÁN
   =================================== */
CREATE TABLE Tuitions (
    TuitionId INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentId INT NOT NULL,
    TotalFee DECIMAL(18, 2) NOT NULL,
    AmountPaid DECIMAL(18, 2) NOT NULL DEFAULT 0,
    DueDate DATE,
    Status NVARCHAR(20) NOT NULL DEFAULT N'Pending', -- Pending, Paid, Overdue
    FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(EnrollmentId) ON DELETE CASCADE
);

CREATE TABLE Receipts (
    ReceiptId INT IDENTITY(1,1) PRIMARY KEY,
    TuitionId INT NOT NULL,
    CashierId INT NOT NULL, -- người thu tiền (Admin)
    ReceiptCode NVARCHAR(50) NOT NULL UNIQUE,
    Amount DECIMAL(18, 2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Note NVARCHAR(500),
    FOREIGN KEY (TuitionId) REFERENCES Tuitions(TuitionId),
    FOREIGN KEY (CashierId) REFERENCES Users(UserId)
);

/* ===================================
   7. THÔNG BÁO TỪ ADMIN
   =================================== */
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    CreatorId INT NOT NULL, -- Admin
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CreatorId) REFERENCES Users(UserId)
);

CREATE TABLE NotificationRecipients (
    NotificationId INT NOT NULL,
    RecipientId INT NOT NULL, -- Student UserId
    IsRead BIT NOT NULL DEFAULT 0,
    PRIMARY KEY (NotificationId, RecipientId),
    FOREIGN KEY (NotificationId) REFERENCES Notifications(NotificationId) ON DELETE CASCADE,
    FOREIGN KEY (RecipientId) REFERENCES Users(UserId)
);

/* ===================================
   8. NHẬT KÝ HOẠT ĐỘNG
   =================================== */
CREATE TABLE ActionLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    Details NVARCHAR(MAX),
    LogDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO
