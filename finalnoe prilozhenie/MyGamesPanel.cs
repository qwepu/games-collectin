using System.Data;

namespace GameCollection;

public class MyGamesPanel : ListPanel
{
    readonly TextBox tbSearch = new() { PlaceholderText = "Название игры…" };
    readonly ComboBox cbStatus = Ui.Dict(Ui.Statuses, true);
    readonly ComboBox cbGenre = Ui.Lookup("select id, name from genres order by name", true);
    readonly ComboBox cbSort1, cbSort2;
    readonly CheckBox chkDesc = DescBox();

    static readonly Dictionary<string, string> Sorts = new()
    {
        ["Игра"] = "g.title", ["Жанр"] = "ge.name", ["Платформа"] = "p.name",
        ["Статус"] = "ug.status", ["Оценка"] = "ug.rating", ["Дата добавления"] = "ug.added_at"
    };

    public MyGamesPanel()
    {
        cbSort1 = SortCombo(Sorts.Keys, false, 5);
        cbSort2 = SortCombo(Sorts.Keys, true, 0);
        chkDesc.Checked = true;
        AddFilter("Поиск", tbSearch, 220);
        AddFilter("Статус", cbStatus);
        AddFilter("Жанр", cbGenre);
        AddFilter("Сортировать по", cbSort1, 150);
        AddFilter("затем по", cbSort2, 150);
        AddFilter(" ", chkDesc);

        AddAction("Изменить статус / оценку", Kind.Primary, Edit);
        AddAction("Убрать из коллекции", Kind.Danger, Remove);
        AddAction("Excel: моя коллекция", Kind.Secondary, () => Ui.Export(Query(), "Моя коллекция"));
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
        string order = BuildOrder(Sorts, cbSort1, cbSort2, chkDesc, "ug.id");
        return Db.Query($@"
            select ug.id as ""Id"", g.title as ""Игра"", ge.name as ""Жанр"", p.name as ""Платформа"",
                   case ug.status when 'planned' then 'В планах' when 'playing' then 'Играю'
                                  when 'completed' then 'Пройдена' else 'Брошена' end as ""Статус"",
                   ug.rating as ""Оценка"", ug.added_at::date as ""Добавлена""
            from user_games ug
            join games g on g.id = ug.game_id
            join genres ge on ge.id = g.genre_id
            join platforms p on p.id = g.platform_id
            where ug.user_id = @u
              and (@t = '' or g.title ilike '%' || @t || '%')
              and (@s = '' or ug.status = @s)
              and (@g = 0 or g.genre_id = @g)
            order by {order}",
            ("u", Session.UserId), ("t", Ui.Like(tbSearch.Text.Trim())),
            ("s", cbStatus.SelectedValue as string ?? ""), ("g", Ui.Sel(cbGenre)));
    }

    void Edit()
    {
        var id = Pick("игру"); if (id == null) return;
        var r = Db.Query("select status, rating from user_games where id=@i and user_id=@u", ("i", id.Value), ("u", Session.UserId));
        if (r.Rows.Count == 0) { Ui.Error("Запись не найдена."); Reload(); return; }
        using var f = new StatusForm(id.Value, (string)r.Rows[0]["status"], Ui.IntOrNull(r.Rows[0]["rating"]));
        if (f.ShowDialog() != DialogResult.OK) return;
        Db.Exec("update user_games set status=@s, rating=@r where id=@i and user_id=@u",
            ("s", f.Status), ("r", (object)f.Rating), ("i", id.Value), ("u", Session.UserId));
        Ui.Toast("Статус и оценка сохранены");
        Reload();
    }

    void Remove()
    {
        var id = Pick("игру"); if (id == null) return;
        if (!Ui.Confirm("Убрать игру из вашей коллекции?")) return;
        Db.Exec("delete from user_games where id=@i and user_id=@u", ("i", id.Value), ("u", Session.UserId));
        Ui.Toast("Игра убрана из коллекции");
        Reload();
    }
}

class StatusForm : BaseDialog
{
    readonly ComboBox cbStatus = Ui.Dict(Ui.Statuses, false);
    readonly ComboBox cbRating = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public string Status => (string)cbStatus.SelectedValue;
    public int? Rating => cbRating.SelectedIndex == 0 ? null : cbRating.SelectedIndex;

    public StatusForm(int id, string status, int? rating) : base("Статус и оценка")
    {
        cbRating.Items.Add("— без оценки —");
        for (int i = 1; i <= 10; i++) cbRating.Items.Add(i);
        cbStatus.SelectedValue = status; cbRating.SelectedIndex = rating ?? 0;
        AddRow("Id", IdBox(id), 100);
        AddRow("Статус *", cbStatus);
        AddRow("Оценка (1–10)", cbRating);
    }

    protected override string Check() => cbStatus.SelectedValue == null ? "Выберите статус из списка." : null;
}
