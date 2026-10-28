CREATE DOMAIN login_domain AS VARCHAR(50)

CHECK (LENGTH(VALUE) >= 3);





CREATE DOMAIN game_title_domain AS VARCHAR(100)

CHECK (LENGTH(VALUE) >= 1);





CREATE DOMAIN rating_domain AS INTEGER

CHECK (VALUE BETWEEN 1 AND 10);





CREATE DOMAIN game_status_domain AS VARCHAR(20)

CHECK (VALUE IN ('planned', 'playing', 'completed'));





CREATE DOMAIN user_role_domain AS VARCHAR(20)

CHECK (VALUE IN ('admin', 'manager', 'user'));