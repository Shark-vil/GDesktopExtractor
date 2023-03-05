::dotnet build  -r osx.10.12-x64 -p:PublishSingleFile=true --self-contained true

dotnet publish  --configuration Release -r win10-x64 -p:PublishSingleFile=true --self-contained true
dotnet publish  --configuration Release -r linux-x64 -p:PublishSingleFile=true --self-contained true

dotnet publish  --configuration Release -r win10-x86 -p:PublishSingleFile=true --self-contained true
dotnet publish  --configuration Release -r linux-x86 -p:PublishSingleFile=true --self-contained true