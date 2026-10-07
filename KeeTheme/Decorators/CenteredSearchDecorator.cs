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
            // WM_NCPAINT owns both the frame and scrollbars. Suppressing it on
            // multiline editors leaves stale pixels until focus changes.
            private bool HasNativeScrollBars
            {
                get
                {
                    var text = _combo as TextBox;
                    return text != null && text.Multiline && text.ScrollBars != ScrollBars.None;
                }
            }
            private readonly IntPtr _backgroundBrush = CreateSolidBrush(0x00262525);
            private CalendarBorderWindow _calendarBorder;
            private CalendarBorderWindow _calendarHeader;
            [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr hwnd);
            [DllImport("user32.dll", EntryPoint="GetWindowLongW")] private static extern int GetWindowLong(IntPtr hwnd,int index);
            [DllImport("gdi32.dll")] private static extern IntPtr CreateSolidBrush(int color);
            [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
            [DllImport("gdi32.dll")] private static extern int SetBkColor(IntPtr dc, int color);
            [DllImport("gdi32.dll")] private static extern int SetTextColor(IntPtr dc, int color);
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
                var picker = combo as DateTimePicker;
                if (picker != null) { picker.DropDown += OnCalendarOpened; picker.CloseUp += OnCalendarClosed; }
                IntPtr handle = combo.Handle;
                if (Handle == IntPtr.Zero) AssignHandle(handle);
            }
            private void OnCreated(object sender, EventArgs e) { AssignHandle(_combo.Handle); }
            private void OnDestroyed(object sender, EventArgs e) { ReleaseHandle(); }
            private void OnFocus(object sender, EventArgs e) { _combo.Invalidate(); }
            private void OnCalendarOpened(object sender, EventArgs e)
            {
                OnCalendarClosed(sender,e);
                IntPtr calendar = ListViewNativeWindow.SendMessage(_combo.Handle,0x1008,IntPtr.Zero,IntPtr.Zero);
                if (calendar != IntPtr.Zero)
                {
                    IntPtr popup = GetParent(calendar);
                    // DateTimePicker hosts SysMonthCal32 in a separate popup window.
                    if (popup == IntPtr.Zero || (GetWindowLong(popup,-16) & unchecked((int)0x80000000)) == 0) popup = calendar;
                    _calendarBorder = new CalendarBorderWindow(popup);
                    if (popup != calendar) _calendarHeader = new CalendarBorderWindow(calendar,true);
                }
            }
            private void OnCalendarClosed(object sender, EventArgs e)
            {
                if (_calendarBorder != null) { _calendarBorder.Dispose(); _calendarBorder = null; }
                if (_calendarHeader != null) { _calendarHeader.Dispose(); _calendarHeader = null; }
            }
            protected override void WndProc(ref Message m)
            {
                // The disabled edit child of a ComboBox asks its parent for colors.
                // Returning a dark brush also covers the modal login/locked state.
                if (_combo is ComboBox && (m.Msg == 0x0133 || m.Msg == 0x0138))
                {
                    SetBkColor(m.WParam, 0x00262525);
                    SetTextColor(m.WParam, _combo.Enabled ? 0x00F1F1F1 : 0x00BEBEBE);
                    m.Result = _backgroundBrush;
                    return;
                }
                // Do not let the native edit border flash white before our border.
                // Caret and mouse messages can request non-client paint independently.
                if (_field && _combo is TextBoxBase && !HasNativeScrollBars && m.Msg == 0x0085)
                    m.Result = IntPtr.Zero;
                else base.WndProc(ref m);
                bool dateInteraction = _combo is DateTimePicker && (m.Msg == 0x0007 || m.Msg == 0x0008 || m.Msg == 0x0100 || m.Msg == 0x0101 || m.Msg == 0x0201 || m.Msg == 0x0202);
                if (m.Msg != 0x000F && m.Msg != 0x0085 && !dateInteraction) return;
                IntPtr dc = GetWindowDC(m.HWnd);
                if (dc == IntPtr.Zero) return;
                try
                {
                    using (var graphics = Graphics.FromHdc(dc))
                    {
                        var date = _combo as DateTimePicker;
                        if (date != null)
                            DrawDateField(graphics, date.ClientRectangle, date.Text, date.Font, date.Enabled);
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
                var picker = _combo as DateTimePicker;
                if (picker != null) { picker.DropDown -= OnCalendarOpened; picker.CloseUp -= OnCalendarClosed; }
                OnCalendarClosed(null,EventArgs.Empty);
                ReleaseHandle();
                if (!_combo.IsDisposed) _combo.Invalidate(true);
            }
        }

        private sealed class CalendarBorderWindow : NativeWindow, IDisposable
        {
            private readonly bool _weekdays;
            [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left,Top,Right,Bottom; }
            [StructLayout(LayoutKind.Sequential)] private struct HitInfo {
                public uint Size; public int X,Y; public uint Hit;
                public ushort Year,Month,DayOfWeek,Day,Hour,Minute,Second,Milliseconds;
            }
            [DllImport("user32.dll",EntryPoint="SendMessageW")] private static extern IntPtr HitTest(IntPtr hwnd,int msg,IntPtr w,ref HitInfo info);
            [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hwnd,out Rect rect);
            [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
            [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hwnd);
            [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd,IntPtr dc);
            internal CalendarBorderWindow(IntPtr handle) : this(handle,false) { }
            internal CalendarBorderWindow(IntPtr handle,bool weekdays) { _weekdays=weekdays; AssignHandle(handle); }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg != 0x000F && m.Msg != 0x0085) return;
                Rect rect;
                if (!GetWindowRect(m.HWnd,out rect)) return;
                IntPtr dc=GetWindowDC(m.HWnd);
                if(dc==IntPtr.Zero)return;
                try {
                    using(var graphics=Graphics.FromHdc(dc)) {
                        int width=rect.Right-rect.Left,height=rect.Bottom-rect.Top;
                        if (_weekdays) { DrawWeekdays(graphics,m.HWnd); return; }
                        // Cover the classic raised edge, then draw one gray line.
                        using(var pen=new Pen(Color.FromArgb(37,37,38),3))
                            graphics.DrawRectangle(pen,1,1,width-3,height-3);
                        using(var pen=new Pen(Color.FromArgb(65,65,65)))
                            graphics.DrawRectangle(pen,0,0,width-1,height-1);
                    }
                } finally { ReleaseDC(m.HWnd,dc); }
            }
            private static void DrawWeekdays(Graphics graphics,IntPtr hwnd)
            {
                Rect client;
                if(!GetClientRect(hwnd,out client))return;
                int top=-1,bottom=-1;
                // Use native hit testing rather than guessing header height/DPI.
                for(int y=0;y<client.Bottom;y++) {
                    var hit=new HitInfo(); hit.Size=(uint)Marshal.SizeOf(typeof(HitInfo)); hit.X=client.Right/2; hit.Y=y;
                    HitTest(hwnd,0x100E,IntPtr.Zero,ref hit);
                    if(hit.Hit==0x20002) { if(top<0)top=y; bottom=y+1; }
                    else if(top>=0)break;
                }
                if(top<0)return;
                int first=ListViewNativeWindow.SendMessage(hwnd,0x1010,IntPtr.Zero,IntPtr.Zero).ToInt32() & 0xFFFF;
                var names=System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
                int left=0,index=0;
                while(left<client.Right && index<7) {
                    var hit=new HitInfo(); hit.Size=(uint)Marshal.SizeOf(typeof(HitInfo)); hit.X=left; hit.Y=top;
                    HitTest(hwnd,0x100E,IntPtr.Zero,ref hit);
                    if(hit.Hit!=0x20002){left++;continue;}
                    int start=left;
                    // Header cells share the hit code; their widths follow the date grid.
                    int width=Math.Max(1,(client.Right-start)/ (7-index));
                    var bounds=new Rectangle(start,top,width,bottom-top);
                    using(var brush=new SolidBrush(Color.FromArgb(37,37,38)))graphics.FillRectangle(brush,bounds);
                    TextRenderer.DrawText(graphics,names[(first+1+index)%7],SystemFonts.MenuFont,bounds,Color.FromArgb(190,190,190),
                        TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
                    left=start+width;index++;
                }
            }
            public void Dispose() { ReleaseHandle(); }
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
        internal static void DrawDateField(Graphics graphics, Rectangle bounds, string text, Font font, bool enabled)
        {
            using (var brush = new SolidBrush(Color.FromArgb(37,37,38))) graphics.FillRectangle(brush, bounds);
            int buttonWidth = SystemInformation.VerticalScrollBarWidth + 4;
            var button = new Rectangle(bounds.Right-buttonWidth, bounds.Top, buttonWidth, bounds.Height);
            var content = new Rectangle(bounds.Left+3,bounds.Top,Math.Max(0,bounds.Width-buttonWidth-6),bounds.Height);
            TextRenderer.DrawText(graphics,text,font,content,enabled ? Color.FromArgb(241,241,241) : Color.FromArgb(190,190,190),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            DrawComboButton(graphics,button);
        }
    }
}
