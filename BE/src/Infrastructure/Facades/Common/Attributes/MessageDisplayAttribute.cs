namespace HuyHieuDang.Infrastructure.Facades.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageDisplayAttribute : Attribute
    {
        public MessageDisplayAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}