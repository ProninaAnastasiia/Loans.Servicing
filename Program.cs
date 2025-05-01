using AutoMapper;
using Loans.Servicing.Data;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Enums;
using Loans.Servicing.Data.Mappers;
using Loans.Servicing.Data.Models;
using Loans.Servicing.Data.Repositories;
using Loans.Servicing.Kafka;
using Loans.Servicing.Kafka.Consumers;
using Loans.Servicing.Kafka.Events;
using Loans.Servicing.Kafka.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");
builder.Services.AddDbContext<OperationsDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IOperationRepository, OperationRepository>();

builder.Services.AddScoped<IEventHandler<DraftContractCreatedEvent>, DraftContractCreatedHandler>();
builder.Services.AddScoped<IEventHandler<CreateContractFailedEvent>, CreateContractFailedHandler>();
builder.Services.AddScoped<IEventHandler<RepaymentScheduleCalculatedEvent>, RepaymentScheduleCalculatedHandler>();
builder.Services.AddScoped<IEventHandler<FullLoanValueCalculatedEvent>, FullLoanValueCalculatedHandler>();
builder.Services.AddScoped<IEventHandler<ContractValuesCalculatedEvent>, ContractValuesCalculatedHandler>();

builder.Services.AddHostedService<CalculateRepaymentConsumer>();
builder.Services.AddHostedService<CreateContractConsumer>();
builder.Services.AddHostedService<UpdateContractConsumer>();
builder.Services.AddHostedService<CalculateIndebtednessConsumer>();

builder.Services.AddSingleton<KafkaProducerService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/create-contract", async ([FromBody] LoanApplicationRequest application, KafkaProducerService producer, IConfiguration config, IMapper mapper, IOperationRepository repository) =>
{
    Guid operationId = Guid.NewGuid();
    var operation = new OperationEntity
    {
        OperationId = operationId,
        Description = "Создание черновика контракта",
        Status = OperationStatus.Started,
        ContextJson = JsonConvert.SerializeObject(application),
        StartedAt = DateTime.UtcNow
    };
    await repository.SaveAsync(operation);
    var @event = mapper.Map<CreateContractRequestedEvent>(application, opt => opt.Items["OperationId"] = operationId);
    var jsonMessage = JsonConvert.SerializeObject(@event);
    var topic = config["Kafka:Topics:CreateContractRequested"];
    
    await producer.PublishAsync(topic, jsonMessage);
});


app.Run();
