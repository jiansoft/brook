CREATE DATABASE test;
USE test;
create table account
(
  id    serial  not null,
  name  varchar not null,
  email varchar not null
);

insert into account (name,email)values('Ben Nuttall', 'eddie@postgresql.com');
insert into account (name,email)values('許功蓋', 'RedHat@postgresql.com');