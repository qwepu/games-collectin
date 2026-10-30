using System.Data;
using System.Text.RegularExpressions;

namespace GameCollection;

// Раздел только для администратора
public class UsersPanel : ListPanel
{
    const string RoleSql = "select id, case name when 'admin' then 'Администратор' when 'manager' then 'Менеджер' else 'Пользователь' end as name from roles order by id";

    readonly TextBox tbSearch = new() { PlaceholderText = "Логин…" };
    readonly ComboBox cbRole = Ui.Lookup(RoleSql, true);
    readonly ComboBox cbSort;
    readonly CheckBox chkDesc = DescBox();

    static readonly Dictionary<string, string> Sorts = new()
    { ["Id"] = "u.id", ["Логин"] = "u.login", ["Роль"] = "r.name", ["Создан"] = "u.created_at" };

    public UsersPanel()
    {
        cbSort = SortCombo(Sorts.Keys, false, 0);
        AddFilter("Поиск по логину", tbSearch, 220);
        AddFilter("Роль", cbRole);
        AddFilter("Сортировать по", cbSort, 140);
        AddFilter(" ", chkDesc);

        AddAction("＋  Добавить", Kind.Primary, Add);
        AddAction("Изменить роль / пароль", Kind.Secondary, Edit);
        AddAction("Удалить", Kind.Danger, Delete);
        AddAction("Excel: пользователи", Kind.Secondary, () => Ui.Export(Query(), "Пользователи"));
        Start();
    }

    protected override void ResetFilters()
    { tbSearch.Clear(); cbRole.SelectedIndex = 0; cbSort.SelectedIndex = 0; chkDesc.Checked = false; }

    protected override void OnOpen() => Edit();

    protected override DataTable Query()
    {
        string order = BuildOrder(Sorts, cbSort, null, chkDesc, "u.id");
        return Db.Query($@"
            select u.id as ""Id"", u.login as ""Логин"",
                   case r.name when 'admin' then 'Администратор' when 'manager' then 'Менеджер' else 'Пользователь' end as ""Роль"",
                   u.created_at::timestamp(0) as ""Создан""
            from users u join roles r on r.id = u.role_id
            where (@t = '' or u.login ilike '%' || @t || '%') and (@r = 0 or u.role_id = @r)
            order by {order}", ("t", Ui.Like(tbSearch.Text.Trim())), ("r", Ui.Sel(cbRole)));
    }

    void Add()
    {
        using var f = new UserForm(null);
        if (f.ShowDialog() != DialogResult.OK) return;
        Db.Exec("insert into users(login, password_hash, role_id) values (@l, crypt(@p, gen_salt('bf')), @r)",
            ("l", f.LoginText), ("p", f.Password), ("r", f.RoleId));
        Ui.Toast("Пользователь создан");
        Reload();
    }

    void Edit()
    {
        var id = Pick("пользователя"); if (id == null) return;
        if (id == Session.UserId) { Ui.Error("Свою учётную запись здесь изменять нельзя."); return; }
        var r = Db.Query("select login, role_id from users where id=@i", ("i", id.Value));
        if (r.Rows.Count == 0) { Ui.Error("Пользователь не найден."); Reload(); return; }
        using var f = new UserForm(id.Value, (string)r.Rows[0]["login"], Convert.ToInt32(r.Rows[0]["role_id"]));
        if (f.ShowDialog() != DialogResult.OK) return;
        Db.Exec(@"update users set role_id=@r,
                  password_hash = case when @p = '' then password_hash else crypt(@p, gen_salt('bf')) end
                  where id=@i", ("r", f.RoleId), ("p", f.Password), ("i", id.Value));
        Ui.Toast("Данные пользователя обновлены");
        Reload();
    }

    void Delete()
    {
        var id = Pick("пользователя"); if (id == null) return;
        if (id == Session.UserId) { Ui.Error("Нельзя удалить самого себя."); return; }
        if (!Ui.Confirm("Удалить пользователя вместе с его коллекцией и предложениями?")) return;
        Db.Exec("delete from users where id=@i", ("i", id.Value));
        Ui.Toast("Пользователь удалён");
        Reload();
    }
}

class UserForm : BaseDialog
{
    readonly TextBox tbLogin = new() { MaxLength = 30, PlaceholderText = "3–30 символов: a-z, 0-9, _" };
    readonly TextBox tbPass = new() { MaxLength = 64, UseSystemPasswordChar = true };
    readonly ComboBox cbRole = Ui.Lookup("select id, case name when 'admin' then 'Администратор' when 'manager' then 'Менеджер' else 'Пользователь' end as name from roles order by id", false);
    readonly bool isNew;

    public string LoginText => tbLogin.Text.Trim();
    public string Password => tbPass.Text;
    public int RoleId => Ui.Sel(cbRole);

    public UserForm(int? id, string login = "", int roleId = 0) : base(id == null ? "Новый пользователь" : "Изменение пользователя")
    {
        isNew = id == null;
        tbLogin.Text = login; tbLogin.Enabled = isNew;               // логин после создания не меняется
        if (roleId != 0) cbRole.SelectedValue = roleId;
        if (!isNew) AddRow("Id", IdBox(id.Value), 100);
        AddRow("Логин *", tbLogin);
        AddRow(isNew ? "Пароль *" : "Новый пароль", tbPass);
        AddRow("Роль *", cbRole);
        if (!isNew) tbPass.PlaceholderText = "оставьте пустым, чтобы не менять";
    }

    protected override string Check()
    {
        if (isNew && !Regex.IsMatch(LoginText, "^[A-Za-z0-9_]{3,30}$"))
        { tbLogin.Focus(); return "Логин: от 3 до 30 символов, только латинские буквы, цифры и «_»."; }
        if (isNew && Password.Length < 6) { tbPass.Focus(); return "Пароль должен содержать не менее 6 символов."; }
        if (!isNew && Password.Length > 0 && Password.Length < 6) { tbPass.Focus(); return "Новый пароль — не менее 6 символов."; }
        if (RoleId == 0) { cbRole.Focus(); return "Выберите роль."; }
        if (isNew && Db.Scalar("select 1 from users where lower(login) = lower(@l)", ("l", LoginText)) != null)
        { tbLogin.Focus(); return "Пользователь с таким логином уже существует."; }
        return null;
    }
}
