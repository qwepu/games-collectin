--
-- PostgreSQL database cluster dump
--

-- Started on 2026-10-28 15:04:41

\restrict 2TD1IrlNgJP58YDjpQeFtASzYRVLtgEA6qVKoIEfRpRWHa5KltTGIU9g9t2qI04

SET default_transaction_read_only = off;

SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;

--
-- Roles
--

CREATE ROLE user1;
ALTER ROLE user1 WITH SUPERUSER INHERIT CREATEROLE CREATEDB LOGIN REPLICATION BYPASSRLS PASSWORD 'SCRAM-SHA-256$4096:HaSG/OdQKXWQQ+PGLOzn2Q==$xCitQyBAhWvYRK4oJ0mG3FiyXvP8lZLUNNN/jShUVrY=:50h7iLgQjL/HxkeAjOLwwdL14RvwsuYwwtECwxmi4mk=';

--
-- User Configurations
--








\unrestrict 2TD1IrlNgJP58YDjpQeFtASzYRVLtgEA6qVKoIEfRpRWHa5KltTGIU9g9t2qI04

--
-- Databases
--

--
-- Database "template1" dump
--

\connect template1

--
-- PostgreSQL database dump
--

\restrict 3MNeXYYTPGfAbraB7Tq1Ek1oRrofRjP4uNeaYTxcL3C7u6uoDAja7dbHbMmOe4I

-- Dumped from database version 18.6 (Debian 18.6-1.pgdg13+2)
-- Dumped by pg_dump version 18.4

-- Started on 2026-10-28 15:04:41

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

-- Completed on 2026-10-28 15:04:41

--
-- PostgreSQL database dump complete
--

\unrestrict 3MNeXYYTPGfAbraB7Tq1Ek1oRrofRjP4uNeaYTxcL3C7u6uoDAja7dbHbMmOe4I

--
-- Database "MyDataBase" dump
--

--
-- PostgreSQL database dump
--

\restrict kbWZhbBQKta63eBO6cow1aQjvmdGmJqpMWcm7sreCSEzT1UD6vOmEhr2O5OQuSI

-- Dumped from database version 18.6 (Debian 18.6-1.pgdg13+2)
-- Dumped by pg_dump version 18.4

-- Started on 2026-10-28 15:04:41

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
-- TOC entry 3558 (class 1262 OID 16384)
-- Name: MyDataBase; Type: DATABASE; Schema: -; Owner: user1
--

CREATE DATABASE "MyDataBase" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'en_US.utf8';


ALTER DATABASE "MyDataBase" OWNER TO user1;

\unrestrict kbWZhbBQKta63eBO6cow1aQjvmdGmJqpMWcm7sreCSEzT1UD6vOmEhr2O5OQuSI
\connect "MyDataBase"
\restrict kbWZhbBQKta63eBO6cow1aQjvmdGmJqpMWcm7sreCSEzT1UD6vOmEhr2O5OQuSI

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
-- TOC entry 898 (class 1247 OID 16540)
-- Name: game_status_domain; Type: DOMAIN; Schema: public; Owner: user1
--

CREATE DOMAIN public.game_status_domain AS character varying(20)
	CONSTRAINT game_status_domain_check CHECK (((VALUE)::text = ANY ((ARRAY['planned'::character varying, 'playing'::character varying, 'completed'::character varying])::text[])));


ALTER DOMAIN public.game_status_domain OWNER TO user1;

--
-- TOC entry 890 (class 1247 OID 16534)
-- Name: game_title_domain; Type: DOMAIN; Schema: public; Owner: user1
--

CREATE DOMAIN public.game_title_domain AS character varying(100)
	CONSTRAINT game_title_domain_check CHECK ((length((VALUE)::text) >= 1));


ALTER DOMAIN public.game_title_domain OWNER TO user1;

--
-- TOC entry 886 (class 1247 OID 16531)
-- Name: login_domain; Type: DOMAIN; Schema: public; Owner: user1
--

CREATE DOMAIN public.login_domain AS character varying(50)
	CONSTRAINT login_domain_check CHECK ((length((VALUE)::text) >= 3));


ALTER DOMAIN public.login_domain OWNER TO user1;

--
-- TOC entry 894 (class 1247 OID 16537)
-- Name: rating_domain; Type: DOMAIN; Schema: public; Owner: user1
--

CREATE DOMAIN public.rating_domain AS integer
	CONSTRAINT rating_domain_check CHECK (((VALUE >= 1) AND (VALUE <= 10)));


ALTER DOMAIN public.rating_domain OWNER TO user1;

--
-- TOC entry 902 (class 1247 OID 16543)
-- Name: user_role_domain; Type: DOMAIN; Schema: public; Owner: user1
--

CREATE DOMAIN public.user_role_domain AS character varying(20)
	CONSTRAINT user_role_domain_check CHECK (((VALUE)::text = ANY ((ARRAY['admin'::character varying, 'manager'::character varying, 'user'::character varying])::text[])));


ALTER DOMAIN public.user_role_domain OWNER TO user1;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 232 (class 1259 OID 16499)
-- Name: game_suggestions; Type: TABLE; Schema: public; Owner: user1
--

CREATE TABLE public.game_suggestions (
    id integer NOT NULL,
    user_id integer NOT NULL,
    title character varying(100) NOT NULL,
    description text DEFAULT ''::text,
    status character varying(20) DEFAULT 'pending'::character varying NOT NULL,
    reviewed_by integer,
    CONSTRAINT game_suggestions_status_check CHECK (((status)::text = ANY ((ARRAY['pending'::character varying, 'approved'::character varying, 'rejected'::character varying])::text[])))
);


ALTER TABLE public.game_suggestions OWNER TO user1;

--
-- TOC entry 231 (class 1259 OID 16498)
-- Name: game_suggestions_id_seq; Type: SEQUENCE; Schema: public; Owner: user1
--

CREATE SEQUENCE public.game_suggestions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.game_suggestions_id_seq OWNER TO user1;

--
-- TOC entry 3559 (class 0 OID 0)
-- Dependencies: 231
-- Name: game_suggestions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: user1
--

ALTER SEQUENCE public.game_suggestions_id_seq OWNED BY public.game_suggestions.id;


--
-- TOC entry 228 (class 1259 OID 16446)
-- Name: games; Type: TABLE; Schema: public; Owner: user1
--

CREATE TABLE public.games (
    id integer NOT NULL,
    title character varying(100) NOT NULL,
    genre_id integer NOT NULL,
    platform_id integer NOT NULL,
    release_year integer,
    description text DEFAULT ''::text,
    CONSTRAINT games_release_year_check CHECK (((release_year >= 1950) AND (release_year <= 2100)))
);


ALTER TABLE public.games OWNER TO user1;

--
-- TOC entry 227 (class 1259 OID 16445)
-- Name: games_id_seq; Type: SEQUENCE; Schema: public; Owner: user1
--

CREATE SEQUENCE public.games_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.games_id_seq OWNER TO user1;

--
-- TOC entry 3560 (class 0 OID 0)
-- Dependencies: 227
-- Name: games_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: user1
--

ALTER SEQUENCE public.games_id_seq OWNED BY public.games.id;


--
-- TOC entry 224 (class 1259 OID 16424)
-- Name: genres; Type: TABLE; Schema: public; Owner: user1
--

CREATE TABLE public.genres (
    id integer NOT NULL,
    name character varying(40) NOT NULL
);


ALTER TABLE public.genres OWNER TO user1;

--
-- TOC entry 223 (class 1259 OID 16423)
-- Name: genres_id_seq; Type: SEQUENCE; Schema: public; Owner: user1
--

CREATE SEQUENCE public.genres_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.genres_id_seq OWNER TO user1;

--
-- TOC entry 3561 (class 0 OID 0)
-- Dependencies: 223
-- Name: genres_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: user1
--

ALTER SEQUENCE public.genres_id_seq OWNED BY public.genres.id;


--
-- TOC entry 226 (class 1259 OID 16435)
-- Name: platforms; Type: TABLE; Schema: public; Owner: user1
--

CREATE TABLE public.platforms (
    id integer NOT NULL,
    name character varying(50) NOT NULL
);


ALTER TABLE public.platforms OWNER TO user1;

--
-- TOC entry 225 (class 1259 OID 16434)
-- Name: platforms_id_seq; Type: SEQUENCE; Schema: public; Owner: user1
--

CREATE SEQUENCE public.platforms_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.platforms_id_seq OWNER TO user1;

--
-- TOC entry 3562 (class 0 OID 0)
-- Dependencies: 225
-- Name: platforms_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: user1
--

ALTER SEQUENCE public.platforms_id_seq OWNED BY public.platforms.id;


--
-- TOC entry 220 (class 1259 OID 16390)
-- Name: roles; Type: TABLE; Schema: public; Owner: user1
--

CREATE TABLE public.roles (
    id integer NOT NULL,
    name character varying(20) NOT NULL,
    CONSTRAINT roles_name_check CHECK (((name)::text = ANY ((ARRAY['admin'::character varying, 'manager'::character varying, 'user'::character varying])::text[])))
);


ALTER TABLE public.roles OWNER TO user1;

--
-- TOC entry 219 (class 1259 OID 16389)
-- Name: roles_id_seq; Type: SEQUENCE; Schema: public; Owner: user1
--

CREATE SEQUENCE public.roles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.roles_id_seq OWNER TO user1;

--
-- TOC entry 3563 (class 0 OID 0)
-- Dependencies: 219
-- Name: roles_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: user1
--

ALTER SEQUENCE public.roles_id_seq OWNED BY public.roles.id;


--
-- TOC entry 230 (class 1259 OID 16471)
-- Name: user_games; Type: TABLE; Schema: public; Owner: user1
--

CREATE TABLE public.user_games (
    id integer NOT NULL,
    user_id integer NOT NULL,
    game_id integer NOT NULL,
    status character varying(20) DEFAULT 'planned'::character varying NOT NULL,
    rating integer,
    added_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT user_games_rating_check CHECK (((rating >= 1) AND (rating <= 10))),
    CONSTRAINT user_games_status_check CHECK (((status)::text = ANY ((ARRAY['planned'::character varying, 'playing'::character varying, 'completed'::character varying])::text[])))
);


ALTER TABLE public.user_games OWNER TO user1;

--
-- TOC entry 229 (class 1259 OID 16470)
-- Name: user_games_id_seq; Type: SEQUENCE; Schema: public; Owner: user1
--

CREATE SEQUENCE public.user_games_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.user_games_id_seq OWNER TO user1;

--
-- TOC entry 3564 (class 0 OID 0)
-- Dependencies: 229
-- Name: user_games_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: user1
--

ALTER SEQUENCE public.user_games_id_seq OWNED BY public.user_games.id;


--
-- TOC entry 222 (class 1259 OID 16402)
-- Name: users; Type: TABLE; Schema: public; Owner: user1
--

CREATE TABLE public.users (
    id integer NOT NULL,
    login character varying(50) NOT NULL,
    password_hash text NOT NULL,
    role_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.users OWNER TO user1;

--
-- TOC entry 221 (class 1259 OID 16401)
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: user1
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO user1;

--
-- TOC entry 3565 (class 0 OID 0)
-- Dependencies: 221
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: user1
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- TOC entry 3349 (class 2604 OID 16502)
-- Name: game_suggestions id; Type: DEFAULT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.game_suggestions ALTER COLUMN id SET DEFAULT nextval('public.game_suggestions_id_seq'::regclass);


--
-- TOC entry 3344 (class 2604 OID 16449)
-- Name: games id; Type: DEFAULT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.games ALTER COLUMN id SET DEFAULT nextval('public.games_id_seq'::regclass);


--
-- TOC entry 3342 (class 2604 OID 16427)
-- Name: genres id; Type: DEFAULT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.genres ALTER COLUMN id SET DEFAULT nextval('public.genres_id_seq'::regclass);


--
-- TOC entry 3343 (class 2604 OID 16438)
-- Name: platforms id; Type: DEFAULT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.platforms ALTER COLUMN id SET DEFAULT nextval('public.platforms_id_seq'::regclass);


--
-- TOC entry 3339 (class 2604 OID 16393)
-- Name: roles id; Type: DEFAULT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.roles ALTER COLUMN id SET DEFAULT nextval('public.roles_id_seq'::regclass);


--
-- TOC entry 3346 (class 2604 OID 16474)
-- Name: user_games id; Type: DEFAULT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.user_games ALTER COLUMN id SET DEFAULT nextval('public.user_games_id_seq'::regclass);


--
-- TOC entry 3340 (class 2604 OID 16405)
-- Name: users id; Type: DEFAULT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- TOC entry 3552 (class 0 OID 16499)
-- Dependencies: 232
-- Data for Name: game_suggestions; Type: TABLE DATA; Schema: public; Owner: user1
--

COPY public.game_suggestions (id, user_id, title, description, status, reviewed_by) FROM stdin;
1	1	Suggested Game	Предложение пользователя	approved	2
\.


--
-- TOC entry 3548 (class 0 OID 16446)
-- Dependencies: 228
-- Data for Name: games; Type: TABLE DATA; Schema: public; Owner: user1
--

COPY public.games (id, title, genre_id, platform_id, release_year, description) FROM stdin;
2	Persona 5 Royal	1	2	2019	Японская ролевая игра
3	Final Fantasy VII Remake	1	2	2020	Ремейк японской RPG
4	Monster Hunter World	1	1	2018	Охота на фантастических существ
5	Cyberpunk 2077	1	1	2020	Ролевая игра в футуристическом городе
6	Fallout 4	1	1	2015	Постапокалиптическая ролевая игра
7	Mass Effect Legendary Edition	1	1	2021	Космическая ролевая игра
8	Undertale	1	1	2015	Необычная ролевая игра
9	Dragon Age Inquisition	1	1	2014	Фэнтезийная ролевая игра
10	Elden Ring	1	1	2022	Ролевая игра в открытом мире
11	Path of Exile	1	1	2013	Бесплатная ролевая игра
12	Baldurs Gate 3	1	1	2023	Пошаговая ролевая игра
13	Diablo IV	1	1	2023	Ролевая игра с видом сверху
14	Kingdom Come Deliverance	1	1	2018	Историческая ролевая игра
15	Disco Elysium	1	1	2019	Ролевая игра о расследовании
16	The Witcher 3: Wild Hunt	1	1	2015	Фэнтезийная ролевая игра
17	Skyrim	1	1	2011	Фэнтезийная ролевая игра
18	Metroid Dread	2	4	2021	Фантастический экшен
19	God of War	2	2	2018	Приключенческий боевик
20	Ghost of Tsushima	2	2	2020	Приключение самурая
21	God of War Ragnarok	2	2	2022	Продолжение истории Кратоса
22	Dead Space	2	1	2023	Космический хоррор
23	Dishonored	2	1	2012	Стелс-боевик
24	Batman Arkham Knight	2	1	2015	Боевик о Бэтмене
25	Dying Light	2	1	2015	Выживание в городе с заражёнными
26	Dead Cells	2	1	2018	Динамичный рогалик
27	Marvels Spider-Man Remastered	2	1	2022	Супергеройский боевик
28	Resident Evil 4	2	1	2023	Приключенческий хоррор
29	Sekiro Shadows Die Twice	2	1	2019	Сложный приключенческий боевик
30	Grand Theft Auto V	2	1	2013	Криминальный открытый мир
31	Among Us	2	1	2018	Многопользовательская игра на дедукцию
32	Hades	2	1	2020	Рогалик с мифологическим сюжетом
33	Assassins Creed Origins	2	1	2017	Приключение в Древнем Египте
34	Red Dead Redemption 2	2	1	2018	Приключение на Диком Западе
35	Prey	2	1	2017	Научно-фантастический боевик
36	Cuphead	2	1	2017	Сложный мультяшный платформер
37	Risk of Rain 2	2	1	2020	Кооперативный рогалик
38	Assassins Creed Odyssey	2	1	2018	Приключение в Древней Греции
39	Resident Evil 2	2	1	2019	Хоррор на выживание
40	Control	2	1	2019	Сверхъестественный боевик
41	Pikmin 4	3	4	2023	Стратегическое приключение
42	Total War Warhammer III	3	1	2022	Фэнтезийная стратегия
43	Into the Breach	3	1	2018	Пошаговая тактическая стратегия
44	Crusader Kings III	3	1	2020	Историческая стратегия
45	XCOM 2	3	1	2016	Тактическая стратегия
46	Age of Empires II Definitive Edition	3	1	2019	Историческая стратегия
47	Company of Heroes 2	3	1	2013	Стратегия о Второй мировой войне
48	Civilization VI	3	1	2016	Пошаговая глобальная стратегия
49	Command and Conquer Remastered	3	1	2020	Классическая стратегия
50	StarCraft II	3	1	2010	Научно-фантастическая стратегия
51	Stellaris	3	1	2016	Космическая стратегия
52	The Legend of Zelda Tears of the Kingdom	4	4	2023	Приключение в мире Хайрула
53	Luigis Mansion 3	4	4	2019	Приключение в призрачном отеле
54	The Legend of Zelda Breath of the Wild	4	4	2017	Приключение в открытом мире
55	Super Mario Odyssey	4	4	2017	Приключение Марио
56	Ori and the Will of the Wisps	4	3	2020	Красочный приключенческий платформер
57	It Takes Two	4	2	2021	Кооперативное приключение
58	Uncharted 4	4	2	2016	Приключение охотника за сокровищами
59	Sea of Thieves	4	1	2018	Морские приключения
60	Firewatch	4	1	2016	Атмосферное сюжетное приключение
61	Outer Wilds	4	1	2019	Исследование космоса
62	Horizon Zero Dawn	4	1	2017	Приключение в мире машин
63	Valheim	4	1	2021	Выживание в мире викингов
64	No Mans Sky	4	1	2016	Исследование бескрайнего космоса
65	Rise of the Tomb Raider	4	1	2015	Продолжение приключений Лары
66	Celeste	4	1	2018	Платформер о восхождении на гору
67	Detroit Become Human	4	1	2018	Интерактивная история
68	Tomb Raider	4	1	2013	Приключение Лары Крофт
69	The Forest	4	1	2018	Выживание на таинственном острове
70	Life Is Strange	4	1	2015	Сюжетное приключение
1	Updated Game	1	4	2026	Японская ролевая игра
71	Hollow Knight	4	1	2017	Приключение в подземном королевстве
72	Portal 2	4	1	2011	Головоломка с порталами
73	Terraria	4	1	2011	Исследование и строительство
74	Alan Wake 2	4	1	2023	Психологический триллер
75	Subnautica	4	1	2018	Исследование подводного мира
76	Mario Kart 8 Deluxe	5	4	2017	Аркадные гонки с персонажами Nintendo
77	Forza Motorsport	5	3	2023	Автомобильный симулятор
78	Forza Horizon 5	5	3	2021	Гонки в открытом мире
79	Gran Turismo 7	5	2	2022	Автомобильный симулятор
80	Need for Speed Unbound	5	1	2022	Аркадные уличные гонки
81	F1 23	5	1	2023	Симулятор Формулы-1
82	Trackmania	5	1	2020	Аркадные трассы и гонки
83	Need for Speed Heat	5	1	2019	Уличные гонки
84	Dirt Rally 2.0	5	1	2019	Раллийные гонки
85	The Crew 2	5	1	2018	Гонки на разных видах транспорта
86	Splatoon 3	6	4	2022	Командный шутер с краской
87	Halo Infinite	6	3	2021	Фантастический шутер
88	Apex Legends	6	1	2019	Королевская битва
89	Doom Eternal	6	1	2020	Динамичный фантастический шутер
90	Valorant	6	1	2020	Тактический командный шутер
91	Half Life 2	6	1	2004	Научно-фантастический шутер
92	Left 4 Dead 2	6	1	2009	Кооперативный шутер о зомби
93	Call of Duty Modern Warfare II	6	1	2022	Военный шутер
94	Call of Duty Black Ops Cold War	6	1	2020	Шутер о холодной войне
95	Battlefield 1	6	1	2016	Шутер о Первой мировой войне
96	Counter-Strike 2	6	1	2023	Командный соревновательный шутер
97	Titanfall 2	6	1	2016	Фантастический шутер с роботами
98	Overwatch 2	6	1	2022	Командный геройский шутер
99	Animal Crossing New Horizons	7	4	2020	Симулятор жизни на острове
100	House Flipper	7	1	2018	Ремонт и продажа домов
101	Microsoft Flight Simulator	7	1	2020	Симулятор полётов
102	Cities Skylines II	7	1	2023	Современный градостроительный симулятор
103	Cities Skylines	7	1	2015	Симулятор строительства города
104	Planet Zoo	7	1	2019	Симулятор зоопарка
105	Stardew Valley	7	1	2016	Фермерский симулятор
106	Planet Coaster	7	1	2016	Симулятор парка развлечений
107	The Sims 4	7	1	2014	Симулятор жизни
108	Farming Simulator 22	7	1	2021	Симулятор фермы
109	Minecraft	7	1	2011	Песочница с открытым миром
110	Euro Truck Simulator 2	7	1	2012	Симулятор дальнобойщика
112	New Game	1	1	2025	Описание игры
\.


--
-- TOC entry 3544 (class 0 OID 16424)
-- Dependencies: 224
-- Data for Name: genres; Type: TABLE DATA; Schema: public; Owner: user1
--

COPY public.genres (id, name) FROM stdin;
1	RPG
2	Action
3	Strategy
4	Adventure
5	Racing
6	Shooter
7	Simulation
\.


--
-- TOC entry 3546 (class 0 OID 16435)
-- Dependencies: 226
-- Data for Name: platforms; Type: TABLE DATA; Schema: public; Owner: user1
--

COPY public.platforms (id, name) FROM stdin;
1	PC
2	PlayStation 5
3	Xbox Series X
4	Nintendo Switch
\.


--
-- TOC entry 3540 (class 0 OID 16390)
-- Dependencies: 220
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: user1
--

COPY public.roles (id, name) FROM stdin;
1	admin
2	manager
3	user
\.


--
-- TOC entry 3550 (class 0 OID 16471)
-- Dependencies: 230
-- Data for Name: user_games; Type: TABLE DATA; Schema: public; Owner: user1
--

COPY public.user_games (id, user_id, game_id, status, rating, added_at) FROM stdin;
1	3	1	playing	9	2026-10-27 10:38:38.692045
2	1	1	completed	10	2026-10-28 08:51:49.155695
\.


--
-- TOC entry 3542 (class 0 OID 16402)
-- Dependencies: 222
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: user1
--

COPY public.users (id, login, password_hash, role_id, created_at) FROM stdin;
1	admin	test_hash_admin	1	2026-10-27 10:37:01.562143
2	manager	test_hash_manager	2	2026-10-27 10:37:01.562143
3	player1	test_hash_player1	3	2026-10-27 10:37:01.562143
4	player2	HASH_PASSWORD	3	2026-10-28 08:51:49.155695
5	stepa	$2a$11$HdykrT8DUrKbmyJkb2pP7OKLZercgD6hWWSa2eRdCm2jA1KekxZ3S	1	2026-10-28 09:43:00.046939
\.


--
-- TOC entry 3566 (class 0 OID 0)
-- Dependencies: 231
-- Name: game_suggestions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: user1
--

SELECT pg_catalog.setval('public.game_suggestions_id_seq', 1, true);


--
-- TOC entry 3567 (class 0 OID 0)
-- Dependencies: 227
-- Name: games_id_seq; Type: SEQUENCE SET; Schema: public; Owner: user1
--

SELECT pg_catalog.setval('public.games_id_seq', 112, true);


--
-- TOC entry 3568 (class 0 OID 0)
-- Dependencies: 223
-- Name: genres_id_seq; Type: SEQUENCE SET; Schema: public; Owner: user1
--

SELECT pg_catalog.setval('public.genres_id_seq', 7, true);


--
-- TOC entry 3569 (class 0 OID 0)
-- Dependencies: 225
-- Name: platforms_id_seq; Type: SEQUENCE SET; Schema: public; Owner: user1
--

SELECT pg_catalog.setval('public.platforms_id_seq', 4, true);


--
-- TOC entry 3570 (class 0 OID 0)
-- Dependencies: 219
-- Name: roles_id_seq; Type: SEQUENCE SET; Schema: public; Owner: user1
--

SELECT pg_catalog.setval('public.roles_id_seq', 3, true);


--
-- TOC entry 3571 (class 0 OID 0)
-- Dependencies: 229
-- Name: user_games_id_seq; Type: SEQUENCE SET; Schema: public; Owner: user1
--

SELECT pg_catalog.setval('public.user_games_id_seq', 2, true);


--
-- TOC entry 3572 (class 0 OID 0)
-- Dependencies: 221
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: user1
--

SELECT pg_catalog.setval('public.users_id_seq', 5, true);


--
-- TOC entry 3384 (class 2606 OID 16513)
-- Name: game_suggestions game_suggestions_pkey; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.game_suggestions
    ADD CONSTRAINT game_suggestions_pkey PRIMARY KEY (id);


--
-- TOC entry 3374 (class 2606 OID 16459)
-- Name: games games_pkey; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.games
    ADD CONSTRAINT games_pkey PRIMARY KEY (id);


--
-- TOC entry 3366 (class 2606 OID 16433)
-- Name: genres genres_name_key; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.genres
    ADD CONSTRAINT genres_name_key UNIQUE (name);


--
-- TOC entry 3368 (class 2606 OID 16431)
-- Name: genres genres_pkey; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.genres
    ADD CONSTRAINT genres_pkey PRIMARY KEY (id);


--
-- TOC entry 3370 (class 2606 OID 16444)
-- Name: platforms platforms_name_key; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.platforms
    ADD CONSTRAINT platforms_name_key UNIQUE (name);


--
-- TOC entry 3372 (class 2606 OID 16442)
-- Name: platforms platforms_pkey; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.platforms
    ADD CONSTRAINT platforms_pkey PRIMARY KEY (id);


--
-- TOC entry 3358 (class 2606 OID 16400)
-- Name: roles roles_name_key; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_name_key UNIQUE (name);


--
-- TOC entry 3360 (class 2606 OID 16398)
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (id);


--
-- TOC entry 3380 (class 2606 OID 16485)
-- Name: user_games user_games_pkey; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.user_games
    ADD CONSTRAINT user_games_pkey PRIMARY KEY (id);


--
-- TOC entry 3382 (class 2606 OID 16487)
-- Name: user_games user_games_user_id_game_id_key; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.user_games
    ADD CONSTRAINT user_games_user_id_game_id_key UNIQUE (user_id, game_id);


--
-- TOC entry 3362 (class 2606 OID 16417)
-- Name: users users_login_key; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_login_key UNIQUE (login);


--
-- TOC entry 3364 (class 2606 OID 16415)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 3375 (class 1259 OID 16525)
-- Name: idx_games_genre; Type: INDEX; Schema: public; Owner: user1
--

CREATE INDEX idx_games_genre ON public.games USING btree (genre_id);


--
-- TOC entry 3376 (class 1259 OID 16526)
-- Name: idx_games_platform; Type: INDEX; Schema: public; Owner: user1
--

CREATE INDEX idx_games_platform ON public.games USING btree (platform_id);


--
-- TOC entry 3377 (class 1259 OID 16524)
-- Name: idx_games_title; Type: INDEX; Schema: public; Owner: user1
--

CREATE INDEX idx_games_title ON public.games USING btree (title);


--
-- TOC entry 3378 (class 1259 OID 16527)
-- Name: idx_user_games_user; Type: INDEX; Schema: public; Owner: user1
--

CREATE INDEX idx_user_games_user ON public.user_games USING btree (user_id);


--
-- TOC entry 3390 (class 2606 OID 16519)
-- Name: game_suggestions game_suggestions_reviewed_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.game_suggestions
    ADD CONSTRAINT game_suggestions_reviewed_by_fkey FOREIGN KEY (reviewed_by) REFERENCES public.users(id);


--
-- TOC entry 3391 (class 2606 OID 16514)
-- Name: game_suggestions game_suggestions_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.game_suggestions
    ADD CONSTRAINT game_suggestions_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- TOC entry 3386 (class 2606 OID 16460)
-- Name: games games_genre_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.games
    ADD CONSTRAINT games_genre_id_fkey FOREIGN KEY (genre_id) REFERENCES public.genres(id);


--
-- TOC entry 3387 (class 2606 OID 16465)
-- Name: games games_platform_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.games
    ADD CONSTRAINT games_platform_id_fkey FOREIGN KEY (platform_id) REFERENCES public.platforms(id);


--
-- TOC entry 3388 (class 2606 OID 16493)
-- Name: user_games user_games_game_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.user_games
    ADD CONSTRAINT user_games_game_id_fkey FOREIGN KEY (game_id) REFERENCES public.games(id);


--
-- TOC entry 3389 (class 2606 OID 16488)
-- Name: user_games user_games_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.user_games
    ADD CONSTRAINT user_games_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- TOC entry 3385 (class 2606 OID 16418)
-- Name: users users_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: user1
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id);


-- Completed on 2026-10-28 15:04:41

--
-- PostgreSQL database dump complete
--

\unrestrict kbWZhbBQKta63eBO6cow1aQjvmdGmJqpMWcm7sreCSEzT1UD6vOmEhr2O5OQuSI

--
-- Database "postgres" dump
--

\connect postgres

--
-- PostgreSQL database dump
--

\restrict TYuaJaP1hyE07MzVgenv6H2NXzELVwJDFJzzAkcUpzHlOMvWFADv6xfDxzhD6tw

-- Dumped from database version 18.6 (Debian 18.6-1.pgdg13+2)
-- Dumped by pg_dump version 18.4

-- Started on 2026-10-28 15:04:41

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

-- Completed on 2026-10-28 15:04:41

--
-- PostgreSQL database dump complete
--

\unrestrict TYuaJaP1hyE07MzVgenv6H2NXzELVwJDFJzzAkcUpzHlOMvWFADv6xfDxzhD6tw

-- Completed on 2026-10-28 15:04:41

--
-- PostgreSQL database cluster dump complete
--

