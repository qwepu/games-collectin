using Npgsql;
namespace GameShelfApp;
internal static class Db
{
    // ЗАМЕНИ пароль на пароль PostgreSQL из настроек Docker.
    public const string ConnectionString =
        "Host=localhost;Port=5432;Database=MyDataBase;Username=user1;Password=12345678";

    public static NpgsqlConnection Connect()
    {
        var c = new NpgsqlConnection(ConnectionString);
        c.Open();
        return c;
    }
}
