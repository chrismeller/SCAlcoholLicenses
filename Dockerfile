FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy only the solution and project files to reduce the chances the layer will be invalidated
COPY ./*.sln ./
COPY ./src/SCAlcoholLicenses.Client/*.csproj ./src/SCAlcoholLicenses.Client/
COPY ./src/SCAlcoholLicenses.Data/*.csproj ./src/SCAlcoholLicenses.Data/
COPY ./src/SCAlcoholLicenses.Domain/*.csproj ./src/SCAlcoholLicenses.Domain/
COPY ./src/SCAlcoholLicenses.Host/*.csproj ./src/SCAlcoholLicenses.Host/

# restore all nuget packages
RUN dotnet restore

# copy everything else
COPY ./* ./
COPY ./src/SCAlcoholLicenses.Client/* ./src/SCAlcoholLicenses.Client/
COPY ./src/SCAlcoholLicenses.Data/* ./src/SCAlcoholLicenses.Data/
COPY ./src/SCAlcoholLicenses.Domain/* ./src/SCAlcoholLicenses.Domain/
COPY ./src/SCAlcoholLicenses.Host/* ./src/SCAlcoholLicenses.Host/

# make sure everything is clean in case we copied over some build artifacts
RUN dotnet clean

# publish
RUN dotnet publish -c release -o /app --no-restore ./src/SCAlcoholLicenses.Host

# copy all our build artifacts over
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS release
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "SCAlcoholLicenses.Host.dll"]