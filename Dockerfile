FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM base AS final
WORKDIR /app
COPY build/publish .
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]