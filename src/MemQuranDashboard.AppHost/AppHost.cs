using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var localstack = builder.AddContainer("localstack", "localstack/localstack:4.4.0")
    .WithHttpEndpoint(port: 4566, targetPort: 4566, isProxied: false)
    .WithExternalHttpEndpoints()
    .WithBindMount($"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".docker-data", "localstack", "volume")}", "/var/lib/localstack")
    .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock")
    .WithLifetime(ContainerLifetime.Persistent);

var redisConnectionString = builder.Configuration.GetConnectionString("RedisCache");
var redisCache = !string.IsNullOrEmpty(redisConnectionString)
    ? builder.AddConnectionString("RedisCache")
    : builder.AddRedis("RedisCache").WithDataVolume("memquran-api-redis-cache-data").WithHostPort(6379).WithPassword(builder.AddParameter("AspireRedisCachePassword", secret: true)).WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.MemQuran_Api>("memquran-api")
    .WithHttpEndpoint(port: 3122, targetPort: 3122, isProxied: false, name: "memquran-api-http")
    .WithHttpsEndpoint(port: 3123, targetPort: 3123, isProxied: false, name: "memquran-api-https")
    .WithExternalHttpEndpoints()
    .WithReference(redisCache)
    .WaitFor(redisCache)
    .WaitFor(localstack)
    ;

builder.AddProject<Projects.MemQuranDashboard_Api>("memquran-dashboard-api")
    .WithHttpEndpoint(port: 5147, targetPort: 5147, isProxied: false, name: "memquran-dashboard-api-http")
    .WithHttpsEndpoint(port: 7161, targetPort: 7161, isProxied: false, name: "memquran-dashboard-api-https")
    .WithExternalHttpEndpoints()
    // .WithReference(redisCache)
    // .WaitFor(redisCache)
    // .WaitFor(localstack)
    ;

builder.Build().Run();
