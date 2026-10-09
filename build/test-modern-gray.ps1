param([string]$PluginPath,[string]$PreviousPluginPath,[string]$PreviewPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
[Windows.Forms.Application]::EnableVisualStyles()
[Reflection.Assembly]::LoadFrom('C:\Program Files\KeePass Password Safe 2\KeePass.exe')|Out-Null
$a=[Reflection.Assembly]::LoadFrom($PluginPath);$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$reader=$a.GetType('KeeTheme.TemplateReader');$templates=$reader.GetMethod('GetTemplatesFromResources',$f).Invoke($null,@())
foreach($name in @('Modern Dark','Modern Gray','Dark Theme','Dark Theme Win11')){if(!($templates|Where-Object {$_.Name -eq $name})){throw "Theme missing: $name"}}
if($reader.GetField('DefaultTemplatePath',$f).GetRawConstantValue() -ne 'KeeTheme.Resources.ModernDark.ini'){throw 'Default theme changed'}
$ini=$reader.GetMethod('GetFromResources',$f).Invoke($null,@('KeeTheme.Resources.ModernGray.ini'))
$template=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomThemeTemplate'),$f,$null,@($ini),$null)
$theme=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomTheme'),$f,$null,@($template),$null)
if($theme.Form.BackColor.R -ne 176 -or $theme.Control.BackColor.R -ne 184 -or $theme.Form.ForeColor.R -ne 32){throw 'Gray palette incorrect'}
if(!$theme.MenuItem.ModernIcons -or $theme.ScrollBar.UseExplorerDarkMode){throw 'Modern icons or gray scrollbar settings incorrect'}
$round=$template.GetType().GetMethod('GetIniFile',$f).Invoke($template,@())
$round=[Activator]::CreateInstance($template.GetType(),$f,$null,@($round),$null)
if($round.Name -ne 'Modern Gray' -or $round.Form.BackColor.R -ne 176){throw 'Gray editor roundtrip failed'}
if($PreviousPluginPath){
 $old=[Reflection.Assembly]::LoadFile($PreviousPluginPath)
 foreach($resource in @('ModernDark.ini','DarkTheme.ini','DarkThemeWin11.ini')){
  $name='KeeTheme.Resources.'+$resource;$one=$old.GetManifestResourceStream($name);$two=$a.GetManifestResourceStream($name)
  $oneReader=New-Object IO.StreamReader($one);$twoReader=New-Object IO.StreamReader($two)
  try {if($oneReader.ReadToEnd().Replace("`r`n","`n") -ne $twoReader.ReadToEnd().Replace("`r`n","`n")){throw "Existing theme changed: $name"}}
  finally {$oneReader.Dispose();$twoReader.Dispose()}
 }
}
$form=New-Object Windows.Forms.Form;$form.Text='Modern Gray – Demo';$form.Size=New-Object Drawing.Size(720,330)
$form.BackColor=$theme.Form.BackColor;$form.ForeColor=$theme.Form.ForeColor
$toolbar=New-Object Windows.Forms.ToolStrip;$toolbar.BackColor=$theme.MenuItem.BackColor;$toolbar.Renderer=$theme.ToolStripRenderer
$search=New-Object Windows.Forms.ToolStripComboBox;$search.Text='Demo-Suche';$search.Name='m_tbQuickFind'
$search.ComboBox.BackColor=$theme.Control.BackColor;$search.ComboBox.ForeColor=$theme.Control.ForeColor
$toolbar.Items.Add($search)|Out-Null;$form.Controls.Add($toolbar)
$center=[Activator]::CreateInstance($a.GetType('KeeTheme.Decorators.CenteredSearchDecorator'),$f,$null,@($toolbar.PSObject.BaseObject,$search.PSObject.BaseObject,$true),$null)
$tree=New-Object Windows.Forms.TreeView;$tree.Location=New-Object Drawing.Point(12,50);$tree.Size=New-Object Drawing.Size(170,205)
$tree.BackColor=$theme.TreeView.BackColor;$tree.ForeColor=$theme.TreeView.ForeColor
$tree.Nodes.Add('Demo-Datenbank')|Out-Null;$tree.Nodes[0].Nodes.Add('Arbeit')|Out-Null;$tree.Nodes[0].Nodes.Add('Privat')|Out-Null;$tree.Nodes[0].Expand();$form.Controls.Add($tree)
$label=New-Object Windows.Forms.Label;$label.Text='Modern Gray';$label.Location=New-Object Drawing.Point(205,55);$label.AutoSize=$true;$form.Controls.Add($label)
$combo=New-Object Windows.Forms.ComboBox;$combo.Name='m_cmbKeyFile';$combo.Location=New-Object Drawing.Point(205,85);$combo.Width=470
$combo.DropDownStyle='DropDownList';$combo.Items.Add('C:\Demo\Beispiel.keyx')|Out-Null;$combo.SelectedIndex=0
$form.Controls.Add($combo)
$type=$a.GetType('KeeTheme.Decorators.CenteredSearchDecorator+SearchBorderWindow')
$applyType=$a.GetType('KeeTheme.KeeTheme')
$applyInstance=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($applyType)
$mapField=$applyType.GetField('_comboDrawModes',$f);$mapField.SetValue($applyInstance,[Activator]::CreateInstance($mapField.FieldType))
$applyType.GetField('_enabled',$f).SetValue($applyInstance,$true)
$applyCombo=$applyType.GetMethods($f)|Where-Object {$_.Name -eq 'Apply' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType -eq [Windows.Forms.ComboBox]}
$guard=$null
try {
 foreach($gray in @($true,$false,$true)){
  if($guard){$guard.Dispose()}
  $combo.BackColor=if($gray){$theme.Control.BackColor}else{[Drawing.Color]::FromArgb(37,37,38)}
  $combo.ForeColor=if($gray){$theme.Control.ForeColor}else{[Drawing.Color]::FromArgb(241,241,241)}
  $selectedTheme=$theme
  if(!$gray){$darkIni=$reader.GetMethod('GetFromResources',$f).Invoke($null,@('KeeTheme.Resources.ModernDark.ini'));$darkTemplate=[Activator]::CreateInstance($template.GetType(),$f,$null,@($darkIni),$null);$selectedTheme=[Activator]::CreateInstance($theme.GetType(),$f,$null,@($darkTemplate),$null)}
  $applyType.GetField('_theme',$f).SetValue($applyInstance,$selectedTheme)
  $applyCombo.Invoke($applyInstance,@($combo.PSObject.BaseObject))|Out-Null
  $guard=[Activator]::CreateInstance($type,$f,$null,@($combo.PSObject.BaseObject,$true,$gray),$null)
  if($combo.SelectedIndex -ne 0 -or $combo.Text -ne 'C:\Demo\Beispiel.keyx'){throw 'Theme switch changed key-file selection'}
 }
 $paletteType=$a.GetType('KeeTheme.Theme.ModernPalette');$palette=[Activator]::CreateInstance($paletteType,$f,$null,@($true),$null)
 $bitmap=New-Object Drawing.Bitmap(470,25);$g=[Drawing.Graphics]::FromImage($bitmap)
 $draw=$a.GetType('KeeTheme.Decorators.CenteredSearchDecorator').GetMethod('DrawDateFieldWithPalette',$f)
 $bounds=New-Object Drawing.Rectangle(0,0,470,25)
 $draw.Invoke($null,@($g.PSObject.BaseObject,$bounds.PSObject.BaseObject,'09.10.2026 00:00:00',[Drawing.SystemFonts]::MenuFont,$true,$palette))|Out-Null
 if($bitmap.GetPixel(200,12).R -ne 184){throw 'Gray date field still dark'}
 $date=New-Object Windows.Forms.DateTimePicker;$date.Location=New-Object Drawing.Point(205,130);$date.Width=470;$date.Value=New-Object DateTime(2026,10,9);$form.Controls.Add($date)
 $dateGuard=[Activator]::CreateInstance($type,$f,$null,@($date.PSObject.BaseObject,$true,$true),$null)
 $note=New-Object Windows.Forms.Label;$note.Text='Fiktive Daten · vorhandene Themes bleiben erhalten';$note.AutoSize=$true;$note.Location=New-Object Drawing.Point(205,175);$form.Controls.Add($note)
 $form.Show();[Windows.Forms.Application]::DoEvents()
 if($PreviewPath){
  Add-Type -TypeDefinition 'using System;using System.Runtime.InteropServices;public static class GrayCapture{[DllImport("user32.dll")]public static extern bool PrintWindow(IntPtr h,IntPtr dc,uint flags);}'
  $capture=New-Object Drawing.Bitmap($form.Width,$form.Height);$cg=[Drawing.Graphics]::FromImage($capture);$dc=$cg.GetHdc()
  try{if(![GrayCapture]::PrintWindow($form.Handle,$dc,0)){throw 'Isolated window capture failed'}}finally{$cg.ReleaseHdc($dc)}
  try{$capture.Save($PreviewPath)}finally{$cg.Dispose();$capture.Dispose()}
 }
 $g.Dispose();$bitmap.Dispose()
 'PASS four discoverable themes, unchanged default and old theme resources, gray palette/editor roundtrip/date drawing, Gray-Dark-Gray selection preservation'
} finally {if($guard){$guard.Dispose()};if($dateGuard){$dateGuard.Dispose()};$center.Dispose();$form.Dispose()}
