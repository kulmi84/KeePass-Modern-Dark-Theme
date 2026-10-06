# KeeTheme Modern Dark – erste Testversion

Branch: feature/windows11-dark-v1. KeePass selbst wird nicht verändert.

## Installation und Rückweg

KeePass schließen. Bisherige KeeTheme.dll/KeeTheme.plgx sichern und aus Plugins entfernen, damit nur eine Version geladen wird. Die neue KeeTheme.dll nach Plugins kopieren. KeePass starten, unter Extras > Optionen > KeeTheme das Theme Modern Dark auswählen und aktivieren (standardmäßig Strg+T). Bestehende Theme-Einstellungen werden respektiert; Modern Dark ist nur für neue Konfigurationen der Standard. Zum Rückweg KeePass schließen und die gesicherte Plugin-Version zurückkopieren.

## Version 1

Hintergrund #1E1E1E, Controls/Panels #252526, Menüs #2D2D30, Text #F1F1F1, deaktivierte Menütexte #BEBEBE. Baumselektion und Menü-Hover #3E3E42, Listenheader #252526. Die vorhandenen ListView-/TreeView-Zeichner und Windows-Dark-Scrollbar-Unterstützung bleiben erhalten. Inaktive Texte außerhalb von Menüs verwenden teilweise weiterhin die native Windows-/KeeTheme-Darstellung.

Neue Linienicons für Neu, Öffnen, Speichern, Eintrag hinzufügen und Sperren in der Haupttoolbar; Neu/Öffnen/Speichern auch im Hauptmenü, wenn dort ein Image vorhanden ist. Zeichnung skaliert mit dem vom Control gelieferten Bildrechteck. Es werden keine ToolStripItem.Images ersetzt; beim Theme-Wechsel gilt wieder der ursprüngliche Renderer. Unbekannte Kommandos und fremde Fenster erhalten ihre ursprünglichen Icons.

Die Eintragsliste zeichnet die Standardicons Schlüssel, Ordner und offener Ordner neu. Nur m_lvEntries mit PwListItem-Tag, leerer CustomIconUuid und Index kleiner PwIcon.Count wird berücksichtigt. Alle eigenen Icons und alle anderen Standardicons bleiben unverändert. Keine Datenbankbilder oder ImageLists werden geschrieben.

## Erreichbarkeit und Grenzen

| Oberfläche | Plugin-Zugang | Status dieser Version |
| --- | --- | --- |
| TreeView | OwnerDrawText, Farben, natives Dark-Theme | Farben und Selektion; Gruppenicons noch original |
| ListView | OwnerDraw, Header-/Gruppenzeichner, SmallImageList | Farben, Header und drei Standardicon-Typen beim Zeichnen |
| Menu/ContextMenu | ToolStripRenderer, DropdownOpening | Farben, Hover, inaktive Texte; drei Hauptmenüicons |
| Toolbar | ToolStripRenderer.OnRenderItemImage | Fünf bekannte Kommandos als Linienicons |
| Standard-/Custom-ImageLists | MainForm.ClientIcons ist öffentlich; ImageList.Images veränderbar | Bewusst keine Mutation der gemeinsamen Listen |
| Native Dialoge/Checkboxen/Comboboxen | Teilweise Windows-eigene Zeichnung | Keine vollständige WinUI-3-/Windows-11-Umstellung |

KeePass MainForm_Functions.cs (UpdateImageLists) baut die Listen bei Icon-Updates neu auf: Standardbilder zuerst, eigene Bilder ab PwIcon.Count. Gruppen/Einträge mit eigener UUID verweisen auf diese angehängten Slots. Ein globaler Austausch müsste Wiederaufbau, Datenbankwechsel, DPI-Wechsel, mehrere Dialoge und Wiederherstellung beim Abschalten behandeln. Die private Standardbild-Cache-Liste ist kein stabiler Plugin-Vertrag. Ein KeePass-Fork ist für die hier implementierten Änderungen nicht nötig; für vollständiges WinUI, Mica, native Rundungen und sämtliche Betriebssystemdialoge reicht KeeTheme nicht aus.

## Build und Prüfung

.NET Framework 3.5 und Windows PowerShell 5.1 erforderlich. Gegen die lokal installierte KeePass.exe gebaut. build/build-local.ps1 kann mit -KeePassPath und -OutputPath andere Pfade erhalten; es installiert nichts und kopiert den Build nicht automatisch in Plugins. Visual Studio/MSBuild bleibt über die Solution möglich; der lokale Ersatzbuild erzeugt Ressourcen ohne zusätzliches Windows SDK. Ein vorhandener C#-3.5-Typinferenzfehler wurde durch einen expliziten Lambda-Ausdruck behoben. Upstream-UpdateUrl ist leer, damit ein Fork-Build nicht über den Upstream-Updatehinweis ersetzt wird.

Build erfolgreich, nur bestehende Warnungen zu ungenutzten Variablen/Feldern. build/test-smoke.ps1 prüft alle drei eingebetteten Themes, Farben, Theme-Editor-Roundtrip, Standardicon-Zeichnung und Ausschluss eigener UUIDs/Slots sowie Rückfall bei ausgeschalteten modernen Icons. Diese Tests sind bestanden. Eine visuelle Prüfung in einer laufenden KeePass-Instanz ist noch offen; der Build ist eine Testversion.

Manuell prüfen: Theme an/aus und Wechsel zu alten Themes; Menü-/Kontextmenü-Hover und deaktivierte Befehle; Toolbar bei 100/125/150/200%; Standard-/eigene Icons in Baum, Liste und Eintragsdialog; zwei Datenbanken wechseln; Sperren/Entsperren; Theme-Editor speichern/laden; Windows-Synchronisierung und Hotkey. Keine echte Passwortdatenbank für den ersten Test erforderlich.

## Nächste Schritte

Gruppenicons über einen eigenen, sauber rücksetzbaren TreeView-Zeichner; weitere Standardicons und Toolbar-Kommandos; explizite Listen-Auswahlfarben und vollständigerer deaktivierter Text. Vor einer stabilen Veröffentlichung ist die manuelle Matrix mit mehreren KeePass-Versionen erforderlich.
