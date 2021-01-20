# AspNetCore ExceptionHandler

[![CircleCI](https://circleci.com/gh/Frogvall/aspnetcore-exceptionhandler/tree/master.svg?style=svg)](https://circleci.com/gh/Frogvall/aspnetcore-exceptionhandler/tree/master)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.svg?label=ExceptionHandler)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.AwsXRay.svg?label=AwsXRay)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.AwsXRay/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.ModelValidation.svg?label=ModelValidation)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.ModelValidation/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson.svg?label=NewtonsoftJson)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson/)
[![Nuget](https://img.shields.io/nuget/v/Frogvall.AspNetCore.ExceptionHandling.Swagger.svg?label=Swagger)](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.Swagger/)

An exception handler for aspnet core that extends Microsofts exception handler middleware as well as implementing an exception handler filter.
It catches exceptions in your aspnet operations and transforms the exception to json response messages, and sets the status code of the response.
The package also comes with an exception mapper, that maps thrown exceptions to selected error messages and status codes. By implementing an exception mapping profile you get full control over how an exception should be mapped.
There are also a few other packages included in this repo that builds on the exception handler, but is not necessarily exception handling per se.

## Table of Contents

- [Installing the package](#installing-the-package)
  - [Extension packages](#extension-packages)
- [Using the exception handler](#using-the-exception-handler)
  - [Exception listeners](#exception-listeners)
- [Adding the exception mapper](#adding-the-exception-mapper)
  - [Mapping profiles](#mapping-profiles)
  - [Mapper options](#mapper-options)
- [Api error](#api-error)
- [Model validation package](#model-validation-package)
  - [Model validation filter](#model-validation-filter)
  - [Skip model validation filter](#skip-model-validation-filter)
- [Newtonsoft json package](#newtonsoft-json-package)
- [Swagger package](#swagger-package)
- [AWS XRay package](#aws-xray-package)
  - [Exception status code decorator](#exception-status-code-decorator)
  - [AWS XRay exception listener](#aws-xray-exception-listener)

## Installing the package

Install the nuget package from [nuget](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling/)

Either add it with the PM-Console.

```text
Install-Package Frogvall.AspNetCore.ExceptionHandling
```

Or add it to your csproj file.

```xml
<ItemGroup>
        ...
        <PackageReference Include="Frogvall.AspNetCore.ExceptionHandling" Version="6.0.0" />
        ...
</ItemGroup>
```

### Extension packages

A few other packages are handled by this repo that builds upon the functionality of the main package:

- [AwsXRay](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.AwsXRay/): Adds an extra middleware for decorating the status code of the AWS XRay trace record, needed if using the exception handler middleware in unison with AWS XRay. Also adds an exception listener for decorating the trace record with the catched exception.
- [ModelValidation](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.ModelValidation/): Adds another filter for automatically validating the models in your controller and returning with a http content on the same format as the exception handler, to make the result unison no matter if the exception handler returns it or the model validation fails. Also supplies an attribute like `[Required]`, but for non-nullable types, like integers, guids, etc.
- [NewtonsoftJson](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson/): From version 5.0.0 this package relies on the `System.Text.Json` library for writing and parsing json. This includes the extension methods for parsing an ApiError. This package adds extra parsing options for those who are using Newtonsoft.Json instead.
- [Swagger](https://www.nuget.org/packages/Frogvall.AspNetCore.ExceptionHandling.Swagger/): Adds a couple of OperationFilters for those that use Swashbuckle.Swagger and wants to automatically decorate their Open Api documentation with 400 and 500, which the exception handler can throw for any operation.

## Using the exception handler

You could hook the exception handler into your asp.net core pipeline in two ways. Either as a middleware or a filter. When hooking it in as a middleware, the exception handler uses Microsoft's exception handler middleware under the hood, which clears headers upon handling the exception. This will result in a CORS error if called from a javascript application.
If used as a filter, the headers will remain intact.
It's certainly viable to have both the middleware and the filter hooked in at the same time. What will happen in such case is that the filter will catch any error within the controller method and handle it accordingly, while the middleware will function as a final safe guard, in case the application encounters a problem before the controller method has been reached, for example while invoking another middleware.

To add the exception handler filter, add the following to according `MVCOptions` in your `ConfigureServices()` method.

```csharp
mvcOptions.Filters.Add<ApiExceptionFilter>();
```

Example:

```csharp
services.AddControllers(mvcOptions =>
    {
        mvcOptions.Filters.Add<ApiExceptionFilter>();
    });
```

To hook it into the middleware pipeline, add this to the `Configure()` method.

```csharp
app.UseApiExceptionHandler();
```

Since middlewares are dependent on the order they are executed, make sure that any middleware that executes before the exception handler middleware can never throw an exception. If that happens you service will terminate.

### Exception listeners

Sometimes you want to do some things when the exception handler catches an exception. One such example could be that you would want to add exception metadata to your tracing context, for example Amazon XRay. In order to do so, you can pass one or several actions to the exception handler middleware and filter. The actions will be executed when an exception is catched, before the http response is built.

```csharp
services.AddControllers(mvcOptions =>
    {
        mvcOptions.Filters.Add(new ApiExceptionFilter(MyExceptionListener.HandleException));
    });
```

```csharp
app.UseApiExceptionHandler(MyExceptionListener.HandleException);
```

## Adding the exception mapper

Included in the exception handler package is an exception mapper. You don't have to utilize the mapper for the exception handler to work, but you still have to initialize it.
To initialize the mapper add this to the `ConfigureServices()`method:

```csharp
services.AddExceptionMapper();
```

This is all that is needed to use the basic functionality of this package. Exceptions will be handled and parsed into a http response with a status code of 500. The exception stack trace will be pushed with the response when run locally, and a generic message will take it's place when `IsDevelopment()` is false.

### Mapping profiles

If you want to handle 4xx and 5xx errors in your api's by casting exceptions, you can create a mapping profile. Anywhere in your assembly, put a class that implements the abstract `ExceptionMappingProfile<>` class. The generic type should be an enum that describes the error. The int and string representation of the enum will both exist in the response, so keep that in mind when naming them. Exception mapping happens in the constructor and there you inform the mapper what exceptions should result in what http status code and what internal errorcode it should represent.

Example:

```csharp
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

### Mapper options

When initializing the exception mapper, there are some options you can pass in. Those are:

- `ServiceName`: Setting this option will override the default service name that will be added to the respond message. By default the entry assembly name will be chosen, but there are cases where this is not the correct name. When running in AWS Lambda for example, the default name would be "LambdaExecutor".
- `RespondWithDeveloperContext`: A boolean descibing wether the developer context should be written in the respons. This is handy for example in local development, but not recommended in production. This could for example be set to `IHostEnvironment.IsDevelopment()`. The default is `false`.

## Api error

The exception handler uses an `ApiError` record that is serialized into the response body. There are also a couple of extension methods that can be used to parse an ApiError received as the response body from a downstream service.

To parse an ApiError in an asynchronous context, use the `ParseApiErrorAsync` extension method.

```csharp
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

To parse an ApiError in a synchronous context you can call `TryParseApiError(out ApiError error)`

```csharp
if (response.TryParseApiError(out var error))
{
    //Handle api error here
}
else
{
    //Handle non-api error here
}
```

As the context and developer context of the api error class can be any object, they are deserialized as `System.Text.Json.JsonElement`.

## Model validation package

As an extension package you can add automatic model validation to your controller actions. The model validation filter will check your incoming requests against your models and return with a `400 Bad Request` if the model validation fails. The return body will be formatted in the same way as the exception handler formats any other exception. In order to utilize the model validation functionalities you need to add the model validation package to your dependencies.

```text
Install-Package Frogvall.AspNetCore.ExceptionHandling.ModelValidation
```

or

```xml
<ItemGroup>
        ...
        <PackageReference Include="Frogvall.AspNetCore.ExceptionHandling.ModelValidation" Version="6.0.0" />
        ...
</ItemGroup>
```

### Model validation filter

In order for all your controller actions to have their models automatically validated you can add the `ModelValidationFilter` to your `MVCOptions` in your `ConfigureServices()` method.

```csharp
services.AddMvc(options =>
    {
        options.Filters.Add(new ValidateModelFilter { ErrorCode = 123 } );
    });
```

Alternatively, you could add the model validation filter as an attribute to specific controllers or controller methods.

```csharp
[ValidateModelFilter(ErrorCode = 123)]
```

### Skip model validation filter

If using the mvc filter to add model validation as default, and you for some reason want to exclude a single controller or controller action from the model validation, this can be done by appending the `SkipModelValidation` attribute.

```csharp
[SkipModelValidationFilter]
```

## Newtonsoft json package

The exception handler uses System.Text.Json for serializing and deserializing json. If you are using `Newtonsoft.Json` for serialization and deserialization in your service, the exception handler still works fine. The only caveat is that the extension methods that parses the `ApiError` class is going to include `System.Text.Json.JsonElement` objects. If you rather would have them as `Newtonsoft.Json.JObjects` you can use the extension methods that are included in this package instead. In order to do so you need to add the newtonsoft json package to your dependencies.

```text
Install-Package Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson
```

or

```xml
<ItemGroup>
        ...
        <PackageReference Include="Frogvall.AspNetCore.ExceptionHandling.NewtonsoftJson" Version="6.0.0" />
        ...
</ItemGroup>
```

You can then call the newtonsoft json extension methods instead. To parse an ApiError in an asynchronous context, use the `ParseApiErrorUsingNewtonsoftJsonAsync` extension method.

```csharp
var response = await _client.PostAsync(...);
var error = await response.ParseApiErrorUsingNewtonsoftJsonAsync();
if (error != null)
{
    //Handle api error here
}
else
{
    //Handle non-api error here
}
```

To parse an ApiError in a synchronous context you can call `TryParseApiErrorUsingNewtonsoftJson(out ApiError error)`

```csharp
if (response.TryParseApiErrorUsingNewtonsoftJson(out var error))
{
    //Handle api error here
}
else
{
    //Handle non-api error here
}
```

## Swagger package

The swagger extension package is a very small package that comes with a couple of operation filters, that can be attached to your `Swashbuckle.Swagger` swagger specification in order to always decorate your swagger documentation with the two http status codes that the exception handler return. To use those operation filters you need to add the swagger package to your dependencies.

```text
Install-Package Frogvall.AspNetCore.ExceptionHandling.Swagger
```

or

```xml
<ItemGroup>
        ...
        <PackageReference Include="Frogvall.AspNetCore.ExceptionHandling.Swagger" Version="6.0.0" />
        ...
</ItemGroup>
```

You can then add the operation filters to your swagger options object.

```csharp
services.AddSwaggerGen(options =>
{
    options.OperationFilter<ValidateModelOperationFilter>();
    options.OperationFilter<InternalServerErrorOperationFilter>();
});
```

## AWS XRay package

When deploying your service in AWS and using AWS XRay for tracing, this package comes with a couple of handy addons for making your XRay traces better. In order to utilize these addons you need to add the aws xray package to your dependencies.

```text
Install-Package Frogvall.AspNetCore.ExceptionHandling.AwsXRay
```

or

```xml
<ItemGroup>
        ...
        <PackageReference Include="Frogvall.AspNetCore.ExceptionHandling.AwsXRay" Version="6.0.0" />
        ...
</ItemGroup>
```

### Exception status code decorator

When using the exception handling middleware, the status code for the response has not yet been set when the AWS XRay middleware catches the exception. Hence, the status code will be set to 200 in your XRay trace, even though it should be something else. This could be remedified by adding XRay before the exception handler, but then the exception metadata will be missing from the XRay trace instead, and any exception thrown by the XRay middleware will crash the application.
Another way to remedify the problem is to add the `ExceptionStatusCodeDecoratorMiddleware` after the XRay middleware. The status decorator will catch the exception, use the exception mapper to decorate the status code and rethrow. The exception will then be catched by the XRay middleware, the XRay trace will be decorated with the correct status code and exception metadata and rethrown and finally handled by the exception handler middleware.

```csharp
// Order is important
app.UseApiExceptionHandler();
app.UseXRay("MyServiceName");
app.ExceptionStatusCodeDecoratorMiddleware();
```

### AWS XRay exception listener

When using the exception handler filter in concordance with the AWS XRay middleware, the exception thrown will never reach the AWS XRay middleware and hence the XRay trace will never be decorated with the exception metadata. This package includes an exception listener (see #exception-listeners) that will decorate the XRay trace when the exception handler filter catches an exception. To use the AWS XRay exception listener, add its handle action when adding the exception handler filter.

```csharp
services.AddControllers(mvcOptions =>
    {
        mvcOptions.Filters.Add(new ApiExceptionFilter(AwsXRayExceptionListener.AddExceptionMetadataToAwsXRay));
    });
```
