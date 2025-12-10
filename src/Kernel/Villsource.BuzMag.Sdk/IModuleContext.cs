using Villsource.BuzMag.Sdk.ValueObjects;

namespace Villsource.BuzMag.Sdk;

public interface IModuleContext
{
    public ModuleNameValue Name { get; }
    public IEndpointMapper? EndpointMapper { get; }
}