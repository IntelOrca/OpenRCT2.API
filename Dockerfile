# Build api using an image with build tools
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build-env

WORKDIR /openrct2-api-build
COPY . ./
RUN cd src/OpenRCT2.API \
 && dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o /openrct2-api \
 && rm /openrct2-api/*.pdb

# Build lightweight runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine

WORKDIR /openrct2-api
COPY --from=build-env /openrct2-api .
CMD ["./openrct2-api"]

EXPOSE 80
