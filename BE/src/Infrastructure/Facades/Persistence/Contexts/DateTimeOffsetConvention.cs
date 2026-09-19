using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HuyHieuDang.Infrastructure.Facades.Persistence.Contexts
{
    public class DateTimeOffsetConvention : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        public DateTimeOffsetConvention()
            : base(
                d => d.ToUniversalTime(),
                d => d.ToUniversalTime())
        {
        }
    }
}
