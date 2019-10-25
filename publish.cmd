mkdir nupkg

cd nupkg

del *.nupkg

cd ..

cd DotnetMigrations.Lib
dotnet pack -o ../nupkg
cd ..

cd DotnetMigrations
dotnet pack -o ../nupkg
cd ..

cd nupkg

for %%i in (*) do dotnet nuget push -k %NUGET_KEY% -s https://nuget.org %%i

cd ..
