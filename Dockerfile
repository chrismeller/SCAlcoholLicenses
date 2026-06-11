FROM mcr.microsoft.com/playwright/dotnet:latest AS base

RUN useradd dotnet


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# install the entity framework tools
RUN dotnet tool install --global dotnet-ef

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
RUN dotnet publish -c Release -o /app --no-restore ./src/SCAlcoholLicenses.Host

# generate a migrations bundle
RUN dotnet tool restore
RUN dotnet ef migrations bundle --project ./src/SCAlcoholLicenses.Host --no-build --verbose --configuration Release -o /app/efbundle.exe

# copy all our build artifacts over
FROM base AS release
WORKDIR /app
COPY --from=build --chown=dotnet:dotnet /app ./

# and run our app as that user
USER dotnet

CMD ["dotnet", "SCAlcoholLicenses.Host.dll"]