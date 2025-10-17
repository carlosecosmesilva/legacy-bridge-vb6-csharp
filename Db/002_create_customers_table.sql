-- Tabela de clientes
SET
  search_path TO app,
  public;

CREATE TABLE IF NOT EXISTS customers (
  id BIGSERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  document VARCHAR(20),
  -- CPF/CNPJ/etc (opcional)
  status VARCHAR(20) NOT NULL DEFAULT 'ATIVO',
  -- ATIVO/INATIVO/BLOQUEADO
  created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Evita duplicidade por documento (quando informado)
CREATE UNIQUE INDEX IF NOT EXISTS ux_customers_document ON customers (document)
WHERE
  document IS NOT NULL;

-- Índice para busca textual por nome (com pg_trgm)
CREATE INDEX IF NOT EXISTS idx_customers_name_trgm ON customers USING GIN (name gin_trgm_ops);

-- Função de busca por clientes usando ILIKE (case-insensitive)
SET
  search_path TO app,
  public;

CREATE
OR REPLACE FUNCTION fn_busca_clientes(
  p_term TEXT,
  p_limit INT DEFAULT 50,
  p_offset INT DEFAULT 0
) RETURNS TABLE (
  id BIGINT,
  nome TEXT,
  documento VARCHAR,
  status VARCHAR
) LANGUAGE sql AS $ $
SELECT
  c.id,
  c.name AS nome,
  c.document AS documento,
  c.status AS status
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

$ $;