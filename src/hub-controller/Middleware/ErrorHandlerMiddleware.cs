using Amazon.Lambda.Core;
using HubController.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace HubController.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                LambdaLogger.Log($"**************** {error.Message}\n${error.StackTrace}");
                var response = context.Response;
                response.ContentType = "application/json";

                response.StatusCode = error switch
                {
                    ArgumentException _ => (int)HttpStatusCode.BadRequest,
                    KeyNotFoundException _ => (int)HttpStatusCode.NotFound,
                    LimitExceededException _ => (int) HttpStatusCode.TooManyRequests,
                    _ => (int)HttpStatusCode.InternalServerError,
                };
                var result = JsonSerializer.Serialize(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}