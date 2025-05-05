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
