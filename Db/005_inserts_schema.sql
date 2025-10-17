-- filepath: d:\Github\legacy-bridge-vb6-csharp\Db\005_seed.sql
-- Dados mínimos para desenvolvimento e demonstração
SET
    search_path TO app,
    public;

-- Clientes (usa ON CONFLICT para idempotência quando documento existe)
INSERT INTO
    customers (name, document, status)
VALUES
    ('João Silva', '11111111111', 'ATIVO'),
    ('Maria Oliveira', '22222222222', 'ATIVO'),
    ('Carlos Souza', '33333333333', 'INATIVO'),
    ('Mariana Costa', '44444444444', 'ATIVO'),
    ('José Santos', '55555555555', 'ATIVO') ON CONFLICT (document) DO NOTHING;

-- Produtos
INSERT INTO
    products (name, price, active)
VALUES
    ('Plano Básico', 49.90, TRUE),
    ('Plano Profissional', 129.90, TRUE),
    ('Plano Empresarial', 299.90, TRUE),
    ('Add-on Relatórios', 19.90, TRUE),
    ('Produto Descontinuado', 9.90, FALSE) ON CONFLICT (name) DO NOTHING;