﻿﻿ 
create table idm_user
(
  id				int				not null auto_increment,
  user_name			VARCHAR(150)	not null,
  first_name		VARCHAR(100)	not null,
  last_name			VARCHAR(100)	not null,
  email				VARCHAR(100)	not null,
  user_type			VARCHAR(50)	not null,
  inactive_flag		TINYINT default(0) not null,
  inactive_date		date,
  created_on   		date			default(sysdate),
  created_by   		VARCHAR(100),
  updated_on   		date			default(sysdate),
  updated_by   		VARCHAR(100),
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

alter table student auto_increment=1000;


