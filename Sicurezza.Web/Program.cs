using Microsoft.Data.SqlClient;
using Npgsql;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.AddNpgsqlDataSource("db-postgres");
builder.AddSqlServerClient("db-sqlserver");
//string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var connectionString = builder.Configuration.GetConnectionString("db-sqlserver");
Console.WriteLine($"DEBUG: Connection String is: {connectionString}"); builder.Services.AddOpenApi();
var app = builder.Build();
app.MapOpenApi();

app.MapPost("/api/db1/users/login", async (LoginModel model, SqlConnection connection) =>
{
    string query = $"SELECT username FROM Users WHERE password = '{model.Password}' AND Username = '{model.Username}' ";
    //string query = $"SELECT 1 FROM Users WHERE Username = '{model.Username}' AND password = '{model.Password}'";

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

app.MapPost("/api/db2/users/login", async (LoginModel model, NpgsqlConnection connection) =>
{
    string query = $"SELECT nome_utente FROM utenti WHERE pwd = '{model.Password}' AND nome_utente = '{model.Username}' ";
    //string query = $"SELECT 1 FROM Users WHERE Username = '{model.Username}' AND password = '{model.Password}'";

    using var command = new NpgsqlCommand(query, connection);

    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var isOk = await reader.ReadAsync();
        if (isOk)
            return Results.Ok(reader["nome_utente"]);
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
    string query = $"SELECT null, null, null, null, numero, scadenza, emittente FROM carte WHERE numero like '%{text}%' or codice like '%{text}%' or emittente like '%{text}%'";

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

app.Run();

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