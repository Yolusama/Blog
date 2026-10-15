namespace Blog.EFCoreMigration.Services.Interfaces;

public interface IProcessDispatcher
{
    public Task DispatchProcess(string command,bool useShellExecute = false,string? arguments =  null);
}