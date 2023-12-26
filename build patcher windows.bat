@ECHO OFF
cd PWTrilogyPatcher.Desktop
dotnet publish -c Release -r win-x86 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\Publish\net7.0-windows"
ren bin\Publish\net7.0-windows\PWTrilogyPatcher.Desktop.exe AATrilogyPatcher.exe