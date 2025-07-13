IF (OBJECT_ID('[dbo].[Employees]') IS NOT NULL)
    DROP TABLE [dbo].[Employees];

IF (OBJECT_ID('[dbo].[Banknotes]') IS NOT NULL)
    DROP TABLE [dbo].[Banknotes];

CREATE TABLE [Employees] (
    Id INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Salary INT NOT NULL,
    Manager INT NULL,
    EmploymentDate DATETIME NOT NULL,
);

INSERT INTO [Employees] (Id, Name, Salary, Manager, EmploymentDate) VALUES
(1, 'Gustavo Fring', 3000, NULL, '2022-10-10'),
(2, 'Jesse Pinkman', 2500, 1, '2023-01-01'),
(3, 'Walter White', 2600, 1, '2023-01-01');

CREATE TABLE [Banknotes] (
    Amount INT NOT NULL,
);

INSERT INTO [Banknotes] (Amount) VALUES
(100),
(50),
(20),
(10),
(5),
(1);