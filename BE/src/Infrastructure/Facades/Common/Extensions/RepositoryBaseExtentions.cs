using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions
{
    public static class RepositoryBaseExtentions
    {
        public static bool IsExistByUnique<T, TProperty>(this IRepositoryBase<T> repositorybase, Expression<Func<T, TProperty>> propertySelector, object uniqueValue, Guid? exceptId = null)
            where T : BaseEntity
        {
            PropertyInfo info = propertySelector.GetPropertyFromExpression();
            Type propertyType = info.PropertyType;
            if (
                    propertyType == typeof(string)
                    || propertyType.IsValueType
                    || propertyType.IsEnum
            )
            {
                StringBuilder stringBuilder = new(info.Name);
                if (uniqueValue is string stringValue)
                {
                    stringBuilder.Append(".ToLower()");
                    uniqueValue = stringValue.ToLower();
                }

                stringBuilder
                    .Append(" == @0")
                    .Append(" and ")
                    .Append(nameof(BaseEntity.Id))
                    .Append(" != @1");

                string expression = stringBuilder.ToString();

                return repositorybase.Find().Where(expression, uniqueValue, exceptId).FirstOrDefault() != default;
            }
            else
            {
                throw new InvalidOperationException($"{info.Name} is value type, string or enum");
            }
        }
    }
}
