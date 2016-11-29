# OpenRCT2 Public REST API

## Windows / macOS / Linux
See instructions at https://www.microsoft.com/net/core

## Docker
```
cd=`pwd`
docker pull microsoft/dotnet:latest
docker run -v "$cd:/work" -w /work -i -t -p 5004:5004 microsoft/dotnet:latest bash
```

## Building / Launching
```
cd src/OpenRCT2.API
dotnet restore
dotnet run http://localhost:5004

# For docker:
dotnet run http://*:5004
```

## Configuration
~/.openrct2/api.config.json:
```
{
  "database": {
    "host": "...",
    "user": "...",
    "password": "...",
    "name": "openrct2"
  },
  "openrct2.org": {
    "applicationToken": "..."
  }
}
```
