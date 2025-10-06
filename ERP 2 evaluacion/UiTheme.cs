using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ERP_2_evaluacion;

public static class UiTheme
{
    public static readonly Color BackgroundColor = Color.FromArgb(243, 246, 250);
    public static readonly Color SurfaceColor = Color.White;
    public static readonly Color AccentColor = Color.FromArgb(46, 100, 255);
    public static readonly Color AccentColorHover = Color.FromArgb(32, 82, 233);
    public static readonly Color TextColor = Color.FromArgb(28, 37, 54);
    public static readonly Color MutedTextColor = Color.FromArgb(113, 120, 137);
    public static readonly Color BorderColor = Color.FromArgb(220, 224, 230);
    public static readonly Color SecondaryButtonColor = Color.FromArgb(234, 238, 246);
    public static readonly Color SecondaryButtonHoverColor = Color.FromArgb(218, 224, 236);
    public static readonly Color DangerColor = Color.FromArgb(218, 63, 60);
    public static readonly Color DangerColorHover = Color.FromArgb(185, 50, 48);
    public static readonly Color SuccessColor = Color.FromArgb(46, 160, 67);
    public static readonly Color SuccessColorHover = Color.FromArgb(35, 138, 55);

    public static readonly Font BaseFont = new("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font TitleFont = new("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font SectionTitleFont = new("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font HeaderFont = new("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);

    public static void ApplyMinimalStyle(Form form)
    {
        form.Font = BaseFont;
        form.BackColor = BackgroundColor;
        form.ForeColor = TextColor;
        form.Padding = new Padding(32);
        form.AutoScaleMode = AutoScaleMode.Dpi;

        AttachMenuStripHandler(form);
    }

    private static void AttachMenuStripHandler(Control control)
    {
        StyleMenuStripsInControl(control);

        control.ControlAdded -= Control_ControlAdded;
        control.ControlAdded += Control_ControlAdded;

        foreach (Control child in control.Controls)
        {
            AttachMenuStripHandler(child);
        }
    }

    private static void Control_ControlAdded(object? sender, ControlEventArgs e)
    {
        AttachMenuStripHandler(e.Control);
    }

    private static void StyleMenuStripsInControl(Control control)
    {
        if (control is MenuStrip menuStrip)
        {
            StyleMenuStrip(menuStrip);
        }

        foreach (Control child in control.Controls)
        {
            StyleMenuStripsInControl(child);
        }
    }

    public static void StyleMenuStrip(MenuStrip menuStrip)
    {
        menuStrip.RenderMode = ToolStripRenderMode.System;
        menuStrip.ImageScalingSize = new Size(20, 20);
        menuStrip.Padding = new Padding(4, 2, 4, 2);
        menuStrip.AutoSize = true;

        foreach (var item in menuStrip.Items.OfType<ToolStripMenuItem>())
        {
            StyleMenuItem(item);
        }
    }

    private static void StyleMenuItem(ToolStripMenuItem item)
    {
        item.AutoSize = true;
        item.Margin = Padding.Empty;
        item.Padding = new Padding(6, 2, 6, 2);
        item.ShowShortcutKeys = true;
        item.ImageScaling = ToolStripItemImageScaling.SizeToFit;

        if (item.DropDown is ToolStripDropDownMenu dropDown)
        {
            dropDown.AutoSize = true;
            dropDown.ShowImageMargin = true;
            dropDown.ShowCheckMargin = false;
            dropDown.Padding = new Padding(2);
            dropDown.RenderMode = ToolStripRenderMode.System;
        }

        foreach (var child in item.DropDownItems.OfType<ToolStripMenuItem>())
        {
            StyleMenuItem(child);
        }
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
        textBox.Margin = new Padding(0, 6, 0, 16);
        textBox.MinimumSize = new Size(280, 36);
        textBox.Dock = DockStyle.Fill;
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = SurfaceColor;
        comboBox.ForeColor = TextColor;
        comboBox.Margin = new Padding(0, 6, 0, 16);
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.Dock = DockStyle.Fill;
        comboBox.MinimumSize = new Size(280, 36);
        comboBox.IntegralHeight = false;
    }

    public static void StyleCheckBox(CheckBox checkBox)
    {
        checkBox.FlatStyle = FlatStyle.Flat;
        checkBox.ForeColor = TextColor;
        checkBox.Margin = new Padding(0, 16, 0, 0);
        checkBox.AutoSize = true;
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

    public static void StyleSuccessButton(Button button)
    {
        StyleBaseButton(button);
        button.BackColor = SuccessColor;
        button.ForeColor = Color.White;
        button.FlatAppearance.MouseOverBackColor = SuccessColorHover;
        button.FlatAppearance.BorderColor = SuccessColor;
    }

    private static void StyleBaseButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Padding = new Padding(20, 10, 20, 10);
        button.Margin = new Padding(8, 0, 0, 0);
        button.AutoSize = true;
        button.MinimumSize = new Size(160, 44);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
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
        grid.Dock = DockStyle.Fill;
        grid.Margin = new Padding(0, 8, 0, 0);
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
