# IdentityServer4 authentication server

Это [IdentityServer4](https://github.com/IdentityServer/IdentityServer4) натянутый на aspnetcore webapi

## Build and Run

```
> dotnet restore
> dotnet run
```

## Убедиться что работает

```
GET https://localhost:6001/.well-known/openid-configuration
```