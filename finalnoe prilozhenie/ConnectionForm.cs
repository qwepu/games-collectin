using Npgsql;

namespace GameCollection;

public class ConnectionForm : BaseDialog
{
    readonly TextBox tbHost = new(), tbDb = new(), tbUser = new(), tbPass = new() { UseSystemPasswordChar = true };
    readonly NumericUpDown nudPort = new() { Minimum = 1, Maximum = 65535 };

    public ConnectionForm() : base("Настройки подключения к PostgreSQL", "Проверить и сохранить")
    {
        var b = new NpgsqlConnectionStringBuilder(Db.ConnStr);
        tbHost.Text = b.Host; nudPort.Value = b.Port; tbDb.Text = b.Database; tbUser.Text = b.Username; tbPass.Text = b.Password;
        AddRow("Сервер", tbHost);
        AddRow("Порт", nudPort, 100);
        AddRow("База данных", tbDb);
        AddRow("Пользователь", tbUser);
        AddRow("Пароль", tbPass);
    }

    protected override string Check()
    {
        if (tbHost.Text.Trim() == "" || tbDb.Text.Trim() == "" || tbUser.Text.Trim() == "")
            return "Заполните сервер, базу данных и пользователя.";
        var cs = new NpgsqlConnectionStringBuilder
        {
            Host = tbHost.Text.Trim(), Port = (int)nudPort.Value, Database = tbDb.Text.Trim(),
            Username = tbUser.Text.Trim(), Password = tbPass.Text, Timeout = 5
        }.ConnectionString;
        try { using var c = new NpgsqlConnection(cs); c.Open(); }
        catch (Exception ex) { return Db.Explain(ex); }
        Db.SaveConnection(cs);
        return null;
    }
}
