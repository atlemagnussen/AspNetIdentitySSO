using System.Security.Claims;
using AspNetIdentity.Data;
using AspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
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
        authBuilder.AddGoogle(options =>
        {
            options.ClientId = authConfig.Google.ClientId;
            options.ClientSecret = authConfig.Google.ClientSecret ?? throw new ApplicationException("missing google secret");
            options.CallbackPath = new PathString("/signin-oidc");
        });
        // authBuilder.AddOpenIdConnect("Google", "Google", options =>
        // {
        //     options.Authority = "https://accounts.google.com/o/oauth2/v2/auth";
        //     options.ClientId = authConfig.Google.ClientId;
        //     options.ClientSecret = authConfig.Google.ClientSecret;
        //     options.ResponseType = OpenIdConnectResponseType.Code;
        //     options.CallbackPath = new PathString("/signin-oidc");

        //     //options.TokenEndpoint = "https://oauth2.googleapis.com/token";
        //     //options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        //     options.UsePkce = true;
        //     options.Scope.Add("openid");
        //     options.Scope.Add("profile");
        //     options.Scope.Add("email");

        //     options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id"); // v2
        //     options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub"); // v3
        //     options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        //     options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
        //     options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
        //     options.ClaimActions.MapJsonKey("urn:google:profile", "link");
        //     options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        // });
    }
}