FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY host/ ./host/
COPY migrations/ ./migrations/
RUN chmod -R a+rX /app/host /app/migrations
WORKDIR /app/host
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Host.dll"]
