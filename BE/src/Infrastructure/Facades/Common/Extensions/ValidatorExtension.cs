using AutoMapper.Internal;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class ValidatorExtension
{
    public static IRuleBuilderOptions<T, IFormFile?> IsValidContentType<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, ICollection<string> contentTypes)
    {
        return ruleBuilder.Must(file => file is null || contentTypes.Any(x => file.ContentType.StartsWith(x)));
    }

    public static IRuleBuilderOptions<T, IFormFile?> IsValidContentType<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, params string[] contentTypes)
    {
        return ruleBuilder.Must(file => file is null || Array.Exists(contentTypes, x => file.ContentType.StartsWith(x)));
    }

    public static IRuleBuilderOptions<T, ICollection<TProperty>?> NotDuplicate<T, TProperty>(this IRuleBuilder<T, ICollection<TProperty>?> ruleBuilder)
    {
        return ruleBuilder.Must(collections => collections != null && collections.Distinct().Count() == collections.Count);
    }

    public static IRuleBuilderOptions<T, ICollection<TProperty>?> NotDuplicateBy<T, TProperty, TBy>(this IRuleBuilder<T, ICollection<TProperty>?> ruleBuilder, Expression<Func<TProperty, TBy>> propertyLambda)
        where TProperty : class
    {
        PropertyInfo propertyInfo = propertyLambda.GetPropertyFromExpression();
        return ruleBuilder.Must(collections =>
        {
            if (collections == null)
            {
                return false;
            }

            IEnumerable<object?> byValues = collections.Select(element => propertyInfo.GetValue(element));
            return byValues.Distinct().Count() == collections.Count;
        });
    }

    public static IRuleBuilderOptions<T, ICollection<TProperty>?> GreaterOrEqualTo<T, TProperty>(this IRuleBuilder<T, ICollection<TProperty>?> ruleBuilder, int value)
    {
        return ruleBuilder.Must(collections => collections == null || collections.Count >= value);
    }

    public static IRuleBuilderOptions<T, ICollection<TProperty>?> LessThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, ICollection<TProperty>?> ruleBuilder, int value)
    {
        return ruleBuilder.Must(collections => collections == null || collections.Count <= value);
    }

    public static IRuleBuilderOptions<T, string?> IsValidPhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        /*
            - Numbers can start with +84, +840 or 84 840 (eg +84981234567, +840981234567, 84981234567, 840981234567)
            - The prefixes 03, 05, 07, 08, 09 (eg 0981234567)
        */
        return ruleBuilder.Matches(RegexExtension.VnPhoneNumber);
    }

    public static IRuleBuilderOptions<T, string?> IsValidPassword<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Matches(RegexExtension.NistPassword);
    }

    /// <summary>
    /// not only special characters
    /// </summary>
    /// <typeparam name="T"> request </typeparam>
    public static IRuleBuilderOptions<T, string?> NotSpecialCharacter<T>(this IRuleBuilder<T, string?> ruleBuilder, string? acceptCharacter = null)
    {
        string pattern = $"^[A-z0-9{acceptCharacter}]*$";
        return ruleBuilder.Matches(pattern);
    }

    public static IRuleBuilderOptions<T, string?> NotWhiteSpace<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.Matches(@"^[\S]*$");
    }
}

public static class ValidationContextExtension
{
    public static IEnumerable<ValidationResult> Required(this ValidationContext validationContext, params string[] ignoreProperties)
    {
        foreach (PropertyInfo propertyInfo in validationContext.ObjectType.GetProperties())
        {
            if (ignoreProperties.Contains(propertyInfo.Name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            Type propertyType = propertyInfo.PropertyType;
            object? propValue = propertyInfo.GetValue(validationContext.ObjectInstance);
            object? defaultVal;
            string message = $"{propertyInfo.Name} of {validationContext.ObjectType.FullName} is required";
            if (propertyType == typeof(string))
            {
                if (string.IsNullOrEmpty(propValue?.ToString()))
                {
                    yield return new ValidationResult(
                    message,
                    new[] { propertyInfo.Name });
                }
            }
            else
            {
                defaultVal = propertyType.IsNullableType() ? null : Activator.CreateInstance(propertyType);

                if (propValue?.Equals(defaultVal) == true)
                {
                    yield return new ValidationResult(
                        message,
                        new[] { propertyInfo.Name });
                }
            }
        }
    }
}