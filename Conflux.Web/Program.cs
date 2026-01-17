using Conflux.Application.Abstracts;
using Conflux.Application.Implementations;
using Microsoft.EntityFrameworkCore;
using Conflux.Components;
using Conflux.Domain;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Conflux.Domain.Entities;
using Conflux.Domain.Events;
using Conflux.Services;
using Conflux.Services.Abstracts;
using Conflux.Services.Authorization;
using Conflux.Services.Hubs;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

if (builder.Environment.EnvironmentName != "Production") {
    builder.Configuration
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", false, true);
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Adding authentication services.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ApplicationAuthenticationStateProvider>();

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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAuthenticatedUser", policy => {
        policy.RequireAuthenticatedUser();
    })
    .AddPolicy("CreateCommunityChannelCategory", policy => {
        policy.Requirements.Add(new CreateCommunityChannelCategoryRequirement());
    })
    .AddPolicy("CreateCommunityChannel", policy => {
        policy.Requirements.Add(new CreateCommunityChannelRequirement());
    })
    .AddPolicy("CreateCommunityRole", policy => {
        policy.Requirements.Add(new CreateCommunityRoleRequirement());
    })
    .AddPolicy("DeleteCommunityRole", policy => {
        policy.Requirements.Add(new DeleteCommunityRoleRequirement());
    })
    .AddPolicy("RenameCommunityRole", policy => {
        policy.Requirements.Add(new RenameCommunityRoleRequirement());
    })
    .AddPolicy("AccessCommunityControlPanel", policy => {
        policy.Requirements.Add(new AccessCommunityControlPanelRequirement());
    })
    .AddPolicy("UpdateCommunityRolePermissions", policy => {
        policy.Requirements.Add(new UpdateCommunityRolePermissionsRequirement());
    })
    .AddPolicy("UpdateCommunityMemberRole", policy => {
        policy.Requirements.Add(new UpdateCommunityMemberRoleRequirement());
    });

builder.Services.AddSingleton<IAuthorizationHandler, CreateCommunityChannelCategoryAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CreateCommunityChannelAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CreateCommunityRoleAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DeleteCommunityRoleAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, RenameCommunityRoleAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, UpdateCommunityRolePermissionsAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AccessCommunityControlPanelAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, UpdateCommunityMemberRoleAuthorizationHandler>();

// Add database services.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), options => {
        options.MigrationsAssembly("Conflux.Infrastructure");
    });
});

// More authentication services.
builder.Services
    .AddIdentityCore<ApplicationUser>(options => {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters += ' ';
    })
    .AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<RoleManager<IdentityRole>>();

// TODO: Email sending services.
// builder.Services.AddSingleton<IEmailSender<ApplicationUser>, ...>();

// System services.
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ApplicationRedirectManager>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IFriendshipEventDispatcher, FriendshipEventDispatcher>();
builder.Services.AddViteServices();
builder.Services.AddSingleton<MarkdownPipeline>(services => {
    var pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    return pipeline;
});
builder.Services.AddScoped<ProfileSanitizingService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IConversationEventDispatcher, ConversationEventDispatcher>();
builder.Services.AddScoped<ISnackbarService, SnackbarService>();
builder.Services.AddScoped<ModalService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<ICommunityRoleService, CommunityRoleService>();
builder.Services.AddScoped<ICommunityEventDispatcher, CommunityEventDispatcher>();
builder.Services.AddScoped<IReportService, ReportService>();

// SignalR related services.
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = !builder.Environment.IsProduction();
});
builder.Services.AddResponseCompression(option => {
    option.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([ "application/octet-stream" ]);
});

// Controllers.
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();

    if (context.Database.GetPendingMigrations().Any()) {
        context.Database.Migrate();
    }
    
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await CreateRoles(roleManager);

    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    if (environment.IsDevelopment()) {
        await CreateFakeUsers(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }
}

app.UseResponseCompression();

app.MapControllers();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
    app.MapOpenApi();
} else {
    if (bool.Parse(builder.Configuration["Vite:Server:Enabled"] ?? string.Empty)) {
        app.UseViteDevelopmentServer(true);
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

string uploadDirectory = Path.Combine(builder.Environment.ContentRootPath, "Uploads");

if (!Directory.Exists(uploadDirectory)) {
    Directory.CreateDirectory(uploadDirectory);
}

PhysicalFileProvider uploadsFileProvider = new PhysicalFileProvider(uploadDirectory);

app.Environment.WebRootFileProvider = new CompositeFileProvider(app.Environment.WebRootFileProvider, uploadsFileProvider);

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = uploadsFileProvider,
    RequestPath = "/uploads",
    ContentTypeProvider = new ApplicationContentTypeProvider(),
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/confirm-profile-setup", async (ClaimsPrincipal claims, [FromServices] IUserService userService, [FromForm(Name = "ReturnUrl")] string returnUrl) => {
    await userService.UpdateProfileSetup(claims, true);
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.MapPost("/auth/logout", async (ClaimsPrincipal claims, [FromServices] SignInManager<ApplicationUser> signInManager, [FromForm(Name = "ReturnUrl")] string returnUrl) => {
    await signInManager.SignOutAsync();
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.MapHub<FriendshipHub>("/hub/notification");
app.MapHub<ConversationHub>("/hub/conversation");
app.MapHub<CommunityHub>("/hub/community");

// Add Roles, and assign to users.
app.Run();
return;

async Task CreateRoles(RoleManager<IdentityRole> roleManager) {
    string[] roles = [
        "Moderator",
        "Admin",
        "SystemDeveloper",
    ];

    foreach (var role in roles) {
        if (!await roleManager.RoleExistsAsync(role)) {
            await roleManager.CreateAsync(new(role));
        }
    }
}

async Task CreateFakeUsers(UserManager<ApplicationUser> userManager) {
    for (int i = 0; i < 10; i++) {
        string email = $"test{i}@example.com";
        
        if (!await userManager.Users.AnyAsync(u => userManager.NormalizeEmail(email) == u.NormalizedEmail)) {
            await userManager.CreateAsync(new() {
                Email = email,
                UserName = $"TestUser{i}",
                DisplayName = $"TestUser{i}",
                EmailConfirmed = true,
                IsProfileSetup = false,
            }, "Password1!");
        }
    }
}