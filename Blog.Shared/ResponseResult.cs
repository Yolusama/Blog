namespace Blog.Shared;

public class ResponseResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int Code { get; set; }
    public object Data { get; set; }
    
    public static ResponseResult Fail(string msg = "fail", int code = 500)
    {
        return new ResponseResult { Success = false, Message = msg, Code = code };
    }
    
    public static ResponseResult OK(string msg = "OK", int code = 200, object? data = null)
    {
        return new ResponseResult { Success = true, Message = msg, Code = code, Data = data };
    }
    
    public static ResponseResult<T> Fail<T>(string msg = "fail", int code = 500)
    {
        return new ResponseResult<T> { Success = false, Message = msg, Code = code };
    }
    
    public static ResponseResult<T> OK<T>(string msg = "OK", int code = 200,T? data = default)
    {
        return new ResponseResult<T> { Success = true, Message = msg, Code = code, Data = data };
    }

    public static ResponseResult<T> OK<T>(T data)
    {
        return new ResponseResult<T> { Success = true, Message = "OK", Code = 200, Data = data };
    }
    
    public static ResponseResult<T> OK<T>(string msg,T data)
    {
        return new ResponseResult<T> { Success = true, Message = msg, Code = 200, Data = data };
    }
}

public class ResponseResult<T>  : ResponseResult
{
    public ResponseResult(){}
    public ResponseResult (T data) 
    {
        Data = data;
        Message = "OK";
        Code = 200;
        Success = true;
    }
    public new T Data { get; set; }
}