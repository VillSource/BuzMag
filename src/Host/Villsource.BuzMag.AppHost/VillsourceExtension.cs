// ReSharper disable once CheckNamespace
namespace Aspire.Hosting;

internal static class VillsourceExtension
{
    public static IDistributedApplicationBuilder ReplaceDashboardWithGrafana(this IDistributedApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var lgtm = builder.AddContainer("otel-lgtm", "grafana/otel-lgtm")
            .WithHttpEndpoint(port: 3333, targetPort: 3000, name: "grafana") // Grafana UI
            .WithHttpEndpoint(port: 4427, targetPort: 4317, name: "otlp-grpc") // OTLP gRPC Receiver
            .WithHttpEndpoint(port: 4427, targetPort: 4318, name: "otlp-http") // OTLP HTTP Receiver
            // Anonymous Access
            .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
            .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Admin") // ให้สิทธิ์เป็น Admin (หรือ Viewer/Editor)
            .WithEnvironment("GF_AUTH_DISABLE_LOGIN_FORM", "true") // ซ่อนหน้าฟอร์ม Login
            .WithEnvironment("GF_SECURITY_ALLOW_EMBEDDING", "true"); // (Optional) เผื่อใช้ iframe ฝังในเว็บอื่น
        
        builder.Eventing.Subscribe<BeforeResourceStartedEvent>((e, ct) =>
        {
            // เช็คว่า Resource ที่กำลังจะรันเป็น Project (.NET App) หรือไม่
            if (e.Resource is ProjectResource project)
            {
                // กำหนด Endpoint ชี้ไปที่ OTLP gRPC ของ lgtm container
                project.Annotations.Add(new EnvironmentCallbackAnnotation("OTEL_EXPORTER_OTLP_ENDPOINT",
                    () => lgtm.GetEndpoint("otlp-grpc").Url));
            }

            return Task.CompletedTask;
        });
        
        return builder;
    }
}