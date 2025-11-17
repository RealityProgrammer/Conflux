using Microsoft.EntityFrameworkCore;
using Conflux.Components;
using Conflux.Database;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Conflux.Components.Account;
using Conflux.Components.Services;
using Conflux.Components.Services.Abstracts;
using Conflux.Database.Entities;
using Markdig;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Adding authentication services.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddIdentityCookies(cookieBuilder => {
    cookieBuilder.ApplicationCookie!.Configure(options => {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/denied";
    });
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

// Add database services.
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// More authentication services.
builder.Services.AddIdentityCore<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddSignInManager().AddDefaultTokenProviders();

// TODO: Email sending services.
// builder.Services.AddSingleton<IEmailSender<ApplicationUser>, ...>();

// System services.
builder.Services.AddScoped<ApplicationRedirectManager>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddViteServices();
builder.Services.AddScoped<MarkdownPipeline>(services => {
    var pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    return pipeline;
});
builder.Services.AddScoped<ProfileSanitizingService>();

// Controllers.
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Nuget Packages
// ...

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
    app.MapOpenApi();
} else {
    app.UseViteDevelopmentServer(true);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/logout", async (ClaimsPrincipal claims, [FromServices] SignInManager<ApplicationUser> signInManager, [FromForm(Name = "ReturnUrl")] string returnUrl) => {
    await signInManager.SignOutAsync();
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.Run();