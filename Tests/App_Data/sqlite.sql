CREATE TABLE IF NOT EXISTS account (
  id INTEGER PRIMARY KEY  AUTOINCREMENT,
  name TEXT not null,
  email TEXT not null
);
insert into account (name,email)values('Ben Nuttall', 'Nuttall@sqlite.com');
insert into account (name,email)values('許功蓋', 'RedHat@sqlite.com');