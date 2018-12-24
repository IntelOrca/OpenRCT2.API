# OpenRCT2 Public REST API

| Branch      | Status  |
|-------------|---------|
| **master**  | [![AppVeyor](https://ci.appveyor.com/api/projects/status/4pmkp4ymiku0vrcg/branch/master?svg=true)](https://ci.appveyor.com/project/IntelOrca/openrct2-api) |

## Windows / macOS / Linux
See instructions at https://www.microsoft.com/net/core

## Docker
```
cd=`pwd`
docker pull microsoft/dotnet:2.2-sdk
docker run -v "$cd:/work" -w /work -it -p 5000:80 microsoft/dotnet:2.2-sdk bash
```

## Building / Launching
```
cd src/OpenRCT2.API
dotnet run
```

## Configuration
~/.openrct2/api.config.yml:
```
api:
  bind:
  baseUrl:
database:
  host:
  user:
  password:
  name:
s3:
  key:
  secret:
  region:
  endpoint:
openrct2.org:
  applicationToken:
```
