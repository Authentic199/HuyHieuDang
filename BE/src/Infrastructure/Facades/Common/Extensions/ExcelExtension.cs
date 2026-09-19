using MiniExcelLibs;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class ExcelExtension
{
    public static Stream Export<T>(IEnumerable<T> data)
    {
        Stream memoryStream = new MemoryStream();
        memoryStream.SaveAs(data);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public static Stream ExportByTemplate<T>(IEnumerable<T> data, string templateName)
    {
        string templatePath = PathExtension.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Files/ExcelTemplates/{templateName}.xlsx");
        Stream memoryStream = new MemoryStream();
        memoryStream.SaveAsByTemplate(templatePath, data);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
