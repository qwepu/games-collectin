using System.Text.RegularExpressions;

namespace GameCollection;

public class LoginForm : Form
{
    readonly TextBox tbLogin = new() { PlaceholderText = "Логин", MaxLength = 30 };
    readonly TextBox tbPass = new() { PlaceholderText = "Пароль", MaxLength = 64, UseSystemPasswordChar = true };
    readonly TextBox tbPass2 = new() { PlaceholderText = "Повторите пароль", MaxLength = 64, UseSystemPasswordChar = true };
    readonly CheckBox chkShow = new() { Text = "Показать пароль", AutoSize = true, ForeColor = Theme.Muted, Margin = new Padding(0, 4, 0, 4) };
    readonly Label lblTitle = new() { Text = "Вход", Font = Theme.H1, AutoSize = true };
    readonly Label lblSub = new() { Text = "Войдите, чтобы открыть свою коллекцию", ForeColor = Theme.Muted, AutoSize = true, Margin = new Padding(0, 0, 0, 14) };
    readonly Label lblError = new() { AutoSize = true, MaximumSize = new Size(330, 0), MinimumSize = new Size(330, 26), Margin = new Padding(0, 6, 0, 6) };
    readonly Label cap2 = Cap("Повтор пароля");
    readonly Button btnMain;
    readonly LinkLabel lnkMode = Link("Нет аккаунта? Зарегистрироваться");
    readonly LinkLabel lnkConn = Link("⚙  Настройки подключения к БД");
    bool reg;

    static Label Cap(string t) => new() { Text = t, AutoSize = true, ForeColor = Theme.Muted, Font = Theme.Small, Margin = new Padding(0, 10, 0, 2) };
    static LinkLabel Link(string t) => new()
    {
        Text = t, AutoSize = true, LinkColor = Theme.Accent, ActiveLinkColor = Theme.AccentHover, VisitedLinkColor = Theme.Accent,
        LinkBehavior = LinkBehavior.HoverUnderline, Margin = new Padding(0, 10, 0, 0), Font = Theme.Base
    };

    public LoginForm()
    {
        Text = "Коллекция игр"; Font = Theme.Base; BackColor = Theme.Surface;
        FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(820, 560);

        // Левая цветная панель
        var left = new Panel { Dock = DockStyle.Left, Width = 300, BackColor = Theme.Accent, Padding = new Padding(30, 80, 30, 30) };
        var lf = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        lf.Controls.Add(new Label { Text = "🎮", Font = new Font("Segoe UI Emoji", 44f), ForeColor = Color.White, AutoSize = true });
        lf.Controls.Add(new Label { Text = "Коллекция игр", Font = Theme.H1, ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 10, 0, 10) });
        lf.Controls.Add(new Label
        {
            Text = "Храните свои игры, ставьте оценки, отслеживайте прохождение и предлагайте новые игры в каталог.",
            ForeColor = Color.FromArgb(224, 231, 255), AutoSize = true, MaximumSize = new Size(240, 0)
        });
        left.Controls.Add(lf);

        // Правая панель с формой
        var right = new FlowLayoutPanel
        { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(70, 60, 30, 20) };
        foreach (var t in new[] { tbLogin, tbPass, tbPass2 }) { Theme.Input(t); t.Width = 330; }
        btnMain = Theme.MakeButton("Войти", Kind.Primary, Submit);
        btnMain.AutoSize = false; btnMain.Size = new Size(330, 44); btnMain.Margin = new Padding(0, 6, 0, 0);
        AcceptButton = btnMain;

        right.Controls.Add(lblTitle); right.Controls.Add(lblSub);
        right.Controls.Add(Cap("Логин")); right.Controls.Add(tbLogin);
        right.Controls.Add(Cap("Пароль")); right.Controls.Add(tbPass);
        right.Controls.Add(cap2); right.Controls.Add(tbPass2);
        right.Controls.Add(chkShow); right.Controls.Add(lblError); right.Controls.Add(btnMain);
        right.Controls.Add(lnkMode); right.Controls.Add(lnkConn);
        cap2.Visible = tbPass2.Visible = false;

        chkShow.CheckedChanged += (s, e) => tbPass.UseSystemPasswordChar = tbPass2.UseSystemPasswordChar = !chkShow.Checked;
        lnkMode.LinkClicked += (s, e) => SetMode(!reg);
        lnkConn.LinkClicked += (s, e) => { using var f = new ConnectionForm(); if (f.ShowDialog(this) == DialogResult.OK) ShowOk("Подключение сохранено."); };

        Controls.Add(right); Controls.Add(left);
    }

    void SetMode(bool registration)
    {
        reg = registration; lblError.Text = "";
        cap2.Visible = tbPass2.Visible = reg;
        lblTitle.Text = reg ? "Регистрация" : "Вход";
        lblSub.Text = reg ? "Создайте аккаунт — это займёт минуту" : "Войдите, чтобы открыть свою коллекцию";
        btnMain.Text = reg ? "Зарегистрироваться" : "Войти";
        lnkMode.Text = reg ? "Уже есть аккаунт? Войти" : "Нет аккаунта? Зарегистрироваться";
    }

    void ShowError(string m) { lblError.ForeColor = Theme.Danger; lblError.Text = "⚠  " + m; }
    void ShowOk(string m) { lblError.ForeColor = Theme.Success; lblError.Text = "✓  " + m; }

    void Submit()
    {
        lblError.Text = "";
        try { if (reg) Register(); else Login(); }
        catch (Exception ex) { ShowError(Db.Explain(ex)); }
    }

    bool ValidateInput()
    {
        var l = tbLogin.Text.Trim();
        if (l.Length == 0) { ShowError("Введите логин."); tbLogin.Focus(); return false; }
        if (!Regex.IsMatch(l, "^[A-Za-z0-9_]{3,30}$"))
        { ShowError("Логин: от 3 до 30 символов, только латинские буквы, цифры и «_»."); tbLogin.Focus(); return false; }
        if (tbPass.Text.Length == 0) { ShowError("Введите пароль."); tbPass.Focus(); return false; }
        return true;
    }

    void Login()
    {
        if (!ValidateInput()) return;
        var dt = Db.Query(
            @"select u.id, u.login, r.name as role from users u join roles r on r.id = u.role_id
              where u.login = @l and u.password_hash = crypt(@p, u.password_hash)",
            ("l", tbLogin.Text.Trim()), ("p", tbPass.Text));
        if (dt.Rows.Count == 0) { ShowError("Неверный логин или пароль."); tbPass.SelectAll(); tbPass.Focus(); return; }
        Session.UserId = Convert.ToInt32(dt.Rows[0]["id"]);
        Session.Login = (string)dt.Rows[0]["login"];
        Session.Role = (string)dt.Rows[0]["role"];
        DialogResult = DialogResult.OK;
    }

    void Register()
    {
        if (!ValidateInput()) return;
        if (tbPass.Text.Length < 6) { ShowError("Пароль должен содержать не менее 6 символов."); tbPass.Focus(); return; }
        if (tbPass.Text != tbPass2.Text) { ShowError("Пароли не совпадают."); tbPass2.Focus(); return; }
        if (Db.Scalar("select 1 from users where lower(login) = lower(@l)", ("l", tbLogin.Text.Trim())) != null)
        { ShowError("Пользователь с таким логином уже существует."); tbLogin.Focus(); return; }
        Db.Exec(@"insert into users(login, password_hash, role_id)
                  values (@l, crypt(@p, gen_salt('bf')), (select id from roles where name = 'user'))",
            ("l", tbLogin.Text.Trim()), ("p", tbPass.Text));
        tbPass2.Clear(); SetMode(false);
        ShowOk("Регистрация прошла успешно. Теперь войдите.");
    }
}
