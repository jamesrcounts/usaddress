pushd .\AddressParser\bin\Debug\
..\..\..\.nuget\NuGet.exe push AddressParser.0.0.??.nupkg -Verbosity detailed -Source https://api.nuget.org/v3/index.json
popd