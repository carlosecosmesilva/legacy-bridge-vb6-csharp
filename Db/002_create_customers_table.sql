-- Tabela de clientes
CREATE TABLE IF NOT EXISTS customers (
    id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    document VARCHAR(20), -- CPF / CNPJ / etc (opcional),
    active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
); 

-- Habilita a extensão pg_trgm (se ainda não estiver criada)
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Evita duplicidade por documento (quando informado) 
CREATE UNIQUE INDEX IF NOT EXISTS ux_customers_document ON customers (document) WHERE document IS NOT NULL; 

-- Índice para busca textual por nome (com pg_trgm) 
CREATE INDEX IF NOT EXISTS idx_customers_name_trgm ON customers USING GIN (name gin_trgm_ops);