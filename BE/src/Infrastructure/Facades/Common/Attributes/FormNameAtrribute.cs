namespace HuyHieuDang.Infrastructure.Facades.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FormNameAttribute : Attribute
    {
        public FormNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}