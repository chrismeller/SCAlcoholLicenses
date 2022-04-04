FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base

RUN useradd dotnet


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

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

# publish
RUN dotnet publish -c release -o /app --no-restore ./src/SCAlcoholLicenses.Host

# copy all our build artifacts over
FROM base AS release
WORKDIR /app
COPY --from=build /app ./

# make sure everything is owned by the right user
RUN chown -R dotnet:dotnet /app

# and run our app as that user
USER dotnet

ENTRYPOINT ["dotnet", "SCAlcoholLicenses.Host.dll"]