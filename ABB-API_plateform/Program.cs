using Abb.Business;
using Abb.Data;
using ABB_API_plateform.Business;
using ABB_API_plateform.Infrastructure; // ⭐ Add this
using Dapper; // ⭐ Add this

var builder = WebApplication.CreateBuilder(args);

// ⭐ Register Dapper type handlers for DateOnly
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new DateOnlyNullableTypeHandler());

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
builder.Services.AddScoped<ILogging, Logging>();


// 3. Define the CORS policy correctly
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReact", policy => {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://yourgetawayatsylvan.com",
                "https://www.yourgetawayatsylvan.com", 
                "http://yourgetawayatsylvan.com",       
                "http://www.yourgetawayatsylvan.com"   
              )
              .AllowAnyMethod()
              .AllowAnyHeader();
              
    });
});

var app = builder.Build();

// 4. Configure the HTTP request pipeline order is CRITICAL
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// UseCors MUST come after UseRouting (implied) and BEFORE UseAuthorization
app.UseCors("AllowReact");

app.UseAuthorization();

app.MapControllers();

app.Run();