# sql-contest

An application for hosting SQL contests.

![Screenshot](images/screenshot.png "Screenshot")

## Installation

1. Clone the repository.
2. Configure the application by adjusting the environment variables in `web.env` file.
```text
...
Contester_Email__Host=smtp.gmail.com
Contester_Email__Port=465
...
```
3. In diploma/ClientApp/src/environments/environment.prod.ts change basePath to match your server address.
4. Create certs directory in diploma/ and put `localhost.pfx` certificate file there.
5. Run `docker compose up sql_server_db`, `docker compose up oracle_db` and `docker compose up postgres_db`. Then wait for the DBMS to start. This step is needed to ensure that the databases will start before the application itself.
6. Build and run the container with `docker compose up web`. Database connections will be established automatically.
