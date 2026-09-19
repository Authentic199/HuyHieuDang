using System.Reflection;

namespace HuyHieuDang.Core.Helpers.PropertyFlatten
{
    public record PropertyFlatten(string Path, object? Value, PropertyInfo PropertyInfo, int Depth, int? Index);
}