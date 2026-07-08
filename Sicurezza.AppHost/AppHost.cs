var builder = DistributedApplication.CreateBuilder(args);

// Configura PostgreSQL
var postgres = builder.AddPostgres("postgres")
                      .WithImageTag("latest")
                      .WithDataVolume()
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithEndpoint("tcp", c => {
                          c.Port = 57476;
                          c.TargetPort = 5432;
                      })
                      .AddDatabase("db-postgres");

// Configura sql server
var sqlServer = builder.AddSqlServer("sqlserver")
                      .WithDataVolume()
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithImageTag("latest")
                      .WithEndpoint("tcp", c => {
                          c.Port = 57477;
                          c.TargetPort = 1433;
                      })
                      .AddDatabase("db-sqlserver");

builder.AddProject<Projects.Sicurezza_Web>("sicurezza-web")
    .WithReference(sqlServer)
    .WithReference(postgres);
builder.Build().Run();
