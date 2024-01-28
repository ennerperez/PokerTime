FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG configuration=Debug
WORKDIR /app
COPY . .
RUN chmod +x build.sh
RUN ./build.sh --configuration $configuration --target Publish
RUN dotnet dev-certs https -ep /app/publish/PokerTime.Web/certificate.pfx -p 0ca271cdf1764804a136eba180f02673

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS run
RUN apt-get update && apt-get install -y libfontconfig1
EXPOSE 6000/tcp
EXPOSE 6001/tcp
WORKDIR /app
COPY --from=build /app/publish/PokerTime.Web .
ENV ASPNETCORE_URLS=http://+:6000;https://+:6001
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="0ca271cdf1764804a136eba180f02673"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=certificate.pfx
ENTRYPOINT ["dotnet", "PokerTime.Web.dll"]
