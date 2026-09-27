using PersonalFinanceTracker.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddBusinessLogic(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Personal Finance Tracker API v1"
        ));
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
await app.Services.InitializeDatabaseAsync();
app.Run();