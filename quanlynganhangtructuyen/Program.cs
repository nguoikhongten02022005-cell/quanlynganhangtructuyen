using DAL;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<NganHangDbContext>(tuyChon =>
    tuyChon.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.CommandTimeout(30)));

builder.Services.AddScoped<KhachHangDAL>();
// Nếu có BLL thì thêm:
// builder.Services.AddScoped<KhachHangBusiness>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
