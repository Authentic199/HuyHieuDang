namespace HuyHieuDang.Core.Common.Interfaces
{
    public interface IService
    {
        public delegate IService ServiceResolver(string key);
    }
}