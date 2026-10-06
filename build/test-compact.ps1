param([string]$PluginPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
[Reflection.Assembly]::LoadFrom('C:\Program Files\KeePass Password Safe 2\KeePass.exe') | Out-Null
$a=[Reflection.Assembly]::LoadFrom($PluginPath)
$f=[Reflection.BindingFlags]'Public,NonPublic,Instance,Static'
$ini=$a.GetType('KeeTheme.TemplateReader').GetMethod('GetFromResources',$f).Invoke($null,@('KeeTheme.Resources.ModernDark.ini'))
$template=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomThemeTemplate'),$f,$null,@($ini),$null)
$theme=[Activator]::CreateInstance($a.GetType('KeeTheme.Theme.CustomTheme'),$f,$null,@($template),$null)
$type=$a.GetType('KeeTheme.KeeTheme')
$instance=[Runtime.Serialization.FormatterServices]::GetUninitializedObject($type)
foreach($field in $type.GetFields($f) | Where-Object {$_.Name -in @('_toolbarPadding','_toolbarAvailable','_searchSize','_searchAutoSize')}) {
    $field.SetValue($instance,[Activator]::CreateInstance($field.FieldType))
}
$type.GetField('_theme',$f).SetValue($instance,$theme)
$type.GetField('_enabled',$f).SetValue($instance,$true)
$form=New-Object KeePass.Forms.MainForm
$toolbar=New-Object Windows.Forms.ToolStrip
$form.Controls.Add($toolbar)
foreach($name in @('m_tbOpenDatabase','m_tbSaveDatabase','m_tbAddEntry','m_tbFind','m_tbCopyUserName','m_tbCopyPassword','m_tbLockWorkspace','m_tbSaveAll','thirdPartyButton')) {
    $item=New-Object Windows.Forms.ToolStripButton($name); $item.Name=$name; $toolbar.Items.Add($item) | Out-Null
}
$search=New-Object Windows.Forms.ToolStripComboBox
$search.Name='m_tbQuickFind'; $search.Width=120; $toolbar.Items.Add($search) | Out-Null
$originalWidth=$search.Width; $originalAuto=$search.AutoSize
$apply=$type.GetMethods($f) | Where-Object {$_.Name -eq 'Apply' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.FullName -eq 'System.Windows.Forms.ToolStripItemCollection'}
$apply.Invoke($instance,[object[]](,$toolbar.Items.PSObject.BaseObject)) | Out-Null
foreach($name in @('m_tbCopyUserName','m_tbCopyPassword','m_tbLockWorkspace','m_tbSaveAll')) {if($toolbar.Items[$name].Available){throw ('Still visible '+$name)}}
foreach($name in @('m_tbOpenDatabase','m_tbSaveDatabase','m_tbAddEntry','m_tbFind','thirdPartyButton')) {if(!$toolbar.Items[$name].Available){throw ('Hidden '+$name)}}
if($search.Width -lt 320){throw 'Search is too narrow'}
$padding=$toolbar.Items['m_tbOpenDatabase'].Padding
$apply.Invoke($instance,[object[]](,$toolbar.Items.PSObject.BaseObject)) | Out-Null
if($toolbar.Items['m_tbOpenDatabase'].Padding -ne $padding){throw 'Padding accumulated'}
$type.GetField('_enabled',$f).SetValue($instance,$false)
$apply.Invoke($instance,[object[]](,$toolbar.Items.PSObject.BaseObject)) | Out-Null
if(!$toolbar.Items['m_tbLockWorkspace'].Available -or $search.Width -ne $originalWidth -or $search.AutoSize -ne $originalAuto){throw 'Restoration failed'}
$form.Dispose()
$icons=$a.GetType('KeeTheme.Theme.ModernStandardIcons').GetMethod('Draw',$f)
$bitmap=New-Object Drawing.Bitmap(24,24); $graphics=[Drawing.Graphics]::FromImage($bitmap)
$rect=New-Object Drawing.Rectangle(0,0,24,24); $color=[Drawing.Color]::White
foreach($icon in @('Folder','FolderOpen','UserCommunication','Key','NetworkServer','EMail','Tool','Home','Monitor','TrashBin')) {
    $index=[int][Enum]::Parse([KeePassLib.PwIcon],$icon)
    if(!$icons.Invoke($null,@($graphics.PSObject.BaseObject,$rect.PSObject.BaseObject,$index,$color.PSObject.BaseObject))){throw ('Missing '+$icon)}
}
if($icons.Invoke($null,@($graphics.PSObject.BaseObject,$rect.PSObject.BaseObject,[int][KeePassLib.PwIcon]::Count,$color.PSObject.BaseObject))){throw 'Custom slot was replaced'}
$graphics.Dispose(); $bitmap.Dispose()
Write-Output 'PASS compact toolbar, third-party preservation, search width, repeated application, restoration and standard icon mapping'
$type.GetField('_enabled',$f).SetValue($instance,$true)
$tree=New-Object Windows.Forms.TreeView
$tree.Name='m_tvGroups'; $tree.Size=New-Object Drawing.Size(240,160)
$images=New-Object Windows.Forms.ImageList
$images.ColorDepth=[Windows.Forms.ColorDepth]::Depth32Bit
$original=New-Object Drawing.Bitmap(16,16)
$ig=[Drawing.Graphics]::FromImage($original); $ig.Clear([Drawing.Color]::Magenta); $ig.Dispose()
$images.Images.Add($original) | Out-Null; $tree.ImageList=$images
$group=New-Object KeePassLib.PwGroup($true,$true,'Test',[KeePassLib.PwIcon]::Folder)
$group.CustomIconUuid=New-Object KeePassLib.PwUuid($true)
$node=New-Object Windows.Forms.TreeNode('Test'); $node.Tag=$group; $node.ImageIndex=0; $node.SelectedImageIndex=0
$tree.Nodes.Add($node) | Out-Null; $handle=$tree.Handle
$tree.DrawMode=[Windows.Forms.TreeViewDrawMode]::OwnerDrawAll
$canvas=New-Object Drawing.Bitmap(240,160); $cg=[Drawing.Graphics]::FromImage($canvas)
$event=New-Object Windows.Forms.DrawTreeNodeEventArgs($cg,$node,$node.Bounds,[Windows.Forms.TreeNodeStates]::Default)
$method=$type.GetMethod('DrawModernGroup',$f)
if(!$method.Invoke($instance,@($event.PSObject.BaseObject))){throw 'Tree drawing failed'}
$x=$node.Bounds.X-16-3+8; $y=$node.Bounds.Y+($node.Bounds.Height-16)/2+8
if($canvas.GetPixel([int]$x,[int]$y).ToArgb() -ne [Drawing.Color]::Magenta.ToArgb()){throw 'Custom tree icon was changed'}
$group.CustomIconUuid=[KeePassLib.PwUuid]::Zero
$node.ImageIndex=[int][KeePassLib.PwIcon]::Folder
$method.Invoke($instance,@($event.PSObject.BaseObject)) | Out-Null
$cg.Dispose(); $canvas.Dispose(); $tree.Dispose(); $images.Dispose(); $original.Dispose()
Write-Output 'PASS group tree renderer and custom UUID image preservation'
