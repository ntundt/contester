# Problem statement to be used with tests

This problem is created to be used with unit tests.
The schema has the following tables:

- `Employees`

| Id | Name          | Salary | Manager | EmploymentDate |
|----|---------------|--------|---------|----------------|
| 1  | Gustavo Fring | $3000  | NULL    | 2022-10-10     |
| 2  | Jesse Pinkman | $2500  | 1       | 2023-01-01     |
| 3  | Walter White  | $2600  | 1       | 2023-01-01     |

- `Banknotes`

| Amount |
|--------|
| 100    |
| 50     |
| 20     |
| 10     |
| 5      |
| 1      |

For the result set, please select employees with salary higher than $2700.