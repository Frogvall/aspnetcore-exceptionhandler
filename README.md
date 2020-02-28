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
You could hook the exception handler into your asp.net core pipeline in two ways. Either as a middleware or a filter. When hooking it in as a middleware, the exception handler uses Microsoft's exception handler middleware under the hood, which clears headers upon handling the exception. This will result in a CORS error if called from a javascript application.
If used as a filter, the headers will remain intact.
It's certainly viable to have both the middleware and the filter hooked in at the same time. What will happen in such case is that the filter will catch any error within the controller method and handle it accordingly, while the middleware will function as a final safe guard, in case the application encounters a problem before the controller method has been reached, for example while invoking another middleware.

To add the exception handler filter, add the following to according `MVCOptions` in your `ConfigureServices()` method:

```cs
mvcOptions.Filters.Add<ApiExceptionFilter>();
```

For example:

```cs
services.AddControllers(mvcOptions =>
    {
        mvcOptions.Filters.Add<ApiExceptionFilter>();
    });
```

To hook it into the middleware pipeline, add this to the `Configure()` method:

```cs
app.UseApiExceptionHandler();
```

## Add the exception mapper
Included in the exception handler package is an exception mapper. You don't have to utilize the mapper for the exception handler to work, but you still have to initialize it.
To initialize the mapper add this to the `ConfigureServices()`method:

```cs
services.AddExceptionMapper();
```

This is all that is needed to use the basic functionality of this package. Exceptions will be handled and parsed into a http response with a status code of 500. The exception stack trace will be pushed with the response when run locally, and a generic message will take it's place when `IsDevelopment()` is false.

## Adding mapping profiles

If you want to handle 4xx and 5xx errors in your api's by casting exceptions, you can create a mapping profile. Anywhere in your assembly, put a class that implements the abstract `ExceptionMappingProfile<>` class. The generic type should be an enum that describes the error. The int and string representation of the enum will both exist in the response, so keep that in mind when naming them. Exception mapping happens in the constructor and there you inform the mapper what exceptions should result in what http status code and what internal errorcode it should represent.

Example:
```cs
public class MyMappingProfile : ExceptionMappingProfile<MyErrorEnum>
    {
        public MyMappingProfile()
        {
            AddMapping<MyException>(HttpStatusCode.BadRequest, MyErrorEnum.MyErrorCode);
        }
    }
```

As an alternative, the AddMapping function can take a lambda instead of an enum.

The exception mapper only handles exceptions that implement the `BaseApiException` included in this package, as a way for you to prove to it that you own the exception. The reasoning for that is that you don't necessary know where other exceptions come from in beforehand and therefore can't be sure it's mapped correctly.

After an exception is mapped, it can be thrown from anyewhere in order to abort the current api action and return with the corresponding http status code.

## Exception listener

## Exception handler options

======= OLD =======

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


