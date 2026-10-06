param([string]$PluginPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
[Reflection.Assembly]::LoadFrom('C:\Program Files\KeePass Password Safe 2\KeePass.exe') | Out-Null
$a=[Reflection.Assembly]::LoadFrom($PluginPath)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$ini=$a.GetType('KeeTheme.TemplateReader').GetMethod('GetFromResources',$f).Invoke($null,@('KeeTheme.Resources.ModernDark.ini'))
$template=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomThemeTemplate'),$f,$null,@($ini),$null)
$theme=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomTheme'),$f,$null,@($template),$null)
$form=New-Object Windows.Forms.Form
$lv=New-Object Windows.Forms.ListView
$lv.View=[Windows.Forms.View]::Details; $lv.Size=New-Object Drawing.Size(300,250)
$form.Controls.Add($lv)
$lv.Columns.Add('Title',100) | Out-Null
$lv.Columns.Add('Username',100) | Out-Null
$d=[Activator]::CreateInstance($a.GetType('KeeTheme.Decorators.ListViewDecorator'),$f,$null,@($lv.PSObject.BaseObject,$theme.PSObject.BaseObject),$null)
$enable=$d.GetType().GetMethod('EnableTheme',$f)
$paint=$d.GetType().GetMethod('HandleBackgroundPaint',$f)
$enable.Invoke($d,@($true,$theme.PSObject.BaseObject)) | Out-Null
$bmp=New-Object Drawing.Bitmap(300,250); $g=[Drawing.Graphics]::FromImage($bmp)
$event=New-Object Windows.Forms.PaintEventArgs($g,$lv.ClientRectangle)
$sentinel=[Drawing.Color]::Magenta
foreach($count in @(0,1,50)){
 $lv.Items.Clear()
 for($i=0;$i -lt $count;$i++){$lv.Items.Add('Entry '+$i) | Out-Null}
 $g.Clear($sentinel)
 $paint.Invoke($d,@($null,$event.PSObject.BaseObject)) | Out-Null
 if($bmp.GetPixel(50,0).ToArgb() -ne $sentinel.ToArgb()){throw 'Header overwritten'}
 if($count -lt 2){
  if($bmp.GetPixel(98,220).ToArgb() -ne $theme.ListView.BackColor.ToArgb()){throw 'Empty-area divider not covered'}
  if($count -eq 1 -and $bmp.GetPixel(50,$lv.Items[0].Bounds.Top+2).ToArgb() -ne $sentinel.ToArgb()){throw 'Entry overwritten'}
 }elseif($bmp.GetPixel(98,220).ToArgb() -ne $sentinel.ToArgb()){throw 'Visible rows overwritten in long list'}
}
$lv.Items.Clear(); $theme.ListView.ShowColumnSeparators=$true
$g.Clear($sentinel); $paint.Invoke($d,@($null,$event.PSObject.BaseObject)) | Out-Null
if($bmp.GetPixel(98,220).ToArgb() -ne $sentinel.ToArgb()){throw 'Legacy theme altered'}
$theme.ListView.ShowColumnSeparators=$false
$enable.Invoke($d,@($false,$theme.PSObject.BaseObject)) | Out-Null
$g.Clear($sentinel); $paint.Invoke($d,@($null,$event.PSObject.BaseObject)) | Out-Null
if($bmp.GetPixel(98,220).ToArgb() -ne $sentinel.ToArgb()){throw 'Disabled theme altered'}
$g.Dispose();$bmp.Dispose();$form.Dispose()
Write-Output 'PASS unused report area covered, headers/entries/long lists/legacy/disabled themes preserved'
