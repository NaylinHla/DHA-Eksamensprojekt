drop schema if exists weatherstation cascade;
create schema if not exists weatherstation;

CREATE TABLE weatherstation.devicelog
(
    deviceid  text                     NOT NULL,
    value     numeric                  NOT NULL,
    id        text                     NOT NULL,
    unit      text                     NOT NULL,
    timestamp timestamp with time zone NOT NULL,
    CONSTRAINT devicelog_pkey PRIMARY KEY (id)
);


CREATE TABLE weatherstation."user"
(
    id    text NOT NULL,
    email text NOT NULL,
    hash  text NOT NULL,
    salt  text NOT NULL,
    role  text NOT NULL,
    CONSTRAINT user_pkey PRIMARY KEY (id)
);

