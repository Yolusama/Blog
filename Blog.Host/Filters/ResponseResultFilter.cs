using System.Text;
using Blog.Shared;
using Blog.Shared.Functional;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Services.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

public class ResponseResultFilter(IMemoryCache cache) : IAsyncResultFilter
{
    const string key = nameof(ResponseResult) +"_" + "ValueTypes";
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // 1. 先判断 Action 声明的返回类型
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var methodReturnType = actionDescriptor?.MethodInfo.ReturnType;

        // 解包 Task<T> / ValueTask<T>
        var unwrapped = UnwrapTask(methodReturnType);

        // 2. 无返回值：void / Task / ValueTask → 非泛型 Result
        if (unwrapped == typeof(void) || unwrapped == typeof(Task) || unwrapped == typeof(ValueTask))
        {
            await next.Invoke(); // 让空响应正常走完

            var json = ResponseResult.OK()
                .ToJson(Constants.SmallCamelcase);
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(json));
            return;
        }

        // 3. 有返回值：string / Task<string> 等
        //    MVC 已把它包成 ObjectResult，Value 就是实际值
        if (context.Result is ObjectResult objectResult)
        {
            var value = objectResult.Value;

            var valueTypes = (List<Type>?)cache.Get(key) ?? [];
            // 泛型参数优先用声明类型，null 值时也能正确包装
            var valueType = unwrapped ?? value?.GetType() ?? typeof(object);
            Type wrapperType = null;
            var cachedType = valueTypes.FirstOrDefault(t => t.GenericTypeArguments.First() == valueType);
            if(cachedType == null)
            {
                wrapperType = typeof(ResponseResult<>).MakeGenericType(valueType);
                valueTypes.Add(wrapperType);
                cache.Set(key, valueTypes);
            }
            else 
                wrapperType = cachedType;
            var wrapped = Activator.CreateInstance(wrapperType, value);

            objectResult.Value = wrapped;
            objectResult.DeclaredType = wrapperType;
        }

        await next.Invoke();
    }

    private static Type? UnwrapTask(Type? type)
    {
        if (type is null) return null;

        if (type == typeof(Task) || type == typeof(ValueTask))
            return typeof(void);

        if (type.IsGenericType)
        {
            var def = type.GetGenericTypeDefinition();
            if (def == typeof(Task<>) || def == typeof(ValueTask<>))
                return type.GetGenericArguments()[0];
        }

        return type;
    }
}