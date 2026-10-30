using System.Data;
using System.Text.Json;
using Npgsql;

namespace GameCollection;

public static class Db
{
    const string DefaultConn = "Host=localhost;Port=5432;Database=MyDataBase;Username=user1;Password=";
    static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    public static string ConnStr { get; private set; } = Load();

    static string Normalize(string cs)
    {
        try { return new NpgsqlConnectionStringBuilder(cs) { Timeout = 5 }.ConnectionString; }
        catch { return cs; }
    }

    static string Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                using var d = JsonDocument.Parse(File.ReadAllText(ConfigPath));
                if (d.RootElement.TryGetProperty("ConnectionString", out var v) && !string.IsNullOrWhiteSpace(v.GetString()))
                    return Normalize(v.GetString());
            }
        }
        catch { }
        return Normalize(DefaultConn);
    }

    public static void SaveConnection(string cs)
    {
        ConnStr = Normalize(cs);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(new { ConnectionString = cs },
            new JsonSerializerOptions { WriteIndented = true }));
    }

    // ВСЕ значения передаются только параметрами -> защита от SQL-инъекций
    static NpgsqlCommand Cmd(NpgsqlConnection c, string sql, (string n, object v)[] p)
    {
        var cmd = new NpgsqlCommand(sql, c);
        foreach (var (n, v) in p) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        return cmd;
    }

    public static DataTable Query(string sql, params (string n, object v)[] p)
    {
        using var c = new NpgsqlConnection(ConnStr); c.Open();
        using var cmd = Cmd(c, sql, p);
        var dt = new DataTable(); dt.Load(cmd.ExecuteReader()); return dt;
    }

    public static int Exec(string sql, params (string n, object v)[] p)
    {
        using var c = new NpgsqlConnection(ConnStr); c.Open();
        using var cmd = Cmd(c, sql, p);
        return cmd.ExecuteNonQuery();
    }

    public static object Scalar(string sql, params (string n, object v)[] p)
    {
        using var c = new NpgsqlConnection(ConnStr); c.Open();
        using var cmd = Cmd(c, sql, p);
        var r = cmd.ExecuteScalar();
        return r is DBNull ? null : r;
    }

    // Понятные сообщения об ошибках
    public static string Explain(Exception ex)
    {
        if (ex is PostgresException pg)
            return pg.SqlState switch
            {
                "23505" => "Такая запись уже существует (значение должно быть уникальным).",
                "23502" => "Не заполнено обязательное поле: " + pg.ColumnName + ".",
                "23503" => "Операция невозможна: запись связана с другими данными.",
                "23514" => "Недопустимое значение: нарушено ограничение «" + pg.ConstraintName + "».",
                "22001" => "Введённое значение слишком длинное.",
                "28P01" or "28000" => "PostgreSQL отклонил логин или пароль. Откройте «Настройки подключения».",
                "3D000" => "База данных не найдена. Проверьте имя БД в «Настройках подключения».",
                "42P01" => "В базе нет нужной таблицы. Выполните sql/01_setup.sql в pgAdmin. (" + pg.MessageText + ")",
                "42703" => "Структура таблицы отличается от ожидаемой приложением: " + pg.MessageText,
                "42501" => "У пользователя БД нет прав на эту операцию: " + pg.MessageText,
                _ => "Ошибка базы данных: " + pg.MessageText
            };
        if (ex is NpgsqlException || ex is System.Net.Sockets.SocketException || ex.InnerException is System.Net.Sockets.SocketException)
            return "Нет соединения с сервером PostgreSQL. Запущен ли контейнер в Docker Desktop? Проверьте «Настройки подключения».";
        return ex.Message;
    }
}

public static class Session
{
    public static int UserId;
    public static string Login;
    public static string Role;      // admin / manager / user
    public static bool IsAdmin => Role == "admin";
    public static bool IsManager => Role == "manager" || Role == "admin";
    public static string RoleRu => Role switch { "admin" => "Администратор", "manager" => "Менеджер", _ => "Пользователь" };
}
