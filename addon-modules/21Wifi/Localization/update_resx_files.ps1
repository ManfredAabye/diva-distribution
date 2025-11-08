# PowerShell script to convert .po files to .resx files
$languages = @("da", "de", "el", "en", "es", "fr", "it", "ja", "ko", "nl", "pl", "pt", "pt-BR", "ru", "sv", "tr", "zh-CN", "zh-TW")

Write-Host "Converting .po files to .resx files" -ForegroundColor Green
Write-Host ("=" * 50)

function Get-PoEntries {
    param($filePath)
    $entries = @{}
    $content = Get-Content $filePath -Encoding UTF8
    $currentMsgId = $null
    $currentMsgStr = $null
    $multilineMode = $null
    
    foreach ($line in $content) {
        $line = $line.Trim()
        if ($line -match '^#' -or $line -eq '') { continue }
        if ($line -match '^msgid\s+"(.*)"$') {
            $currentMsgId = $matches[1]
            $multilineMode = 'msgid'
        }
        elseif ($line -match '^msgstr\s+"(.*)"$') {
            $currentMsgStr = $matches[1]
            $multilineMode = 'msgstr'
            if ($currentMsgId -and $currentMsgId -ne "" -and $currentMsgStr -ne "") {
                $entries[$currentMsgId] = $currentMsgStr
            }
            $currentMsgId = $null
        }
        elseif ($line -match '^"(.*)"$') {
            $continuation = $matches[1]
            if ($multilineMode -eq 'msgid') { $currentMsgId += $continuation }
            elseif ($multilineMode -eq 'msgstr') { $currentMsgStr += $continuation }
        }
    }
    return $entries
}

function New-ResxFile {
    param([string]$outputPath, [hashtable]$entries)
    $xml = '<?xml version="1.0" encoding="utf-8"?>' + "`n"
    $xml += '<root>' + "`n"
    $xml += '  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>' + "`n"
    $xml += '  <resheader name="version"><value>2.0</value></resheader>' + "`n"
    $xml += '  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>' + "`n"
    $xml += '  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>' + "`n"
    foreach ($key in $entries.Keys | Sort-Object) {
        $keyEsc = [System.Security.SecurityElement]::Escape($key)
        $valEsc = [System.Security.SecurityElement]::Escape($entries[$key])
        $xml += '  <data name="' + $keyEsc + '" xml:space="preserve"><value>' + $valEsc + '</value></data>' + "`n"
    }
    $xml += '</root>'
    $xml | Out-File -FilePath $outputPath -Encoding UTF8
}

foreach ($lang in $languages) {
    $poFile = "Diva.Wifi.$lang.po"
    $resxFile = "Diva.Wifi.$lang.resx"
    if (Test-Path $poFile) {
        Write-Host "Processing $poFile..." -ForegroundColor Cyan
        $entries = Get-PoEntries -filePath $poFile
        if ($entries.Count -gt 0) {
            New-ResxFile -outputPath $resxFile -entries $entries
            Write-Host "  Created $resxFile with $($entries.Count) translations" -ForegroundColor Green
        }
    }
}

Write-Host ("`n" + ("=" * 50)) -ForegroundColor Green
Write-Host "Done! Run make_all_language.bat next" -ForegroundColor Cyan
