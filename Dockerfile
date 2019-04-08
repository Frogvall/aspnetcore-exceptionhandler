FROM mcr.microsoft.com/dotnet/core/sdk:2.2
RUN mkdir app
WORKDIR /app

COPY *.sln .
COPY ./src/aspnetcore-exceptionhandler/aspnetcore-exceptionhandler.csproj /app/src/aspnetcore-exceptionhandler/aspnetcore-exceptionhandler.csproj
COPY ./src/aspnetcore-exceptionhandler-awsxray/aspnetcore-exceptionhandler-awsxray.csproj /app/src/aspnetcore-exceptionhandler-awsxray/aspnetcore-exceptionhandler-awsxray.csproj
COPY ./src/aspnetcore-exceptionhandler-modelvalidation/aspnetcore-exceptionhandler-modelvalidation.csproj /app/src/aspnetcore-exceptionhandler-modelvalidation/aspnetcore-exceptionhandler-modelvalidation.csproj
COPY ./src/aspnetcore-exceptionhandler-swagger/aspnetcore-exceptionhandler-swagger.csproj /app/src/aspnetcore-exceptionhandler-swagger/aspnetcore-exceptionhandler-swagger.csproj
COPY ./test/aspnetcore-exceptionhandler-test/aspnetcore-exceptionhandler-test.csproj /app/test/aspnetcore-exceptionhandler-test/aspnetcore-exceptionhandler-test.csproj

RUN dotnet restore

COPY . .

RUN ["sh", "build-container.sh"]

