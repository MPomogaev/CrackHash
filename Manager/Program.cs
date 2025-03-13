using Manager;
using Manager.Database;
using Manager.Services;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

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

builder.Services.AddTransient<ICrackHashDatabase,  CrackHashDatabase>();
builder.Services.AddSingleton<IWorkerTaskService, WorkerTaskService>();
builder.Services.AddSingleton<IWorkerClient, WorkerClient>();
builder.Services.AddTransient<IWorkerApiService, WorkerApiService>();
builder.Services.AddTransient<ITimeoutService, TimeoutService>();

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
