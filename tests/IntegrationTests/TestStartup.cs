using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class TestStartup
{
    // public void ConfigureServices(IServiceCollection services)
    // {
    //     services.AddControllers();
    //     services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestBearerHandler>("Test", _ => { });
    //     services.AddAuthorization();
    // }
    //
    // public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    // {
    //     app.UseRouting();
    //     app.UseAuthentication();
    //     app.UseAuthorization();
    //     app.UseEndpoints(endpoints => endpoints.MapControllers());
    // }
}