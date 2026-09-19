using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace HuyHieuDang.Infrastructure.Facades.OpenAPI;

internal class EnumTypesSchemaFilter : ISchemaFilter
{
    private readonly XDocument? xmlComments;

    public EnumTypesSchemaFilter(string xmlPath)
    {
        if (File.Exists(xmlPath))
        {
            xmlComments = XDocument.Load(xmlPath);
        }
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (xmlComments != null && schema.Enum?.Count > 0 && context.Type?.IsEnum == true)
        {
            StringBuilder schemaDescription = new("<p>Members:</p><ul>");

            foreach (object enumMemberValue in Enum.GetValues(context.Type))
            {
                string fullEnumMemberName = $"F:{context.Type.FullName}.{enumMemberValue}";

                string? enumMemberComment = xmlComments.XPathEvaluate($"normalize-space(//member[@name = '{fullEnumMemberName}']/summary/text())") as string;

                schemaDescription.Append("<li><i>")
                    .Append(Convert.ChangeType(enumMemberValue, Enum.GetUnderlyingType(context.Type)).ToString())
                    .Append(" - ").Append(enumMemberValue).Append("</i>: ")
                    .Append(enumMemberComment?.Trim())
                    .Append("</li> ");
            }

            schema.Description = schemaDescription.Append("</ul>").ToString();
        }
    }
}
