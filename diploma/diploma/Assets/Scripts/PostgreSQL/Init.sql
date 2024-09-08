drop database if exists sql_contest;
create database sql_contest;
drop owned by sql_contest_user;
drop user if exists sql_contest_user;
create user sql_contest_user with password 'Password123';
grant all privileges on database sql_contest to sql_contest_user;
alter database sql_contest owner to sql_contest_user;