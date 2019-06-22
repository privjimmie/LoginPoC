using System.Linq;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Amazon;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace LoginPoC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {


            //Inject cognito for using on GetToken method
            var cognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(RegionEndpoint.EUWest1); //Move region to appConfig
            var cognitoUserPool = new CognitoUserPool(AppConfig.UserPoolId, AppConfig.ClientId, cognitoIdentityProvider, AppConfig.ClientSecret);
            services.AddSingleton<IAmazonCognitoIdentityProvider>(cognitoIdentityProvider);
            services.AddSingleton<CognitoUserPool>(cognitoUserPool);


            // Add authentication before adding MVC
            ConfigureAuthentication(services);



            //Enable Authentication by default. Exclude by using [AllowAnonymous] attribute
            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
                config.RespectBrowserAcceptHeader = true;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2); ;

        }


        private void ConfigureAuthentication(IServiceCollection services)
        {
            //Solution to use multiple auth-methods and decide on runtime found at https://github.com/aspnet/Security/issues/1469
            //Thank you https://github.com/openidauthority


            // Add JWT or OpenIdConnect and cookies.
            // Use a dynamic policy scheme to choose the correct authentication scheme at runtime
            services.AddAuthentication(sharedOptions =>
                {

                    sharedOptions.DefaultAuthenticateScheme = "JwtOrCookie";
                    sharedOptions.DefaultChallengeScheme = "JwtOrOpenId";
                    sharedOptions.DefaultSignInScheme = "NullOrCookie";
                    sharedOptions.DefaultSignOutScheme = "NullOrCookie";

                })
                .AddPolicyScheme("JwtOrCookie", "Authorization Bearer or OIDC", options =>
                {
                    options.ForwardDefaultSelector = context => IsJwt(context) ? JwtBearerDefaults.AuthenticationScheme : CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddPolicyScheme("JwtOrOpenId", "Authorization Bearer or OIDC", options =>
                {
                    options.ForwardDefaultSelector = context => IsJwt(context) ? JwtBearerDefaults.AuthenticationScheme : OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddPolicyScheme("NullOrCookie", "Authorization Bearer or OIDC", options =>
                {
                    options.ForwardDefaultSelector = context => IsJwt(context) ? null : CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Audience = AppConfig.ClientId;
                    options.Authority = AppConfig.Authority;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.ResponseType = AppConfig.ResponseType;
                    options.MetadataAddress = AppConfig.MetadataAddress;
                    options.ClientId = AppConfig.ClientId;
                    options.ClientSecret = AppConfig.ClientSecret;
                });
        }

        private static bool IsJwt(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            return authHeader?.ToLower().StartsWith("bearer ") == true;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
            app.UseMvcWithDefaultRoute();
        }
    }
}
