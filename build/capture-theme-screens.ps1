param([Parameter(Mandatory=$true)][string]$PluginPath,
      [string]$Theme='ModernGray', [string]$OutputDirectory, [string]$KeePassPath='C:\Program Files\KeePass Password Safe 2\KeePass.exe')
$ErrorActionPreference='Stop'
$testDir=Join-Path ([IO.Path]::GetTempPath()) ('KeeTheme-integration-'+[Guid]::NewGuid())
New-Item -ItemType Directory -Path (Join-Path $testDir 'Plugins') -Force | Out-Null
$sourceDir=Split-Path $KeePassPath
foreach($pattern in @('KeePass.exe','KeePass.exe.config','KeePass.XmlSerializers.dll','KeePassLibN*.dll')) {
    Get-ChildItem $sourceDir -Filter $pattern | Copy-Item -Destination $testDir
}
Copy-Item -LiteralPath $PluginPath -Destination (Join-Path $testDir 'Plugins\KeeTheme.dll')
$config=@'
<Configuration>
<Meta><PreferUserConfiguration>false</PreferUserConfiguration></Meta>
<Application><Start><OpenLastFile>false</OpenLastFile><CheckForUpdate>false</CheckForUpdate><CheckForUpdateConfigured>true</CheckForUpdateConfigured></Start></Application>
<Integration><LimitToSingleInstance>false</LimitToSingleInstance></Integration>
<Security><MasterKeyOnSecureDesktop>false</MasterKeyOnSecureDesktop></Security>
<Custom><Item><Key>KeeTheme.Enabled</Key><Value>True</Value></Item><Item><Key>KeeTheme.Template</Key><Value>KeeTheme.Resources.ModernDark.ini</Value></Item></Custom>
</Configuration>
'@
$config=$config.Replace('ModernDark.ini',($Theme+'.ini'));
$config | Set-Content (Join-Path $testDir 'KeePass.config.xml') -Encoding UTF8
$config=$config.Replace('ModernDark.ini',($Theme+'.ini'));
$config | Set-Content (Join-Path $testDir 'local.config.xml') -Encoding UTF8
& "$env:WINDIR\Microsoft.NET\Framework\v3.5\csc.exe" /nologo /target:library (('/out:')+(Join-Path $testDir 'Plugins\AThemeScreens.dll')) (('/r:')+(Join-Path $testDir 'KeePass.exe')) /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'integration\AThemeScreens.cs')
if($LASTEXITCODE -ne 0){throw 'Integration actor compilation failed'}
$exe=Join-Path $testDir 'KeePass.exe'
$process=Start-Process -FilePath $exe -ArgumentList ('-cfg-local:"'+(Join-Path $testDir 'local.config.xml')+'"') -WorkingDirectory $testDir -WindowStyle Hidden -PassThru
if(!$process.WaitForExit(30000)) {
    if($process.Path -eq $exe){Stop-Process -Id $process.Id}
    throw "Isolated KeePass timed out; artifacts: $testDir"
}
if(Test-Path (Join-Path $testDir 'screens/error.txt')){throw [IO.File]::ReadAllText((Join-Path $testDir 'screens/error.txt'))}
if(!(Test-Path (Join-Path $testDir 'screens/done.txt'))){throw ('No screenshot completion: '+$testDir)}
foreach($kind in @('main','entry','options')){Copy-Item -LiteralPath (Join-Path $testDir ('screens/'+$kind+'.png')) -Destination (Join-Path $OutputDirectory ($Theme+'-'+$kind+'.png'))}
Write-Output ('PASS fictional KeePass screenshots: '+$Theme)