using Npgsql;
namespace GameShelfApp;

public class LoginForm : Form
{
    TextBox login = new() { Width = 220 };
    TextBox password = new() { Width = 220, UseSystemPasswordChar = true };
    Label message = new() { AutoSize = true, ForeColor = Color.Firebrick };

    public LoginForm()
    {
        Text = "GameShelf — вход"; Width = 370; Height = 230;
        StartPosition = FormStartPosition.CenterScreen;
        var p = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15), FlowDirection = FlowDirection.TopDown };
        p.Controls.Add(new Label { Text = "Логин:" }); p.Controls.Add(login);
        p.Controls.Add(new Label { Text = "Пароль:" }); p.Controls.Add(password);
        var buttons = new FlowLayoutPanel { AutoSize = true };
        var enter = new Button { Text = "Войти", AutoSize = true };
        var register = new Button { Text = "Регистрация", AutoSize = true };
        enter.Click += (_, _) => SignIn();
        register.Click += (_, _) => Register();
        buttons.Controls.Add(enter); buttons.Controls.Add(register);
        p.Controls.Add(buttons); p.Controls.Add(message); Controls.Add(p); AcceptButton = enter;
    }

    void SignIn()
    {
        try
        {
            using var c = Db.Connect();
            using var q = new NpgsqlCommand(@"SELECT u.id,u.login,u.password_hash,r.name
                FROM users u JOIN roles r ON r.id=u.role_id WHERE u.login=@login", c);
            q.Parameters.AddWithValue("login", login.Text.Trim());
            using var r = q.ExecuteReader();
            if (!r.Read()) { message.Text = "Неверный логин или пароль."; return; }
            int id = r.GetInt32(0); string name = r.GetString(1);
            string hash = r.GetString(2); string role = r.GetString(3); r.Close();
            bool ok;
            try { ok = BCrypt.Net.BCrypt.Verify(password.Text, hash); } catch { ok = false; }
            if (!ok) { message.Text = "Неверный логин или пароль. Создайте аккаунт через регистрацию."; return; }
            Hide();
            using var f = new MainForm(id, name, role);
            f.ShowDialog();
            Close();
        }
        catch (Exception ex) { MessageBox.Show("Ошибка подключения/входа:\n" + ex.Message + "\nПроверьте Db.cs и базу gameshelf."); }
    }

    void Register()
    {
        string name = login.Text.Trim(), pass = password.Text;
        if (name.Length < 3) { message.Text = "Логин — минимум 3 символа."; return; }
        if (pass.Length < 8) { message.Text = "Пароль — минимум 8 символов."; return; }
        try
        {
            using var c = Db.Connect();
            using var q = new NpgsqlCommand(@"INSERT INTO users(login,password_hash,role_id)
                VALUES(@login,@hash,(SELECT id FROM roles WHERE name='user'))", c);
            q.Parameters.AddWithValue("login", name);
            q.Parameters.AddWithValue("hash", BCrypt.Net.BCrypt.HashPassword(pass));
            q.ExecuteNonQuery();
            message.ForeColor = Color.DarkGreen; message.Text = "Аккаунт создан. Нажмите «Войти».";
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") { message.Text = "Такой логин уже существует."; }
        catch (Exception ex) { MessageBox.Show("Ошибка регистрации. Проверьте, что в roles есть роль user.\n" + ex.Message); }
    }
}
