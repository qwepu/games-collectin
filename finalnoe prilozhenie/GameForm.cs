namespace GameCollection;

// Форма игры / предложения игры. Id отображается, но не редактируется.
public class GameForm : BaseDialog
{
    readonly TextBox tbTitle = new() { MaxLength = 100, PlaceholderText = "Например: The Witcher 3" };
    readonly ComboBox cbGenre, cbPlat;
    readonly NumericUpDown nudYear = new() { Minimum = 1950, Maximum = 2100 };
    readonly TextBox tbDesc = new() { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };
    readonly int? id;
    readonly bool checkDup;

    public string GameTitle => tbTitle.Text.Trim();
    public int GenreId => Ui.Sel(cbGenre);
    public int PlatformId => Ui.Sel(cbPlat);
    public int Year => (int)nudYear.Value;
    public string Description => tbDesc.Text.Trim();

    public GameForm(string caption, int? id = null, string title = "", int genre = 0, int plat = 0,
                    int? year = null, string desc = "", bool checkDuplicates = false) : base(caption)
    {
        this.id = id; checkDup = checkDuplicates;
        cbGenre = Ui.Lookup("select id, name from genres order by name", false);
        cbPlat = Ui.Lookup("select id, name from platforms order by name", false);
        if (genre != 0) cbGenre.SelectedValue = genre;
        if (plat != 0) cbPlat.SelectedValue = plat;
        tbTitle.Text = title; tbDesc.Text = desc;
        nudYear.Value = Math.Clamp(year ?? DateTime.Now.Year, 1950, 2100);

        if (id != null) AddRow("Id", IdBox(id.Value), 100);
        AddRow("Название *", tbTitle);
        AddRow("Жанр *", cbGenre);
        AddRow("Платформа *", cbPlat);
        AddRow("Год выпуска *", nudYear, 100);
        AddRow("Описание", tbDesc);
    }

    protected override string Check()
    {
        if (GameTitle.Length == 0) { tbTitle.Focus(); return "Введите название игры — это обязательное поле."; }
        if (GenreId == 0) { cbGenre.Focus(); return "Выберите жанр из списка."; }
        if (PlatformId == 0) { cbPlat.Focus(); return "Выберите платформу из списка."; }
        if (checkDup && Db.Scalar("select 1 from games where lower(title) = lower(@t) and platform_id = @p and id <> @i",
                ("t", GameTitle), ("p", PlatformId), ("i", id ?? 0)) != null)
        { tbTitle.Focus(); return "Игра с таким названием на этой платформе уже есть в каталоге."; }
        return null;
    }
}
