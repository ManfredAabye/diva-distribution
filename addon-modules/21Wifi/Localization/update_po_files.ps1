# PowerShell script to merge old .po files with new .pot file
# This is a simple fallback if gettext msgmerge is not available

Write-Host "PowerShell .po file updater" -ForegroundColor Green
Write-Host "=" * 50

$languages = @("da", "de", "el", "en", "es", "fr", "it", "ja", "ko", "nl", "pl", "pt", "pt-BR", "ru", "sv", "tr", "zh-CN", "zh-TW")
$potFile = "Diva.Wifi.pot"

# Read .pot file and extract msgid entries
Write-Host "`nReading $potFile..." -ForegroundColor Yellow
$potContent = Get-Content $potFile -Encoding UTF8 -Raw

# Simple parser for msgid/msgstr pairs
function Parse-PoFile {
    param($content)
    
    $entries = @{}
    $lines = $content -split "`n"
    $currentMsgId = $null
    $currentComment = ""
    $inMsgId = $false
    $inMsgStr = $false
    
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i].Trim()
        
        if ($line -match '^#(.*)') {
            $currentComment += $line + "`n"
        }
        elseif ($line -match '^msgid\s+"(.*)"') {
            $currentMsgId = $matches[1]
            $inMsgId = $true
            $inMsgStr = $false
        }
        elseif ($line -match '^msgstr\s+"(.*)"' -and $currentMsgId -ne $null) {
            if ($currentMsgId -ne "") {
                $entries[$currentMsgId] = @{
                    msgstr = $matches[1]
                    comment = $currentComment
                }
            }
            $currentMsgId = $null
            $currentComment = ""
            $inMsgId = $false
            $inMsgStr = $true
        }
        elseif ($line -eq "") {
            $currentComment = ""
            $inMsgId = $false
            $inMsgStr = $false
        }
    }
    
    return $entries
}

$potEntries = Parse-PoFile -content $potContent

foreach ($lang in $languages) {
    $poFile = "Diva.Wifi.$lang.po"
    
    if (Test-Path $poFile) {
        Write-Host "`nUpdating $poFile..." -ForegroundColor Cyan
        
        # Read existing .po file
        $poContent = Get-Content $poFile -Encoding UTF8 -Raw
        $existingEntries = Parse-PoFile -content $poContent
        
        # Extract header
        $header = ($poContent -split 'msgid ""')[0]
        if ($poContent -match '(msgid ""\s+msgstr ".*?"\s+.*?)(?=\r?\n\r?\n[^#])') {
            $header = $matches[1]
        }
        
        # Build updated .po file
        $output = $header + "`n`n"
        
        $newCount = 0
        $existingCount = 0
        
        # Parse pot file line by line to maintain order and comments
        $potLines = $potContent -split "`n"
        $currentComment = ""
        $currentMsgId = ""
        
        for ($i = 0; $i -lt $potLines.Count; $i++) {
            $line = $potLines[$i]
            
            if ($line -match '^#') {
                $currentComment += $line + "`n"
            }
            elseif ($line -match '^msgid\s+"(.*)"') {
                $currentMsgId = $matches[1]
                
                if ($currentMsgId -eq "") {
                    # Skip header msgid
                    $currentComment = ""
                    continue
                }
                
                # Add comment
                $output += $currentComment
                $output += $line + "`n"
                
                # Find msgstr
                $j = $i + 1
                while ($j -lt $potLines.Count -and $potLines[$j] -notmatch '^msgstr') {
                    $output += $potLines[$j] + "`n"
                    $j++
                }
                
                # Add msgstr (from existing translation or empty)
                if ($existingEntries.ContainsKey($currentMsgId) -and $existingEntries[$currentMsgId].msgstr -ne "") {
                    $output += "msgstr `"$($existingEntries[$currentMsgId].msgstr)`"`n"
                    $existingCount++
                } else {
                    $output += "msgstr `"`"`n"
                    $newCount++
                }
                
                $output += "`n"
                $currentComment = ""
                $i = $j
            }
            elseif ($line.Trim() -eq "") {
                $currentComment = ""
            }
        }
        
        # Write updated .po file
        $output | Out-File -FilePath $poFile -Encoding UTF8 -NoNewline
        
        Write-Host "  ✓ Kept $existingCount existing translations" -ForegroundColor Green
        Write-Host "  ✓ Added $newCount new entries (need translation)" -ForegroundColor Yellow
    }
    else {
        Write-Host "`nCreating new $poFile..." -ForegroundColor Yellow
        Copy-Item $potFile $poFile
    }
}

Write-Host "`n" + ("=" * 50) -ForegroundColor Green
Write-Host "All .po files have been updated!" -ForegroundColor Green
Write-Host "`nNext: Translate the new entries, then run update_resx_files.bat" -ForegroundColor Cyan
