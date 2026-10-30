using System.Data;
using ClosedXML.Excel;

namespace GameCollection;

public interface IReload { void Reload(); }

public static class Ui
{
    public static readonly Dictionary<string, string> Statuses = new()
    { ["planned"] = "В планах", ["playing"] = "Играю", ["completed"] = "Пройдена", ["dropped"] = "Брошена" };

    public static readonly Dictionary<string, string> SugStatuses = new()
    { ["new"] = "Новое", ["accepted"] = "Принято", ["rejected"] = "Отклонено", ["received"] = "Получено" };

    public static Action<string> ToastHandler;
    public static void Toast(string m) { if (ToastHandler != null) ToastHandler(m); else Info(m); }

    public static void Error(string m) => MessageBox.Show(m, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Info(string m) => MessageBox.Show(m, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static bool Confirm(string m) =>
        MessageBox.Show(m, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static void Try(Action a)
    {
        try { a(); } catch (Exception ex) { Error(Db.Explain(ex)); }
    }

    public static int? IntOrNull(object o) => o == null || o is DBNull ? null : Convert.ToInt32(o);

    // Экранирование % и _ в строке поиска
    public static string Like(string s) => s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    // Выпадающий список из БД: колонка 0 = значение, колонка 1 = подпись
    public static ComboBox Lookup(string sql, bool all, string allText = "Все")
    {
        var dt = Db.Query(sql);
        if (all) { var r = dt.NewRow(); r[0] = 0; r[1] = allText; dt.Rows.InsertAt(r, 0); }
        return new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = dt.Columns[1].ColumnName, ValueMember = dt.Columns[0].ColumnName, DataSource = dt
        };
    }

    // Выпадающий список из словаря констант
    public static ComboBox Dict(Dictionary<string, string> d, bool all, string allText = "Все")
    {
        var list = d.Select(k => new KeyValuePair<string, string>(k.Key, k.Value)).ToList();
        if (all) list.Insert(0, new KeyValuePair<string, string>("", allText));
        return new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Value", ValueMember = "Key", DataSource = list
        };
    }

    public static int Sel(ComboBox c) => c.SelectedValue == null ? 0 : Convert.ToInt32(c.SelectedValue);

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false
        };
        Theme.StyleGrid(g);
        return g;
    }

    // Id всегда в первой колонке и только для чтения
    public static int? SelectedId(DataGridView g) =>
        g.CurrentRow == null ? null : Convert.ToInt32(g.CurrentRow.Cells[0].Value);

    // Цветные статусы в таблицах
    public static void ColorStatus(DataGridView g) => g.CellFormatting += (s, e) =>
    {
        if (e.RowIndex < 0 || e.Value == null || g.Columns[e.ColumnIndex].HeaderText != "Статус") return;
        e.CellStyle.ForeColor = e.Value.ToString() switch
        {
            "В планах" => Theme.Muted, "Играю" => Theme.Info, "Пройдена" => Theme.Success, "Брошена" => Theme.Danger,
            "Новое" => Theme.Warn, "Принято" => Theme.Success, "Отклонено" => Theme.Danger, "Получено" => Theme.Info,
            _ => Theme.Ink
        };
        e.CellStyle.Font = Theme.Bold;
    };

    public static void Export(DataTable dt, string name)
    {
        if (dt.Rows.Count == 0) { Info("Нет данных для экспорта."); return; }
        using var sfd = new SaveFileDialog
        { Filter = "Excel (*.xlsx)|*.xlsx", FileName = $"{name}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx" };
        if (sfd.ShowDialog() != DialogResult.OK) return;
        Try(() =>
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet(name);
            ws.Cell(1, 1).InsertTable(dt, "Report", true);
            ws.Columns().AdjustToContents();
            wb.SaveAs(sfd.FileName);
            Toast("Отчёт сохранён: " + Path.GetFileName(sfd.FileName));
        });
    }
}
