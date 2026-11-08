using Elsa.Studio.Branding;

namespace App2.Server;

#pragma warning disable CA1515
public class StudioBrandingProvider : DefaultBrandingProvider
#pragma warning restore CA1515
{
    public override string AppNameWithVersion => "BuzMag Workflow Engin";
}