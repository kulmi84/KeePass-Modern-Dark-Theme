using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KeePass.UI.ToolStripRendering;

namespace KeeTheme.Theme
{
    class CustomToolStripRenderer : ProExtTsr
    {
        private readonly CustomTheme _customTheme;
        protected override bool EnsureTextContrast { get { return false; } }
        public CustomToolStripRenderer(CustomTheme theme, ProfessionalColorTable table) : base(table)
        { _customTheme = theme; }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (!e.Item.Enabled && !_customTheme.MenuItem.DisabledForeColor.IsEmpty)
                e.TextColor = _customTheme.MenuItem.DisabledForeColor;
            else if (e.Item.Pressed || e.Item.Selected)
                e.TextColor = _customTheme.MenuItem.HighlightColor;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            var owner = e.Item.Owner;
            var dropdown = owner as ToolStripDropDown;
            while (dropdown != null && dropdown.OwnerItem != null)
            {
                owner = dropdown.OwnerItem.Owner;
                dropdown = owner as ToolStripDropDown;
            }
            var form = owner == null ? null : owner.FindForm();
            var color = e.Item.Enabled ? _customTheme.MenuItem.ForeColor : _customTheme.MenuItem.DisabledForeColor;
            if (color.IsEmpty) color = Color.FromArgb(190, 190, 190);
            if (!_customTheme.MenuItem.ModernIcons || form == null ||
                form.GetType().FullName != "KeePass.Forms.MainForm" ||
                !ModernToolbarIcons.Draw(e.Graphics, e.ImageRectangle, e.Item.Name, color))
                base.OnRenderItemImage(e);
        }

        private bool DrawToolbarButton(ToolStripItemRenderEventArgs e, bool isChecked)
        {
            if (!_customTheme.MenuItem.ModernIcons || e.ToolStrip is ToolStripDropDown || e.ToolStrip is MenuStrip)
                return false;
            if (!e.Item.Selected && !e.Item.Pressed && !isChecked) return true;
            var rect = new RectangleF(1, 1, e.Item.Width - 2, e.Item.Height - 2);
            if (rect.Width <= 0 || rect.Height <= 0) return true;
            float radius = System.Math.Min(4f * e.Item.Height / 24f, System.Math.Min(rect.Width, rect.Height) / 2);
            var state = e.Graphics.Save();
            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath())
                using (var brush = new SolidBrush(e.Item.Pressed ? Color.FromArgb(73,73,78) : Color.FromArgb(62,62,66)))
                {
                    float d = radius * 2;
                    path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    path.AddArc(rect.Right-d, rect.Y, d, d, 270, 90);
                    path.AddArc(rect.Right-d, rect.Bottom-d, d, d, 0, 90);
                    path.AddArc(rect.X, rect.Bottom-d, d, d, 90, 90);
                    path.CloseFigure();
                    e.Graphics.FillPath(brush, path);
                }
            }
            finally { e.Graphics.Restore(state); }
            return true;
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var button = e.Item as ToolStripButton;
            if (!DrawToolbarButton(e, button != null && button.Checked)) base.OnRenderButtonBackground(e);
        }
        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        { if (!DrawToolbarButton(e, false)) base.OnRenderDropDownButtonBackground(e); }
        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (!DrawToolbarButton(e, false)) { base.OnRenderSplitButtonBackground(e); return; }
            var button = (ToolStripSplitButton)e.Item;
            OnRenderArrow(new ToolStripArrowRenderEventArgs(e.Graphics, button, button.DropDownButtonBounds,
                _customTheme.MenuItem.ForeColor, ArrowDirection.Down));
        }
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (_customTheme.MenuItem.ModernIcons)
                e.ArrowColor = e.Item.Enabled ? _customTheme.MenuItem.ForeColor : _customTheme.MenuItem.DisabledForeColor;
            base.OnRenderArrow(e);
        }
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (!_customTheme.MenuItem.ModernIcons) { base.OnRenderSeparator(e); return; }
            using (var pen = new Pen(Color.FromArgb(65,65,69)))
            {
                if (e.Vertical) e.Graphics.DrawLine(pen,e.Item.Width/2,5,e.Item.Width/2,e.Item.Height-5);
                else e.Graphics.DrawLine(pen,6,e.Item.Height/2,e.Item.Width-6,e.Item.Height/2);
            }
        }
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is MenuStrip || _customTheme.MenuItem.ModernIcons)
            {
                using (var brush = new SolidBrush(_customTheme.MenuItem.BackColor))
                    e.Graphics.FillRectangle(brush,e.AffectedBounds);
            }
            else base.OnRenderToolStripBackground(e);
        }
    }
}
