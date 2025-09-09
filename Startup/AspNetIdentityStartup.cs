using AspNetIdentity.Data;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AspNetIdentity;

public static class AspNetIdentityStartup
{

    public static AuthenticationSettings AddAuthenticationConfiguration(this IServiceCollection services, IConfigurationManager configuration)
    {
        var section = configuration.GetSection("Authentication");
        services.Configure<AuthenticationSettings>(section);
        var auth = section.Get<AuthenticationSettings>();

        if (auth is null || auth.Microsoft is null)
            throw new ApplicationException("missing auth microsoft");
        
        if (string.IsNullOrWhiteSpace(auth.Microsoft.ClientId))
            throw new ApplicationException("missing microsoft clientId");

        return auth;
    }
    public static void ConfigureAspNetIdentity(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        // Db EF Core
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        // Identity
        services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        // no login without razorPages so put it here
        services.AddRazorPages();

        var authConfig = services.AddAuthenticationConfiguration(builder.Configuration);


        // add local auth + microsoft SSO
        var authBuilder = services.AddAuthentication();

        authBuilder.AddOpenIdConnect("Microsoft", "Microsoft", options =>
        {
            options.Authority = "https://login.microsoftonline.com/common/v2.0/";
            options.ClientId = authConfig.Microsoft.ClientId;
            options.ResponseType = OpenIdConnectResponseType.IdToken;
            options.CallbackPath = "/signin-oidc";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "name",
                RoleClaimType = "role",
                ValidateIssuer = false
            };
        });
    }
}