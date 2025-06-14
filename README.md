# sql-contest

An application for hosting SQL contests.

![Screenshot](images/screenshot.png "Screenshot")

## Roadmap

- [x] Live scoreboard updates
- [x] Show result set when re-evaluating attempts
- [x] Migrate from SQLite to PostgreSQL for data storage
- [ ] Runtime-configurable connection strings setting
- [ ] Rate-limit the solution submission API call
- [ ] Default password setting in web.env for admin user
- [ ] Password-less authentication for most users
- [ ] Proper refresh token
- [ ] Optimisation of scoreboard calculation
- [ ] Remove/update attempts tab. The current one is causing too much network traffic
- [ ] Cache the results for ethalon solutions
- [ ] Multiple test-cases for each problem
- [ ] Show queue position when evaluating the attempt
- [ ] Live updates of submitted solutions on the problem page
- [ ] Language selection without logging in
- [ ] Update checker on startup
- [ ] Warnings in scoreboard if originality is low

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
