pushd .\src\AddressParser\bin\Debug\
dotnet nuget -v Verbose  push AddressParser.1.?.?.symbols.nupkg -s https://api.nuget.org/v3/index.json
popd