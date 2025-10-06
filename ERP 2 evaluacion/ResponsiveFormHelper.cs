using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ERP_2_evaluacion;

internal static class ResponsiveFormHelper
{
    private const int DefaultDesignWidth = 1280;
    private const int DefaultDesignHeight = 840;

    public static void Attach(Form form)
    {
        if (form is null)
        {
            throw new ArgumentNullException(nameof(form));
        }

        var designSize = form.ClientSize;
        if (designSize.Width <= 0 || designSize.Height <= 0)
        {
            designSize = new Size(DefaultDesignWidth, DefaultDesignHeight);
        }

        void OnLoad(object? sender, EventArgs e)
        {
            ApplySizing(form, designSize);
            form.Resize -= OnResizeOrMove;
            form.Resize += OnResizeOrMove;
            form.Move -= OnResizeOrMove;
            form.Move += OnResizeOrMove;
            ApplySizing(form, designSize);
        }

        void OnResizeOrMove(object? _, EventArgs __)
        {
            KeepVisible(form);
        }

        void OnDisplaySettingsChanged(object? _, EventArgs __)
        {
            if (form.IsDisposed)
            {
                SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
                return;
            }

            ApplySizing(form, designSize);
        }

        void OnDisposed(object? _, EventArgs __)
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            form.Resize -= OnResizeOrMove;
            form.Move -= OnResizeOrMove;
        }

        form.Load -= OnLoad;
        form.Load += OnLoad;
        form.Disposed -= OnDisposed;
        form.Disposed += OnDisposed;
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    private static void ApplySizing(Form form, Size designSize)
    {
        if (form.WindowState == FormWindowState.Minimized)
        {
            return;
        }

        var screen = Screen.FromControl(form);
        var working = screen.WorkingArea;

        var horizontalMargin = Math.Max(48, (int)Math.Round(working.Width * 0.04));
        var verticalMargin = Math.Max(48, (int)Math.Round(working.Height * 0.06));
        var availableWidth = Math.Max(working.Width - horizontalMargin, 0);
        var availableHeight = Math.Max(working.Height - verticalMargin, 0);

        var targetWidth = availableWidth > 0
            ? Math.Min(designSize.Width, availableWidth)
            : designSize.Width;
        var targetHeight = availableHeight > 0
            ? Math.Min(designSize.Height, availableHeight)
            : designSize.Height;

        if (form.WindowState != FormWindowState.Maximized)
        {
            form.Size = new Size(targetWidth, targetHeight);
            CenterForm(form, working);
        }

        form.MaximumSize = Size.Empty;
        form.MinimumSize = Size.Empty;

        var padding = (int)Math.Round(Math.Clamp(working.Width * 0.015, 20, 48));
        form.Padding = new Padding(padding);

        form.AutoScroll = true;
        var preferred = form.PreferredSize;
        var scrollWidth = Math.Max(Math.Max(designSize.Width, preferred.Width), form.AutoScrollMinSize.Width);
        var scrollHeight = Math.Max(Math.Max(designSize.Height, preferred.Height), form.AutoScrollMinSize.Height);
        form.AutoScrollMinSize = new Size(scrollWidth, scrollHeight);
    }

    private static void CenterForm(Form form, Rectangle working)
    {
        var x = working.X + Math.Max(0, (working.Width - form.Width) / 2);
        var y = working.Y + Math.Max(0, (working.Height - form.Height) / 2);
        form.Location = new Point(x, y);
    }

    private static void KeepVisible(Form form)
    {
        if (form.WindowState == FormWindowState.Maximized || form.WindowState == FormWindowState.Minimized)
        {
            return;
        }

        var working = Screen.FromControl(form).WorkingArea;

        var x = Math.Min(Math.Max(form.Left, working.Left), working.Right - form.Width);
        var y = Math.Min(Math.Max(form.Top, working.Top), working.Bottom - form.Height);

        form.Location = new Point(x, y);
    }
}
