namespace Blog.EFCoreMigration.Services.Interfaces;

public interface IProcessDispatcher
{
    public Task DispatchProcessAsync(string command,string workingDirectory,bool useShellExecute = false,string? arguments =  null);
    public void DispatchProcess(string command,string workingDirectory,bool useShellExecute = false,string? arguments =  null);
}