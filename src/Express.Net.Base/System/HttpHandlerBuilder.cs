using Express.Net.System.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Express.Net.System
{
    internal static class HttpHandlerBuilder
    {
        private static readonly MethodInfo ChangeTypeMethodInfo = GetMethodInfo<Func<object, Type, object>>((value, type) => Convert.ChangeType(value, type));
        private static readonly MethodInfo ExecuteTaskOfTMethodInfo = typeof(HttpHandlerBuilder).GetMethod(nameof(ExecuteTask), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo ExecuteValueTaskOfTMethodInfo = typeof(HttpHandlerBuilder).GetMethod(nameof(ExecuteValueTask), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo ExecuteTaskResultOfTMethodInfo = typeof(HttpHandlerBuilder).GetMethod(nameof(ExecuteTaskResult), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo ExecuteValueResultTaskOfTMethodInfo = typeof(HttpHandlerBuilder).GetMethod(nameof(ExecuteValueTaskResult), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo GetRequiredServiceMethodInfo = typeof(HttpHandlerBuilder).GetMethod(nameof(GetRequiredService), BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly MethodInfo ObjectResultExecuteAsync = typeof(BaseResponse).GetMethod(nameof(BaseResponse.ExecuteAsync), BindingFlags.Public | BindingFlags.Instance)!;
        private static readonly MethodInfo ResultExecuteAsync = typeof(IResult).GetMethod(nameof(IResult.ExecuteAsync), BindingFlags.Public | BindingFlags.Instance)!;
        private static readonly MethodInfo StringResultExecuteAsync = GetMethodInfo<Func<HttpResponse, string, Task>>((response, text) => HttpResponseWritingExtensions.WriteAsync(response, text, default));
        private static readonly MethodInfo JsonResultExecuteAsync = GetMethodInfo<Func<HttpResponse, object, Task>>((response, value) => HttpResponseJsonExtensions.WriteAsJsonAsync(response, value, default));

        private static readonly MemberInfo CompletedTaskMemberInfo = GetMemberInfo<Func<Task>>(() => Task.CompletedTask);

        private static readonly Expression CancellationTokenNoneExpr = Expression.Property(null, typeof(CancellationToken).GetProperty(nameof(CancellationToken.None))!);

        internal static void Build<THttpHandler>(IEndpointRouteBuilder routes)
        {
            Build(typeof(THttpHandler), routes);
        }

        internal static void Build(Type handlerType, IEndpointRouteBuilder routes)
        {
            var model = HttpModel.FromType(handlerType, typeof(IResult).Assembly);

            var factory = (ObjectFactory?)null;

            foreach (var method in model.Methods)
            {
                // Nothing to route to
                if (method.RoutePattern == null)
                {
                    continue;
                }

                var needForm = false;
                var needBody = false;
                var bodyType = (Type?)null;

                var httpContextArg = Expression.Parameter(typeof(HttpContext), "httpContext");
                var deserializedBodyArg = Expression.Parameter(typeof(object), "bodyValue");

                var requestServicesExpr = Expression.Property(httpContextArg, nameof(HttpContext.RequestServices));
                var ctors = handlerType.GetConstructors();

                var httpHandlerExpression = (Expression?)null;

                if (!method.MethodInfo.IsStatic)
                {
                    if (ctors.Length == 1 && ctors[0].GetParameters().Length == 0)
                    {
                        httpHandlerExpression = Expression.New(ctors[0]);
                    }
                    else
                    {
                        if (factory == null)
                        {
                            factory = ActivatorUtilities.CreateFactory(handlerType, Type.EmptyTypes);
                        }

                        var invokeFactoryExpr = Expression.Invoke(Expression.Constant(factory), requestServicesExpr, Expression.Constant(null, typeof(object[])));
                        httpHandlerExpression = Expression.Convert(invokeFactoryExpr, handlerType);
                    }
                }

                var args = new List<Expression>();

                var httpRequestExpr = Expression.Property(httpContextArg, nameof(HttpContext.Request));
                var httpResponseExpr = Expression.Property(httpContextArg, nameof(HttpContext.Response));

                foreach (var parameter in method.Parameters)
                {
                    Expression paramterExpression = Expression.Default(parameter.ParameterType);

                    if (parameter.FromQuery != null)
                    {
                        var queryProperty = Expression.Property(httpRequestExpr, nameof(HttpRequest.Query));
                        paramterExpression = BindArgument(queryProperty, parameter, parameter.FromQuery);
                    }
                    else if (parameter.FromHeader != null)
                    {
                        var headersProperty = Expression.Property(httpRequestExpr, nameof(HttpRequest.Headers));
                        paramterExpression = BindArgument(headersProperty, parameter, parameter.FromHeader);
                    }
                    else if (parameter.FromRoute != null)
                    {
                        var routeValuesProperty = Expression.Property(httpRequestExpr, nameof(HttpRequest.RouteValues));
                        paramterExpression = BindArgument(routeValuesProperty, parameter, parameter.FromRoute);
                    }
                    else if (parameter.FromCookie != null)
                    {
                        var cookiesProperty = Expression.Property(httpRequestExpr, nameof(HttpRequest.Cookies));
                        paramterExpression = BindArgument(cookiesProperty, parameter, parameter.FromCookie);
                    }
                    else if (parameter.FromServices)
                    {
                        paramterExpression = Expression.Call(GetRequiredServiceMethodInfo.MakeGenericMethod(parameter.ParameterType), requestServicesExpr);
                    }
                    else if (parameter.FromForm != null)
                    {
                        needForm = true;

                        var formProperty = Expression.Property(httpRequestExpr, nameof(HttpRequest.Form));
                        paramterExpression = BindArgument(formProperty, parameter, parameter.FromForm);
                    }
                    else if (parameter.FromBody)
                    {
                        if (needBody)
                        {
                            throw new InvalidOperationException(method.MethodInfo.Name + " cannot have more than one FromBody attribute.");
                        }

                        if (needForm)
                        {
                            throw new InvalidOperationException(method.MethodInfo.Name + " cannot mix FromBody and FromForm on the same method.");
                        }

                        needBody = true;
                        bodyType = parameter.ParameterType;
                        paramterExpression = Expression.Convert(deserializedBodyArg, bodyType);
                    }
                    else
                    {
                        if (parameter.ParameterType == typeof(IFormCollection))
                        {
                            needForm = true;

                            paramterExpression = Expression.Property(httpRequestExpr, nameof(HttpRequest.Form));
                        }
                        else if (parameter.ParameterType == typeof(HttpContext))
                        {
                            paramterExpression = httpContextArg;
                        }
                    }

                    args.Add(paramterExpression);
                }

                Expression? body = null;

                var methodCall = Expression.Call(httpHandlerExpression, method.MethodInfo, args);

                // Exact request delegate match
                if (method.MethodInfo.ReturnType == typeof(void))
                {
                    var bodyExpressions = new List<Expression>
                    {
                        methodCall,
                        Expression.Property(null, (PropertyInfo)CompletedTaskMemberInfo)
                    };

                    body = Expression.Block(bodyExpressions);
                }
                else if (AwaitableInfo.IsTypeAwaitable(method.MethodInfo.ReturnType, out var info))
                {
                    if (method.MethodInfo.ReturnType == typeof(Task))
                    {
                        body = methodCall;
                    }
                    else if (method.MethodInfo.ReturnType.IsGenericType &&
                             method.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var typeArg = method.MethodInfo.ReturnType.GetGenericArguments()[0];

                        if (typeof(IResult).IsAssignableFrom(typeArg))
                        {
                            body = Expression.Call(
                                               ExecuteTaskResultOfTMethodInfo.MakeGenericMethod(typeArg),
                                               methodCall,
                                               httpContextArg);
                        }
                        else
                        {
                            // ExecuteTask<T>(handler.Method(..), httpContext);
                            body = Expression.Call(
                                               ExecuteTaskOfTMethodInfo.MakeGenericMethod(typeArg),
                                               methodCall,
                                               httpContextArg);
                        }
                    }
                    else if (method.MethodInfo.ReturnType.IsGenericType &&
                             method.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    {
                        var typeArg = method.MethodInfo.ReturnType.GetGenericArguments()[0];

                        if (typeof(IResult).IsAssignableFrom(typeArg))
                        {
                            body = Expression.Call(
                                               ExecuteValueResultTaskOfTMethodInfo.MakeGenericMethod(typeArg),
                                               methodCall,
                                               httpContextArg);
                        }
                        else
                        {
                            // ExecuteTask<T>(handler.Method(..), httpContext);
                            body = Expression.Call(
                                           ExecuteValueTaskOfTMethodInfo.MakeGenericMethod(typeArg),
                                           methodCall,
                                           httpContextArg);
                        }
                    }
                    else
                    {
                        // TODO: Handle custom awaitables
                        throw new NotSupportedException("Unsupported return type " + method.MethodInfo.ReturnType);
                    }
                }
                else if (typeof(IResult).IsAssignableFrom(method.MethodInfo.ReturnType))
                {
                    body = Expression.Call(methodCall, ResultExecuteAsync, httpContextArg);
                }
                else if (method.MethodInfo.ReturnType == typeof(string))
                {
                    body = Expression.Call(StringResultExecuteAsync, httpResponseExpr, methodCall, CancellationTokenNoneExpr);
                }
                else
                {
                    body = Expression.Call(JsonResultExecuteAsync, httpResponseExpr, methodCall, CancellationTokenNoneExpr);
                }

                RequestDelegate? requestDelegate = null;

                if (needBody && bodyType != null)
                {
                    // We need to generate the code for reading from the body before calling into the 
                    // delegate
                    var lambda = Expression.Lambda<Func<HttpContext, object?, Task>>(body, httpContextArg, deserializedBodyArg);
                    var invoker = lambda.Compile();

                    requestDelegate = async httpContext =>
                    {
                        var bodyValue = await httpContext.Request.ReadFromJsonAsync(bodyType);

                        await invoker(httpContext, bodyValue);
                    };
                }
                else if (needForm)
                {
                    var lambda = Expression.Lambda<RequestDelegate>(body, httpContextArg);
                    var invoker = lambda.Compile();

                    requestDelegate = async httpContext =>
                    {
                        // Generating async code would just be insane so if the method needs the form populate it here
                        // so the within the method it's cached
                        await httpContext.Request.ReadFormAsync();

                        await invoker(httpContext);
                    };
                }
                else
                {
                    var lambda = Expression.Lambda<RequestDelegate>(body, httpContextArg);
                    var invoker = lambda.Compile();

                    requestDelegate = invoker;
                }

                var displayName = (method.MethodInfo.DeclaringType is null) ?
                    method.MethodInfo.Name :
                    method.MethodInfo.DeclaringType.Name + "." + method.MethodInfo.Name;

                routes.Map(method.RoutePattern, requestDelegate).Add(b =>
                {
                    foreach (CustomAttributeData item in method.Metadata)
                    {
                        var attr = item.Constructor.Invoke(item.ConstructorArguments.Select(a => a.Value).ToArray());
                        b.Metadata.Add(attr);
                    }

                    b.Metadata.Add(method);
                });
            }
        }

        private static Expression BindArgument(Expression sourceExpression, ParameterModel parameter, string name)
        {
            var key = name ?? parameter.Name;
            var type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
            var valueArg = Expression.Convert(Expression.MakeIndex(
                sourceExpression,
                sourceExpression.Type.GetProperty("Item"),
                new [] { Expression.Constant(key) }),
                typeof(string));

            var parseMethod = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(methodInfo =>
                {
                    if (methodInfo.Name == "Parse")
                    {
                        var parameters = methodInfo.GetParameters();

                        return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                    }

                    return false;
                });

            var expr = (Expression)valueArg;

            if (parseMethod != null)
            {
                expr = Expression.Call(parseMethod, valueArg);
            }
            else if (parameter.ParameterType != valueArg.Type)
            {
                expr = Expression.Call(ChangeTypeMethodInfo, valueArg, Expression.Constant(type));
            }

            if (expr.Type != parameter.ParameterType)
            {
                expr = Expression.Convert(expr, parameter.ParameterType);
            }

            Expression defaultValue = parameter.DefaultValue != null && parameter.ParameterType.IsAssignableFrom(parameter.DefaultValue.GetType()) ?
                Expression.Constant(parameter.DefaultValue) :
                Expression.Default(parameter.ParameterType);

            return Expression.Condition(
                Expression.Equal(valueArg, Expression.Constant(null)),
                defaultValue,
                expr);
        }

        private static MethodInfo GetMethodInfo<T>(Expression<T> expr)
        {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }

        private static MemberInfo GetMemberInfo<T>(Expression<T> expr)
        {
            var mc = (MemberExpression)expr.Body;
            return mc.Member;
        }

        private static T GetRequiredService<T>(IServiceProvider sp)
            where T : notnull
        {
            return sp.GetRequiredService<T>();
        }

        private static async Task ExecuteTask<T>(Task<T> task, HttpContext httpContext)
        {
            await new BaseResponse(await task).ExecuteAsync(httpContext);
        }

        private static Task ExecuteValueTask<T>(ValueTask<T> task, HttpContext httpContext)
        {
            static async Task ExecuteAwaited(ValueTask<T> task, HttpContext httpContext)
            {
                await new BaseResponse(await task).ExecuteAsync(httpContext);
            }

            if (task.IsCompletedSuccessfully)
            {
                return new BaseResponse(task.GetAwaiter().GetResult()).ExecuteAsync(httpContext);
            }

            return ExecuteAwaited(task, httpContext);
        }

        private static Task ExecuteValueTaskResult<T>(ValueTask<T> task, HttpContext httpContext) where T : IResult
        {
            static async Task ExecuteAwaited(ValueTask<T> task, HttpContext httpContext)
            {
                await (await task).ExecuteAsync(httpContext);
            }

            if (task.IsCompletedSuccessfully)
            {
                return task.GetAwaiter().GetResult().ExecuteAsync(httpContext);
            }

            return ExecuteAwaited(task, httpContext);
        }

        private static async Task ExecuteTaskResult<T>(Task<T> task, HttpContext httpContext) where T : IResult
        {
            await (await task).ExecuteAsync(httpContext);
        }
    }
}