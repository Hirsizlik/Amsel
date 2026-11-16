#!/bin/sh
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
OPENSSL_ENABLE_SHA1_SIGNATURES=1 dotnet publish $SCRIPTPATH/src/Amsel.Avalonia -c Release -r linux-x64 --output $SCRIPTPATH/publish-avalonia -p:TreatWarningsAsErrors=false 
