@ECHO OFF
cd AATrilogyPatcher
dotnet publish -c Release-Windows -r win-x86 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\Publish\net5.0-windows"