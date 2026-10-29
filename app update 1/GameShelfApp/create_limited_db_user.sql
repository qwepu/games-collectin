-- Выполнять в pgAdmin от postgres, подключившись к базе MyDataBase.
-- Сначала замени пароль. Это создаёт отдельного пользователя приложения без прав владельца БД.
DO $$ BEGIN
 IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname='gameshelf_app') THEN
  CREATE ROLE gameshelf_app LOGIN PASSWORD 'CHANGE_THIS_PASSWORD';
 END IF;
END $$;
GRANT CONNECT ON DATABASE "MyDataBase" TO gameshelf_app;
GRANT USAGE ON SCHEMA public TO gameshelf_app;
GRANT SELECT ON roles,genres,platforms,games TO gameshelf_app;
GRANT SELECT,INSERT ON users TO gameshelf_app;
GRANT SELECT,INSERT,UPDATE ON user_games TO gameshelf_app;
GRANT SELECT,INSERT,UPDATE ON game_suggestions TO gameshelf_app;
GRANT USAGE,SELECT ON ALL SEQUENCES IN SCHEMA public TO gameshelf_app;
-- В Db.cs можно заменить Username=postgres на Username=gameshelf_app и указать пароль выше.
-- Для операций создания/изменения/удаления каталога надо отдельно выдать нужные права staff-ролям;
-- не выдавай приложению SUPERUSER или CREATEDB.
