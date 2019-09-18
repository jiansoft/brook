CREATE DATABASE test;
USE test;
create table account
(
  id  bigint auto_increment  primary key,
  name  varchar(64) not null,
  email varchar(64) not null
);
insert into account (name,email)values('Ben Nuttall', 'Nuttall@mysql.com');
insert into account (name,email)values('許功蓋', 'RedHat@mysql.com');

delimiter #
CREATE PROCEDURE test.ReturnValue(param1 INT)
BEGIN
  SELECT param1;
END#