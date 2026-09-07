#!/bin/sh
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
OPENSSL_ENABLE_SHA1_SIGNATURES=1 dotnet publish $SCRIPTPATH/src/Amsel.Blazor -c Release -r linux-x64 --output $SCRIPTPATH/publish -p:TreatWarningsAsErrors=false 
