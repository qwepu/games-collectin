using System.Data;

namespace GameCollection;

public class GamesPanel : ListPanel
{
    readonly TextBox tbSearch = new() { PlaceholderText = "Название или описание…" };
    readonly ComboBox cbGenre = Ui.Lookup("select id, name from genres order by name", true);
    readonly ComboBox cbPlat = Ui.Lookup("select id, name from platforms order by name", true);
    readonly NumericUpDown nudFrom = new() { Minimum = 0, Maximum = 2100 };
    readonly NumericUpDown nudTo = new() { Minimum = 0, Maximum = 2100 };
    readonly ComboBox cbSort1, cbSort2;
    readonly CheckBox chkDesc = DescBox();

    static readonly Dictionary<string, string> Sorts = new()
    {
        ["Id"] = "g.id", ["Название"] = "g.title", ["Жанр"] = "ge.name", ["Платформа"] = "p.name",
        ["Год"] = "g.release_year", ["Ср. оценка"] = "r.avgr"
    };

    public GamesPanel()
    {
        cbSort1 = SortCombo(Sorts.Keys, false, 1);
        cbSort2 = SortCombo(Sorts.Keys, true, 0);
        AddFilter("Поиск", tbSearch, 230);
        AddFilter("Жанр", cbGenre);
        AddFilter("Платформа", cbPlat);
        AddFilter("Год от (0 — любой)", nudFrom, 110);
        AddFilter("Год до (0 — любой)", nudTo, 110);
        AddFilter("Сортировать по", cbSort1, 140);
        AddFilter("затем по", cbSort2, 140);
        AddFilter(" ", chkDesc);

        AddAction("＋  В мою коллекцию", Kind.Primary, AddToCollection);
        if (Session.IsManager)
        {
            AddAction("Добавить игру", Kind.Secondary, Add);
            AddAction("Изменить", Kind.Secondary, Edit);
            AddAction("Удалить", Kind.Danger, Delete);
            AddAction("Excel: каталог", Kind.Secondary, () => Ui.Export(Query(), "Каталог"));
            AddAction("Excel: по жанрам", Kind.Secondary, GenreReport);
        }
        Start();
    }

    protected override void ResetFilters()
    {
        tbSearch.Clear(); cbGenre.SelectedIndex = 0; cbPlat.SelectedIndex = 0; nudFrom.Value = 0; nudTo.Value = 0;
        cbSort1.SelectedIndex = 1; cbSort2.SelectedIndex = 0; chkDesc.Checked = false;
    }

    protected override void OnOpen() { if (Session.IsManager) Edit(); }

    protected override DataTable Query()
    {
        string order = BuildOrder(Sorts, cbSort1, cbSort2, chkDesc, "g.id");
        return Db.Query($@"
            select g.id as ""Id"", g.title as ""Название"", ge.name as ""Жанр"", p.name as ""Платформа"",
                   g.release_year as ""Год"", r.avgr as ""Ср. оценка"", g.description as ""Описание""
            from games g
            join genres ge on ge.id = g.genre_id
            join platforms p on p.id = g.platform_id
            left join lateral (select round(avg(ug.rating), 1) as avgr from user_games ug where ug.game_id = g.id) r on true
            where (@t = '' or g.title ilike '%' || @t || '%' or coalesce(g.description, '') ilike '%' || @t || '%')
              and (@g = 0 or g.genre_id = @g)
              and (@p = 0 or g.platform_id = @p)
              and (@y1 = 0 or g.release_year >= @y1)
              and (@y2 = 0 or g.release_year <= @y2)
            order by {order}",
            ("t", Ui.Like(tbSearch.Text.Trim())), ("g", Ui.Sel(cbGenre)), ("p", Ui.Sel(cbPlat)),
            ("y1", (int)nudFrom.Value), ("y2", (int)nudTo.Value));
    }

    void Add()
    {
        using var f = new GameForm("Новая игра", checkDuplicates: true);
        if (f.ShowDialog() != DialogResult.OK) return;
        Db.Exec("insert into games(title, genre_id, platform_id, release_year, description) values (@t,@g,@p,@y,@d)",
            ("t", f.GameTitle), ("g", f.GenreId), ("p", f.PlatformId), ("y", f.Year), ("d", f.Description));
        Ui.Toast("Игра добавлена в каталог");
        Reload();
    }

    void Edit()
    {
        var id = Pick("игру"); if (id == null) return;
        var r = Db.Query("select * from games where id = @i", ("i", id.Value));
        if (r.Rows.Count == 0) { Ui.Error("Запись не найдена (возможно, её уже удалили)."); Reload(); return; }
        var row = r.Rows[0];
        using var f = new GameForm("Изменение игры", id, (string)row["title"], Convert.ToInt32(row["genre_id"]),
            Convert.ToInt32(row["platform_id"]), Ui.IntOrNull(row["release_year"]), row["description"] as string ?? "", true);
        if (f.ShowDialog() != DialogResult.OK) return;
        Db.Exec("update games set title=@t, genre_id=@g, platform_id=@p, release_year=@y, description=@d where id=@i",
            ("t", f.GameTitle), ("g", f.GenreId), ("p", f.PlatformId), ("y", f.Year), ("d", f.Description), ("i", id.Value));
        Ui.Toast("Изменения сохранены");
        Reload();
    }

    void Delete()
    {
        var id = Pick("игру"); if (id == null) return;
        if (!Ui.Confirm("Удалить игру из каталога?\nОна также исчезнет из коллекций пользователей.")) return;
        Db.Exec("delete from games where id = @i", ("i", id.Value));
        Ui.Toast("Игра удалена");
        Reload();
    }

    void AddToCollection()
    {
        var id = Pick("игру"); if (id == null) return;
        int n = Db.Exec(@"insert into user_games(user_id, game_id) values (@u, @g)
                          on conflict (user_id, game_id) do nothing", ("u", Session.UserId), ("g", id.Value));
        if (n == 0) Ui.Info("Эта игра уже есть в вашей коллекции.");
        else Ui.Toast("Игра добавлена в вашу коллекцию (статус «В планах»)");
    }

    void GenreReport() => Ui.Export(Db.Query(@"
        select ge.name as ""Жанр"", count(g.id) as ""Количество игр""
        from genres ge left join games g on g.genre_id = ge.id
        group by ge.name order by 2 desc, 1"), "Жанры");
}
