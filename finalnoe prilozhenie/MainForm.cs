namespace GameCollection;

public class MainForm : Form
{
    class NavItem { public Button Btn; public string Title; public Func<Control> Make; public Control Inst; }

    readonly List<NavItem> items = new();
    readonly Panel body = new() { Dock = DockStyle.Fill, BackColor = Theme.Bg };
    readonly Label lblTitle = new() { Dock = DockStyle.Top, Height = 64, Font = Theme.H1, ForeColor = Theme.Ink, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(0, 6, 0, 0) };
    readonly Label lblToast = new() { Dock = DockStyle.Bottom, Height = 30, Font = Theme.Base, TextAlign = ContentAlignment.MiddleLeft };
    readonly System.Windows.Forms.Timer toastTimer = new() { Interval = 3500 };
    readonly FlowLayoutPanel nav = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 8, 0, 0) };

    public MainForm()
    {
        Text = $"Коллекция игр — {Session.Login} ({Session.RoleRu})";
        Font = Theme.Base; BackColor = Theme.Bg; StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1320, 780); MinimumSize = new Size(1080, 640);

        // ----- Боковая панель -----
        var side = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.Sidebar };
        var brand = new Label
        { Text = "🎮  Game Box", Dock = DockStyle.Top, Height = 84, ForeColor = Color.White, Font = Theme.H2, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(22, 0, 0, 0) };

        var user = new Panel { Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(18, 12, 18, 16) };
        var logout = Theme.MakeButton("Выйти из аккаунта", Kind.Secondary, () => { DialogResult = DialogResult.Retry; Close(); });
        logout.AutoSize = false; logout.Size = new Size(214, 38); logout.Dock = DockStyle.Bottom;
        logout.BackColor = Theme.SidebarHover; logout.ForeColor = Color.White;
        logout.FlatAppearance.BorderSize = 0; logout.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 65, 81);
        var roleColor = Session.Role == "admin" ? Color.FromArgb(252, 165, 165) : Session.Role == "manager" ? Color.FromArgb(253, 224, 71) : Color.FromArgb(134, 239, 172);
        var lblRole = new Label { Text = Session.RoleRu, Dock = DockStyle.Top, Height = 24, ForeColor = roleColor, Font = Theme.Small };
        var lblUser = new Label { Text = "👤  " + Session.Login, Dock = DockStyle.Top, Height = 28, ForeColor = Color.White, Font = Theme.Bold };
        user.Controls.Add(logout); user.Controls.Add(lblRole); user.Controls.Add(lblUser);

        side.Controls.Add(nav); side.Controls.Add(user); side.Controls.Add(brand);

        // ----- Разделы -----
        AddNav("📚   Каталог игр", "Каталог игр", () => new GamesPanel());
        AddNav("🎯   Моя коллекция", "Моя коллекция", () => new MyGamesPanel());
        AddNav("💡   Предложения", Session.IsManager ? "Предложения игр" : "Мои предложения", () => new SuggestionsPanel());
        if (Session.IsAdmin) AddNav("👥   Пользователи", "Пользователи", () => new UsersPanel());

        // ----- Область содержимого -----
        var main = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg, Padding = new Padding(28, 12, 28, 8) };
        main.Controls.Add(body); main.Controls.Add(lblToast); main.Controls.Add(lblTitle);
        Controls.Add(main); Controls.Add(side);

        // Всплывающие уведомления об успехе
        Ui.ToastHandler = m => { lblToast.ForeColor = Theme.Success; lblToast.Text = "✓  " + m; toastTimer.Stop(); toastTimer.Start(); };
        toastTimer.Tick += (s, e) => { toastTimer.Stop(); lblToast.Text = ""; };
        FormClosed += (s, e) => Ui.ToastHandler = null;

        Shown += (s, e) => Navigate(items[0]);
    }

    void AddNav(string text, string title, Func<Control> make)
    {
        var b = new Button
        {
            Text = text, Width = 250, Height = 48, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(22, 0, 0, 0), Font = Theme.Base, ForeColor = Color.FromArgb(209, 213, 219),
            BackColor = Theme.Sidebar, Cursor = Cursors.Hand, Margin = Padding.Empty, UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Theme.SidebarHover;
        var item = new NavItem { Btn = b, Title = title, Make = make };
        b.Click += (s, e) => Navigate(item);
        items.Add(item); nav.Controls.Add(b);
    }

    void Navigate(NavItem n)
    {
        foreach (var x in items)
        {
            bool on = x == n;
            x.Btn.BackColor = on ? Theme.Accent : Theme.Sidebar;
            x.Btn.ForeColor = on ? Color.White : Color.FromArgb(209, 213, 219);
            x.Btn.FlatAppearance.MouseOverBackColor = on ? Theme.Accent : Theme.SidebarHover;
        }
        lblTitle.Text = n.Title;
        bool fresh = false;
        if (n.Inst == null)
        {
            try { n.Inst = n.Make(); fresh = true; }
            catch (Exception ex) { Ui.Error(Db.Explain(ex)); return; }
            n.Inst.Dock = DockStyle.Fill;
        }
        body.Controls.Clear();
        body.Controls.Add(n.Inst);
        if (!fresh) (n.Inst as IReload)?.Reload();
    }
}
