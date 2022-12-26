using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TwitchBot.Api.Helpers.ErrorExceptions;
using TwitchBotDb.Context;

#region Builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
        Version = "v2022.07.26",
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
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseMiddleware<ErrorHandlerMiddleware>(); // global error handler
//app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});

app.Run();
#endregion