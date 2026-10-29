-- Заполняет каталог 100 тестовыми играми. Названия Demo Game — учебные.
-- Запускай после requirements_setup.sql, если в genres/platforms уже есть значения.
INSERT INTO games(title,genre_id,platform_id,release_year,description)
SELECT 'Demo Game ' || lpad(n::text,3,'0'),
       (SELECT id FROM genres ORDER BY id LIMIT 1 OFFSET ((n-1) % (SELECT count(*) FROM genres))),
       (SELECT id FROM platforms ORDER BY id LIMIT 1 OFFSET ((n-1) % (SELECT count(*) FROM platforms))),
       2000 + (n % 25), 'Учебная тестовая запись №' || n
FROM generate_series(1,100) n
WHERE (SELECT count(*) FROM genres)>0 AND (SELECT count(*) FROM platforms)>0
ON CONFLICT DO NOTHING;
