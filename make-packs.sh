#!/bin/bash

DIVADISTRO="../diva-distribution"
MAUTIL="$DIVADISTRO/bin/mautil.exe"
MIREPO="../mono-addin-repos/metaverseink"
wd=$(pwd)

cd "$DIVADISTRO" || { echo "Failed to change directory to $DIVADISTRO"; exit 1; }

"$MAUTIL" pack bin/Diva.AddinExample.dll
"$MAUTIL" pack bin/Diva.Interfaces.dll
"$MAUTIL" pack bin/Diva.Wifi.dll

mv Diva.AddinExample_* "$MIREPO"
mv Diva.Interfaces_* "$MIREPO"
mv Diva.Wifi_* "$MIREPO"

"$MAUTIL" rep-build "$MIREPO"

cd "$wd" || { echo "Failed to return to the original directory"; exit 1; }
