# Wifi W3.CSS Anpassung - Anleitung

## Überblick

Die Wifi-Oberfläche wurde auf W3.CSS umgestellt mit modernem Grid-Layout und konfigurierbaren Farben/Sprachen.

## Neue Dateien

- **wifi-custom.css** - Hauptstyles mit W3.CSS Grid-Layout, Cards und modernem Design
- **wifi-config.css** - Konfigurationsdatei für Farben und Sprache (HIER ANPASSEN!)
- **sync-wifipages.bat** - Synchronisiert WifiPages zwischen den beiden Verzeichnissen

## Layout-Änderungen

### Rechte Seitenleiste ist zurück! 🎉

Die Seitenleiste wurde mit W3.CSS-Cards neu aufgebaut:
- **Hauptmenü Card** - Navigation
- **Erweiterungen Card** - Add-ons
- **Anmeldung Card** - Login/Logout
- **Links Card** - Viewer-Downloads und Links

### Responsive Design

- Desktop: Content links, Sidebar rechts (Grid-Layout)
- Mobile: Sidebar oben, Content unten (stapelt sich)

## Farben anpassen

Bearbeiten Sie `bin/WifiPages/wifi-config.css`:

### Voreingestellte Themes:

1. **Orange** (Standard - Diva Original) ✓
2. **Blau** - Entfernen Sie `/*` und `*/` um das blaue Theme zu aktivieren
3. **Grün** - Entfernen Sie `/*` und `*/` um das grüne Theme zu aktivieren
4. **Lila** - Entfernen Sie `/*` und `*/` um das lila Theme zu aktivieren
5. **Rot** - Entfernen Sie `/*` und `*/` um das rote Theme zu aktivieren
6. **Türkis** - Entfernen Sie `/*` und `*/` um das türkise Theme zu aktivieren

### Eigenes Theme erstellen:

```css
:root {
  --wifi-orange: #IHR_FARBCODE;
  --wifi-orange-hover: #IHR_FARBCODE;
  --wifi-orange-active: #IHR_FARBCODE;
  --wifi-accent: #IHR_FARBCODE;
}
```

## Sprache umschalten

### Deutsch (Standard)

Die Seiten zeigen standardmäßig deutsche Texte.

### Englisch aktivieren

In `wifi-config.css` entfernen Sie die Kommentare um diesen Block:

```css
body .lang-de { display: none !important; }
body .lang-en { display: inline !important; }
```

## Hellmodus (Experimentell)

In `wifi-config.css` gibt es einen experimentellen Hellmodus.
Entfernen Sie die Kommentare um den "HELLMODUS" Block zu aktivieren.

## Synchronisation der WifiPages

**WICHTIG:** Es gibt zwei WifiPages-Verzeichnisse:

1. `opensimsource/WifiPages` - Templates (Wifi liest von hier)
2. `opensimsource/bin/WifiPages` - Statische Ressourcen (CSS, Bilder werden von hier ausgeliefert)

### Änderungen synchronisieren:

Verwenden Sie das Sync-Script:

```powershell
.\sync-wifipages.bat 1   # Von außen nach innen (WifiPages -> bin\WifiPages)
.\sync-wifipages.bat 2   # Von innen nach außen (bin\WifiPages -> WifiPages)
.\sync-wifipages.bat 3   # Beide Richtungen (neuere Dateien gewinnen)
```

### Workflow für Anpassungen:

1. **CSS/Farben ändern**: Bearbeiten Sie `bin/WifiPages/wifi-config.css`
2. **HTML ändern**: Bearbeiten Sie `bin/WifiPages/*.html`
3. **Synchronisieren**: `.\sync-wifipages.bat 1`
4. **OpenSim neu starten**: Die Änderungen werden sichtbar

## CSS-Klassen für eigene Anpassungen

### Layout-Klassen:

- `.wifi-container` - Hauptcontainer (max-width, zentriert)
- `.wifi-grid` - Grid-Layout (Content + Sidebar)
- `.wifi-content` - Content-Bereich (links)
- `.wifi-sidebar` - Sidebar-Bereich (rechts)

### Card-Klassen:

- `.wifi-card` - Card-Container
- `.wifi-card-header` - Card-Überschrift
- `.wifi-card-content` - Card-Inhalt

### Text-Klassen:

- `.wifi-text-white` - Weißer Text
- `.wifi-text-gray` - Grauer Text
- `.wifi-text-orange` - Oranger Text (Akzentfarbe)
- `.wifi-text-center` - Zentrierter Text
- `.wifi-text-left` - Linksbündiger Text
- `.wifi-text-right` - Rechtsbündiger Text

### Komponenten-Klassen:

- `.wifi-menu` - Menü-Liste (für Navigation)
- `.wifi-links` - Links-Liste (für Sidebar-Links)
- `.wifi-button` - Standard-Button
- `.wifi-button-orange` - Oranger Button
- `.wifi-input` - Eingabefeld
- `.wifi-textarea` - Textbereich
- `.wifi-table` - Tabelle

### Utility-Klassen:

- `.wifi-mt-1/2/3` - Margin-Top (0.5em, 1em, 1.5em)
- `.wifi-mb-1/2/3` - Margin-Bottom (0.5em, 1em, 1.5em)
- `.wifi-p-1/2/3` - Padding (0.5em, 1em, 1.5em)
- `.wifi-rounded` - Abgerundete Ecken (8px)
- `.wifi-shadow` - Box-Shadow
- `.wifi-divider` - Trennlinie

### Sprach-Klassen:

- `.lang-de` - Deutscher Text (sichtbar bei Deutsch)
- `.lang-en` - Englischer Text (sichtbar bei Englisch)

## Beispiel: Mehrsprachige Überschrift

```html
<h2>
  <span class="lang-de">Willkommen</span>
  <span class="lang-en">Welcome</span>
</h2>
```

## Beispiel: Eigene Sidebar-Card hinzufügen

```html
<div class="wifi-card">
  <div class="wifi-card-header">
    <span class="lang-de">Meine Card</span>
    <span class="lang-en">My Card</span>
  </div>
  <div class="wifi-card-content">
    <p>Ihr Inhalt hier...</p>
  </div>
</div>
```

## CSS-Variablen

Alle Farben sind als CSS-Variablen definiert und können in `wifi-config.css` angepasst werden:

```css
:root {
  /* Primärfarben */
  --wifi-orange: #f67d15;
  --wifi-orange-hover: #ff9933;
  --wifi-orange-active: #ff0000;
  --wifi-accent: #BD6B54;
  
  /* Hintergrundfarben */
  --wifi-bg-dark: #000000;
  --wifi-bg-box: #0F0F0F;
  --wifi-bg-input: #090909;
  
  /* Textfarben */
  --wifi-text-light: #f8f8f8;
  --wifi-text-gray: #929292;
  --wifi-text-white: #FFFFFF;
  --wifi-text-muted: #8C8D94;
  
  /* Rahmen */
  --wifi-border: #404040;
  --wifi-border-table: #444444;
}
```

## Troubleshooting

### Änderungen sind nicht sichtbar

1. Cache leeren (Strg + F5 im Browser)
2. Sync-Script ausführen: `.\sync-wifipages.bat 1`
3. OpenSim neu starten

### Sidebar fehlt

- Überprüfen Sie, ob `wifi-custom.css` geladen wird
- Browser-Konsole öffnen (F12) und CSS-Fehler prüfen

### Falsche Sprache angezeigt

- Prüfen Sie `wifi-config.css` - nur EIN Sprachblock sollte aktiv sein
- Cache leeren (Strg + F5)

## Support

Bei Fragen oder Problemen:
- OpenSimulator Forum: https://opensimulator.org
- Diva Distro: http://metaverseink.com

---

**Erstellt:** November 2025  
**Version:** 1.0 - W3.CSS Migration  
**Kompatibel mit:** OpenSimulator 0.9.3+ / Diva Distro
