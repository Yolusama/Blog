namespace Blog.Shared.Utils;

public static class FileUtil
{
    public static void RemoveFile(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if(!fileInfo.Exists)
            fileInfo.Delete();
    }
}