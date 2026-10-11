using System.Text.Json;

namespace Blog.Shared.Functional;

public static class Expansions
{
    public static string ToJson(this object obj,JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// 浅拷贝-拷贝赋值一遍属性
    /// 目标值空值会自动新建一个属性未完成赋值的对象
    /// </summary>
    /// <param name="src">源实体</param>
    /// <param name="dest">目标拷贝实体</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    /// <returns>拷贝后的实体</returns>
    public static T2 ShallowCopy<T1, T2>(this T1 src, T2? dest = default)
    {
        if(dest == null)
            dest = Activator.CreateInstance<T2>();
        var type = dest.GetType();
        var srcProperties = typeof(T1).GetProperties();
        foreach (var property in type.GetProperties().Where(p=>p.CanWrite))
        {
           var srcProperty = srcProperties
               .FirstOrDefault(x => x.Name == property.Name);
           if(srcProperty == null) continue;
           if(srcProperty.PropertyType.IsAssignableTo(property.PropertyType))
           {
               var value = srcProperty.GetValue(src);
               property.SetValue(dest, value);
           }
        }
        return dest;
    }
    
    /// <summary>
    /// 浅拷贝集合
    /// </summary>
    /// <param name="src">源实体集合</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    /// <returns>拷贝后的实体集合</returns>
    public static IEnumerable<T2> ShallowCopy<T1, T2>(this IEnumerable<T1> src)
    {
        return src.Select(x => x.ShallowCopy<T1,T2>());
    }

    /// <summary>
    /// 深拷贝-即遇到非基本类型对其属性再次递归浅拷贝知道完成所以基本类型的复制
    /// 目标值空值会自动新建一个属性未完成赋值的对象
    /// </summary>
    /// <param name="src">源实体</param>
    /// <param name="dest">目标拷贝实体</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    /// <returns>拷贝后的实体</returns>
    public static T2 DeepCopy<T1, T2>(this T1 src, T2? dest = default)
    {
        if(dest == null)
            dest = Activator.CreateInstance<T2>();
        var type = dest.GetType();
        var srcProperties = type.GetProperties();
        foreach (var property in type.GetProperties().Where(p=>p.CanWrite))
        {
            var srcProperty = srcProperties
                .FirstOrDefault(x => x.Name == property.Name);
            if(srcProperty == null) continue;
            if(srcProperty.PropertyType.IsAssignableTo(property.PropertyType))
            { 
                var value = srcProperty.GetValue(src);
                if(IsBaseType(property.PropertyType))
                    property.SetValue(dest, value);
                else
                {
                    var instance = value.DeepCopy(Activator.CreateInstance(property.PropertyType));
                    property.SetValue(dest, instance);
                }
            }
        }
        return dest;
    }
    
    /// <summary>
    /// 深拷贝集合
    /// </summary>
    /// <param name="src">源实体集合</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    /// <returns>拷贝后的实体集合</returns>
    public static IEnumerable<T2> DeepCopy<T1, T2>(this IEnumerable<T1> src)
    {
        return src.Select(x => x.DeepCopy<T1,T2>());
    }

    public static void SafeDispose(this IDisposable disposable)
    {
        try
        {
            disposable.Dispose();
        }
        catch { return; }
    }

    public static BasePageQueryResult<T> ToPagedResult<T>(this IEnumerable<T> result, int page, int pageSize,long total)
    {
         return new BasePageQueryResult<T>()
         {
             Data = result.ToList(),
             Page = page,
             PageSize = pageSize,
             Total = total
         };
    }
    
    private static bool IsBaseType(Type type)
    {
        return type.IsPrimitive || type.IsEnum 
                                || type == typeof(string)
                                || type == typeof(decimal)
                                || type == typeof(DateTime)
                                || type == typeof(DateTimeOffset)
                                || type == typeof(DateOnly)
                                || type == typeof(TimeOnly);
    }
}