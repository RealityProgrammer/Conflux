using Conflux.Application.Abstracts;
using Conflux.Application.Implementations;
using Microsoft.EntityFrameworkCore;
using Conflux.Web.Components;
using Conflux.Domain;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Conflux.Domain.Entities;
using Conflux.Web.Authentication;
using Conflux.Web.Authorization;
using Conflux.Web.Hubs;
using Conflux.Web.Services;
using Conflux.Web.Services.Abstracts;
using Conflux.Web.Services.Implementations;
using Markdig;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Adding authentication services.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ApplicationAuthenticationStateProvider>();

var authBuilder = builder.Services.AddAuthentication(options => {
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

authBuilder.AddGoogle(options => {
    IConfigurationSection googleAuth = builder.Configuration.GetSection("Authentication:Google");
    
    options.ClientId = googleAuth["ClientId"] ?? throw new InvalidOperationException("Missing Google ClientId");
    options.ClientSecret = googleAuth["ClientSecret"] ?? throw new InvalidOperationException("Missing Google ClientSecret");
    
    options.CallbackPath = "/auth/signin-google";
    options.ClaimActions.MapJsonKey("picture", "picture");
});

authBuilder.AddIdentityCookies(configCookies => {
    configCookies.ApplicationCookie!.Configure(configOptions => {
        configOptions.LoginPath = "/auth/login";
        configOptions.LogoutPath = "/auth/logout";
        configOptions.AccessDeniedPath = "/denied";
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
    .AddPolicy("AccessCommunityReports", policy => {
        policy.Requirements.Add(new AccessCommunityReportRequirement());
    })
    .AddPolicy("UpdateCommunityRolePermissions", policy => {
        policy.Requirements.Add(new UpdateCommunityRolePermissionsRequirement());
    })
    .AddPolicy("UpdateCommunityMemberRole", policy => {
        policy.Requirements.Add(new UpdateCommunityMemberRoleRequirement());
    })
    .AddPolicy("ManageCommunityReports", policy => {
        policy.Requirements.Add(new ManageCommunityReportsRequirement());
    })
    .AddPolicy("DeleteMemberMessage", policy => {
        policy.Requirements.Add(new DeleteMemberMessageRequirement());
    })
    .AddPolicy("BanCommunityMember", policy => {
        policy.Requirements.Add(new BanCommunityMemberRequirement());
    })
    .AddPolicy("AccessAdministratorPage", policy => {
        policy.RequireRole("Moderator", "Admin", "SystemDeveloper");
    })
    .AddPolicy("AccessUserProfileInformation", policy => {
        policy.RequireRole("Moderator", "Admin", "SystemDeveloper");
    })
    .AddPolicy("ReadUserSystemRole", policy => {
        policy.RequireRole("Moderator", "Admin", "SystemDeveloper");
    })
    .AddPolicy("UserNotBanned", policy => {
        policy.Requirements.Add(new UserNotBannedRequirement());
    });

builder.Services.AddSingleton<IAuthorizationHandler, CreateCommunityChannelCategoryAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CreateCommunityChannelAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CreateCommunityRoleAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DeleteCommunityRoleAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, RenameCommunityRoleAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, UpdateCommunityRolePermissionsAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AccessCommunityControlPanelAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, AccessCommunityReportsAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, UpdateCommunityMemberRoleAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ManageCommunityReportsAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DeleteMemberMessageAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, BanCommunityMemberAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, UserNotBannedAuthorizationHandler>();

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
        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IMailService, MailService>();

// System services.
builder.Services.AddMemoryCache();
builder.Services.AddDistributedPostgresCache(options => {
    options.ConnectionString = builder.Configuration.GetConnectionString("PostgresCache");
    options.SchemaName = builder.Configuration.GetValue<string>("PostgresCache:SchemaName", "public");
    options.TableName = builder.Configuration.GetValue<string>("PostgresCache:TableName", "cache");
    options.CreateIfNotExists = builder.Configuration.GetValue("PostgresCache:CreateIfNotExists", true);
    options.UseWAL = builder.Configuration.GetValue("PostgresCache:UseWAL", false);
    
    var expirationInterval = builder.Configuration.GetValue<string>("PostgresCache:ExpiredItemsDeletionInterval");
    if (!string.IsNullOrEmpty(expirationInterval) && TimeSpan.TryParse(expirationInterval, out var interval)) {
        options.ExpiredItemsDeletionInterval = interval;
    }
    
    var slidingExpiration = builder.Configuration.GetValue<string>("PostgresCache:DefaultSlidingExpiration");
    if (!string.IsNullOrEmpty(slidingExpiration) && TimeSpan.TryParse(slidingExpiration, out var sliding)) {
        options.DefaultSlidingExpiration = sliding;
    }
});

builder.Services.AddSingleton<ICacheService, CacheService>();

builder.Services.AddHttpClient<CloudflareTurnServerClient>();

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
builder.Services.AddSingleton<IContentService, ContentService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IConversationEventDispatcher, ConversationEventDispatcher>();
builder.Services.AddScoped<ISnackbarService, SnackbarService>();
builder.Services.AddScoped<ModalService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<ICommunityRoleService, CommunityRoleService>();
builder.Services.AddScoped<ICommunityEventDispatcher, CommunityEventDispatcher>();
builder.Services.AddScoped<IModerationService, ModerationService>();
builder.Services.AddScoped<IWebUserNotificationService, UserNotificationService>();
builder.Services.AddScoped<IUserNotificationService>(provider => provider.GetRequiredService<IWebUserNotificationService>());
builder.Services.AddSingleton<ICallService, CallService>();
builder.Services.AddScoped<IUserCallService, UserCallService>();
builder.Services.AddSingleton<IStatisticsService, StatisticsService>();
    
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
    
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    await CreateRoles(roleManager);
    await CreateAdminUser(userManager, scope.ServiceProvider.GetRequiredService<IConfiguration>());

    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    if (environment.IsDevelopment()) {
        await CreateFakeUsers(userManager);
    }
}

if (!app.Environment.IsDevelopment()) {
    app.UseResponseCompression();
}

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

app.MapGet("/auth/external-login", ([FromQuery(Name = "Provider")] string provider, [FromServices] SignInManager<ApplicationUser> signInManager) => {
    var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, "/auth/external-login/google");
    return Results.Challenge(properties, [provider]);
});

app.MapPost("/auth/confirm-profile-setup", async (ClaimsPrincipal claims, [FromServices] IUserService userService, [FromForm(Name = "ReturnUrl")] string returnUrl) => {
    await userService.UpdateProfileSetup(claims, true);
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.MapPost("/auth/logout", async (ClaimsPrincipal claims, [FromServices] SignInManager<ApplicationUser> signInManager, [FromForm(Name = "ReturnUrl")] string? returnUrl) => {
    await signInManager.SignOutAsync();
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.MapHub<FriendshipHub>("/hub/friendship");
app.MapHub<ConversationHub>("/hub/conversation");
app.MapHub<CommunityHub>("/hub/community");
app.MapHub<UserNotificationHub>("/hub/user-notification");
app.MapHub<CallingHub>("/hub/calling");

// Add Roles, and assign to users.
app.Run();
return;

async Task CreateRoles(RoleManager<IdentityRole<Guid>> roleManager) {
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
                CreatedAt = DateTime.UtcNow,
            }, "Password1!");
        }
    }
}

async Task CreateAdminUser(UserManager<ApplicationUser> userManager, IConfiguration config) {
    const string adminEmail = "admin@conflux.com";
    
    if (!await userManager.Users.AnyAsync(u => userManager.NormalizeEmail(adminEmail) == u.NormalizedEmail)) {
        var adminUser = new ApplicationUser {
            Email = adminEmail,
            UserName = "Admin",
            DisplayName = "Admin",
            EmailConfirmed = true,
            IsProfileSetup = true,
            CreatedAt = DateTime.UtcNow,
        };
        
        var result = await userManager.CreateAsync(adminUser, config["InitialAdminPassword"]!);

        if (!result.Succeeded) {
            var error = result.Errors.First();
            
            throw new($"Failed to create Admin user ({error.Code} - {error.Description})");
        }

        result = await userManager.AddToRoleAsync(adminUser, "Admin");
        
        if (!result.Succeeded) {
            var error = result.Errors.First();
            
            throw new($"Failed to add Admin role to Admin user ({error.Code} - {error.Description})");
        }
    }
}