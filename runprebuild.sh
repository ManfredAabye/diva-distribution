#!/bin/sh

case "$1" in

 'clean')
    dotnet bin/prebuild.dll /file divaprebuild.xml /clean

  ;;


  'autoclean')

    echo y|dotnet bin/prebuild.dll /file divaprebuild.xml /clean

  ;;



  *)

    cp bin/System.Drawing.Common.dll.linux bin/System.Drawing.Common.dll
    dotnet bin/prebuild.dll /target vs2022 /targetframework net8_0 /excludedir = "obj | bin" /file divaprebuild.xml
    echo "dotnet build -c Release DivaOpenSim.sln" > divacompile.sh
    chmod +x divacompile.sh

  ;;

esac
