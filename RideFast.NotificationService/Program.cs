using RideFast.Contracts;
using RideFast.NotificationService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<NotificationSettings>(builder.Configuration.GetSection(NotificationSettings.Section));
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.Section));

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<NotificationSender>();
builder.Services.AddHostedService<NotificationConsumer>();

builder.Build().Run();
