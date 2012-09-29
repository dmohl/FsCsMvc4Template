c:\nuget\nuget.exe pack .\FsSpa-Backbone.nuspec
md c:\nuget\FsSpa-Backbone\
copy .\*.nupkg c:\nuget\FsSpa-Backbone\ /Y
pause