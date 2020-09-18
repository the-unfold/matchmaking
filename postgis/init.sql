CREATE EXTENSION
IF NOT EXISTS postgis;
CREATE EXTENSION
IF NOT EXISTS postgis_topology;

CREATE TABLE users
(
  id SERIAL,
  name text NOT NULL,
  CONSTRAINT users_pkey PRIMARY KEY (id)
);

INSERT INTO users
  (name)
VALUES
  ('Test User'),
  ('John The Pastor'),
  ('Peter The Minister на Гагаринской'),
  ('Sara The Doctor на Красном проспекте'),
  ('Vasya The Student на Площади Ленина');

CREATE TABLE tags
(
  id SERIAL,
  name text NOT NULL,
  CONSTRAINT tags_pkey PRIMARY KEY (id)
);

INSERT INTO tags
  (name)
VALUES
  ('OhMyGod'),
  ('JavaScript'),
  ('ES6'),
  ('Abracadabra'),
  ('Haskell'),
  ('F#'),
  ('Scala'),
  ('Category theory'),
  ('Functional Programming'),
  ('Books'),
  ('Rock-n-roll'),
  ('Elm'),
  ('Elmish'),
  ('Typescript'),
  ('Music'),
  ('Drinking');

CREATE TABLE events
(
    id SERIAL,
    title text NOT NULL,
    description text NOT NULL,
    image_url text,
    location_geo text,
    location_url text,
    date_time TIMESTAMP,
    CONSTRAINT events_pkey PRIMARY KEY (id)
)

INSERT INTO events
  (title, description, location_url, date_time)
VALUES
  (
    'FP Specialty test event', 
    'Мы профессиональная команда, нубы идут лесом.Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку.', 
    'https://us02web.zoom.us/j/81359006423', 
    '2020-12-12 10:00'
  )