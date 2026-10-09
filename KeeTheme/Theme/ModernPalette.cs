using System.Drawing;
namespace KeeTheme.Theme
{
    // Modern Dark remains byte-for-byte identical; only the additional Gray
    // template maps the existing modern chrome to its own neutral palette.
    internal sealed class ModernPalette
    {
        internal readonly bool Gray;
        internal bool Green;
        internal bool MintFields;
        internal readonly bool WhiteFields;
        internal ModernPalette(bool gray) : this(gray,false) { }
        internal ModernPalette(bool gray,bool whiteFields) { Gray=gray; WhiteFields=whiteFields; }
        internal static ModernPalette ForTheme(string name) { return new ModernPalette(name=="Modern Gray") { Green=name=="Modern Green" }; }
        internal Color Color(Color original)
        {
            if(Green) {
                int mintValue=original.ToArgb();
                if(mintValue==System.Drawing.Color.FromArgb(37,37,38).ToArgb())return MintFields ? System.Drawing.Color.FromArgb(129,241,180) : System.Drawing.Color.FromArgb(227,255,240);
                if(mintValue==System.Drawing.Color.FromArgb(241,241,241).ToArgb())return System.Drawing.Color.FromArgb(23,59,44);
                if(mintValue==System.Drawing.Color.FromArgb(190,190,190).ToArgb())return System.Drawing.Color.FromArgb(68,104,86);
                if(mintValue==System.Drawing.Color.FromArgb(56,101,138).ToArgb())return System.Drawing.Color.FromArgb(129,241,180);
                if(mintValue==System.Drawing.Color.FromArgb(73,73,78).ToArgb())return System.Drawing.Color.FromArgb(67,125,101);
                if(mintValue==System.Drawing.Color.FromArgb(62,62,66).ToArgb())return System.Drawing.Color.FromArgb(58,106,89);
                if(mintValue==System.Drawing.Color.FromArgb(65,65,65).ToArgb() || mintValue==System.Drawing.Color.FromArgb(65,65,69).ToArgb())return System.Drawing.Color.FromArgb(79,125,103);
                return original;
            }
            if(!Gray)return original;
            int value=original.ToArgb();
            if(value==System.Drawing.Color.FromArgb(37,37,38).ToArgb())return WhiteFields ? System.Drawing.Color.FromArgb(145,145,145) : System.Drawing.Color.FromArgb(135,135,135);
            if(value==System.Drawing.Color.FromArgb(241,241,241).ToArgb())return System.Drawing.Color.FromArgb(16,16,16);
            if(value==System.Drawing.Color.FromArgb(190,190,190).ToArgb())return System.Drawing.Color.FromArgb(48,48,48);
            if(value==System.Drawing.Color.FromArgb(56,101,138).ToArgb())return System.Drawing.Color.FromArgb(160,176,192);
            if(value==System.Drawing.Color.FromArgb(45,45,48).ToArgb())return System.Drawing.Color.FromArgb(127,127,127);
            if(value==System.Drawing.Color.FromArgb(73,73,78).ToArgb())return System.Drawing.Color.FromArgb(111,111,111);
            if(value==System.Drawing.Color.FromArgb(62,62,66).ToArgb())return System.Drawing.Color.FromArgb(119,119,119);
            if(value==System.Drawing.Color.FromArgb(110,110,110).ToArgb())return System.Drawing.Color.FromArgb(72,72,72);
            if(value==System.Drawing.Color.FromArgb(65,65,65).ToArgb() || value==System.Drawing.Color.FromArgb(65,65,69).ToArgb())return System.Drawing.Color.FromArgb(96,96,96);
            return original;
        }
    }
}
