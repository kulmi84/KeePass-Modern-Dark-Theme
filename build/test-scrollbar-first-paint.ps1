param([string]$PluginPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
$a=[Reflection.Assembly]::LoadFrom($PluginPath)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class PaintProbe {
 [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h,int m,IntPtr w,IntPtr l);
 [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr dc);
 [DllImport("gdi32.dll")] public static extern int GetBkColor(IntPtr dc);
 [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr dc);
 [DllImport("user32.dll")] public static extern IntPtr GetWindowDC(IntPtr h);
 [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr h,IntPtr dc);
 [DllImport("gdi32.dll")] public static extern int GetPixel(IntPtr dc,int x,int y);
}
"@
$t=$a.GetType('KeeTheme.Decorators.CenteredSearchDecorator+SearchBorderWindow')
$form=New-Object Windows.Forms.Form
$form.StartPosition='Manual';$form.Location=New-Object Drawing.Point(50,50)
$combo=New-Object Windows.Forms.ComboBox;$form.Controls.Add($combo)
$decorator=[Activator]::CreateInstance($t,$f,$null,@($combo.PSObject.BaseObject),$null)
$dc=[PaintProbe]::CreateCompatibleDC([IntPtr]::Zero)
try {
 foreach($enabled in @($true,$false,$true)) {
  $form.Enabled=$enabled
  foreach($msg in @(0x133,0x138)) {
   $brush=[PaintProbe]::SendMessage($combo.Handle,$msg,$dc,[IntPtr]::Zero)
   if($brush -eq [IntPtr]::Zero -or [PaintProbe]::GetBkColor($dc) -ne 0x262525){throw 'Combo child background became light'}
  }
 }
} finally {[PaintProbe]::DeleteDC($dc)|Out-Null;$decorator.Dispose()}
$edit=New-Object Windows.Forms.TextBox;$edit.Location=New-Object Drawing.Point(0,40);$form.Controls.Add($edit)
$decorator=[Activator]::CreateInstance($t,$f,$null,@($edit.PSObject.BaseObject,$true),$null)
$form.Show();[Windows.Forms.Application]::DoEvents()
try {
 for($i=0;$i -lt 20;$i++) {
  [PaintProbe]::SendMessage($edit.Handle,0x85,[IntPtr]1,[IntPtr]::Zero)|Out-Null
  $dc=[PaintProbe]::GetWindowDC($edit.Handle)
  try {
   $pixel=[PaintProbe]::GetPixel($dc,0,0)
   if($pixel -ne 0x414141 -and $pixel -ne 0x8A6538){throw ('Native frame resurfaced: '+$pixel)}
  } finally {[PaintProbe]::ReleaseDC($edit.Handle,$dc)|Out-Null}
 }
} finally {$decorator.Dispose();$form.Dispose()}
'PASS modal/disabled ComboBox child colors and repeated native frame repaint'


$form=New-Object Windows.Forms.Form;$form.StartPosition='Manual';$form.Location=New-Object Drawing.Point(50,50)
$notes=New-Object Windows.Forms.TextBox;$notes.Multiline=$true;$notes.ScrollBars='Vertical';$notes.Size=New-Object Drawing.Size(220,120);$notes.Text=(1..80|ForEach-Object {'Synthetic demo line'}) -join "`r`n";$notes.BackColor=[Drawing.Color]::FromArgb(37,37,38);$form.Controls.Add($notes)
$decorator=[Activator]::CreateInstance($t,$f,$null,@($notes.PSObject.BaseObject,$true),$null)
try {
 $form.Show();[Windows.Forms.Application]::DoEvents()
 for($i=0;$i -lt 3;$i++) {
  [PaintProbe]::SendMessage($notes.Handle,0x85,[IntPtr]1,[IntPtr]::Zero)|Out-Null
  $dc=[PaintProbe]::GetWindowDC($notes.Handle)
  try {
   $color=[PaintProbe]::GetPixel($dc,$notes.Width-8,35)
   if($color -eq -1 -or ($color -band 255) -gt 190){throw ('First scrollbar paint light: '+$color)}
  } finally {[PaintProbe]::ReleaseDC($notes.Handle,$dc)|Out-Null}
  [PaintProbe]::SendMessage($notes.Handle,0x115,[IntPtr]1,[IntPtr]::Zero)|Out-Null
 }
 if($notes.Text -notmatch 'Synthetic demo line' -or $notes.SelectionStart -ne 0){throw 'Text/selection changed'}
} finally {$decorator.Dispose();$form.Dispose()}
'PASS multiline TextBox initial native scrollbar pixels and subsequent scroll repaint'