using Microsoft.EntityFrameworkCore;
using Conflux.Components;
using Conflux.Database;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Conflux.Database.Entities;
using Conflux.Services;
using Conflux.Services.Abstracts;
using Conflux.Services.Hubs;
using Markdig;
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
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => {
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .EnableSensitiveDataLogging();
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
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddScoped<RoleManager<IdentityRole>>();

// TODO: Email sending services.
// builder.Services.AddSingleton<IEmailSender<ApplicationUser>, ...>();

// System services.
builder.Services.AddScoped<ApplicationRedirectManager>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddViteServices();
builder.Services.AddScoped<MarkdownPipeline>(services => {
    var pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    return pipeline;
});
builder.Services.AddScoped<ProfileSanitizingService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IConversationCacheService, ConversationCacheService>();
builder.Services.AddScoped<IConversationService, ConversationService>();

// SignalR related services.
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
});
builder.Services.AddResponseCompression(option => {
    option.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([ "application/octet-stream" ]);
});

// Controllers.
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Nuget Packages
// ...

var app = builder.Build();

app.UseResponseCompression();

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

PhysicalFileProvider uploadsFileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Uploads"));

app.Environment.WebRootFileProvider = new CompositeFileProvider(app.Environment.WebRootFileProvider, uploadsFileProvider);

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = uploadsFileProvider,
    RequestPath = "/uploads",
    ContentTypeProvider = new ApplicationContentTypeProvider(),
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/logout", async (ClaimsPrincipal claims, [FromServices] SignInManager<ApplicationUser> signInManager, [FromForm(Name = "ReturnUrl")] string returnUrl) => {
    await signInManager.SignOutAsync();
    return TypedResults.LocalRedirect($"~/{returnUrl}");
});

app.MapHub<NotificationHub>("/hub/notification");
app.MapHub<ConversationHub>("/hub/conversation");

// Setup Application stuffs.
using (var scope = app.Services.CreateScope()) {
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await CreateRoles(roleManager);

    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

    if (environment.IsDevelopment()) {
        await CreateFakeUsers(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }
}

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
    for (int i = 0; i < 200; i++) {
        string email = $"test{i}@example.com";
        
        if (!await userManager.Users.AnyAsync(u => userManager.NormalizeEmail(email) == u.NormalizedEmail)) {
            await userManager.CreateAsync(new() {
                Email = email,
                UserName = $"TestUser{i}",
                DisplayName = $"TestUser{i}",
                EmailConfirmed = true,
                IsProfileSetup = true,
            }, "Password1!");
        }
    }

    // var fakeUser = new Faker<ApplicationUser>()
    //     .RuleFor(u => u.Id, f => {
    //         UInt128 id = (UInt128)(f.IndexFaker + 1);
    //
    //         Span<byte> buffer = stackalloc byte[16];
    //         BitConverter.TryWriteBytes(buffer, id);
    //
    //         return new Guid(buffer).ToString();
    //     })
    //     .RuleFor(u => u.UserName, f => f.Internet.UserName())
    //     .RuleFor(u => u.DisplayName, (f, u) => u.UserName)
    //     .RuleFor(u => u.Pronouns, f => f.PickRandom("he/him", "she/her", "they/them"))
    //     .RuleFor(u => u.Bio, f => {
    //         var lorem = f.Lorem.Paragraph();
    //         return lorem[..int.Min(255, lorem.Length)];
    //     })
    //     .RuleFor(u => u.Email, f => f.Internet.Email())
    //     .RuleFor(u => u.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow))
    //     .RuleFor(u => u.StatusText, f => {
    //         string str = string.Join(' ', f.Lorem.Words(16));
    //         return str[..int.Min(128, str.Length)];
    //     })
    //     .RuleFor(u => u.IsProfileSetup, f => true)
    //     .RuleFor(u => u.EmailConfirmed, f => true);
    //
    // var users = fakeUser.Generate(64);
    //
    // for (int i = 0; i < users.Count; i++) {
    //     if (await userManager.FindByIdAsync(users[i].Id) != null) continue;
    //     
    //     await userManager.CreateAsync(users[i], "Password1!");
    // }
}