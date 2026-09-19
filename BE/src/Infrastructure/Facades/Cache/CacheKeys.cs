namespace HuyHieuDang.Infrastructure.Facades.Cache
{
    public static class CacheKeys
    {
        public static string GetKeyByModel<T>(Guid id) => typeof(T).Name + id;
    }
}
