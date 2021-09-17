/*CREATE ROLE QuoteBot WITH ENCRYPTED PASSWORD '$DB_PASSWORD';

CREATE DATABASE QuoteBot WITH OWNER=quotebot ENCODING='UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;
*/

CREATE TABLE quotes (
                        id integer NOT NULL,
                        text text NOT NULL,
                        createdByTwitchId character varying (30) NOT NULL
);

ALTER TABLE quotes OWNER TO quotebot;

CREATE SEQUENCE quotes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE quotes_id_seq OWNER TO quotebot;

ALTER SEQUENCE quotes_id_seq OWNED BY quotes.id;

ALTER TABLE ONLY quotes ALTER COLUMN id SET DEFAULT nextval('quotes_id_seq'::regclass);

ALTER TABLE ONLY quotes
    ADD CONSTRAINT quotes_pkey PRIMARY KEY (id);
