using Manager;
using Manager.Database;
using Manager.Services;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Manager.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.Configure<WorkersOptions>(builder.Configuration.GetSection("WorkersOptions"));
builder.Services.Configure<CrackHashDatabaseOptions>(builder.Configuration.GetSection("CrackHashDatabaseOptions"));
builder.Services.Configure<RabbitMQServiceOptions>(builder.Configuration.GetSection("RabbitMQServiceOptions"));

builder.Services.AddTransient<IRabbitMQService, RabbitMQService>();
builder.Services.AddTransient<ICrackHashDatabase,  CrackHashDatabase>();
builder.Services.AddSingleton<IWorkerTaskService, WorkerTaskService>();
builder.Services.AddTransient<ITimeoutService, TimeoutService>();

builder.Services.AddHostedService<RabbitMQConsumerService>();
builder.Services.AddHostedService<PendingTasksService>();

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
