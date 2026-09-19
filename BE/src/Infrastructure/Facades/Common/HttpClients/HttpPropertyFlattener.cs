using HuyHieuDang.Core.Helpers.PropertyFlatten;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using System.Reflection;

namespace HuyHieuDang.Infrastructure.Facades.Common.HttpClients
{
    public class HttpPropertyFlattener : PropertyFlattener
    {
        public HttpPropertyFlattener(PropertyFlattenOptions? options = null)
            : base(options)
        {
        }

        public override string GetName(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<FormNameAttribute>()?.Name ?? base.GetName(propertyInfo);
        }
    }
}