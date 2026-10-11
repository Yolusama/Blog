namespace Blog.Shared.Exceptions;

public class ServiceException(string msg = "服务出现异常", int code = 500) : Exception(msg)
{
    public int Code => code;
}