using System.Data;

namespace GameCollection;

// Связанный процесс:
// пользователь предлагает игру -> менеджер принимает (игра попадает в каталог)
// -> администратор при необходимости правит данные -> пользователь забирает игру в коллекцию
public class SuggestionsPanel : ListPanel
{
    readonly TextBox tbSearch = new() { PlaceholderText = "Название игры…" };
    readonly ComboBox cbStatus = Ui.Dict(Ui.SugStatuses, true);
    readonly ComboBox cbGenre = Ui.Lookup("select id, name from genres order by name", true);
    readonly ComboBox cbSort1, cbSort2;
    readonly CheckBox chkDesc = DescBox();

    static readonly Dictionary<string, string> Sorts = new()
    {
        ["Id"] = "s.id", ["Название"] = "s.title", ["Автор"] = "u.login", ["Жанр"] = "ge.name",
        ["Статус"] = "s.status", ["Дата"] = "s.created_at"
    };

    public SuggestionsPanel()
    {
        cbSort1 = SortCombo(Sorts.Keys, false, 5);
        cbSort2 = SortCombo(Sorts.Keys, true, 0);
        chkDesc.Checked = true;
        SetHint("Как это работает: вы предлагаете игру → менеджер принимает её (она появляется в каталоге) → " +
                "администратор при необходимости уточняет данные → вы забираете игру в свою коллекцию.");
        AddFilter("Поиск", tbSearch, 220);
        AddFilter("Статус", cbStatus);
        AddFilter("Жанр", cbGenre);
        AddFilter("Сортировать по", cbSort1, 140);
        AddFilter("затем по", cbSort2, 140);
        AddFilter(" ", chkDesc);

        AddAction("＋  Предложить игру", Kind.Primary, Add);
        AddAction("Изменить", Kind.Secondary, Edit);
        if (Session.IsManager)
        {
            AddAction("Принять", Kind.Secondary, Accept);
            AddAction("Отклонить", Kind.Secondary, Reject);
        }
        AddAction("Забрать в коллекцию", Kind.Secondary, Take);
        AddAction("Удалить", Kind.Danger, Delete);
        Ui.ColorStatus(Grid);
        Start();
    }

    protected override void ResetFilters()
    {
        tbSearch.Clear(); cbStatus.SelectedIndex = 0; cbGenre.SelectedIndex = 0;
        cbSort1.SelectedIndex = 5; cbSort2.SelectedIndex = 0; chkDesc.Checked = true;
    }

    protected override void OnOpen() => Edit();

    protected override DataTable Query()
    {
        string order = BuildOrder(Sorts, cbSort1, cbSort2, chkDesc, "s.id");
        return Db.Query($@"
            select s.id as ""Id"", u.login as ""Автор"", s.title as ""Название"", ge.name as ""Жанр"",
                   p.name as ""Платформа"", s.release_year as ""Год"",
                   case s.status when 'new' then 'Новое' when 'accepted' then 'Принято'
                                 when 'rejected' then 'Отклонено' else 'Получено' end as ""Статус""
            from game_suggestions s
            join users u on u.id = s.user_id
            join genres ge on ge.id = s.genre_id
            join platforms p on p.id = s.platform_id
            where (@all or s.user_id = @u)
              and (@t = '' or s.title ilike '%' || @t || '%')
              and (@s = '' or s.status = @s)
              and (@g = 0 or s.genre_id = @g)
            order by {order}",
            ("all", Session.IsManager), ("u", Session.UserId), ("t", Ui.Like(tbSearch.Text.Trim())),
            ("s", cbStatus.SelectedValue as string ?? ""), ("g", Ui.Sel(cbGenre)));
    }

    void Add()
    {
        using var f = new GameForm("Новое предложение");
        if (f.ShowDialog() != DialogResult.OK) return;
        Db.Exec(@"insert into game_suggestions(user_id, title, genre_id, platform_id, release_year, description)
                  values (@u,@t,@g,@p,@y,@d)",
            ("u", Session.UserId), ("t", f.GameTitle), ("g", f.GenreId), ("p", f.PlatformId), ("y", f.Year), ("d", f.Description));
        Ui.Toast("Предложение отправлено менеджеру");
        Reload();
    }

    void Edit()
    {
        var id = Pick("предложение"); if (id == null) return;
        var r = Db.Query("select * from game_suggestions where id=@i", ("i", id.Value));
        if (r.Rows.Count == 0) { Ui.Error("Запись не найдена."); Reload(); return; }
        var row = r.Rows[0];
        var status = (string)row["status"]; var owner = Convert.ToInt32(row["user_id"]);
        bool can = Session.IsAdmin ? (status == "new" || status == "accepted") : (owner == Session.UserId && status == "new");
        if (!can) { Ui.Error("Это предложение нельзя изменить: у вас нет прав или статус уже не позволяет."); return; }

        using var f = new GameForm("Изменение предложения", id, (string)row["title"], Convert.ToInt32(row["genre_id"]),
            Convert.ToInt32(row["platform_id"]), Ui.IntOrNull(row["release_year"]), row["description"] as string ?? "");
        if (f.ShowDialog() != DialogResult.OK) return;

        // Если предложение уже принято — администратор правит заодно и саму игру в каталоге
        Db.Exec(@"
            with s as (
                update game_suggestions set title=@t, genre_id=@g, platform_id=@p, release_year=@y, description=@d
                where id=@i and status in ('new','accepted') returning game_id)
            update games set title=@t, genre_id=@g, platform_id=@p, release_year=@y, description=@d
            where id = (select game_id from s)",
            ("t", f.GameTitle), ("g", f.GenreId), ("p", f.PlatformId), ("y", f.Year), ("d", f.Description), ("i", id.Value));
        Ui.Toast("Изменения сохранены");
        Reload();
    }

    void Accept()
    {
        var id = Pick("предложение"); if (id == null) return;
        int n = Db.Exec(@"
            with g as (
                insert into games(title, genre_id, platform_id, release_year, description)
                select title, genre_id, platform_id, release_year, description
                from game_suggestions where id=@i and status='new' returning id)
            update game_suggestions set status='accepted', game_id=(select id from g), manager_id=@m
            where id=@i and status='new'", ("i", id.Value), ("m", Session.UserId));
        if (n == 0) Ui.Error("Принять можно только предложение со статусом «Новое».");
        else Ui.Toast("Предложение принято, игра добавлена в каталог");
        Reload();
    }

    void Reject()
    {
        var id = Pick("предложение"); if (id == null) return;
        int n = Db.Exec("update game_suggestions set status='rejected', manager_id=@m where id=@i and status='new'",
            ("i", id.Value), ("m", Session.UserId));
        if (n == 0) Ui.Error("Отклонить можно только предложение со статусом «Новое».");
        else Ui.Toast("Предложение отклонено");
        Reload();
    }

    void Take()
    {
        var id = Pick("предложение"); if (id == null) return;
        var ok = Db.Scalar("select 1 from game_suggestions where id=@i and user_id=@u and status='accepted'",
            ("i", id.Value), ("u", Session.UserId));
        if (ok == null) { Ui.Error("Забрать можно только своё предложение со статусом «Принято»."); return; }
        Db.Exec(@"
            with s as (update game_suggestions set status='received'
                       where id=@i and user_id=@u and status='accepted' returning game_id)
            insert into user_games(user_id, game_id) select @u, game_id from s
            on conflict (user_id, game_id) do nothing", ("i", id.Value), ("u", Session.UserId));
        Ui.Toast("Игра добавлена в вашу коллекцию");
        Reload();
    }

    void Delete()
    {
        var id = Pick("предложение"); if (id == null) return;
        if (!Ui.Confirm("Удалить предложение?")) return;
        int n = Db.Exec("delete from game_suggestions where id=@i and (@adm or (user_id=@u and status='new'))",
            ("i", id.Value), ("adm", Session.IsAdmin), ("u", Session.UserId));
        if (n == 0) Ui.Error("Удалить можно только своё предложение со статусом «Новое» (администратор — любое).");
        else Ui.Toast("Предложение удалено");
        Reload();
    }
}
