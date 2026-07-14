using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDataSource("db-postgres", c =>
{
    var cs = builder.Configuration.GetConnectionString("db-postgres");

    var sb = new NpgsqlConnectionStringBuilder(cs);
    sb.Username = builder.Configuration.GetValue<string>("Postgres:Username");
    sb.Password = builder.Configuration.GetValue<string>("Postgres:Password");
    c.ConnectionString = sb.ConnectionString;
});

builder.AddSqlServerClient("db-sqlserver", c =>
{
    var cs = builder.Configuration.GetConnectionString("db-sqlserver");

    var sb = new SqlConnectionStringBuilder(cs);
    sb.UserID = builder.Configuration.GetValue<string>("SqlServer:Username");
    sb.Password = builder.Configuration.GetValue<string>("SqlServer:Password");
    c.ConnectionString = sb.ConnectionString;
});

builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
// ==========================================
// DB1 - SQL SERVER (VERSIONI VULNERABILI)
// ==========================================

app.MapPost("/api/db1/users/login", async (LoginModel model, SqlConnection connection) =>
{
    string query = $"SELECT username FROM Users WHERE password = '{model.Password}' AND username = '{model.Username}' order by userid";
    using var command = new SqlCommand(query, connection);
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var isOk = await reader.ReadAsync();
        if (isOk)
            return Results.Ok(GenerateToken(reader.GetString(0)));
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<string>(200)
.Produces(404)
.ProducesValidationProblem(400)
.ProducesProblem(500);

app.MapGet("/api/db1/cards/list", async (string text, SqlConnection connection) =>
{
    string query = $"SELECT number, null, expirydate, 1, issuer FROM Cards WHERE number like '%{text}%' or code like '%{text}%' or issuer like '%{text}%'";
    using var command = new SqlCommand(query, connection);
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var result = new List<CardModel>();
        foreach(var item in reader)
        {
            result.Add(new CardModel
            {
                Number = reader.GetString(0),
                ExpiryDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                Issuer = reader.GetString(4)
            });
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<IEnumerable<CardModel>>(200)
.ProducesValidationProblem(400)
.ProducesProblem(500);


// ==========================================
// DB2 - POSTGRESQL (VERSIONI VULNERABILI)
// ==========================================

app.MapPost("/api/db2/users/login", async (LoginModel model, NpgsqlConnection connection) =>
{
    // AGGIORNATO: Tabella Users e colonne in inglese
    string query = $"SELECT username FROM Users WHERE password = '{model.Password}' AND username = '{model.Username}' ";
    using var command = new NpgsqlCommand(query, connection);
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var isOk = await reader.ReadAsync();
        if (isOk)
            return Results.Ok(GenerateToken(reader.GetString(0)));
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<string>(200)
.Produces(404)
.ProducesValidationProblem(400)
.ProducesProblem(500);

app.MapGet("/api/db2/cards/list", async (string text, NpgsqlConnection connection) =>
{
    string query = $"SELECT null, null, null, null, number, expirydate, issuer FROM Cards WHERE number like '%{text}%' or code like '%{text}%' or issuer like '%{text}%'";
    using var command = new NpgsqlCommand(query, connection);
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var result = new List<CardModel>();
        foreach (var item in reader)
        {
            result.Add(new CardModel
            {
                Number = reader.GetString(4),
                ExpiryDate = DateOnly.FromDateTime(reader.GetDateTime(5)),
                Issuer = reader.GetString(6)
            });
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<IEnumerable<CardModel>>(200)
.ProducesValidationProblem(400)
.ProducesProblem(500);


// ==========================================
// DB1 - SQL SERVER (VERSIONI SICURE)
// ==========================================

app.MapPost("/api/db1/users/login-secure", async (LoginModel model, SqlConnection connection) =>
{
    string query = "SELECT username FROM Users WHERE password = @password AND username = @username";
    using var command = new SqlCommand(query, connection);
    command.Parameters.AddWithValue("@username", model.Username);
    command.Parameters.AddWithValue("@password", model.Password);
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var isOk = await reader.ReadAsync();
        if (isOk)
            return Results.Ok(GenerateToken(reader.GetString(0)));
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<string>(200)
.Produces(404)
.ProducesValidationProblem(400)
.ProducesProblem(500);

app.MapGet("/api/db1/cards/list-secure", async (string text, SqlConnection connection) =>
{
    // CORRETTO: Adesso usa Cards e le colonne inglesi coerenti con il database
    string query = @"
        SELECT number, expirydate, issuer 
        FROM Cards 
        WHERE number LIKE @search 
           OR code LIKE @search 
           OR issuer LIKE @search";

    using var command = new SqlCommand(query, connection);
    command.Parameters.AddWithValue("@search", $"%{text}%");
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var result = new List<CardModel>();
        while (await reader.ReadAsync())
        {
            var dbDate = reader.GetDateTime(reader.GetOrdinal("expirydate"));
            result.Add(new CardModel
            {
                Number = reader.GetString(reader.GetOrdinal("number")),
                ExpiryDate = DateOnly.FromDateTime(dbDate),
                Issuer = reader.GetString(reader.GetOrdinal("issuer"))
            });
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<IEnumerable<CardModel>>(200)
.ProducesValidationProblem(400)
.ProducesProblem(500);


// ==========================================
// DB2 - POSTGRESQL (VERSIONI SICURE)
// ==========================================

app.MapPost("/api/db2/users/login-secure", async (LoginModel model, NpgsqlConnection connection) =>
{
    // AGGIORNATO: Tabella Users e parametri in inglese
    string query = "SELECT username FROM Users WHERE password = @password AND username = @username";
    using var command = new NpgsqlCommand(query, connection);
    command.Parameters.AddWithValue("@username", model.Username);
    command.Parameters.AddWithValue("@password", model.Password);
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var isOk = await reader.ReadAsync();
        if (isOk)
            return Results.Ok(GenerateToken(reader.GetString(0)));
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<string>(200)
.Produces(404)
.ProducesValidationProblem(400)
.ProducesProblem(500);

app.MapGet("/api/db2/cards/list-secure", async (string text, NpgsqlConnection connection) =>
{
    // AGGIORNATO: Query sicura su schema inglese
    string query = @"
        SELECT number, expirydate, issuer 
        FROM Cards 
        WHERE number LIKE @search 
           OR code LIKE @search 
           OR issuer LIKE @search";

    using var command = new NpgsqlCommand(query, connection);
    command.Parameters.AddWithValue("@search", $"%{text}%");
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var result = new List<CardModel>();
        while (await reader.ReadAsync())
        {
            result.Add(new CardModel
            {
                Number = reader.GetString(reader.GetOrdinal("number")),
                ExpiryDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("expirydate"))),
                Issuer = reader.GetString(reader.GetOrdinal("issuer"))
            });
        }
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces<IEnumerable<CardModel>>(200)
.ProducesValidationProblem(400)
.ProducesProblem(500);

app.Run();


string GenerateToken(string username)
{
    var claims = new List<Claim>
    {
        new("sub", username)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SGYWdsgsku5y4w3gSDge9=£sfaf.w56£$as{35yagdsgsdGSGsdhWE%323GS$&76G4WEyi=?'dgssdASFfa8676DHSH(£sdvfs"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
    var expires = DateTime.UtcNow.AddMinutes(15);

    var token = new JwtSecurityToken(
        issuer: "Issuer",
        audience: "Audience",
        claims: claims,
        expires: expires,
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// DTO Models
class LoginModel
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}

class CardModel
{
    public string Number { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string Issuer { get; set; }
}