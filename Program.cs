using AutoMapper;
using Hangfire;
using Hangfire.PostgreSql;
using Loans.Servicing;
using Loans.Servicing.Data;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Mappers;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka;
using Loans.Servicing.Kafka.Consumers;
using Loans.Servicing.Kafka.Events.CalculateContractValues;
using Loans.Servicing.Kafka.Events.CalculateFullLoanValue;
using Loans.Servicing.Kafka.Events.CreateDraftContract;
using Loans.Servicing.Kafka.Events.GetContractApproved;
using Loans.Servicing.Kafka.Events.InnerEvents;
using Loans.Servicing.Kafka.Handlers;
using Loans.Servicing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<OperationsDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<IEventsRepository, EventsRepository>();

builder.Services.AddScoped<IEventHandler<DraftContractCreatedEvent>, DraftContractCreatedHandler>();
builder.Services.AddScoped<IEventHandler<LoanApplicationRecieved>, LoanApplicationRecievedHandler>();
builder.Services.AddScoped<IEventHandler<CreateContractFailedEvent>, CreateContractFailedHandler>();
builder.Services.AddScoped<IEventHandler<FullLoanValueCalculatedEvent>, FullLoanValueCalculatedHandler>();
builder.Services.AddScoped<IEventHandler<ContractValuesCalculatedEvent>, ContractValuesCalculatedHandler>();
builder.Services.AddScoped<IEventHandler<ContractScheduleCalculatedEvent>, ContractScheduleCalculatedHandler>();
builder.Services.AddScoped<IEventHandler<ContractDetailsResponseEvent>, ContractDetailsResponseHandler>();
builder.Services.AddScoped<IEventHandler<ContractSentToClientEvent>, ContractSentToClientHandler>();
builder.Services.AddScoped<IEventHandler<CalculateScheduleRequested>, CalculateScheduleRequestedHandler>();
builder.Services.AddScoped<IEventHandler<CalculateFullLoanValueRequested>, CalculateFullLoanValueRequestedHandler>();

builder.Services.AddScoped<IDelayedTaskScheduler, DelayedTaskScheduler>();
builder.Services.AddScoped(typeof(HangfireHandlerExecutor<>));

builder.Services.AddHostedService<CreateContractConsumer>();
builder.Services.AddHostedService<UpdateContractConsumer>();
builder.Services.AddHostedService<CalculateContractValuesConsumer>();
builder.Services.AddHostedService<CalculateScheduleConsumer>();
builder.Services.AddHostedService<CalculateFullLoanValueConsumer>();

builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddSingleton<IHandlerDispatcher, HangfireHandlerDispatcher>();


builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(connectionString);
});

// Регистрируем сервер Hangfire как HostedService
builder.Services.AddHangfireServer();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapPost("/api/create-contract", async ([FromBody] LoanApplicationRequest application, KafkaProducerService producer, IConfiguration config, IMapper mapper) =>
{
    var operationId = Guid.NewGuid();
    MetricsRegistry.StartTimer(operationId);
    var @event = mapper.Map<LoanApplicationRecieved>(application, opt => opt.Items["OperationId"] = operationId);
    var jsonMessage = JsonConvert.SerializeObject(@event);
    var topic = config["Kafka:Topics:CreateContractRequested"];
    await producer.PublishAsync(topic, jsonMessage);
});

app.MapPost("/api/calculate-schedule", async ([FromBody] CalculateScheduleRequest request, KafkaProducerService producer, IConfiguration config, IMapper mapper) =>
{
    var operationId = Guid.NewGuid();
    var @event = mapper.Map<CalculateScheduleRequested>(request, opt => opt.Items["OperationId"] = operationId);
    var jsonMessage = JsonConvert.SerializeObject(@event);
    var topic = config["Kafka:Topics:CalculateSchedule"];
    await producer.PublishAsync(topic, jsonMessage);
});

app.MapPost("/api/calculate-full-loan-value", async ([FromBody] CalculateFullLoanValueRequest request, KafkaProducerService producer, IConfiguration config, IMapper mapper) =>
{
    var operationId = Guid.NewGuid();
    var @event = mapper.Map<CalculateFullLoanValueRequested>(request, opt => opt.Items["OperationId"] = operationId);
    var jsonMessage = JsonConvert.SerializeObject(@event);
    var topic = config["Kafka:Topics:CalculateIndebtedness"];
    await producer.PublishAsync(topic, jsonMessage);
});

app.UseHangfireDashboard("/hangfire");

// Метрики HTTP
app.UseHttpMetrics(); 

// Экспонирование метрик на /metrics
app.MapMetrics();

app.Run();