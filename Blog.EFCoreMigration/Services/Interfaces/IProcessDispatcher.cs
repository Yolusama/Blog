namespace Blog.EFCoreMigration.Services.Interfaces;

public interface IProcessDispatcher
{
    public Task DispatchProcess(string command,string? arguments =  null);
}