pushd .\AddressParser\bin\Debug\
..\..\..\.nuget\NuGet.exe push AddressParser.0.0.??.nupkg -Verbosity detailed
..\..\..\.nuget\NuGet.exe push AddressParser.0.0.??.symbols.nupkg -Verbosity detailed
popd