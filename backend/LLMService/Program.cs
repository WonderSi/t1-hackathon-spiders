using LLMService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<ISciboxClient, SciboxClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Scibox:BaseUrl"] ?? "https://llm.t1v.scibox.tech/");
    client.Timeout = TimeSpan.FromSeconds(int.Parse(builder.Configuration["Scibox:TimeoutSeconds"] ?? "60"));
});

// Настройка кэширования
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ILLMService, LLMService.Services.LLMService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();