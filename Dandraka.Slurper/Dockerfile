# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build the project
RUN dotnet build -c Release --no-restore

# Test stage
FROM build AS test
WORKDIR /src
RUN dotnet test -c Release --no-build

# Publish stage
FROM build AS publish
WORKDIR /src
RUN dotnet pack -c Release --no-build -o /app/output

# Final image for using the Slurper library
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/output .

# Optional entrypoint for running sample code
ENTRYPOINT ["dotnet", "Dandraka.Slurper.dll"]