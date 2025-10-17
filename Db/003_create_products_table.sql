-- filepath: d:\Github\legacy-bridge-vb6-csharp\Db\004_create_products_table.sql
-- Tabela de produtos (lista exibida pelo VB6)
SET
    search_path TO app,
    public;

CREATE TABLE IF NOT EXISTS products (
    id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    price NUMERIC(12, 2) NOT NULL CHECK (price >= 0),
    active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Evitar duplicidade por nome de produto
CREATE UNIQUE INDEX IF NOT EXISTS ux_products_name ON products (name);

-- Auxilia listagens
CREATE INDEX IF NOT EXISTS ix_products_active_name ON products (active, name);