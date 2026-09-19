using HuyHieuDang.Core.Bases;
using HuyHieuDang.Core.Common.Interfaces;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class ConfigurationExtension
{
    public static EntityTypeBuilder<T> UnderscoreTable<T>(this EntityTypeBuilder<T> builder)
        where T : class
        => builder.ToTable(typeof(T).Name.Underscore());

    public static EntityTypeBuilder<T> HasCode<T>(this EntityTypeBuilder<T> builder)
        where T : class, ICode
    {
        builder.Property(x => x.Code).IsRequired();
        builder.HasCitextUnique(x => x.Code);
        return builder;
    }

    public static EntityTypeBuilder<T> HasBaseEntity<T>(this EntityTypeBuilder<T> builder)
        where T : BaseEntity
    {
        builder.HasKey(x => x.Id);
        return builder;
    }

    /// <summary>
    /// Creates a unique index on a property of an entity type. This is used in PostgresSQl and requires citext PostgresExtension
    /// </summary>
    /// <typeparam name="T"> the type T must be a class. This is because indexes can only be created on properties of classes.</typeparam>
    /// <param name="builder">An EntityTypeBuilder object that represents the entity type being created</param>
    /// <param name="indexExpression">An expression that represents the property on which the index will be created.</param>
    /// <param name="filter">An optional string that represents the filter condition for the index.</param>
    /// <returns>statement returns the builder object.</returns>
    public static EntityTypeBuilder<T> HasCitextUnique<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, object?>> indexExpression, string? filter = null)
        where T : class
    {
        builder.Property(indexExpression).HasColumnType("citext");
        var indexBuild = builder.HasIndex(indexExpression).IsUnique();

        if (filter != null)
        {
            indexBuild.HasFilter(filter);
        }

        return builder;
    }
}