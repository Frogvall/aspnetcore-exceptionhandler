# AspNetCore ExceptionHandler

[![CircleCI](https://circleci.com/gh/Frogvall/aspnetcore-exceptionhandler/tree/master.svg?style=svg)](https://circleci.com/gh/Frogvall/aspnetcore-exceptionhandler/tree/master)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.svg?label=ExceptionHandler)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.AwsXRay.svg?label=AwsXRay)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.AwsXRay/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.ModelValidation.svg?label=ModelValidation)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.ModelValidation/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson.svg?label=NewtonsoftJson)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.Swagger.svg?label=Swagger)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.Swagger/)

An exception handler for asp.net core that extends Microsofts exception handler middleware as well as implementing an exception handler filter.
It catches exceptions in your asp net operations and transforms the exception to json response messages, and sets the status code of the response.
The package also comes with an exception mapper, that maps thrown exceptions to selected error messages and status codes. By implementing an exception mapping profile you get full control over how an exception should be mapped.
There are also a few other packages included in this repo that builds on the exception handler, but is not necessarily exception handling per se.

## Table of Contents

## Getting started

### Installing the package

Install the nuget package from [nuget](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling/)

Either add it with the PM-Console:

        Install-Package Frogvall.AspNetCore.ExceptionHandling

Or add it to your csproj file:
```xml
        <ItemGroup>
                ...
                <PackageReference Include="Frogvall.AspNetCore.ExceptionHandling" Version="5.0.0" />
                ...
        </ItemGroup>
```

### Extension packages

A few other packages are handled by this repo that builds upon the functionality of the main package:

- [AwsXray](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.AwsXRay/) - Adds an extra middleware for decorating the status code of the xray trace record, needed if using the exception handler middleware in unison with Amazon XRay. Also adds an exception listener for decorating the trace record with the catched exception.
- [ModelValidation](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.ModelValidation/) - Adds another filter for automatically validating the models in your controller and returning with a http content on the same format as the exception handler, to make the result unison no matter if the exception handler returns it or the model validation fails. Also supplies an attribute like `[Required]`, but for non-nullable types, like integers, guids, etc.
- [NewtonsoftJson](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson/) - From version 5.0.0 this package relies on the `System.Text.Json` library for writing and parsing json. This includes the extension methods for parsing an ApiError. This package adds extra parsing options for those who are using Newtonsoft.Json instead.
- [Swagger](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.Swagger/) - Adds a couple of OperationFilters for those that use Swashbuckle.Swagger and wants to automatically decorate their Open Api documentation with 400 and 500, which the exception handler can throw for any operation.

## Using the exception handler

======= OLD =======

AspNetCore Exception Handler for asp.net core that include things like an Exception Handler middleware, modelstate validation by attribute, RequireNonDefault attribute for controller models, and swagger operation filters for 400 and 500.

## Getting started

### Install the package
Install the nuget package from [nuget](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling/)

Either add it with the PM-Console:

        Install-Package Frogvall.AspNetCore.ExceptionHandling

Or add it to csproj file ->
```xml
        <ItemGroup>
                ...
                <PackageReference Include="Frogvall.AspNetCore.ExceptionHandling" Version="x.y.z" />
                ...
        </ItemGroup>
```

Additional features, not directly related to exception handling per se, but that builds on this package has been moved into separate nugets:
Frogvall.AspNetCore.ExceptionHandling.AwsXRay
Frogvall.AspNetCore.ExceptionHandling.ModelValidation
Frogvall.AspNetCore.ExceptionHandling.Swagger

### Using the utilites

Edit your Startup.cs ->
```cs
        public void ConfigureServices(IServiceCollection services)
        {
          //...

          services.AddExceptionMapper();
          services.AddMvc(options =>
             {
                options.Filters.Add<ApiExceptionFilter>();
             });

          //...
        }

        public void Configure()
        {
           //...

           app.UseApiExceptionHandler();

           //...
        }
```
Create an exeption that inherits BaseApiException ->
```cs
        public class MyException : BaseApiException
```

Create an enum that describes your error codes ->
```cs
        public enum MyErrorEnum
        {
           MyErrorCode = 1337
        }
```

Create one or more exception mapper profiles anywhere in your project. Add mappings in the constructor of the profile ->
```cs
        public class MyMappingProfile : ExceptionMappingProfile<MyErrorEnum>
        {
          public MyMappingProfile()
          {
             AddMapping<MyException>(HttpStatusCode.BadRequest, MyErrorEnum.MyErrorCode);
          }
        }
```
Throw when returning non 2xx ->
```cs
        throw new MyException("Some message.", new { AnyProperty = "AnyValue."});
```
Either add to controller or controller method ->
```cs
        [ValidateModelFilter(ErrorCode = 123)]
```
Or add to the filters of MVC ->
```cs
        services.AddMvc(options =>
           {
               options.Filters.Add(new ValidateModelFilter { ErrorCode = 123 } );
           });
```

Add to controller model (dto) property ->
```cs
        [RequireNonDefault]
```
Add to swagger spec ->
```cs
        options.OperationFilter<ValidateModelOperationFilter>();
        options.OperationFilter<InternalServerErrorOperationFilter>();
```

To consume the api error from another service using this package in an asynchronous context ->
```cs
        var response = await _client.PostAsync(...);
        var error = await response.ParseApiErrorAsync();
        if (error != null)
        {
            //Handle api error here
        }
        else
        {
            //Handle non-api error here
        }
```

To consume the api error from another service using this package in a synchronous context ->
```cs
        if (response.TryParseApiError(out var error))
        {
            //Handle api error here
        }
        else
        {
            //Handle non-api error here
        }
```

## Build and Publish

### Prequisites

* docker, docker-compose
* dotnet core 2.0 sdk  [download core](https://www.microsoft.com/net/core)

The package is build in docker so you will need to install docker to build and publish the package.
(Of course you could just build it on the machine you are running on and publish it from there.
I prefer to build and publish from docker images to have a reliable environment, plus make it easier
to build this on circleci).

### build

run:
        docker-compose -f docker-compose-build.yml up

this will build & test the code. The testresult will be in folder ./testresults and the package in ./package

! *if you clone the project on a windows machine you will have to change the file endings on the build-container.sh to LF*

### publish

run: (fill in the api key):

        docker run --rm -v ${PWD}/package:/data/package schwamster/nuget-docker push /data/package/*.nupkg <your nuget api key> -Source nuget.org

this will take the package from ./package and push it to nuget.org

### build on circleci

The project contains a working circle ci yml file. All you have to do is to configure the Nuget Api Key in the build projects environment variables on circleci (Nuget_Api_Key)


