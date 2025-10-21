
-- Função de busca de clientes por nome (ILIKE case-insensitive)
-- Requisitos: VB6-b e C#-b

CREATE
OR REPLACE FUNCTION search_customers_by_name(
  p_term TEXT,
  p_limit INT DEFAULT 50,
  p_offset INT DEFAULT 0
) RETURNS TABLE(
  id BIGINT,
  name TEXT,
  document VARCHAR,
  active BOOLEAN
) LANGUAGE plpgsql AS $$ BEGIN RETURN QUERY
SELECT
  c.id,
  c.name,
  c.document,
  c.active
FROM
  customers c
WHERE
  (
    p_term IS NULL
    OR p_term = ''
  )
  OR c.name ILIKE '%' || p_term || '%'
ORDER BY
  c.name
LIMIT
  p_limit OFFSET p_offset;

END;

$$;