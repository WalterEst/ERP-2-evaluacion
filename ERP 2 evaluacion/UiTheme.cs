using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public static class UiTheme
{
    public static readonly Color BackgroundColor = Color.FromArgb(245, 247, 250);
    public static readonly Color SurfaceColor = Color.White;
    public static readonly Color AccentColor = Color.FromArgb(56, 97, 251);
    public static readonly Color AccentColorHover = Color.FromArgb(46, 84, 230);
    public static readonly Color TextColor = Color.FromArgb(31, 41, 55);
    public static readonly Color MutedTextColor = Color.FromArgb(110, 118, 135);
    public static readonly Color BorderColor = Color.FromArgb(224, 227, 231);
    public static readonly Color SecondaryButtonColor = Color.FromArgb(240, 242, 247);
    public static readonly Color SecondaryButtonHoverColor = Color.FromArgb(224, 227, 235);
    public static readonly Color DangerColor = Color.FromArgb(220, 76, 70);
    public static readonly Color DangerColorHover = Color.FromArgb(185, 60, 55);

    public static readonly Font BaseFont = new("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font TitleFont = new("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font SectionTitleFont = new("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font HeaderFont = new("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);

    public static void ApplyMinimalStyle(Form form)
    {
        form.Font = BaseFont;
        form.BackColor = BackgroundColor;
        form.ForeColor = TextColor;
        form.Padding = new Padding(24);
    }

    public static Label CreateTitleLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = TitleFont,
            ForeColor = TextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
    }

    public static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = SectionTitleFont,
            ForeColor = TextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
    }

    public static Panel CreateCardPanel(DockStyle dock = DockStyle.Fill)
    {
        return new CardPanel { Dock = dock };
    }

    public static void StyleTextInput(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = SurfaceColor;
        textBox.ForeColor = TextColor;
        textBox.Margin = new Padding(0, 4, 0, 12);
        textBox.MinimumSize = new Size(200, 32);
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = SurfaceColor;
        comboBox.ForeColor = TextColor;
        comboBox.Margin = new Padding(0, 4, 0, 12);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    public static void StyleCheckBox(CheckBox checkBox)
    {
        checkBox.FlatStyle = FlatStyle.Flat;
        checkBox.ForeColor = TextColor;
        checkBox.Margin = new Padding(0, 12, 0, 0);
    }

    public static void StylePrimaryButton(Button button)
    {
        StyleBaseButton(button);
        button.BackColor = AccentColor;
        button.ForeColor = Color.White;
        button.FlatAppearance.MouseOverBackColor = AccentColorHover;
        button.FlatAppearance.BorderColor = AccentColor;
    }

    public static void StyleSecondaryButton(Button button)
    {
        StyleBaseButton(button);
        button.BackColor = SecondaryButtonColor;
        button.ForeColor = TextColor;
        button.FlatAppearance.MouseOverBackColor = SecondaryButtonHoverColor;
        button.FlatAppearance.BorderColor = SecondaryButtonColor;
    }

    public static void StyleDangerButton(Button button)
    {
        StyleBaseButton(button);
        button.BackColor = DangerColor;
        button.ForeColor = Color.White;
        button.FlatAppearance.MouseOverBackColor = DangerColorHover;
        button.FlatAppearance.BorderColor = DangerColor;
    }

    private static void StyleBaseButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(16, 8, 16, 8);
        button.Margin = new Padding(8, 0, 0, 0);
        button.AutoSize = true;
        button.MinimumSize = new Size(0, 38);
        button.Cursor = Cursors.Hand;
    }

    public static void StyleDataGrid(DataGridView grid)
    {
        grid.BackgroundColor = SurfaceColor;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceColor;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = AccentColor;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 10, 8, 10);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 44;
        grid.RowHeadersVisible = false;
        grid.DefaultCellStyle.BackColor = SurfaceColor;
        grid.DefaultCellStyle.ForeColor = TextColor;
        grid.DefaultCellStyle.SelectionBackColor = AccentColor;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.DefaultCellStyle.Padding = new Padding(4, 6, 4, 6);
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        grid.GridColor = BorderColor;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
    }

    public static void StyleTreeView(TreeView tree)
    {
        tree.FullRowSelect = true;
        tree.HideSelection = false;
        tree.BorderStyle = BorderStyle.None;
        tree.BackColor = SurfaceColor;
        tree.ForeColor = TextColor;
        tree.LineColor = BorderColor;
        tree.ItemHeight = 28;
        tree.Font = BaseFont;
        tree.Margin = new Padding(0);
        tree.Padding = new Padding(8, 0, 8, 0);
    }

    private sealed class CardPanel : Panel
    {
        public CardPanel()
        {
            BackColor = SurfaceColor;
            Padding = new Padding(24);
            Margin = new Padding(0);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var pen = new Pen(BorderColor);
            e.Graphics.DrawRectangle(pen, rect);
        }
    }
}
