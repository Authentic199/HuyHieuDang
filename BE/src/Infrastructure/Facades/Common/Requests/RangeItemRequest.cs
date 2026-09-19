namespace HuyHieuDang.Infrastructure.Facades.Common.Requests
{
    public class RangeItemRequest<T>
    {
        public ICollection<T>? Items { get; set; }
    }
}