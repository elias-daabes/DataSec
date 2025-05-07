DROP TABLE IF EXISTS Accounts;
CREATE TABLE Accounts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL
);

-- Insert 10 users; first one is admin
INSERT INTO Accounts (UserName, PasswordHash, Role) VALUES
('admin1', 'hashed_password_1', 'Admin'),
('user2', 'hashed_password_2', 'User'),
('user3', 'hashed_password_3', 'User'),
('user4', 'hashed_password_4', 'User'),
('user5', 'hashed_password_5', 'User'),
('user6', 'hashed_password_6', 'User'),
('user7', 'hashed_password_7', 'User'),
('user8', 'hashed_password_8', 'User'),
('user9', 'hashed_password_9', 'User'),
('user10', 'hashed_password_10', 'User');

DROP TABLE IF EXISTS CreditCards;
-- Create CreditCards table
CREATE TABLE CreditCards (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AccountId INT NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    IDNumber NVARCHAR(20) NOT NULL,
    CardNumber NVARCHAR(20) NOT NULL,
    ValidDate NVARCHAR(10) NOT NULL,
    CVC NVARCHAR(4) NOT NULL,
    FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
);



-- Insert credit card info (simple/fake data for testing)
INSERT INTO CreditCards (AccountId, FirstName, LastName, IDNumber, CardNumber, ValidDate, CVC) VALUES
(1, 'Israeli', 'Israeili', '123456789', '1234556789012345', '12/32', '123'),
(2, 'Lior', 'Benayoun', '234567890', '1111222233334444', '11/30', '321'),
(3, 'Dina', 'Katz', '345678901', '5555666677778888', '01/28', '111'),
(4, 'Gal', 'Levi', '456789012', '9999000011112222', '03/25', '222'),
(5, 'Ron', 'Shalev', '567890123', '3333444455556666', '10/29', '456'),
(6, 'Maya', 'Cohen', '678901234', '7777888899990000', '05/31', '789'),
(7, 'Yossi', 'Peretz', '789012345', '1212343456567878', '09/34', '654'),
(8, 'Naomi', 'Golan', '890123456', '1010202030304040', '08/30', '876'),
(9, 'Tamar', 'Rosen', '901234567', '9090808070706060', '07/27', '345'),
(10, 'Avi', 'Mor', '012345678', '5050606070708080', '06/33', '567');
