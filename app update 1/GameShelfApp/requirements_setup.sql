-- Запускай в pgAdmin, подключившись именно к MyDataBase.
-- Если таблицы уже существуют, сначала проверь их через SELECT; не удаляй базу.
CREATE TABLE IF NOT EXISTS roles (
 id SERIAL PRIMARY KEY,
 name VARCHAR(20) UNIQUE NOT NULL CHECK (name IN ('admin','manager','user'))
);
CREATE TABLE IF NOT EXISTS users (
 id SERIAL PRIMARY KEY,
 login VARCHAR(50) UNIQUE NOT NULL CHECK (length(trim(login)) >= 3),
 password_hash TEXT NOT NULL,
 role_id INT NOT NULL REFERENCES roles(id),
 created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE IF NOT EXISTS genres (id SERIAL PRIMARY KEY, name VARCHAR(40) UNIQUE NOT NULL);
CREATE TABLE IF NOT EXISTS platforms (id SERIAL PRIMARY KEY, name VARCHAR(50) UNIQUE NOT NULL);
CREATE TABLE IF NOT EXISTS games (
 id SERIAL PRIMARY KEY, title VARCHAR(100) NOT NULL CHECK(length(trim(title))>0),
 genre_id INT NOT NULL REFERENCES genres(id), platform_id INT NOT NULL REFERENCES platforms(id),
 release_year INT CHECK(release_year BETWEEN 1950 AND 2100), description TEXT NOT NULL DEFAULT ''
);
CREATE TABLE IF NOT EXISTS user_games (
 id SERIAL PRIMARY KEY, user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
 game_id INT NOT NULL REFERENCES games(id) ON DELETE CASCADE,
 status VARCHAR(20) NOT NULL DEFAULT 'planned' CHECK(status IN ('planned','playing','completed')),
 rating INT CHECK(rating BETWEEN 1 AND 10), added_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
 UNIQUE(user_id,game_id)
);
CREATE TABLE IF NOT EXISTS game_suggestions (
 id SERIAL PRIMARY KEY, user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
 title VARCHAR(100) NOT NULL CHECK(length(trim(title))>0), description TEXT NOT NULL DEFAULT '',
 status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK(status IN ('pending','approved','rejected')),
 reviewed_by INT REFERENCES users(id)
);
INSERT INTO roles(name) VALUES ('admin'),('manager'),('user') ON CONFLICT(name) DO NOTHING;
INSERT INTO genres(name) VALUES ('RPG'),('Action'),('Strategy'),('Adventure'),('Racing'),('Shooter'),('Simulation') ON CONFLICT(name) DO NOTHING;
INSERT INTO platforms(name) VALUES ('PC'),('PlayStation 5'),('Xbox Series X'),('Nintendo Switch') ON CONFLICT(name) DO NOTHING;
