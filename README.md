# KeeTheme Modern Dark

Ein KeeTheme-Fork für KeePass 2 mit dunkler Oberfläche, einheitlichen Standardicons und einer aufgeräumten Symbolleiste.

![Modern Dark mit Testeinträgen](docs/ModernDark-main-test.png)

Originalaufnahme mit Testeinträgen, vom Nutzer zur Veröffentlichung bereitgestellt. Die weiteren bearbeiteten Demo-Ansichten enthalten fiktive Daten; einzelne Darstellungsdetails können von der laufenden Anwendung abweichen.

![Moderne Standardicons und benutzerdefinierte Icons](docs/ModernDark-icons-demo.png)

Die Standardicons erscheinen im modernen Stil. Eigene Datenbankicons bleiben erhalten; die abgebildeten Namen wurden durch neutrale Demo-Bezeichnungen ersetzt.

## Funktionen

![Moderner S/W-Banner in Originalhöhe](docs/ModernDark-banner-demo.png)

Aktueller Banner: S/W-KeePass-Logo, scharfe lokalisierte Überschrift und dezentes Tresormotiv rechts. Die Vorschau stammt aus dem Plugin-Zeichner und enthält keine privaten Daten.

- Modern Dark mit dunklen Panels und Menüs sowie hellen Texten.
- Einheitliche Icons in Symbolleiste, Gruppenbaum, Eintragsliste und Icon-Auswahl.
- Breiteres Suchfeld, bei ausreichendem Platz mittig angeordnet.
- Dunklere Zeilen und abschaltbare Spaltentrennlinien, auch in gruppierten Suchergebnissen.
- Originales KeePass-Fensterlogo in Graustufen.
- Moderne Icon-Vorschau und Passwortgenerator-Symbole im Eintragsdialog.
- Dunkle Eingabefeld- und Suchfeldrahmen sowie ein kompakter S/W-Banner.
- Dunkler Datumskalender mit grauem Popup-Rahmen und lesbaren Wochentagen.
- Korrigierte Einblendereihenfolge ohne vorzeitiges Anzeigen auf einem anderen Monitor.

Die vorhandenen KeeTheme-Themes und Funktionen bleiben verfügbar. Passwort- und KDBX-Logik werden nicht verändert.

## Installation und Einstellungen

[**Version 1.1.18 herunterladen**](https://github.com/kulmi84/KeeTheme/releases/tag/v1.1.18) – das ZIP enthält die fertige KeeTheme.dll.

[Installation, Build-Anleitung, Prüfungen und Plugin-Grenzen](docs/ModernDark.md).

Die aktuelle Version aus diesem Fork bauen und KeeTheme.dll in den KeePass-Plugins-Ordner kopieren. Vor dem Austausch KeePass schließen und vorherige KeeTheme.dll/KeeTheme.plgx sichern und aus dem Plugins-Ordner nehmen. Unter **Extras → Optionen → KeeTheme** das Theme **Modern Dark** auswählen.

Die dunklen Rahmen und das neue Banner wurden im laufenden KeePass vom Nutzer bestätigt. Automatisierte Prüfungen ergänzen die praktischen Tests; Details und verbleibende Grenzen stehen in der Dokumentation.

## Kalender

![Dunkler Kalender mit fiktivem Testdatum](docs/ModernDark-calendar-demo.png)

Aufnahme eines echten Kalender-Popups mit fiktivem Testdatum. Bekannte Einschränkung: Die einheitliche dunkle Datumsdarstellung überdeckt beim Bearbeiten die native Markierung einzelner Datumssegmente.

## Weitere Ansichten

Hauptschlüssel der Testdatenbank:

<img src="docs/ModernDark-unlock-test.png" alt="Hauptschlüssel der Testdatenbank" width="424">

Eintrag bearbeiten mit geöffnetem Kalender:

![Testeintrag und dunkler Kalender](docs/ModernDark-entry-test.png)

Menü mit fiktiven Einträgen:

![Dunkles Eintragsmenü mit Demo-Daten](docs/ModernDark-menu-demo-v1.1.17.png)

## Ursprung

Dieser Fork basiert auf [xatupal/KeeTheme](https://github.com/xatupal/KeeTheme) von Krzysztof Łaputa und wird als **KeeTheme Modern Dark** von **Marcin Kulmaczewski (kulmi84)** gepflegt und weiterentwickelt.

KeeTheme-Lizenz: [MIT](LICENSE). Das originale KeePass-Logo stammt aus KeePass; Quellenhinweise stehen in der Dokumentation.
