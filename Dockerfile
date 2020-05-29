# Build api using an image with build tools
FROM microsoft/dotnet:3.1-sdk-alpine AS build-env

WORKDIR /openrct2-api-build
COPY . ./
RUN cd src/OpenRCT2.API && \
    dotnet publish -c Release -o /openrct2-api

# Build lightweight runtime image
FROM microsoft/dotnet:3.1-aspnetcore-runtime-alpine

WORKDIR /openrct2-api
COPY --from=build-env /openrct2-api .
CMD ["dotnet", "OpenRCT2.API.dll"]

EXPOSE 80
