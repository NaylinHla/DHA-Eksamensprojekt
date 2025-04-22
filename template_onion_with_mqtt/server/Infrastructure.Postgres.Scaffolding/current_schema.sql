-- This schema is generated based on the current DBContext. Please check the class Seeder to see.
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'weatherstation') THEN
        CREATE SCHEMA weatherstation;
    END IF;
END $EF$;


CREATE TABLE weatherstation.devicelog (
    id text NOT NULL,
    deviceid text NOT NULL,
    value numeric NOT NULL,
    unit text NOT NULL,
    timestamp timestamp with time zone NOT NULL,
    CONSTRAINT devicelog_pkey PRIMARY KEY (id)
);


CREATE TABLE weatherstation."user" (
    id text NOT NULL,
    email text NOT NULL,
    hash text NOT NULL,
    salt text NOT NULL,
    role text NOT NULL,
    CONSTRAINT user_pkey PRIMARY KEY (id)
);


