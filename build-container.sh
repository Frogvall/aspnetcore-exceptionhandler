#!bin/bash
set -e
cd test/aspnetcore-exceptionhandler-test
dotnet restore
dotnet xunit -xml ${pwd}/../../testresults/out.xml
cd -
dotnet pack src/aspnetcore-exceptionhandler/aspnetcore-exceptionhandler.csproj -c release -o ${pwd}/package
dotnet pack src/aspnetcore-exceptionhandler-awsxray/aspnetcore-exceptionhandler-awsxray.csproj -c release -o ${pwd}/package
dotnet pack src/aspnetcore-exceptionhandler-modelvalidation/aspnetcore-exceptionhandler-modelvalidation.csproj -c release -o ${pwd}/package
dotnet pack src/aspnetcore-exceptionhandler-swagger/aspnetcore-exceptionhandler-swagger.csproj -c release -o ${pwd}/package
