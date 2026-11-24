#!/bin/sh
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
pushd $SCRIPTPATH/externals/unityspy/src/HackF5.UnitySpy.Gui.Avalonia
OPENSSL_ENABLE_SHA1_SIGNATURES=1 LANG=en_us dotnet run
popd
