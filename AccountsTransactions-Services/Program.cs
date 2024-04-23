using AccountsTransactions_BusinessObjects.Mappers;
using AccountsTransactions_BusinessObjects.ServiceModels.RequestModels.Transactions;
using AccountsTransactions_BusinessObjects.Services.Implement;
using AccountsTransactions_BusinessObjects.Services.Interface;
using AccountsTransactions_DataAccess.Models;
using AccountsTransactions_DataAccess.Repository.Implement;
using AccountsTransactions_DataAccess.Repository.Interface;
using AccountsTransactions_Services.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
							policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

//Add Identity
builder.Services.AddIdentity<User, IdentityRole>(op =>
	{
		op.User.RequireUniqueEmail = true;//login email
	})
	.AddEntityFrameworkStores<AccountsTransactionsContext>()
	.AddDefaultTokenProviders();

//Add DBContext
builder.Services.AddDbContext<AccountsTransactionsContext>(ops =>
		{
			ops.UseSqlServer(builder.Configuration.GetConnectionString("DBConnect"),
				b => b.MigrationsAssembly("AccountsTransactions-Services"));
		});

//swagger
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "gRPC Transcoding", Version = "v1" });
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Please enter a valid token",
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		BearerFormat = "JWT",
		Scheme = "Bearer"
	});
	c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new string[]{ }
					}
				});
});

//JWT
builder.Services.AddAuthentication(op =>
{
	op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	op.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(op =>
{
	op.SaveToken = true;
	op.RequireHttpsMetadata = false;
	op.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidAudience = builder.Configuration["JWT:ValidAudience"],
		ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:IssuerSigningKey"]))
	};
});

//add Authorization
builder.Services.AddAuthorization();

//add auto mapper
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddAutoMapper(typeof(OrderProfile));

//Add Scoped
builder.Services.AddScoped<DbContext, AccountsTransactionsContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ISendMailService, SendMailService>();
//options
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));


//hangfire
// Hangfire
var connectionString = builder.Configuration.GetConnectionString("DBConnect");
var sqlServerStorageOptions = new SqlServerStorageOptions
{
	CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
	SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
	QueuePollInterval = TimeSpan.Zero,
	UseRecommendedIsolationLevel = true,
	DisableGlobalLocks = true
};

builder.Services.AddHangfire(config => config
	.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UseSqlServerStorage(connectionString, sqlServerStorageOptions));
builder.Services.AddHangfireServer();

var app = builder.Build();

//swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//grpc
app.MapGrpcService<UsersService>();
app.MapGrpcService<AuthsService>();
app.MapGrpcService<OrdersService>();
app.MapGrpcService<TransactionsService>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
