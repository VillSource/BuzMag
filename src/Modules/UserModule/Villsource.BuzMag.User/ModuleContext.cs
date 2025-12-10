using Villsource.BuzMag.Sdk;
using Villsource.BuzMag.Sdk.ValueObjects;

namespace Villsource.BuzMag.User;

public class ModuleContext : IModuleContext
{
    public ModuleNameValue Name => new("Users");
    public IEndpointMapper EndpointMapper => new EndpointMapper();
}