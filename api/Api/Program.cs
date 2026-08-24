using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AWSOptions options = builder.Configuration.GetAWSOptions();
string? serviceUrl = builder.Configuration["DynamoDB:ServiceUrl"];

if (!string.IsNullOrEmpty(serviceUrl))
{
    options.DefaultClientConfig.ServiceURL = serviceUrl;
    options.Credentials = new BasicAWSCredentials("accessKey", "secretKey");
}

builder.Services.AddDefaultAWSOptions(options);
builder.Services.AddAWSService<IAmazonDynamoDB>();

WebApplication app = builder.Build();

const string tableName = "runs";

// Table creation at startup
IAmazonDynamoDB dynamoDB = app.Services.GetRequiredService<IAmazonDynamoDB>();

try
{
    await dynamoDB.DescribeTableAsync(tableName);
}
catch (ResourceNotFoundException)
{
    await dynamoDB.CreateTableAsync(new CreateTableRequest
    {
        TableName = tableName,
        AttributeDefinitions =
        [
            new AttributeDefinition
            {
                AttributeName = "runId",
                AttributeType = ScalarAttributeType.S
            }
        ],
        KeySchema =
        [
            new KeySchemaElement
            {
                AttributeName = "runId",
                KeyType = KeyType.HASH
            }
        ],
        BillingMode = BillingMode.PAY_PER_REQUEST
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();