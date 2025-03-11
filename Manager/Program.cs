using Manager;
using Manager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.Configure<WorkersOptions>(builder.Configuration.GetSection("WorkersOptions"));

builder.Services.AddSingleton<IWorkerTaskService, WorkerTaskService>();
builder.Services.AddSingleton<IWorkerClient, WorkerClient>();
builder.Services.AddTransient<IWorkerApiService, WorkerApiService>();
builder.Services.AddTransient<ITimeoutService, TimeoutService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
