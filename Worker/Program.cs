using Worker.RabbitMQ;
using Worker.Services;
using Worker.Services.Crack;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.Configure<RabbitMQServiceOptions>(builder.Configuration.GetSection("RabbitMQServiceOptions"));

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddTransient<IWordHandler, WordHandler>();
builder.Services.AddTransient<ICrackService, CrackService>();
builder.Services.AddTransient<IWorkerService, WorkerService>();

builder.Services.AddHostedService<RabbitMQConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
