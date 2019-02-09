FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Pull the hotwrap container  as a build dependency 
FROM fnproject/hotwrap:latest as hotwrap

# build runtime image
FROM microsoft/dotnet:2.2-runtime
WORKDIR /app
COPY --from=build-env /app/out ./
COPY --from=build-env /app/spamdata ./
CMD ["dotnet", "spamdetector.dll"]

# Install hotwrap binary in your container 
COPY --from=hotwrap /hotwrap /hotwrap 
ENTRYPOINT ["/hotwrap"]