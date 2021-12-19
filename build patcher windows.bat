@ECHO OFF
cd AATrilogyPatcher
dotnet publish -c Release -r win-x86 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\Release\net5.0-windows"