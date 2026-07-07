using Microsoft.Data.SqlClient;
using Npgsql;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 🔒 1. BLINDO POSTGRESQL: DISATTIVIAMO ASPIRE E FORZIAMO L'UTENTE LIMITATO
// ============================================================================
// Commentiamo la riga nativa così Aspire SMETTE di iniettare il superuser 'postgres'
// builder.AddNpgsqlDataSource("db-postgres"); 

// Registriamo manualmente la sorgente dati Postgres usando l'utente limitato app_reader
// Modifica la riga inserendo la porta corretta presa dalla dashboard di Aspire
builder.Services.AddNpgsqlDataSource("Host=localhost;Port=57476;Database=db-postgres;Username=app_reader;Password=PasswordSicura123!;");
// ============================================================================


// ============================================================================
// 🔒 2. BLINDO SQL SERVER (Come fatto in precedenza)
// ============================================================================
// builder.AddSqlServerClient("db-sqlserver"); 
builder.Services.AddScoped(_ => new SqlConnection("Server=127.0.0.1,57477;Database=db-sqlserver;User ID=app_reader;Password=PasswordSicura123!;TrustServerCertificate=true"));
// ============================================================================

builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
// ==========================================
// DB1 - SQL SERVER (VERSIONI VULNERABILI)
// ==========================================

app.MapPost("/api/db1/users/login", async (LoginModel model, SqlConnection connection) =>
{
    string query = $"SELECT username FROM Users WHERE password = '{model.Password}' AND username = '{model.Username}' ";
    using var command = new SqlCommand(query, connection);
    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var isOk = await reader.ReadAsync();
        if (isOk)
            return Results.Ok(reader["username"]);
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces(200, typeof(string))
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
            return Results.Ok(reader["username"]);
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces(200, typeof(string))
.Produces(404)
.ProducesValidationProblem(400)
.ProducesProblem(500);

app.MapGet("/api/db2/cards/list", async (string text, NpgsqlConnection connection) =>
{
    // AGGIORNATO: Tabella Cards e colonne in inglese
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
            return Results.Ok(reader["username"]);
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces(200, typeof(string))
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
            return Results.Ok(reader["username"]);
        else
            return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.Produces(200, typeof(string))
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