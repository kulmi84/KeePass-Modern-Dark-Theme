param([string]$PluginPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
[Reflection.Assembly]::LoadFrom('C:\Program Files\KeePass Password Safe 2\KeePass.exe')|Out-Null
$a=[Reflection.Assembly]::LoadFrom($PluginPath);$flags=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$ini=$a.GetType('KeeTheme.TemplateReader').GetMethod('GetFromResources',$flags).Invoke($null,@('KeeTheme.Resources.ModernDark.ini'))
$template=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomThemeTemplate'),$flags,$null,@($ini),$null)
$theme=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomTheme'),$flags,$null,@($template),$null)
$type=$a.GetType('KeeTheme.KeeTheme');$instance=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($type)
$type.GetField('_theme',$flags).SetValue($instance,$theme);$type.GetField('_enabled',$flags).SetValue($instance,$true)
$refresh=$type.GetMethod('RefreshInitialFrames',$flags)
$form=New-Object Windows.Forms.Form;$form.StartPosition='Manual';$form.Location=New-Object Drawing.Point(40,40)
$box=New-Object Windows.Forms.RichTextBox;$box.BackColor=[Drawing.Color]::FromArgb(37,37,38);$box.ForeColor=[Drawing.Color]::White;$box.Size=New-Object Drawing.Size(200,120);$box.ScrollBars='ForcedVertical';$box.Text=(1..80|ForEach-Object {'Demo line'}) -join "`n";$form.Controls.Add($box)
try {
 for($i=0;$i -lt 3;$i++){
  $form.Show();$refresh.Invoke($instance,@($form.PSObject.BaseObject))|Out-Null
  [Windows.Forms.Application]::DoEvents()
  if($box.Text.Length -lt 100 -or $box.SelectionStart -ne 0){throw 'Refresh changed content or selection'}
  $form.Hide()
 }
 $type.GetField('_enabled',$flags).SetValue($instance,$false)
 $refresh.Invoke($instance,@($form.PSObject.BaseObject))|Out-Null
} finally {$form.Dispose()}
'PASS initial frame refresh, repeated visibility, content/selection preservation and disabled-theme guard'
