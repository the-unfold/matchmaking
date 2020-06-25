CREATE EXTENSION
IF NOT EXISTS postgis;
CREATE EXTENSION
IF NOT EXISTS postgis_topology;

CREATE TABLE users
(
  id SERIAL,
  name text,
  location geography,
  radius numeric
);

INSERT INTO users
  (name, radius, location)
values
  ('John The Pastor на Заельцовской', 1000, ST_GeographyFromText('POINT(55.059474 82.912540)')),
  ('Peter The Minister на Гагаринской', 1000, ST_GeographyFromText('POINT(55.050504 82.914907)')),
  ('Sara The Doctor на Красном проспекте', 1000, ST_GeographyFromText('POINT(55.042302 82.917062)')),
  ('Vasya The Student на Площади Ленина', 1000, ST_GeographyFromText('POINT(55.031043 82.920185)'));
