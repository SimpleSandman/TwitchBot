using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using TwitchBot.Api.Helpers;
using TwitchBotDb.Context;

#region Builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SimpleBotContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SimpleBotContext"));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Twitch Bot API",
        Version = "v2022.07.16",
        Description = "Back-end of chat bot specific calls",
        Contact = new OpenApiContact
        {
            Name = "GitHub",
            Email = string.Empty,
            Url = new Uri("https://github.com/SimpleSandman/TwitchBot")
        }
    });
});
#endregion

#region App
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion