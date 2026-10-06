using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KeeTheme.Decorators
{
    internal sealed class CenteredSearchDecorator : System.ComponentModel.Component
    {
        private readonly ToolStrip _strip;
        private readonly ToolStripComboBox _search;
        private readonly ToolStripLabel _space = new ToolStripLabel();
        private bool _layingOut;
        private readonly SearchBorderWindow _border;
        internal CenteredSearchDecorator(ToolStrip strip, ToolStripComboBox search)
        {
            _strip = strip; _search = search;
            _border = new SearchBorderWindow(search.ComboBox);
            _space.Name = "KeeThemeCenterSearchSpacer";
            _space.AutoSize = false;
            _space.Margin = Padding.Empty;
            _space.Size = new System.Drawing.Size(0, 1);
            strip.Disposed += OnStripDisposed;
            strip.Items.Insert(strip.Items.IndexOf(search), _space);
            strip.Layout += OnLayout;
            Reposition();
        }
        private void OnStripDisposed(object sender, EventArgs e) { Dispose(); }
        private void OnLayout(object sender, LayoutEventArgs e) { Reposition(); }
        private void Reposition()
        {
            if (_layingOut || _search.IsDisposed) return;
            _layingOut = true;
            try
            {
                int before = _strip.Padding.Left + (_strip.GripStyle == ToolStripGripStyle.Visible ? _strip.GripRectangle.Width : 0);
                int after = _strip.Padding.Right + 18;
                bool passed = false;
                foreach (ToolStripItem item in _strip.Items)
                {
                    if (item == _search) { passed = true; continue; }
                    if (item == _space || !item.Available) continue;
                    int width = item.GetPreferredSize(System.Drawing.Size.Empty).Width + item.Margin.Horizontal;
                    if (passed) after += width; else before += width;
                }
                int wanted = _strip.ImageScalingSize.Width * 20;
                int widthSearch = Math.Min(wanted, Math.Max(100, _strip.ClientSize.Width-before-after-_search.Margin.Horizontal));
                int target = (_strip.ClientSize.Width-widthSearch)/2;
                int spacer = Math.Max(0, Math.Min(target-before-_search.Margin.Left,
                    _strip.ClientSize.Width-before-after-widthSearch-_search.Margin.Horizontal));
                _search.Width = widthSearch;
                _space.Width = spacer;
            }
            finally { _layingOut = false; }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _strip.Layout -= OnLayout;
                _strip.Disposed -= OnStripDisposed;
                if (!_strip.IsDisposed) _strip.Items.Remove(_space);
                _space.Dispose();
                _border.Dispose();
            }
            base.Dispose(disposing);
        }

        internal sealed class SearchBorderWindow : NativeWindow, IDisposable
        {
            private readonly Control _combo;
            private readonly bool _field;
            [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hwnd);
            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
            [StructLayout(LayoutKind.Sequential)] private struct ComboInfo
            {
                public int Size;
                public RectangleNative Item, Button;
                public int State;
                public IntPtr Combo, Edit, List;
            }
            [StructLayout(LayoutKind.Sequential)] private struct RectangleNative { public int Left, Top, Right, Bottom; }
            [DllImport("user32.dll")] private static extern bool GetComboBoxInfo(IntPtr hwnd, ref ComboInfo info);
            internal SearchBorderWindow(Control combo) : this(combo, false) { }
            internal SearchBorderWindow(Control combo, bool field)
            {
                _combo = combo;
                _field = field;
                combo.HandleCreated += OnCreated;
                combo.HandleDestroyed += OnDestroyed;
                combo.GotFocus += OnFocus;
                combo.LostFocus += OnFocus;
                IntPtr handle = combo.Handle;
                if (Handle == IntPtr.Zero) AssignHandle(handle);
            }
            private void OnCreated(object sender, EventArgs e) { AssignHandle(_combo.Handle); }
            private void OnDestroyed(object sender, EventArgs e) { ReleaseHandle(); }
            private void OnFocus(object sender, EventArgs e) { _combo.Invalidate(); }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg != 0x000F && m.Msg != 0x0085) return;
                IntPtr dc = GetWindowDC(m.HWnd);
                if (dc == IntPtr.Zero) return;
                try
                {
                    using (var graphics = Graphics.FromHdc(dc))
                    {
                        if (_combo is ComboBox)
                        {
                            var info = new ComboInfo(); info.Size = Marshal.SizeOf(typeof(ComboInfo));
                            if (GetComboBoxInfo(m.HWnd, ref info))
                                DrawComboButton(graphics, Rectangle.FromLTRB(info.Button.Left, info.Button.Top, info.Button.Right, info.Button.Bottom));
                        }
                        if (_field) DrawFieldBorder(graphics, _combo.Size, _combo.ContainsFocus);
                        else DrawSearchBorder(graphics, _combo.Size);
                    }
                }
                finally { ReleaseDC(m.HWnd, dc); }
            }
            public void Dispose()
            {
                _combo.HandleCreated -= OnCreated;
                _combo.HandleDestroyed -= OnDestroyed;
                _combo.GotFocus -= OnFocus;
                _combo.LostFocus -= OnFocus;
                ReleaseHandle();
                if (!_combo.IsDisposed) _combo.Invalidate(true);
            }
        }

        internal static void DrawSearchBorder(Graphics graphics, Size size)
        {
            if (size.Width < 2 || size.Height < 2) return;
            using (var pen = new Pen(Color.FromArgb(65,65,65)))
                graphics.DrawRectangle(pen, 0, 0, size.Width - 1, size.Height - 1);
        }
        internal static void DrawFieldBorder(Graphics graphics, Size size, bool focused)
        {
            if (size.Width < 4 || size.Height < 4) return;
            using (var pen = new Pen(focused ? Color.FromArgb(56,101,138) : Color.FromArgb(65,65,65)))
            {
                graphics.DrawRectangle(pen, 0, 0, size.Width-1, size.Height-1);
                graphics.DrawRectangle(pen, 1, 1, size.Width-3, size.Height-3);
            }
        }
        internal static void DrawComboButton(Graphics graphics, Rectangle bounds)
        {
            if (bounds.Width < 4 || bounds.Height < 4) return;
            using (var brush = new SolidBrush(Color.FromArgb(37,37,38))) graphics.FillRectangle(brush, bounds);
            using (var pen = new Pen(Color.FromArgb(65,65,65)))
                graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1);
            int x = bounds.Left + bounds.Width/2, y = bounds.Top + bounds.Height/2;
            using (var brush = new SolidBrush(Color.FromArgb(190,190,190)))
                graphics.FillPolygon(brush, new Point[] { new Point(x-3,y-1), new Point(x+3,y-1), new Point(x,y+2) });
        }
    }
}
