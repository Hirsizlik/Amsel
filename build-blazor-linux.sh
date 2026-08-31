#!/bin/sh
OPENSSL_ENABLE_SHA1_SIGNATURES=1 dotnet publish $PWD/src/Amsel.Blazor -c Release -r linux-x64 --output publish -p:TreatWarningsAsErrors=false 
