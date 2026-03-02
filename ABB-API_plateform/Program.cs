using Abb.Business;
using Abb.Data;
using ABB_API_plateform.Business;
using ABB_API_plateform.Infrastructure;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Register Dapper type handlers for DateOnly
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new DateOnlyNullableTypeHandler());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IAuthentication, AuthenticationService>();
builder.Services.AddScoped<ITransactions, TransactionsService>();
builder.Services.AddScoped<IUsersClass, UsersClass>();
builder.Services.AddScoped<IReservations, ReservationsService>();
builder.Services.AddScoped<IReservationsClass, ReservationsClass>();
builder.Services.AddScoped<IProperties, PropertiesClass>();
builder.Services.AddScoped<IReviews, ReviewsService>();
builder.Services.AddScoped<IReviewsClass, ReviewsClass>();
builder.Services.AddScoped<IEvents, EventsService>();
builder.Services.AddScoped<IEventsClass, EventsClass>();
builder.Services.AddScoped<IInvoices, InvoicesService>();      
builder.Services.AddScoped<IInvoicesClass, InvoicesClass>();    
builder.Services.AddScoped<ILogging, Logging>();


// CORS
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

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthorization();
app.MapControllers();

app.Run();