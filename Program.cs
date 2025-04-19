using AutoMapper;
using Loans.Servicing.Data;
using Loans.Servicing.Data.Dto;
using Loans.Servicing.Data.Mappers;
using Loans.Servicing.Kafka;
using Loans.Servicing.Kafka.Events;
using Loans.Servicing.StateMachines;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");
//builder.Services.AddDbContext<StateMachinesDbContext>(options => options.UseNpgsql(connectionString));

// Регистрируем Kafka consumer как фоновый сервис
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.AddConsumer<KafkaMessageConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");

            k.TopicEndpoint<KafkaMessage>("topic-name", "consumer-group-name", e =>
            {
                e.ConfigureConsumer<KafkaMessageConsumer>(context);
            });
        });
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
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

app.MapPost("/api/create-contract", async ([FromBody] LoanApplicationRequest application, KafkaProducerService producer, IConfiguration config, IMapper mapper) =>
{
    var @event = mapper.Map<CreateContractRequestedEvent>(application);
    var jsonMessage = JsonConvert.SerializeObject(@event);
    var topic = config["Kafka:Topics:LoanApplicationSubmitted"];
    
    await producer.PublishAsync(topic!, jsonMessage);
});

app.Run();
