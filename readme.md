# OpenRCT2 Public REST API

| Branch      | Status  |
|-------------|---------|
| **master**  | [![AppVeyor](https://ci.appveyor.com/api/projects/status/60n0fpq53ddr0gjh/branch/master?svg=true)](https://ci.appveyor.com/project/OpenRCT2/openrct2-api) |

## Windows / macOS / Linux
See instructions at https://www.microsoft.com/net/core

## Docker
```
cd=`pwd`
docker pull docker pull mcr.microsoft.com/dotnet/sdk:6.0
docker run -v "$(pwd)":/work -w /work -it -p 5000:80 mcr.microsoft.com/dotnet/sdk:6.0 bash
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
