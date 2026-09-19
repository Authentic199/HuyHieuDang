using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Common.Services;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;

namespace HuyHieuDang.Infrastructure.Facades.Middleware;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class ExceptionHandlerMiddleware
{
    private const string SupportMessageTemplate = "Please furnish the TraceId {traceId} to our dedicated support team for in-depth analysis and assistance.";
    private readonly RequestDelegate next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext httpContext, IJsonSerializerService jsonSerializerService, IConfiguration configuration)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception exception)
        {
            ErrorResultWrapper errorResultWrapper = HandleException(exception);

            if (errorResultWrapper.StatusCode >= 500)
            {
                LogErrorResultWrapper(errorResultWrapper);
            }

            if (!httpContext.Response.HasStarted)
            {
                ErrorResponseSettings? errorResponseSettings = configuration.GetSection(nameof(ErrorResponseSettings)).Get<ErrorResponseSettings?>();
                HiddenResult(errorResultWrapper, errorResponseSettings);
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = errorResultWrapper.StatusCode;
                await httpContext.Response.WriteAsync(jsonSerializerService.Serialize(errorResultWrapper, x => x.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault));
            }
            else
            {
                Log.Warning("Can't write error response. Response has already started.");
            }
        }
    }

    private static ErrorResultWrapper HandleException(Exception exception)
    {
        return exception is HttpCustomException customException ?
                HandleHttpCustomException(customException)
                :
                HandleDefaultException(exception);
    }

    private static ErrorResultWrapper HandleHttpCustomException(HttpCustomException customException)
    {
        ErrorResultWrapper errorResultWrapper = new()
        {
            StatusCode = (int)customException.StatusCode,
            Message = customException.Message,
        };

        if (customException is InternalServerException && customException.InnerException != null)
        {
            ModifyErrorResultWrapper(errorResultWrapper, customException.InnerException);
        }

        return errorResultWrapper;
    }

    private static ErrorResultWrapper HandleDefaultException(Exception exception)
    {
        ErrorResultWrapper errorResultWrapper = new()
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = exception.Message,
        };

        ModifyErrorResultWrapper(errorResultWrapper, exception);
        return errorResultWrapper;
    }

    private static void ModifyErrorResultWrapper(ErrorResultWrapper errorResultWrapper, Exception exception)
    {
        string traceId = NewId.Next().ToString();
        errorResultWrapper.TraceId = traceId;
        errorResultWrapper.SupportMessage = SupportMessageTemplate.Replace("{traceId}", traceId, StringComparison.Ordinal);
        errorResultWrapper.Exception = exception.ToString();
        errorResultWrapper.Source = exception.TargetSite?.DeclaringType?.FullName;
        errorResultWrapper.Method = exception.TargetSite?.Name;
        errorResultWrapper.Line = new StackTrace(exception, true).GetFrame(0)?.GetFileLineNumber() ?? -1;
    }

    private static void HiddenResult(ErrorResultWrapper errorResultWrapper, ErrorResponseSettings? errorResponseSettings)
    {
        if (errorResponseSettings?.HiddenProperties?.Count > 0)
        {
            Dictionary<string, PropertyInfo> infoDic = errorResultWrapper.GetType().GetProperties().ToDictionary(x => x.Name, x => x);
            foreach (string hiddenProperty in from string hiddenProperty in errorResponseSettings.HiddenProperties
                                              where infoDic.ContainsKey(hiddenProperty)
                                              select hiddenProperty)
            {
                infoDic[hiddenProperty].SetValue(errorResultWrapper, default);
            }
        }
    }

    private static void LogErrorResultWrapper(ErrorResultWrapper errorResultWrapper)
    {
        Log.Error(
            "[Source]: {source}\n[Method]: {method}\n[Line]: {line}\n[Request]: failed with Status Code {statusCode} and TraceId {traceId}.\n[Detail]: {exeption}.",
            errorResultWrapper.Source,
            errorResultWrapper.Method,
            errorResultWrapper.Line,
            errorResultWrapper.StatusCode,
            errorResultWrapper.TraceId,
            errorResultWrapper.Exception
        );
    }
}