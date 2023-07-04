@ECHO OFF
cd AATrilogyPatcher
dotnet publish -c Release -r linux-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --output "bin\Publish\net5.0-linux"
ren bin\Publish\net5.0-linux\AATrilogyPatcher AATrilogyPatcher-linux