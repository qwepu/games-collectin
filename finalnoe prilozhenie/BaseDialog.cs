namespace GameCollection;

// Единый вид всех диалоговых окон: заголовок, поля, ошибка валидации внутри окна, кнопки.
public abstract class BaseDialog : Form
{
    protected readonly TableLayoutPanel Fields = new() { AutoSize = true, ColumnCount = 2, Margin = Padding.Empty };
    readonly Label lblError = new()
    { AutoSize = true, ForeColor = Theme.Danger, MaximumSize = new Size(440, 0), MinimumSize = new Size(0, 24), Margin = new Padding(0, 10, 0, 6), Font = Theme.Base };

    // Возвращает null, если всё верно, иначе текст ошибки
    protected abstract string Check();

    protected BaseDialog(string title, string saveText = "Сохранить")
    {
        Text = title; Font = Theme.Base; BackColor = Theme.Surface; ForeColor = Theme.Ink;
        FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink; Padding = new Padding(28, 22, 28, 22);

        var save = Theme.MakeButton(saveText, Kind.Primary, Submit);
        var cancel = Theme.MakeButton("Отмена", Kind.Secondary, () => DialogResult = DialogResult.Cancel);
        AcceptButton = save; CancelButton = cancel;
        var buttons = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 4, 0, 0) };
        buttons.Controls.Add(save); buttons.Controls.Add(cancel);

        var root = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
        root.Controls.Add(new Label { Text = title, Font = Theme.H2, AutoSize = true, Margin = new Padding(0, 0, 0, 14) });
        root.Controls.Add(Fields); root.Controls.Add(lblError); root.Controls.Add(buttons);
        Controls.Add(root);
    }

    protected void AddRow(string caption, Control c, int width = 300)
    {
        Theme.Input(c); c.Width = width; c.Margin = new Padding(0, 6, 0, 6);
        Fields.Controls.Add(new Label
        { Text = caption, AutoSize = true, ForeColor = Theme.Muted, Anchor = AnchorStyles.Top | AnchorStyles.Left, Margin = new Padding(0, 12, 22, 0) });
        Fields.Controls.Add(c);
    }

    protected static TextBox IdBox(int id) => new() { Text = id.ToString(), ReadOnly = true, BackColor = Theme.Bg, TabStop = false };

    void Submit()
    {
        string m;
        try { m = Check(); } catch (Exception ex) { m = Db.Explain(ex); }
        if (m != null) { lblError.Text = "⚠  " + m; return; }
        DialogResult = DialogResult.OK;
    }
}
