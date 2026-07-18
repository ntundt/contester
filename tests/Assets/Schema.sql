CREATE TABLE employees (
    id int NOT NULL PRIMARY KEY,
    name varchar(50) NOT NULL,
    salary int NOT NULL,
    manager int NULL,
    employment_date date NOT NULL
);

INSERT INTO employees (id, name, salary, manager, employment_date) VALUES
(1, 'Gustavo Fring', 3000, NULL, '2022-10-10'),
(2, 'Jesse Pinkman', 2500, 1, '2023-01-01'),
(3, 'Walter White', 2600, 1, '2023-01-01');

CREATE TABLE banknotes (
    amount INT NOT NULL
);

INSERT INTO banknotes (amount) VALUES
(100),
(50),
(20),
(10),
(5),
(1);
