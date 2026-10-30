using System.Data;

namespace GameCollection;

// Базовая панель-список: фильтры сверху, кнопки действий, таблица, счётчик записей.
// Поиск обновляется сам (с небольшой задержкой), двойной клик по строке = открыть/изменить.
public abstract class ListPanel : UserControl, IReload
{
    protected readonly DataGridView Grid = Ui.Grid();
    readonly Label lblHint = new() { AutoSize = true, ForeColor = Theme.Muted, Visible = false, Margin = new Padding(0, 0, 0, 10), MaximumSize = new Size(1100, 0) };
    readonly FlowLayoutPanel filters = new()
    { Dock = DockStyle.Fill, AutoSize = true, BackColor = Theme.Surface, Padding = new Padding(14, 10, 14, 2), Margin = new Padding(0, 0, 0, 10) };
    readonly FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
    readonly Label lblCount = new() { AutoSize = true, ForeColor = Theme.Muted, Font = Theme.Small, Margin = new Padding(2, 8, 0, 0) };
    readonly System.Windows.Forms.Timer debounce = new() { Interval = 300 };
    bool ready;

    protected abstract DataTable Query();
    protected abstract void ResetFilters();
    protected virtual void OnOpen() { }

    protected ListPanel()
    {
        Dock = DockStyle.Fill; BackColor = Theme.Bg; Font = Theme.Base;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var host = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Border, Padding = new Padding(1) };
        host.Controls.Add(Grid);

        root.Controls.Add(lblHint, 0, 0); root.Controls.Add(filters, 0, 1); root.Controls.Add(actions, 0, 2);
        root.Controls.Add(host, 0, 3); root.Controls.Add(lblCount, 0, 4);
        Controls.Add(root);

        debounce.Tick += (s, e) => { debounce.Stop(); Reload(); };
        Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Ui.Try(OnOpen); };
    }

    protected void SetHint(string text) { lblHint.Text = text; lblHint.Visible = true; }

    protected void AddFilter(string caption, Control c, int width = 170)
    {
        Theme.Input(c);
        if (c is not CheckBox) c.Width = width;
        c.Margin = new Padding(0, 3, 0, 0);
        var p = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 16, 8) };
        p.Controls.Add(new Label { Text = caption, AutoSize = true, ForeColor = Theme.Muted, Font = Theme.Small });
        p.Controls.Add(c);
        filters.Controls.Add(p);
        switch (c)
        {
            case TextBox t: t.TextChanged += (s, e) => Changed(true); break;
            case NumericUpDown n: n.ValueChanged += (s, e) => Changed(true); break;
            case ComboBox cb: cb.SelectedIndexChanged += (s, e) => Changed(false); break;
            case CheckBox ch: ch.CheckedChanged += (s, e) => Changed(false); break;
        }
    }

    void Changed(bool delay)
    {
        if (!ready) return;
        if (delay) { debounce.Stop(); debounce.Start(); } else Reload();
    }

    protected void AddAction(string text, Kind kind, Action a) =>
        actions.Controls.Add(Theme.MakeButton(text, kind, () => Ui.Try(a)));

    // Вызывать в конце конструктора наследника
    protected void Start()
    {
        var reset = Theme.MakeButton("Сбросить", Kind.Secondary, () =>
        {
            ready = false; Ui.Try(ResetFilters); ready = true; Reload();
        });
        reset.Margin = new Padding(0, 22, 0, 8);
        filters.Controls.Add(reset);
        ready = true; Reload();
    }

    public void Reload() => Ui.Try(() =>
    {
        int? keep = Ui.SelectedId(Grid);
        var dt = Query();
        Grid.DataSource = dt;
        if (Grid.Columns.Count > 0 && Grid.Columns[0].HeaderText == "Id") Grid.Columns[0].FillWeight = 22;
        if (Grid.Columns.Contains("Описание")) Grid.Columns["Описание"].FillWeight = 220;
        lblCount.Text = dt.Rows.Count == 0 ? "Ничего не найдено. Измените условия поиска." : $"Найдено записей: {dt.Rows.Count}";
        if (keep != null)
            foreach (DataGridViewRow r in Grid.Rows)
                if (Convert.ToInt32(r.Cells[0].Value) == keep) { Grid.CurrentCell = r.Cells[0]; break; }
    });

    protected int? Pick(string what = "запись")
    {
        var id = Ui.SelectedId(Grid);
        if (id == null) Ui.Error($"Выберите {what} в таблице.");
        return id;
    }

    protected static ComboBox SortCombo(IEnumerable<string> keys, bool optional, int index)
    {
        var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        if (optional) cb.Items.Add("— нет —");
        cb.Items.AddRange(keys.ToArray());
        cb.SelectedIndex = index;
        return cb;
    }

    // ORDER BY только из белого списка: пользовательский ввод в SQL не попадает
    protected static string BuildOrder(Dictionary<string, string> sorts, ComboBox s1, ComboBox s2, CheckBox desc, string tiebreak)
    {
        string dir = desc.Checked ? " desc" : " asc";
        var parts = new List<string> { sorts[s1.Text] + dir + " nulls last" };
        if (s2 != null && s2.SelectedIndex > 0) parts.Add(sorts[s2.Text] + dir + " nulls last");
        parts.Add(tiebreak);
        return string.Join(", ", parts);
    }

    protected static CheckBox DescBox() => new() { Text = "По убыванию", AutoSize = true, ForeColor = Theme.Ink, Margin = new Padding(0, 8, 0, 0) };
}
