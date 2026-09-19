using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Definitions;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class ValidatorMessageExtention
{
    public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> ruleBuilder, MessagesType messagesType)
    {
        string propertyName = GetPropertyName(ruleBuilder);
        return ruleBuilder.WithMessage(Messages<T>.Action(messagesType, propertyName));
    }

    private static string GetPropertyName<T, TProperty>(IRuleBuilderOptions<T, TProperty> ruleBuilder)
    {
        return DefaultValidatorOptions.Configurable(ruleBuilder).PropertyName;
    }
}