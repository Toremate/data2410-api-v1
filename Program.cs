using Microsoft.Data.SqlClient;
using MySqlConnector;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure Students table exists on startup.
var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();
        using var cmd = new MySqlCommand("""
                                          CREATE TABLE IF NOT EXISTS Students (
                                              id INT AUTO_INCREMENT PRIMARY KEY,
                                              name VARCHAR(255) NOT NULL,
                                              course VARCHAR(255) NOT NULL,
                                              marks INT NOT NULL,
                                              grade VARCHAR(255)
                                          )
                                          """, conn);
        cmd.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize database on startup.");
    }
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "running", api = "/api/Students" }));

app.MapGet("/health", (IConfiguration config) =>
{
    var cs = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(cs))
    {
        return Results.Ok(new { database = "No connection string found", hint = "Add DefaultConnection in App Service > Environment variables > Connection strings" });
    }

    try
    {
        using var conn = new MySqlConnection(cs);
        conn.Open();
        return Results.Ok(new { database = "Connected", server = conn.DataSource });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { database = "Failed", error = ex.Message });
    }
});

// app.MapGet("/api/Studentssss", (IConfiguration config) =>
//     {
//         var cs = config.GetConnectionString("DefaultConnection");
//         if (string.IsNullOrEmpty(cs))
//         {
//             return Results.Ok(new { database = "No connection string found", hint = "Add DefaultConnection in App Service > Environment variables > Connection strings" });
//         }
//
//         try
//         {
//             using var conn = new MySqlConnection(cs);
//             conn.Open();
//             using var cmd = new MySqlCommand("SELECT * FROM Students", conn);
//             using var reader = cmd.ExecuteReader();
//
//             var students = new List<object>();
//             while (reader.Read())
//             {
//                 students.Add(new
//                 {
//                     id     = reader["id"],
//                     name   = reader["name"],
//                     course = reader["course"],
//                     marks  = reader["marks"],
//                     grade  = reader["grade"]
//                 });
//             }
//
//             return Results.Ok(students);
//         }
//         catch (Exception ex)
//         {
//             return Results.Ok(new { database = "Failed", error = ex.Message });
//         }
//         
//     }
//
// );

app.MapControllers();

app.Run();
