using System.Data;
using Npgsql;
namespace GameShelfApp;

public class MainForm : Form
{
    readonly int userId;
    readonly string userName, role;
    readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
    readonly TextBox search = new() { Width = 180, PlaceholderText = "Название игры" };
    readonly ComboBox genre = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox platform = new() { Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly Label info = new() { AutoSize = true };

    public MainForm(int id, string name, string userRole)
    {
        userId = id; userName = name; role = userRole;
        Text = $"GameShelf — {name} ({role})"; Width = 1050; Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 85, Padding = new Padding(8), WrapContents = true };
        var find = new Button { Text = "Найти" }; var all = new Button { Text = "Все игры" };
        var collection = new Button { Text = "Моя коллекция" }; var add = new Button { Text = "В коллекцию" };
        top.Controls.AddRange(new Control[] { new Label { Text = "Поиск:" }, search,
            new Label { Text = "Жанр:" }, genre, new Label { Text = "Платформа:" }, platform,
            find, all, add, collection });
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 28, Padding = new Padding(8, 4, 0, 0) };
        bottom.Controls.Add(info);
        Controls.Add(grid); Controls.Add(bottom); Controls.Add(top);
        find.Click += (_, _) => LoadGames();
        all.Click += (_, _) => { search.Clear(); if (genre.Items.Count > 0) genre.SelectedIndex = 0; if (platform.Items.Count > 0) platform.SelectedIndex = 0; LoadGames(); };
        collection.Click += (_, _) => LoadCollection();
        add.Click += (_, _) => AddToCollection();
        Load += (_, _) => { LoadFilters(); LoadGames(); };
    }

    void LoadFilters()
    {
        using var c = Db.Connect();
        var gt = new DataTable(); using (var a = new NpgsqlDataAdapter("SELECT 'Все жанры' name UNION ALL SELECT name FROM genres ORDER BY name", c)) a.Fill(gt);
        genre.DataSource = gt; genre.DisplayMember = "name"; genre.ValueMember = "name";
        var pt = new DataTable(); using (var a = new NpgsqlDataAdapter("SELECT 'Все платформы' name UNION ALL SELECT name FROM platforms ORDER BY name", c)) a.Fill(pt);
        platform.DataSource = pt; platform.DisplayMember = "name"; platform.ValueMember = "name";
    }

    void LoadGames()
    {
        try {
            using var c = Db.Connect();
            using var q = new NpgsqlCommand(@"SELECT g.id,g.title,ge.name AS genre,p.name AS platform,g.release_year
                FROM games g LEFT JOIN genres ge ON ge.id=g.genre_id LEFT JOIN platforms p ON p.id=g.platform_id
                WHERE g.title ILIKE @s AND (@g='Все жанры' OR ge.name=@g)
                AND (@p='Все платформы' OR p.name=@p) ORDER BY g.title", c);
            q.Parameters.AddWithValue("s", "%" + search.Text.Trim() + "%");
            q.Parameters.AddWithValue("g", genre.SelectedValue?.ToString() ?? "Все жанры");
            q.Parameters.AddWithValue("p", platform.SelectedValue?.ToString() ?? "Все платформы");
            var t = new DataTable(); using var a = new NpgsqlDataAdapter(q); a.Fill(t); grid.DataSource = t;
            info.Text = $"Найдено игр: {t.Rows.Count} | Пользователь: {userName} ({role})";
        } catch (Exception ex) { MessageBox.Show("Не удалось загрузить игры: " + ex.Message); }
    }

    void LoadCollection()
    {
        try {
            using var c = Db.Connect();
            using var q = new NpgsqlCommand(@"SELECT g.id,g.title,ug.status,ug.rating
                FROM user_games ug JOIN games g ON g.id=ug.game_id
                WHERE ug.user_id=@id ORDER BY g.title", c);
            q.Parameters.AddWithValue("id", userId);
            var t = new DataTable(); using var a = new NpgsqlDataAdapter(q); a.Fill(t); grid.DataSource = t;
            info.Text = $"Игр в коллекции: {t.Rows.Count}";
        } catch (Exception ex) { MessageBox.Show("Не удалось загрузить коллекцию: " + ex.Message); }
    }

    void AddToCollection()
    {
        if (grid.CurrentRow == null || !grid.Columns.Contains("id")) { MessageBox.Show("Выберите игру в каталоге."); return; }
        int gameId = Convert.ToInt32(grid.CurrentRow.Cells["id"].Value);
        using var f = new Form { Text = "Добавить в коллекцию", Width = 300, Height = 190, StartPosition = FormStartPosition.CenterParent };
        var s = new ComboBox { Left = 20, Top = 20, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
        s.Items.AddRange(new object[] { "planned", "playing", "completed" }); s.SelectedIndex = 0;
        var r = new NumericUpDown { Left = 20, Top = 60, Width = 90, Minimum = 1, Maximum = 10, Value = 8 };
        var b = new Button { Text = "Сохранить", Left = 20, Top = 100, DialogResult = DialogResult.OK };
        f.Controls.AddRange(new Control[] { new Label { Text = "Статус:", Left = 20, Top = 3 }, s,
            new Label { Text = "Оценка 1–10:", Left = 120, Top = 63 }, r, b }); f.AcceptButton = b;
        if (f.ShowDialog(this) != DialogResult.OK) return;
        try {
            using var c = Db.Connect();
            using var q = new NpgsqlCommand(@"INSERT INTO user_games(user_id,game_id,status,rating)
                VALUES(@u,@g,@s,@r) ON CONFLICT(user_id,game_id)
                DO UPDATE SET status=EXCLUDED.status,rating=EXCLUDED.rating", c);
            q.Parameters.AddWithValue("u", userId); q.Parameters.AddWithValue("g", gameId);
            q.Parameters.AddWithValue("s", s.SelectedItem!.ToString()!); q.Parameters.AddWithValue("r", (int)r.Value);
            q.ExecuteNonQuery(); MessageBox.Show("Игра добавлена в коллекцию.");
        } catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
    }
}
