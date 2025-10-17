CREATE
OR REPLACE FUNCTION search_customers_by_name(term text) RETURNS TABLE(id int, name text, document text, email text) AS $ $ BEGIN RETURN QUERY
SELECT
  id,
  name,
  document,
  email
FROM
  customers
WHERE
  name ILIKE '%' || term || '%'
ORDER BY
  name;

END;

$ $ LANGUAGE plpgsql;