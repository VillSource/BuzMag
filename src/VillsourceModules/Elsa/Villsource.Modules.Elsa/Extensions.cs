using Elsa.Common.Multitenancy;
using Elsa.Extensions;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Villsource.Modules.Elsa;

public static class Extensions
{
    public static WebApplication UseElsaDebugApi(this WebApplication app)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(app);

        // 1. Mock Login API หลอกหน้าจอ Elsa Studio ว่า Login สำเร็จแล้ว
        app.MapPost("/elsa/api/identity/login", () =>
        {
            // 1. สร้าง Claims เพื่อหลอก UI ว่าเราเป็น Admin และมีสิทธิ์ทุกอย่าง
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "dev-admin"),
                new Claim(JwtRegisteredClaimNames.Name, "Dev Admin"),
                new Claim("permissions", "*") // สำคัญสุด! ปลดล็อคเมนูทุกอันในหน้า UI
            };

            // 2. สร้าง Key ปลอมๆ สำหรับทำ Signature ให้โครงสร้าง JWT สมบูรณ์
            var dummyKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("this-is-a-dummy-secret-key-for-local-dev-only"));
            var creds = new SigningCredentials(dummyKey, SecurityAlgorithms.HmacSha256);

            // 3. ประกอบร่างเป็น JWT Token จริงๆ
            var token = new JwtSecurityToken(
                issuer: "mock-issuer",
                audience: "mock-audience",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 4. ส่งกลับให้หน้า UI นำไป Decode สิทธิ์การใช้งาน
            return Results.Ok(new
            {
                IsAuthenticated = true,
                AccessToken = tokenString,
                RefreshToken = tokenString // ใช้ตัวเดียวกันหลอกไปเลย
            });
        }).AllowAnonymous().ExcludeFromDescription(); // ซ่อนไว้ไม่ให้ไปโผล่ใน Swagger (ถ้ามี)

        // (เผื่อไว้) บางเวอร์ชัน UI อาจจะเรียกดูข้อมูล User หลังจาก Login สำเร็จ
        app.MapGet("/elsa/api/identity/users/me", () =>
        {
            return Results.Ok(new
            {
                Id = "dev-user",
                Name = "Dev Admin",
                Permissions = new List<string>{ "*" } // ให้สิทธิ์ทุกอย่างในหน้า UI
            });
        }).AllowAnonymous().ExcludeFromDescription();

        // 2. Middleware ยัด Tenant จำลอง
        app.Use(async (context, next) =>
        {
            // ดักเฉพาะ Request ที่มาจาก Elsa เพื่อไม่ให้กระทบ API อื่นๆ ของคุณ
            if (context.Request.Path.StartsWithSegments("/elsa/api", StringComparison.InvariantCultureIgnoreCase))
            {
                var tenantAccessor = context.RequestServices.GetRequiredService<ITenantAccessor>();
                // PushContext ของ Elsa v3
                // (ถ้า Tenant ของคุณใช้คลาสอื่น ให้ปรับชื่อ Property ตามที่ใช้จริงครับ)
                tenantAccessor.PushContext(new Tenant { Id = "acme", Name = "acme", TenantId = "acme" });
            }

            await next();
        });

        // 3. ปิด Security ของแพ็กเกจ Elsa API ทั้งหมด
        global::Elsa.EndpointSecurityOptions.DisableSecurity();
        app.UseWorkflowsApi();
#endif
        return app;
    }

    public static WebApplication UseElsaMultiTenantDatabases(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Use(async (context, next) =>
        {
            var tenantAccessor = context.RequestServices.GetRequiredService<ITenantAccessor>();
            var multiTenantContextAccessor =
                context.RequestServices.GetService<IMultiTenantContextAccessor<AppTenantInfo>>();

            var tenantInfo = multiTenantContextAccessor?.MultiTenantContext?.TenantInfo;

            if (tenantInfo is not null && !string.IsNullOrEmpty(tenantInfo.Id))
            {
                tenantAccessor.PushContext(new Tenant
                {
                    Id = tenantInfo.Id,
                    Name = tenantInfo.Name ?? tenantInfo.Id,
                    TenantId = tenantInfo.Identifier ?? tenantInfo.Id,
                });
            }

            await next();
        });

        return app;
    }
}