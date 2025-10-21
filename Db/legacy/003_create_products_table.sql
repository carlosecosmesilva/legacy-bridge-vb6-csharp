-- Tabela de produtos (lista exibida pelo VB6)

CREATE TABLE IF NOT EXISTS products (
    id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    price NUMERIC(12, 2) NOT NULL CHECK (price >= 0),
    active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Habilita a extensão pg_trgm (se ainda não estiver criada)
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Evitar duplicidade por nome de produto
CREATE UNIQUE INDEX IF NOT EXISTS ux_products_name ON products (name);

-- Auxilia listagens
CREATE INDEX IF NOT EXISTS ix_products_active_name ON products (active, name);