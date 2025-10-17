-- Cria schema lógico do projeto e extensões necessárias
CREATE SCHEMA IF NOT EXISTS app;

-- Extensão para acelerar buscas com ILIKE
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Define o search_path padrão para este script
SET
    search_path TO app,
    public;