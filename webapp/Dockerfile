FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
USER root

# install mode
RUN apt-get update
RUN apt-get -y install curl gnupg
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash -
RUN apt-get -y install nodejs

WORKDIR /build



# reinstall dependencies, if changed
COPY ./package*.json ./
RUN npm install --no-optional

# copy sources
ADD . .

# build
RUN npm run build

FROM nginx:1.18.0-alpine

COPY --from=builder /build/public /usr/share/nginx/html

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]