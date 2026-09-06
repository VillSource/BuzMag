using NanoidDotNet;

namespace Villsource.Modules.HumanResource.Services;

public interface IEmployeeCodeFactory
{
    Task<string> Create();
}

public class EmployeeCodeFactory: IEmployeeCodeFactory
{
    public Task<string> Create()
    {
        return Nanoid.GenerateAsync(alphabet: Nanoid.Alphabets.Digits, size: 10);
    }
}