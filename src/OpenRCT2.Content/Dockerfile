# Build static website using an image with build tools
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env

WORKDIR /openrct2-content-build
COPY . .
RUN cd src/OpenRCT2.Content \
 && dotnet publish -c Release -o /openrct2-content

# Build content website with nginx host
FROM nginx:alpine
COPY --from=build-env /openrct2-content/wwwroot /openrct2/content
COPY ./src/OpenRCT2.Content/content.conf /etc/nginx/conf.d
RUN rm /etc/nginx/conf.d/default.conf

EXPOSE 80
