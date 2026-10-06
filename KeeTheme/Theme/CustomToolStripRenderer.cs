using System.Drawing;
using System.Windows.Forms;
using KeePass.UI.ToolStripRendering;

namespace KeeTheme.Theme
{
	class CustomToolStripRenderer : ProExtTsr
	{
		private readonly CustomTheme _customTheme;

		protected override bool EnsureTextContrast
		{
			get { return false; }
		}

		public CustomToolStripRenderer(CustomTheme customTheme, ProfessionalColorTable ct) : base(ct)
		{
			_customTheme = customTheme;
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			if (!e.Item.Enabled && !_customTheme.MenuItem.DisabledForeColor.IsEmpty)
                e.TextColor = _customTheme.MenuItem.DisabledForeColor;
            else if (e.Item.Pressed || e.Item.Selected)
			{
				e.TextColor = _customTheme.MenuItem.HighlightColor;
			}

			base.OnRenderItemText(e);
		}

		        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            // Restrict replacement to known KeePass commands on the KeePass main form.
            // Never modify Item.Image, shared ImageLists or database custom icons.
            var owner = e.Item.Owner;
            var dropDown = owner as ToolStripDropDown;
            while (dropDown != null && dropDown.OwnerItem != null)
            {
                owner = dropDown.OwnerItem.Owner;
                dropDown = owner as ToolStripDropDown;
            }
            var form = owner == null ? null : owner.FindForm();
            if (!_customTheme.MenuItem.ModernIcons || form == null ||
                form.GetType().FullName != "KeePass.Forms.MainForm")
            {
                base.OnRenderItemImage(e);
                return;
            }
            var name = e.Item.Name;
            if (name != "m_tbNewDatabase" && name != "m_tbOpenDatabase" &&
                name != "m_tbSaveDatabase" && name != "m_tbAddEntry" &&
                name != "m_tbLockWorkspace" && name != "m_menuFileNew" &&
                name != "m_menuFileOpen" && name != "m_menuFileSave")
            {
                base.OnRenderItemImage(e);
                return;
            }
            var state = e.Graphics.Save();
            try
            {
                var r = e.ImageRectangle;
                if (r.Width <= 0 || r.Height <= 0) return;
                e.Graphics.TranslateTransform(r.X, r.Y);
                e.Graphics.ScaleTransform(r.Width / 16f, r.Height / 16f);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var color = e.Item.Enabled ? _customTheme.MenuItem.ForeColor :
                    _customTheme.MenuItem.DisabledForeColor;
                if (color.IsEmpty) color = Color.FromArgb(190, 190, 190);
                using (var pen = new Pen(color, 1.4f))
                {
                    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    if (name == "m_tbAddEntry")
                    {
                        e.Graphics.DrawLine(pen, 8, 3, 8, 13);
                        e.Graphics.DrawLine(pen, 3, 8, 13, 8);
                    }
                    else if (name == "m_tbLockWorkspace")
                    {
                        e.Graphics.DrawArc(pen, 5, 2, 6, 8, 180, 180);
                        e.Graphics.DrawRectangle(pen, 3, 7, 10, 7);
                        e.Graphics.DrawLine(pen, 8, 10, 8, 12);
                    }
                    else if (name == "m_tbSaveDatabase" || name == "m_menuFileSave")
                    {
                        e.Graphics.DrawRectangle(pen, 2, 2, 12, 12);
                        e.Graphics.DrawRectangle(pen, 5, 2, 6, 4);
                        e.Graphics.DrawRectangle(pen, 5, 9, 6, 5);
                    }
                    else if (name == "m_tbOpenDatabase" || name == "m_menuFileOpen")
                    {
                        e.Graphics.DrawLines(pen, new PointF[] { new PointF(2,13),
                            new PointF(2,4), new PointF(6,4), new PointF(8,6),
                            new PointF(14,6), new PointF(12,13), new PointF(2,13) });
                        e.Graphics.DrawLine(pen, 2, 8, 13, 8);
                    }
                    else
                    {
                        e.Graphics.DrawLines(pen, new PointF[] { new PointF(3,14),
                            new PointF(3,2), new PointF(9,2), new PointF(13,6),
                            new PointF(13,14), new PointF(3,14) });
                        e.Graphics.DrawLine(pen, 9, 2, 9, 6);
                        e.Graphics.DrawLine(pen, 9, 6, 13, 6);
                    }
                }
            }
            finally { e.Graphics.Restore(state); }
        }
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			var ms = e.ToolStrip as MenuStrip;
			if (ms != null) 
			{
				using (var menuBackgroundBrush = new SolidBrush(_customTheme.MenuItem.BackColor))
				{
					e.Graphics.FillRectangle(menuBackgroundBrush, e.AffectedBounds);
				}
			} 
			else 
			{
				base.OnRenderToolStripBackground(e);
			}
		}
	}
}
