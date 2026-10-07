using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeeTheme.Decorators
{
    class RichTextBoxNativeWindow : NativeWindow
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct CHARFORMAT2
        {
            public UInt32 cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public UInt32 crTextColor;
            public Byte bCharSet;
            public Byte bPitchAndFamily;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szFaceName;

            public UInt16 wWeight;
            public UInt16 sSpacing;
            public Int32 crBackColor;
            public Int32 lcid;
            public UInt32 dwReserved;
            public Int16 sStyle;
            public Int16 wKerning;
            public Byte bUnderlineType;
            public Byte bAnimation;
            public Byte bRevAuthor;
            public Byte bReserved1;
        }

        private const int WM_SETFOCUS = 0x0007;
        private const int WM_ENABLE = 0x000A;
        private const int WM_PAINT = 0x000F;
        private const int WM_SETCURSOR = 0x0020;
        private const int WM_USER = 0x0400;
        private const int EM_SETCHARFORMAT = WM_USER + 68;
        private const uint CFE_LINK = 0x0020;
        
        private readonly RichTextBox _richTextBox;
        private bool _enabled;
        
        internal bool ModernScrollBars { get; set; }
        [StructLayout(LayoutKind.Sequential)] private struct RECT { public int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] private struct SCROLLINFO { public int Size, Mask, Min, Max, Page, Pos, Track; }
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr h, int index);
        [DllImport("user32.dll")] private static extern bool GetScrollInfo(IntPtr h, int bar, ref SCROLLINFO info);
        [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr h);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr h, IntPtr dc);
        private void PaintScrollBars()
        {
            if (!ModernScrollBars || Handle == IntPtr.Zero) return;
            RECT window, client;
            if (!GetWindowRect(Handle,out window) || !GetClientRect(Handle,out client)) return;
            int style = GetWindowLong(Handle,-16);
            IntPtr dc=GetWindowDC(Handle); if(dc == IntPtr.Zero) return;
            try
            {
                using(var g=Graphics.FromHdc(dc))
                {
                    // Borderless rich edit: the native scrollbar occupies the non-client
                    // strip beyond ClientSize. Keep native input, range and accessibility.
                    if((style & 0x00200000) != 0)
                    {
                        var info=new SCROLLINFO();info.Size=Marshal.SizeOf(typeof(SCROLLINFO));info.Mask=0x17;
                        if(GetScrollInfo(Handle,1,ref info))
                            DrawScrollBar(g,new Rectangle(client.Right,0,window.Right-window.Left-client.Right,client.Bottom),true,info.Min,info.Max,info.Page,info.Pos);
                    }
                    if((style & 0x00100000) != 0)
                    {
                        var info=new SCROLLINFO();info.Size=Marshal.SizeOf(typeof(SCROLLINFO));info.Mask=0x17;
                        if(GetScrollInfo(Handle,0,ref info))
                            DrawScrollBar(g,new Rectangle(0,client.Bottom,client.Right,window.Bottom-window.Top-client.Bottom),false,info.Min,info.Max,info.Page,info.Pos);
                    }
                }
            }
            finally {ReleaseDC(Handle,dc);}
        }
        internal static void DrawScrollBar(Graphics g,Rectangle r,bool vertical,int min,int max,int page,int pos)
        {
            if(r.Width<=0 || r.Height<=0) return;
            using(var b=new SolidBrush(Color.FromArgb(37,37,38)))g.FillRectangle(b,r);
            int length=vertical?r.Height:r.Width, width=vertical?r.Width:r.Height;
            int arrow=Math.Min(width,length/2),track=Math.Max(0,length-arrow*2);
            long range=Math.Max(1,(long)max-min+1);
            int thumb=Math.Min(track,Math.Max(width,(int)(track*Math.Min(range,Math.Max(0,page))/range)));
            long travel=Math.Max(1,range-Math.Max(1,page));
            int offset=arrow+(int)((track-thumb)*Math.Max(0,Math.Min(travel,(long)pos-min))/travel);
            if(range>page && track>0)
                using(var b=new SolidBrush(Color.FromArgb(100,100,100)))
                    g.FillRectangle(b,vertical?new Rectangle(r.X+4,r.Y+offset,Math.Max(2,width-8),thumb):new Rectangle(r.X+offset,r.Y+4,thumb,Math.Max(2,width-8)));
            using(var p=new Pen(Color.FromArgb(190,190,190),1.4f))
            {
                int cx=r.X+r.Width/2,cy=r.Y+r.Height/2;
                if(vertical)
                {
                    int y=r.Y+arrow/2;g.DrawLines(p,new Point[]{new Point(cx-3,y+1),new Point(cx,y-2),new Point(cx+3,y+1)});
                    y=r.Bottom-arrow/2;g.DrawLines(p,new Point[]{new Point(cx-3,y-1),new Point(cx,y+2),new Point(cx+3,y-1)});
                }
                else
                {
                    int x=r.X+arrow/2;g.DrawLines(p,new Point[]{new Point(x+1,cy-3),new Point(x-2,cy),new Point(x+1,cy+3)});
                    x=r.Right-arrow/2;g.DrawLines(p,new Point[]{new Point(x-1,cy-3),new Point(x+2,cy),new Point(x-1,cy+3)});
                }
            }
        }
        internal event PaintEventHandler Paint;
        internal event EventHandler LinkCreated; 

        public RichTextBoxNativeWindow(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
            _enabled = richTextBox.Enabled;
            if (TryAssignHandle(_richTextBox.Handle))
            {
                _richTextBox.HandleCreated += HandleRichTextBoxHandleCreated;
                _richTextBox.HandleDestroyed += HandleRichTextBoxHandleDestroyed;
            }
        }

        private bool TryAssignHandle(IntPtr handle)
        {
            try
            {
                AssignHandle(handle);
                return true;
            }
            catch (InvalidOperationException e)
            {
                return false;
            }
        }
        
        private void HandleRichTextBoxHandleCreated(object sender, EventArgs e)
        {
            if (_richTextBox != null) 
                AssignHandle(_richTextBox.Handle);
        }

        private void HandleRichTextBoxHandleDestroyed(object sender, EventArgs e)
        {
            ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ENABLE)
            {
                _enabled = m.WParam != IntPtr.Zero;
                m.WParam = new IntPtr(1);
            }

            if (_enabled || m.Msg != WM_SETFOCUS && m.Msg != WM_SETCURSOR)
            {
                base.WndProc(ref m);
            }

            if (m.Msg == WM_PAINT || m.Msg == 0x0085 || m.Msg == 0x0114 || m.Msg == 0x0115 || m.Msg == 0x0005)
                PaintScrollBars();
            if (m.Msg == EM_SETCHARFORMAT)
            {
                var cf = (CHARFORMAT2) Marshal.PtrToStructure(m.LParam, typeof(CHARFORMAT2));
                if ((cf.dwEffects & CFE_LINK) != 0)
                {
                    var args = new EventArgs();
                    OnLinkCreated(args);
                }
            }
            
            if (m.Msg == WM_PAINT)
            {
                using (var g = Graphics.FromHwnd(m.HWnd))
                {
                    var rect = new Rectangle(0, 0, _richTextBox.ClientSize.Width, _richTextBox.ClientSize.Height);
                    var args = new PaintEventArgs(g, rect);
                    OnPaint(args);
                }
            }
        }

        protected virtual void OnLinkCreated(EventArgs e)
        {
            if (LinkCreated != null)
                LinkCreated.Invoke(_richTextBox, e);
        }
        
        protected virtual void OnPaint(PaintEventArgs e)
        {
            if (Paint != null)
                Paint.Invoke(_richTextBox, e);
        }
    }
}