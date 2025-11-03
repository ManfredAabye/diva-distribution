#!/bin/bash

echo "Building Diva Distribution for .NET 8"
echo "======================================"

case "$1" in

  'clean')
    echo "Cleaning previous build..."
    mono bin/Prebuild.exe /clean
    rm -rf bin/addin-db-*
    rm -f compile.sh
  ;;

  'autoclean')
    echo "Auto-cleaning previous build..."
    echo y|mono bin/Prebuild.exe /clean
    rm -rf bin/addin-db-*
    rm -f compile.sh
  ;;

  *)
    echo "Running prebuild for .NET 8..."
    mono bin/Prebuild.exe /target vs2022

    # Check if dotnet CLI is available
    if command -v dotnet &> /dev/null; then
        echo "Found .NET CLI"
        echo "Creating compile.sh for .NET 8"
        echo "#!/bin/bash" > compile.sh
        echo "dotnet build opensim.sln -c Debug" >> compile.sh
        echo "echo 'Build completed. To compile in Release mode, use: dotnet build opensim.sln -c Release'" >> compile.sh
        chmod +x compile.sh
    else
        echo "Error: .NET CLI not found!"
        echo "Please install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    fi

    echo "Prebuild completed successfully"
  ;;

esac

rm -fr bin/addin-db-002

