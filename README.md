# SQL-contest

An application for hosting SQL contests.

![Screenshot](images/screenshot.png "Screenshot")

## Installation

1. Clone the repository
2. Configure the application by either creating appsettings.json file in diploma/diploma/ directory or by specifying respective environment variables prefixed with `Contester_` in compose.yaml. An example of appsettings.json file is appsettings.Example.json. Example of environment variables method:
```yaml
...
    environment:
      - Contester_Email__Host=smtp.gmail.com
      - Contester_Email__Port=465
...
```
3. Run `docker compose up sql_server_db` and `docker compose up oracle_db` and wait for the databases to start.
4. Build and run docker container with `docker compose up --build web`. Database connections will be established automatically.
