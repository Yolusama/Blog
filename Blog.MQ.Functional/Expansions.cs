using FreeSql;
using Newtonsoft.Json;

namespace Blog.MQ.Functional;

public static class Expansions
{
       public static string ToJson<T>(this T obj)
       {
           return JsonConvert.SerializeObject(obj);
       }

    /// <summary>
    /// 浅拷贝-拷贝赋值一遍属性
    /// 目标值空值会自动新建一个属性未完成赋值的对象
    /// </summary>
    /// <param name="src">源实体</param>
    /// <param name="dest">目标拷贝实体</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    /// <param name="afterCopy">执行完拷贝时的其他操作</param>
    /// <returns>拷贝后的实体</returns>
    public static T2 ShallowCopy<T1, T2>(this T1 src, T2? dest = default,Action<T1,T2>? afterCopy = null)
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

        if (afterCopy != null)
            afterCopy.Invoke(src, dest);
        return dest;
    }
    
    /// <summary>
    /// 浅拷贝集合
    /// </summary>
    /// <param name="src">源实体集合</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    ///  <param name="afterCopy">执行完拷贝时的其他操作</param>
    /// <returns>拷贝后的实体集合</returns>
    public static IEnumerable<T2> ShallowCopy<T1, T2>(this IEnumerable<T1> src,Action<T1,T2>? afterCopy = null)
    {
        return src.Select(x => x.ShallowCopy(default,afterCopy));
    }

    /// <summary>
    /// 深拷贝-即遇到非基本类型对其属性再次递归浅拷贝知道完成所以基本类型的复制
    /// 目标值空值会自动新建一个属性未完成赋值的对象
    /// </summary>
    /// <param name="src">源实体</param>
    /// <param name="dest">目标拷贝实体</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    ///  <param name="afterCopy">执行完拷贝时的其他操作</param>
    /// <returns>拷贝后的实体</returns>
    public static T2 DeepCopy<T1, T2>(this T1 src, T2? dest = default,Action<T1,T2>? afterCopy = null)
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
        if(afterCopy!=null)
            afterCopy.Invoke(src,dest);
        return dest;
    }
    
    /// <summary>
    /// 深拷贝集合
    /// </summary>
    /// <param name="src">源实体集合</param>
    /// <typeparam name="T1">源类型</typeparam>
    /// <typeparam name="T2">目标类型</typeparam>
    /// <param name="afterCopy">执行完拷贝时的其他操作</param>
    /// <returns>拷贝后的实体集合</returns>

    public static IEnumerable<T2> DeepCopy<T1, T2>(this IEnumerable<T1> src,Action<T1,T2>? afterCopy = null)
    {
        return src.Select(x => x.DeepCopy(default,afterCopy));
    }

    public static int IntValue(this Enum @enum)
    {
        return Convert.ToInt32(@enum);
    }

    public static async Task<bool> TransactionAsync(this IFreeSql orm,Func<IRepositoryUnitOfWork,Task> action,Action<Exception>?
        errorAction = null)
    {
        using var worker = orm.CreateUnitOfWork();
        try
        {
            await action.Invoke(worker);
            worker.Commit();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            worker.Rollback();
            if(errorAction!=null)
                errorAction.Invoke(e);
            return false;
        }
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