namespace HuyHieuDang.Core.Common.Exceptions;

public class ErrorResultWrapper
{
    public ErrorResultWrapper()
    {
    }

    public ErrorResultWrapper(string? message)
    {
        Message = message;
    }

    public string? TraceId { get; set; }

    public string? Exception { get; set; }

    public string? Source { get; set; }

    public string? Method { get; set; }

    public int Line { get; set; }

    public string? Message { get; set; }

    public string? SupportMessage { get; set; }

    public int StatusCode { get; set; }
}

public class SuccessResultWrapper<TData>
{
    public SuccessResultWrapper(TData? data, string? message)
    {
        Data = data;
        Message = message;
    }

    public string? Message { get; set; }

    public TData? Data { get; set; }
}
