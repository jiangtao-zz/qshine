--Create User table
#@ if MYSQL
	#@ var AUTO_INCREMENT = auto_increment;
#@ endif

#@ if MSACCESS
	#@ var AUTO_INCREMENT = AUTOINCREMENT(1000,1);
#@ endif

#@ if MYSQL
	#@var AUTO_INCREMENT = AUTO_INCREMENT;
#@ endif


create table idm_user
(
  id				int				not null #@{AUTO_INCREMENT},
  user_name			varchar(150)	not null,
  first_name		varchar(100)	not null,
  last_name			varchar(100)	not null,
  email				varchar(100)	not null,
  user_type			varchar(50)	not null,
  inactive_flag		tinyint 		default(0) not null,
  inactive_date		date,
  created_on   		date			default(sysdate),
  created_by   		varchar(100),
  updated_on   		date			default(sysdate),
  updated_by   		varchar(100),
  constraint idm_user_pk primary key (id),
  constraint idm_user_uk unique (id),
  constraint idm_user_ct1 check (inactive_flag in (0,1));
);

create index idm_user_ix1 on idm_user
(upper("first_name"));

create index idm_user_ix2 on idm_user
(upper("last_name"));

create index idm_user_ix3 on idm_user
(upper("email"));

create index idm_user_ix4 on idm_user
(created_on);

#@if MYSQL
	alter table student auto_increment=1000;
#@endif

#@if ORACLE
create sequence idm_user_seq
	minvalue 1
	start with 1000
	increment by 1
	cache 10;

create trigger idm_user_t before insert on idm_user
	referencing old as old new as new 
	for each row
begin
	if :new.id is null then
  		select idm_user_seq.nextval into :new.id from dual;
	end if;
end;
#@endif

