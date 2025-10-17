CREATE TABLE customers (
  id serial PRIMARY KEY,
  name text NOT NULL,
  document varchar(20),
  email varchar(255),
  created_at timestamptz DEFAULT now()
);