namespace GameCollection;

public enum Kind { Primary, Secondary, Danger }

public static class Theme
{
    static Color C(string h) => ColorTranslator.FromHtml(h);

    public static readonly Color Bg = C("#F3F4F6"), Surface = Color.White, Sidebar = C("#111827"), SidebarHover = C("#1F2937"),
        Accent = C("#4F46E5"), AccentHover = C("#4338CA"), Ink = C("#111827"), Muted = C("#6B7280"), Border = C("#E5E7EB"),
        Danger = C("#DC2626"), DangerHover = C("#B91C1C"), Success = C("#16A34A"), Warn = C("#D97706"), Info = C("#2563EB"),
        Select = C("#E0E7FF"), HeaderBg = C("#F9FAFB"), Hover = C("#F3F4F6");

    public static readonly Font Base = new("Segoe UI", 10f), Bold = new("Segoe UI Semibold", 10f),
        H1 = new("Segoe UI Semibold", 20f), H2 = new("Segoe UI Semibold", 14f), Small = new("Segoe UI", 9f);

    public static Button MakeButton(string text, Kind kind, Action onClick)
    {
        var (bg, hover, fg) = kind switch
        {
            Kind.Primary => (Accent, AccentHover, Color.White),
            Kind.Danger => (Danger, DangerHover, Color.White),
            _ => (Surface, Hover, Ink)
        };
        var b = new Button
        {
            Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(0, 38),
            Padding = new Padding(8, 0, 8, 0), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = Bold,
            BackColor = bg, ForeColor = fg, Margin = new Padding(0, 0, 8, 0), UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = kind == Kind.Secondary ? 1 : 0;
        b.FlatAppearance.BorderColor = Border;
        b.FlatAppearance.MouseOverBackColor = hover;
        b.FlatAppearance.MouseDownBackColor = hover;
        b.Click += (s, e) => onClick();
        return b;
    }

    public static void Input(Control c)
    {
        c.Font = Base;
        switch (c)
        {
            case TextBox t: t.BorderStyle = BorderStyle.FixedSingle; break;
            case NumericUpDown n: n.BorderStyle = BorderStyle.FixedSingle; break;
            case ComboBox cb: cb.FlatStyle = FlatStyle.Flat; cb.BackColor = Color.White; break;
        }
    }

    public static void StyleGrid(DataGridView g)
    {
        g.BorderStyle = BorderStyle.None; g.BackgroundColor = Surface; g.GridColor = Border;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 42;
        var h = g.ColumnHeadersDefaultCellStyle;
        h.BackColor = HeaderBg; h.ForeColor = Muted; h.Font = Bold; h.SelectionBackColor = HeaderBg;
        h.SelectionForeColor = Muted; h.Padding = new Padding(8, 0, 0, 0); h.Alignment = DataGridViewContentAlignment.MiddleLeft;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.RowTemplate.Height = 38;
        g.DefaultCellStyle.Font = Base; g.DefaultCellStyle.ForeColor = Ink; g.DefaultCellStyle.BackColor = Surface;
        g.DefaultCellStyle.SelectionBackColor = Select; g.DefaultCellStyle.SelectionForeColor = Ink;
        g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = C("#FAFAFB");
        g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Select;
    }
}
