--
-- PostgreSQL database dump
--

\restrict pMp51dund5UPHGFp77KPvEaQ4tIpWxGzwKhUHouxovC6rQEgkMcp4i1tpbRwhZS

-- Dumped from database version 18.0
-- Dumped by pg_dump version 18.0

-- Started on 2025-10-19 12:15:34

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 2 (class 3079 OID 16572)
-- Name: pg_trgm; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;


--
-- TOC entry 5083 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION pg_trgm; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pg_trgm IS 'text similarity measurement and index searching based on trigrams';


--
-- TOC entry 255 (class 1255 OID 16674)
-- Name: search_customers_by_name(text, integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.search_customers_by_name(p_term text, p_limit integer DEFAULT 50, p_offset integer DEFAULT 0) RETURNS TABLE(id bigint, name text, document character varying, active boolean)
    LANGUAGE plpgsql
    AS $$ BEGIN RETURN QUERY
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


ALTER FUNCTION public.search_customers_by_name(p_term text, p_limit integer, p_offset integer) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 221 (class 1259 OID 16554)
-- Name: customers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.customers (
    id bigint NOT NULL,
    name text NOT NULL,
    document character varying(20),
    active boolean DEFAULT true NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.customers OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 16553)
-- Name: customers_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.customers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.customers_id_seq OWNER TO postgres;

--
-- TOC entry 5084 (class 0 OID 0)
-- Dependencies: 220
-- Name: customers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.customers_id_seq OWNED BY public.customers.id;


--
-- TOC entry 223 (class 1259 OID 16656)
-- Name: products; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.products (
    id bigint NOT NULL,
    name text NOT NULL,
    price numeric(12,2) NOT NULL,
    active boolean DEFAULT true NOT NULL,
    created_at timestamp without time zone DEFAULT now() NOT NULL,
    CONSTRAINT products_price_check CHECK ((price >= (0)::numeric))
);


ALTER TABLE public.products OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 16655)
-- Name: products_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.products_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.products_id_seq OWNER TO postgres;

--
-- TOC entry 5085 (class 0 OID 0)
-- Dependencies: 222
-- Name: products_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.products_id_seq OWNED BY public.products.id;


--
-- TOC entry 4910 (class 2604 OID 16557)
-- Name: customers id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers ALTER COLUMN id SET DEFAULT nextval('public.customers_id_seq'::regclass);


--
-- TOC entry 4913 (class 2604 OID 16659)
-- Name: products id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products ALTER COLUMN id SET DEFAULT nextval('public.products_id_seq'::regclass);


--
-- TOC entry 5075 (class 0 OID 16554)
-- Dependencies: 221
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.customers (id, name, document, active, created_at) FROM stdin;
1	João Silva	11111111111	t	2025-10-19 11:39:50.1716
2	Maria Oliveira	22222222222	t	2025-10-19 11:39:50.1716
3	Carlos Souza	33333333333	f	2025-10-19 11:39:50.1716
4	Mariana Costa	44444444444	t	2025-10-19 11:39:50.1716
5	José Santos	55555555555	t	2025-10-19 11:39:50.1716
\.


--
-- TOC entry 5077 (class 0 OID 16656)
-- Dependencies: 223
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.products (id, name, price, active, created_at) FROM stdin;
1	Plano Básico	49.90	t	2025-10-19 11:39:50.1716
2	Plano Profissional	129.90	t	2025-10-19 11:39:50.1716
3	Plano Empresarial	299.90	t	2025-10-19 11:39:50.1716
4	Add-on Relatórios	19.90	t	2025-10-19 11:39:50.1716
5	Produto Descontinuado	9.90	f	2025-10-19 11:39:50.1716
\.


--
-- TOC entry 5086 (class 0 OID 0)
-- Dependencies: 220
-- Name: customers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.customers_id_seq', 5, true);


--
-- TOC entry 5087 (class 0 OID 0)
-- Dependencies: 222
-- Name: products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.products_id_seq', 5, true);


--
-- TOC entry 4918 (class 2606 OID 16567)
-- Name: customers customers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT customers_pkey PRIMARY KEY (id);


--
-- TOC entry 4925 (class 2606 OID 16671)
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);


--
-- TOC entry 4921 (class 2606 OID 16678)
-- Name: customers uq_customers_document; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT uq_customers_document UNIQUE (document);


--
-- TOC entry 4919 (class 1259 OID 16654)
-- Name: idx_customers_name_trgm; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_customers_name_trgm ON public.customers USING gin (name public.gin_trgm_ops);


--
-- TOC entry 4923 (class 1259 OID 16673)
-- Name: ix_products_active_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_products_active_name ON public.products USING btree (active, name);


--
-- TOC entry 4922 (class 1259 OID 16653)
-- Name: ux_customers_document; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_customers_document ON public.customers USING btree (document) WHERE (document IS NOT NULL);


--
-- TOC entry 4926 (class 1259 OID 16672)
-- Name: ux_products_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX ux_products_name ON public.products USING btree (name);


-- Completed on 2025-10-19 12:15:34

--
-- PostgreSQL database dump complete
--

\unrestrict pMp51dund5UPHGFp77KPvEaQ4tIpWxGzwKhUHouxovC6rQEgkMcp4i1tpbRwhZS

