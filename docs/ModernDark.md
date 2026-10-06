# KeeTheme Modern Dark – erste Testversion

Branch: feature/windows11-dark-v1. KeePass selbst wird nicht verändert.

## Installation und Rückweg

KeePass schließen. Bisherige KeeTheme.dll/KeeTheme.plgx sichern und aus Plugins entfernen, damit nur eine Version geladen wird. Die neue KeeTheme.dll nach Plugins kopieren. KeePass starten, unter Extras > Optionen > KeeTheme das Theme Modern Dark auswählen und aktivieren (standardmäßig Strg+T). Bestehende Theme-Einstellungen werden respektiert; Modern Dark ist nur für neue Konfigurationen der Standard. Zum Rückweg KeePass schließen und die gesicherte Plugin-Version zurückkopieren.

## Version 1

Hintergrund #1E1E1E, Controls/Panels #252526, Menüs #2D2D30, Text #F1F1F1, deaktivierte Menütexte #BEBEBE. Baumselektion und Menü-Hover #3E3E42, Listenheader #252526. Die vorhandenen ListView-/TreeView-Zeichner und Windows-Dark-Scrollbar-Unterstützung bleiben erhalten. Inaktive Texte außerhalb von Menüs verwenden teilweise weiterhin die native Windows-/KeeTheme-Darstellung.

Einheitliche Linienicons für Neu, Öffnen, Speichern, Alle speichern, Eintrag hinzufügen, Benutzername, Passwort, URL öffnen/kopieren, Auto-Type, Suche, Ansichten, abgelaufene Einträge, Sperren und Tab schließen in der Haupttoolbar; Neu/Öffnen/Speichern auch im Hauptmenü, wenn dort ein Image vorhanden ist. Zeichnung skaliert mit dem vom Control gelieferten Bildrechteck. Es werden keine ToolStripItem.Images ersetzt; beim Theme-Wechsel gilt wieder der ursprüngliche Renderer. Unbekannte Kommandos und fremde Fenster erhalten ihre ursprünglichen Icons.

Baum und Eintragsliste zeichnen alle 69 Standardicons neu, darunter Schlüssel, Ordner, Benutzer, Server, E-Mail, Werkzeug, Monitor, Haus und Papierkorb. Für unbekannte zukünftige Icon-IDs bleibt das Originalbild erhalten. Nur m_lvEntries mit PwListItem-Tag, leerer CustomIconUuid und Index kleiner PwIcon.Count wird berücksichtigt. Alle eigenen Icons und alle anderen Standardicons bleiben unverändert. Keine Datenbankbilder oder ImageLists werden geschrieben.

## Erreichbarkeit und Grenzen

| Oberfläche | Plugin-Zugang | Status dieser Version |
| --- | --- | --- |
| TreeView | OwnerDrawText, Farben, natives Dark-Theme | Farben und Selektion; moderne Standardsymbole und Chevron-Pfeile; eigene Icons original |
| ListView | OwnerDraw, Header-/Gruppenzeichner, SmallImageList | Farben, Header und alle 69 Standardicon-Typen beim Zeichnen |
| Menu/ContextMenu | ToolStripRenderer, DropdownOpening | Farben, Hover, inaktive Texte; drei Hauptmenüicons |
| Toolbar | ToolStripRenderer.OnRenderItemImage | Alle 15 Standardtoolbar-Icontypen als Linienicons |
| Standard-/Custom-ImageLists | MainForm.ClientIcons ist öffentlich; ImageList.Images veränderbar | Bewusst keine Mutation der gemeinsamen Listen |
| Native Dialoge/Checkboxen/Comboboxen | Teilweise Windows-eigene Zeichnung | Keine vollständige WinUI-3-/Windows-11-Umstellung |

KeePass MainForm_Functions.cs (UpdateImageLists) baut die Listen bei Icon-Updates neu auf: Standardbilder zuerst, eigene Bilder ab PwIcon.Count. Gruppen/Einträge mit eigener UUID verweisen auf diese angehängten Slots. Ein globaler Austausch müsste Wiederaufbau, Datenbankwechsel, DPI-Wechsel, mehrere Dialoge und Wiederherstellung beim Abschalten behandeln. Die private Standardbild-Cache-Liste ist kein stabiler Plugin-Vertrag. Ein KeePass-Fork ist für die hier implementierten Änderungen nicht nötig; für vollständiges WinUI, Mica, native Rundungen und sämtliche Betriebssystemdialoge reicht KeeTheme nicht aus.

## Build und Prüfung

.NET Framework 3.5 und Windows PowerShell 5.1 erforderlich. Gegen die lokal installierte KeePass.exe gebaut. build/build-local.ps1 kann mit -KeePassPath und -OutputPath andere Pfade erhalten; es installiert nichts und kopiert den Build nicht automatisch in Plugins. Visual Studio/MSBuild bleibt über die Solution möglich; der lokale Ersatzbuild erzeugt Ressourcen ohne zusätzliches Windows SDK. Ein vorhandener C#-3.5-Typinferenzfehler wurde durch einen expliziten Lambda-Ausdruck behoben. Upstream-UpdateUrl ist leer, damit ein Fork-Build nicht über den Upstream-Updatehinweis ersetzt wird.

Build erfolgreich, nur bestehende Warnungen zu ungenutzten Variablen/Feldern. build/test-smoke.ps1 prüft alle drei eingebetteten Themes, Farben, Theme-Editor-Roundtrip, Standardicon-Zeichnung und Ausschluss eigener UUIDs/Slots sowie Rückfall bei ausgeschalteten modernen Icons. Diese Tests sind bestanden. Eine visuelle Prüfung in einer laufenden KeePass-Instanz ist noch offen; der Build ist eine Testversion.

Manuell prüfen: Theme an/aus und Wechsel zu alten Themes; Menü-/Kontextmenü-Hover und deaktivierte Befehle; Toolbar bei 100/125/150/200%; Standard-/eigene Icons in Baum, Liste und Eintragsdialog; zwei Datenbanken wechseln; Sperren/Entsperren; Theme-Editor speichern/laden; Windows-Synchronisierung und Hotkey. Keine echte Passwortdatenbank für den ersten Test erforderlich.

## Toolbar-Verfeinerung

Neue Icons mit runden Linienenden, dezente Trenner und abgerundete Hover-/Pressed-Flächen. Zusätzliche horizontale und vertikale Polsterung wird beim Abschalten oder Theme-Wechsel auf die ursprünglichen Werte zurückgesetzt; wiederholtes Anwenden addiert keine weiteren Abstände. Unbekannte Plugin-Kommandos behalten ihr Bild. Der Suchfeldrahmen wird in Modern Dark dunkel (#414141) nachgezeichnet. ModernDark-toolbar.png ist eine aus dem Icon-Zeichner erzeugte Stilvorschau, kein Screenshot einer laufenden KeePass-Instanz. build/test-toolbar.ps1 prüft 15 Icontypen bei 16/20/24/32px, unbekannte Kommandos und die Wiederherstellung des Graphics-Zustands; bestanden.

## Vollständige Toolbar, zentrierte Suche und Gruppenbaum

Die vollständige ursprüngliche Toolbar ist wieder sichtbar, mit den modernen Icons. Das breite Suchfeld wird mit einem dynamischen Abstand in der Toolbar zentriert, sobald Platz zwischen den Buttons und dem rechten Rand vorhanden ist. Bei schmalen Fenstern verkleinert es sich bis auf 100 Pixel und bleibt neben den Buttons; eine geometrische Zentrierung ist dort ohne Überlagerung nicht möglich. Bei Theme-Wechsel werden Polsterung, Suchfeldgröße und AutoSize wiederhergestellt. Der Gruppenbaum verwendet OwnerDrawAll mit Chevron-Pfeilen; Gruppentext, Auswahl, Fokus und eigene ImageList-Bilder bleiben erhalten. Die Standardicon-Auswahl ist nur bei leerer CustomIconUuid und Standardindex zulässig. Gemeinsame ImageLists werden weiterhin nicht verändert. Die Baumdarstellung setzt die übliche links-nach-rechts-Anordnung voraus.

build/test-compact.ps1 prüft die vollständige Toolbar, breitere Suche, fremde Buttons, wiederholtes Anwenden, Wiederherstellung sowie eigene Icons im Gruppenbaum. Alle Prüfungen bestanden. Live-Test im bestehenden KeePass-Fenster bleibt erforderlich; Plugin-Dateien werden nicht automatisch installiert.

## Icon-Auswahldialog

Die Standard-Icon-Liste im IconPickerForm erhält eine eigene ImageList mit denselben Indizes und Schlüsseln. Alle 69 Standardicons zeigen dieselben Linienicons wie Baum/Liste; unbekannte zukünftige IDs behalten ihr Original. Die gemeinsame KeePass-ImageList und die separate Liste benutzerdefinierter Datenbankicons werden nicht verändert. Vorschau wird nach dem Laden des Dialogs angewandt und bei Theme-Wechsel/Schließen sauber wiederhergestellt. Tests prüfen Vorschau, unveränderte Quellbilder und Custom-Slots, Wiederherstellung sowie Suchfeldzentrierung bei breiten und schmalen Fenstern.

## Ruhigere Eintragsliste

Modern Dark zeichnet keine senkrechten Spaltentrenner mehr in Listenzellen, Headern, Gruppen und Hintergrundbildern. Die abwechselnden Zeilen sind #1B1B1B und #202020. ShowColumnSeparators=False und UseThemeAlternatingColors=True sind im Theme-Editor einstellbar. Alte Themes behalten ihre bisherige Darstellung. Benutzerdefinierte Eintragsfarben bleiben erhalten; gespeicherte globale KeePass-Alternativfarben werden in diesem Theme durch die dezenten Theme-Farben ersetzt. Das Umschalten der alternierenden Zeilen über KeePass wird weiter respektiert.

build/test-standard-icons.ps1 prüft alle 69 Standardicons bei 16/24/32px und kontrolliert durch Pixelprüfungen, dass die Spaltentrenner im Modern-Dark-Theme ausbleiben und im bisherigen Modus weiterhin gezeichnet werden. Alle Prüfungen bestanden. Standardicon-Indizes bleiben unverändert; keine Datenbankmigration ist erforderlich.

## Weißes Fensterschloss

Das laufende KeePass-Hauptfenster erhält in Modern Dark das originale KeePass-Logo in Graustufen für Titelleiste und Windows-Fenstersymbol. UIStateUpdated wendet es nach KeePass-Icon-Updates erneut an. Beim Theme-Wechsel/Abschalten und Plugin-Terminierung wird das zuletzt von KeePass gesetzte Icon wiederhergestellt. EXE, Tray-Statussymbol und Datenbankicons werden nicht verändert.

Eine angeheftete Taskleisten-Verknüpfung kann weiterhin das EXE-Icon anzeigen. Dafür liegt KeePass-SW.ico bei: in den Eigenschaften der Verknüpfung unter Anderes Symbol auswählen. Das ICO enthält 16/32/48/64/128/256 Pixel; bis 128 Pixel werden klassische Windows-DIB-Bilder für saubere .NET-/WinForms-Kompatibilität verwendet. test-window-icon.ps1 prüft ICO-Größen, monochrome Windows-Zeichnung und Wiederherstellung.

## Nächste Schritte

Weitere Standardicons; explizite Listen-Auswahlfarben und vollständigerer deaktivierter Text. Vor einer stabilen Veröffentlichung ist die manuelle Matrix mit mehreren KeePass-Versionen erforderlich.

Spaltentrennlinien: Im KeeTheme-Theme-Editor unter ListView die Eigenschaft ShowColumnSeparators auf False setzen. Modern Dark setzt dies bereits voraus. Native GridLines werden ebenfalls abgeschaltet und beim Deaktivieren wiederhergestellt; der Hintergrund wird beim Theme-Wechsel erneuert.

Linien-Fix: Der native leere Bereich unter den Einträgen wird nach WM_PAINT gleichfarbig nachgezeichnet. Dieser zusätzliche Schritt gilt für ungegliederte und gruppierte Detail-Listen mit einfachem Theme-Hintergrund; Bildhintergründe bleiben davon ausgenommen. test-list-background.ps1 prüft leere/kurze/lange Listen, Überschriften, Einträge und Theme-Abschaltung. Live-Prüfung beim Nutzer steht noch aus.
Logoquelle: KeePass/KeePass/Resources/Icons/KeePass.ico aus dlech/KeePass2.x; unveränderte Form, nur Graustufenumwandlung.

Suchansicht: Der Hintergrund-Fix berücksichtigt nun gruppierte Suchtreffer. Alle Eintragszeilen und nativen Gruppenüberschriften werden vom überzeichneten Bereich ausgeschlossen, unabhängig von ihrer Indexreihenfolge. test-list-background.ps1 prüft native Gruppenüberschriften, umgekehrte Trefferreihenfolge und den freien Bereich unterhalb der Suche. Build und Prüfungen bestanden; Live-Prüfung dieser Suchansicht steht noch aus.

Eintragsdialog: Modern Dark zeichnet Standardicon-Vorschau, Passwortgenerator und Ablaufdatum modern. Die Vorschau liest die aktuelle Standard-ID und CustomIconUuid bei jedem Zeichnen; eigene Icons und originale Button-Bilder bleiben erhalten. Fehlende interne Felder fallen auf KeePass-Darstellung zurück. test-entry-icons.ps1 prüft Standard-ID-Wechsel, monochrome Darstellung, Custom-Icon-Schutz und Theme-Abschaltung. Live-Test offen.

Suchfeldrahmen: Native WM_PAINT/WM_NCPAINT-Nachzeichnung nur am zentrierten Modern-Dark-Suchfeld. Eingabe und Dropdown bleiben nativ. Hook wird bei Theme-Abschaltung entfernt; Handle-Neuerstellung berücksichtigt. Build, Layout-/Wiederherstellungs- und Rahmen-Pixeltests bestanden; Live-Prüfung offen.

Eintragsdialog-Rahmen: Textfelder, Kommentar-Rahmen und Ablaufdatum-Rahmen werden in Modern Dark mit #414141 nachgezeichnet; Fokus mit #38658A. Inhalte und native Bedienung bleiben erhalten. Rahmen-Pixeltests und Suchfeld-Layouttests bestanden; Live-Prüfung offen.

Datenbank öffnen: Dunkle Rahmen auch für Passwort und Schlüsseldatei-Auswahl. Die alten Banner-Grafiken in KeyPromptForm und PwEntryForm werden in Modern Dark durch einen flachen Hintergrund mit lokalisierter Überschrift ersetzt, ohne Originalbilder zu verändern. Native Dropdown-Buttons werden einschließlich Trenner und Pfeil dunkel nachgezeichnet; Eingabe und Auswahl bleiben nativ. Banner- und Dropdown-Pixeltests bestanden; Live-Test offen.
Datumsfeld: Im Ruhezustand #252526 Hintergrund und #F1F1F1 Text. Beim Fokussieren bleibt die native Datumsbearbeitung mit Segmentmarkierung erhalten. Kalenderauswahl bleibt nativ. Bitte Live-Darstellung prüfen.
Öffnen-Dialog: Originales KeePass-Logo in Graustufen vor der Überschrift Hauptschlüssel eingeben; Schlüsseldatei-Ordnerbutton mit modernem S/W-Symbol. Live-Test offen.
