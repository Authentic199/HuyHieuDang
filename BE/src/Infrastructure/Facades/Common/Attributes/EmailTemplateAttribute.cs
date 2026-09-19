namespace HuyHieuDang.Infrastructure.Facades.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EmailTemplateAttribute : Attribute
    {
        public EmailTemplateAttribute(string templateName)
        {
            TemplateName = templateName;
        }

        public string TemplateName { get; set; }
    }
}