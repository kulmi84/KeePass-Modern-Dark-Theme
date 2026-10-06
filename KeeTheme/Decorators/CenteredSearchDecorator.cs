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

        private sealed class SearchBorderWindow : NativeWindow, IDisposable
        {
            private readonly ComboBox _combo;
            [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hwnd);
            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
            internal SearchBorderWindow(ComboBox combo)
            {
                _combo = combo;
                combo.HandleCreated += OnCreated;
                combo.HandleDestroyed += OnDestroyed;
                IntPtr handle = combo.Handle;
                if (Handle == IntPtr.Zero) AssignHandle(handle);
            }
            private void OnCreated(object sender, EventArgs e) { AssignHandle(_combo.Handle); }
            private void OnDestroyed(object sender, EventArgs e) { ReleaseHandle(); }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg != 0x000F && m.Msg != 0x0085) return;
                IntPtr dc = GetWindowDC(m.HWnd);
                if (dc == IntPtr.Zero) return;
                try
                {
                    using (var graphics = Graphics.FromHdc(dc))
                        DrawSearchBorder(graphics, _combo.Size);
                }
                finally { ReleaseDC(m.HWnd, dc); }
            }
            public void Dispose()
            {
                _combo.HandleCreated -= OnCreated;
                _combo.HandleDestroyed -= OnDestroyed;
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
    }
}
