var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddScoped<IDbConnector, SqlDbConnector>()
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Enable cors for front-end
builder.Services.AddCors(options =>
{
    options.AddPolicy("TodoItemsController",
        policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
// app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
