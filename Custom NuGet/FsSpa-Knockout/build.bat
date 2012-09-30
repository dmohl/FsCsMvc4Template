c:\nuget\nuget.exe pack .\FsSpa-Knockout.nuspec
md c:\nuget\FsSpa-Knockout\
copy .\*.nupkg c:\nuget\FsSpa-Knockout\ /Y
pause