using Abb.Business;
using Abb.Data;
using Dapper;
using System.Transactions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Register your Auth Service
builder.Services.AddScoped<IAuthentication, AuthenticationService>();
builder.Services.AddScoped<ITransactions, TransactionsService>();
builder.Services.AddScoped<IUsersClass, UsersClass>();
builder.Services.AddScoped<IReservations, ReservationsService>();
builder.Services.AddScoped<IReservationsClass, ReservationsClass>();
builder.Services.AddScoped<IProperties, PropertiesClass>();
builder.Services.AddScoped<ICalendar, CalendarClass>();


// 3. Define the CORS policy correctly
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReact", policy => {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
// 4. Configure the HTTP request pipeline order is CRITICAL
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// UseCors MUST come after UseRouting (implied) and BEFORE UseAuthorization
app.UseCors("AllowReact");

app.UseAuthorization();

app.MapControllers();

app.Run();