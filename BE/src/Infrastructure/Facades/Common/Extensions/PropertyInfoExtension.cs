using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class PropertyInfoExtension
{
    public static PropertyInfo? GetPropertyRecursive(this Type baseType, string propertyNames)
    {
        string[] parts = propertyNames.Split('.');

        PropertyInfo? propertyInfo = baseType.GetProperty(parts[0], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (propertyInfo is null)
        {
            return null;
        }

        return parts.Length > 1
            ? propertyInfo.PropertyType.GetPropertyRecursive(parts.Skip(1).Aggregate((a, i) => a + "." + i))
            : baseType.GetProperty(propertyNames, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    }

    public static PropertyInfo GetPropertyFromExpression<T, TProperty>(this Expression<Func<T, TProperty>> propertyLambdaExpr)
    {
        if (propertyLambdaExpr.Body is not MemberExpression memberExpr)
        {
            throw new ArgumentException($"Expression '{propertyLambdaExpr}' refers to a method, not a value.");
        }

        if (memberExpr.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException($"Expression '{propertyLambdaExpr}' refers to a field, not a value.");
        }

        if (
            typeof(T) != propertyInfo.ReflectedType
            && (propertyInfo.ReflectedType == null || !typeof(T).IsSubclassOf(propertyInfo.ReflectedType))
            && !typeof(T).GetInterfaces().Contains(propertyInfo.ReflectedType)
        )
        {
            throw new ArgumentException($"Expression '{propertyLambdaExpr}' refers to a property that is not from type '{typeof(T)}'.");
        }

        return propertyInfo;
    }

    public static List<string> GetPropertyRecursiveWithMaxDeep(this Type baseType, uint level, params Type[] typeIgnores)
    {
        if (Array.Exists(typeIgnores, x => !x.IsSubclassOf(typeof(Attribute))))
        {
            throw new ArgumentException("Type ignores has an element that refers to a object, not a attribute");
        }

        return DumpObjectTree(baseType.GetProperties(), level, new List<string>(), string.Empty, new List<string>(), typeIgnores.Append(typeof(NotMappedAttribute)));

        List<string> DumpObjectTree(PropertyInfo[] propertyInfoes, uint level, List<string> result, string path, List<string>? objects, IEnumerable<Type> typeIgnores)
        {
            foreach (PropertyInfo propertyInfo in propertyInfoes)
            {
                if (Array.Exists(propertyInfo.CustomAttributes.ToArray(), x => typeIgnores.Contains(x.AttributeType)))
                {
                    continue;
                }

                if (level != 0 && !propertyInfo.PropertyType.FullName!.StartsWith("System") && !objects!.Exists(x => x == propertyInfo.Name))
                {
                    objects!.Add(path.Split('.')[^1]);
                    DumpObjectTree(propertyInfo.PropertyType.GetProperties(), level - 1, result, !string.IsNullOrEmpty(path) ? string.Format("{0}.{1}", path, propertyInfo.Name) : propertyInfo.Name, objects, typeIgnores);
                }

                if (propertyInfo.PropertyType == typeof(string))
                {
                    result.Add(!string.IsNullOrEmpty(path) ? string.Format("{0}.{1}", path, propertyInfo.Name) : propertyInfo.Name);
                }
            }

            return result;
        }
    }
}