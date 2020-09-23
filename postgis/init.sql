CREATE EXTENSION
IF NOT EXISTS postgis;
CREATE EXTENSION
IF NOT EXISTS postgis_topology;

CREATE TABLE users
(
  id SERIAL,
  auth_id INTEGER,
  name text NOT NULL,
  CONSTRAINT PK_Users PRIMARY KEY (id)
);

INSERT INTO users
  (id, auth_id, name)
VALUES
  (1, 123, 'Test User'),
  (2, 345, 'John The Pastor'),
  (3, 456, 'Peter The Minister'),
  (4, 567, 'Sara The Doctor'),
  (5, 678, 'Vasya The Student');

SELECT setval(pg_get_serial_sequence('users', 'id'), 1000);

CREATE TABLE tags
(
  id SERIAL,
  name text NOT NULL,
  slug text NOT NULL,
  CONSTRAINT PK_Tags PRIMARY KEY (id)
);

INSERT INTO tags
  (id, name, slug)
VALUES
  (1, 'OhMyGod', 'ohmygod'),
  (2, 'JavaScript', 'javascript'),
  (3, 'ES6', 'es6'),
  (4, 'Abracadabra', 'abracadabra'),
  (5, 'Haskell', 'haskell'),
  (6, 'F#', 'f#'),
  (7, 'Scala', 'scala'),
  (8, 'Category theory', 'category_theory'),
  (9, 'Functional Programming', 'functional_programming'),
  (10, 'Books', 'books'),
  (11, 'Rock-n-roll', 'rock-n-roll'),
  (12, 'Elm', 'elm'),
  (13, 'Elmish', 'elmish'),
  (14, 'Typescript', 'typescript'),
  (15, 'Music', 'music'),
  (16, 'Drinking', 'drinking');

SELECT setval(pg_get_serial_sequence('tags', 'id'), 1000);

CREATE TABLE events
(
    id SERIAL,
    title text NOT NULL,
    description text NOT NULL,
    image_url text,
    location_geo text,
    location_url text,
    date_time TIMESTAMP,
    CONSTRAINT PK_Events PRIMARY KEY (id)
);

INSERT INTO events
  (id, title, description, location_url, date_time)
VALUES
  (
    1,
    'FP Specialty test event', 
    'Мы профессиональная команда, нубы идут лесом.Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку.', 
    'https://us02web.zoom.us/j/81359006423', 
    '2020-12-12 10:00'
  );

SELECT setval(pg_get_serial_sequence('events', 'id'), 1000);

CREATE TABLE event_tags
(
    id SERIAL,
    event_id integer NOT NULL,
    tag_id integer NOT NULL,
    CONSTRAINT PK_Event_tags PRIMARY KEY (id),
    CONSTRAINT FK_Events_Event_tags FOREIGN KEY (event_id)
        REFERENCES events (id)
        ON DELETE CASCADE,
    CONSTRAINT "FK_Tags_Event_tags" FOREIGN KEY (tag_id)
        REFERENCES tags (id)
        ON DELETE CASCADE
);

INSERT INTO event_tags
  (event_id, tag_id)
VALUES
  (1, 5),
  (1, 8),
  (1, 11);

CREATE TABLE event_organizers
(
  id SERIAL,
  event_id integer NOT NULL,
  user_id integer NOT NULL,
  CONSTRAINT PK_Event_organizers PRIMARY KEY (id),
  CONSTRAINT FK_Events_Event_organizers FOREIGN KEY (event_id)
      REFERENCES events (id)
      ON DELETE CASCADE,
  CONSTRAINT FK_Users_Event_organizers FOREIGN KEY (user_id)
      REFERENCES users (id)
      ON DELETE CASCADE
);

INSERT INTO event_organizers
  (event_id, user_id)
VALUES
  (1, 1);

CREATE TABLE event_attendees
(
  id SERIAL,
  event_id integer NOT NULL,
  user_id integer NOT NULL,
  CONSTRAINT PK_Event_attendees PRIMARY KEY (id),
  CONSTRAINT FK_Events_Event_attendees FOREIGN KEY (event_id)
      REFERENCES events (id)
      ON DELETE CASCADE,
  CONSTRAINT FK_Users_Event_attendees FOREIGN KEy (user_id)
      REFERENCES users (id)
      ON DELETE CASCADE
);

INSERT INTO event_attendees
  (event_id, user_id)
VALUES
  (1, 1),
  (1, 2),
  (1, 3),
  (1, 4);