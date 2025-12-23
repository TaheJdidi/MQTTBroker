using MQTTBrokerService;
using MQTTBrokerService.Models;
using MQTTBrokerService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "MQTT Broker Service";
});

// Configure broker settings from appsettings.json
builder.Services.Configure<BrokerConfiguration>(
    builder.Configuration.GetSection("MqttBroker"));

// Register services
builder.Services.AddSingleton<IMqttBrokerManager, MqttBrokerManager>();
builder.Services.AddHostedService<Worker>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MQTT Broker API",
        Version = "v1",
        Description = "API for managing the MQTT V5 Broker"
    });
});

// Add CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
