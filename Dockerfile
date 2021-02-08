# Build api using an image with build tools
FROM mcr.microsoft.com/dotnet/core/sdk:5.0-alpine AS build-env

WORKDIR /openrct2-api-build
COPY . ./
RUN cd src/OpenRCT2.API \
 && dotnet publish -c Release -o /openrct2-api \
 && rm /openrct2-api/*.pdb

# Build lightweight runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

WORKDIR /openrct2-api
COPY --from=build-env /openrct2-api .
CMD ["./openrct2-api"]

EXPOSE 80
