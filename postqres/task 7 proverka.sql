
SELECT g.id, g.title, ge.name AS genre, p.name AS platform, g.release_year
FROM games g
LEFT JOIN genres ge ON g.genre_id = ge.id
LEFT JOIN platforms p ON g.platform_id = p.id
ORDER BY g.title;


SELECT *
FROM games
WHERE title ILIKE '%witcher%';


SELECT g.*
FROM games g
JOIN genres ge ON g.genre_id = ge.id
JOIN platforms p ON g.platform_id = p.id
WHERE ge.name = 'RPG' AND p.name = 'PC';


INSERT INTO games (title, genre_id, platform_id, release_year, description)
VALUES ('New Game', 1, 1, 2025, 'Описание игры');


UPDATE games
SET title = 'Updated Game', release_year = 2026
WHERE id = 1;





INSERT INTO users (login, password_hash, role_id)
VALUES ('player2', 'HASH_PASSWORD',
        (SELECT id FROM roles WHERE name = 'user'));


SELECT id, login, password_hash, role_id
FROM users
WHERE login = 'player2';


INSERT INTO user_games (user_id, game_id, status, rating)
VALUES (1, 1, 'playing', 9);


SELECT g.title, ug.status, ug.rating
FROM user_games ug
JOIN games g ON ug.game_id = g.id
WHERE ug.user_id = 1
ORDER BY g.title;


UPDATE user_games
SET status = 'completed', rating = 10
WHERE user_id = 1 AND game_id = 1;


SELECT title, release_year
FROM games
ORDER BY release_year DESC;


SELECT u.id, u.login, r.name AS role
FROM users u
JOIN roles r ON u.role_id = r.id
ORDER BY u.login;


INSERT INTO game_suggestions (user_id, title, description)
VALUES (1, 'Suggested Game', 'Предложение пользователя');


UPDATE game_suggestions
SET status = 'approved', reviewed_by = 2
WHERE id = 1;
