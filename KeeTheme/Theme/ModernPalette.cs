using System.Drawing;
namespace KeeTheme.Theme
{
    // Modern Dark remains byte-for-byte identical; only the additional Gray
    // template maps the existing modern chrome to its own neutral palette.
    internal sealed class ModernPalette
    {
        internal readonly bool Gray;
        internal ModernPalette(bool gray) { Gray=gray; }
        internal static ModernPalette ForTheme(string name) { return new ModernPalette(name=="Modern Gray"); }
        internal Color Color(Color original)
        {
            if(!Gray)return original;
            int value=original.ToArgb();
            if(value==System.Drawing.Color.FromArgb(37,37,38).ToArgb())return System.Drawing.Color.FromArgb(184,184,184);
            if(value==System.Drawing.Color.FromArgb(241,241,241).ToArgb())return System.Drawing.Color.FromArgb(32,32,32);
            if(value==System.Drawing.Color.FromArgb(190,190,190).ToArgb())return System.Drawing.Color.FromArgb(80,80,80);
            if(value==System.Drawing.Color.FromArgb(56,101,138).ToArgb())return System.Drawing.Color.FromArgb(160,176,192);
            if(value==System.Drawing.Color.FromArgb(45,45,48).ToArgb())return System.Drawing.Color.FromArgb(176,176,176);
            if(value==System.Drawing.Color.FromArgb(73,73,78).ToArgb())return System.Drawing.Color.FromArgb(152,152,152);
            if(value==System.Drawing.Color.FromArgb(62,62,66).ToArgb())return System.Drawing.Color.FromArgb(164,164,164);
            if(value==System.Drawing.Color.FromArgb(110,110,110).ToArgb())return System.Drawing.Color.FromArgb(96,96,96);
            if(value==System.Drawing.Color.FromArgb(65,65,65).ToArgb() || value==System.Drawing.Color.FromArgb(65,65,69).ToArgb())return System.Drawing.Color.FromArgb(128,128,128);
            return original;
        }
    }
}
