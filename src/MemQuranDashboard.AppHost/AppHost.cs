using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var localstack = builder.AddContainer("localstack", "localstack/localstack:4.4.0")
    .WithHttpEndpoint(port: 4566, targetPort: 4566, isProxied: false)
    .WithExternalHttpEndpoints()
    .WithBindMount($"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".docker-data", "localstack", "volume")}", "/var/lib/localstack")
    .WithLifetime(ContainerLifetime.Persistent);

var redisConnectionString = builder.Configuration.GetConnectionString("RedisCache");
var redisCache = !string.IsNullOrEmpty(redisConnectionString)
    ? builder.AddConnectionString("RedisCache")
    : builder.AddRedis("RedisCache").WithDataVolume("memquran-api-redis-cache-data").WithHostPort(6379).WithPassword(builder.AddParameter("AspireRedisCachePassword", secret: true)).WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.Integrations_Api>("memquran-dashboard-integrations-api")
    // if you get port conflict, make sure the projects launchSettings.json is using the environmentVariables__ASPNETCORE_URLS (not applicationUrl)
    .WithHttpEndpoint(port: 8230, targetPort: 8230, isProxied: false, name: "memquran-dashboard-integrations-api-http")
    .WithHttpsEndpoint(port: 8231, targetPort: 8231, isProxied: false, name: "memquran-dashboard-integrations-api-https")
    .WithExternalHttpEndpoints()
    .WithReference(redisCache)
    .WaitFor(redisCache)
    // .WaitFor(localstack)
    ;

builder.AddAzureFunctionsProject<Projects.Integrations_Sentry_Functions>("memquran-dashboard-integrations-sentry-functions")
    // if you get port conflict, make sure the projects launchSettings.json is using the environmentVariables__ASPNETCORE_URLS (not applicationUrl)
    .WithHttpEndpoint(port: 7268, targetPort: 7268, isProxied: false, name: "memquran-dashboard-integrations-sentry-functions-http")
    .WithExternalHttpEndpoints()
    .WithEnvironment("AzureWebJobsStorage", "UseDevelopmentStorage=true")
    .WithEnvironment("ConnectionStrings__DefaultConnection", builder.Configuration["Parameters:ConnectionStrings__DefaultConnection"])
    .WithEnvironment("SentrySettings__Environment", builder.AddParameterFromConfiguration("sentry-environment", "Parameters:SentrySettings__Environment", secret: false))
    .WithEnvironment("SentrySettings__BaseUrl", builder.AddParameterFromConfiguration("sentry-baseurl", "Parameters:SentrySettings__BaseUrl", secret: false))
    .WithEnvironment("SentrySettings__ApiKey", builder.AddParameterFromConfiguration("sentry-api-key", "Parameters:SentrySettings__ApiKey", secret: true))
    .WithEnvironment("SentrySettings__OrganizationName", builder.AddParameterFromConfiguration("sentry-organization-name", "Parameters:SentrySettings__OrganizationName", secret: false))
    .WithEnvironment("SentrySettings__ProjectId", builder.AddParameterFromConfiguration("sentry-project-id", "Parameters:SentrySettings__ProjectId", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__0", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-0", "Parameters:SentrySettings__EventAggregationPeriods:0", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__1", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-1", "Parameters:SentrySettings__EventAggregationPeriods:1", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__2", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-2", "Parameters:SentrySettings__EventAggregationPeriods:2", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__3", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-3", "Parameters:SentrySettings__EventAggregationPeriods:3", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__4", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-4", "Parameters:SentrySettings__EventAggregationPeriods:4", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__5", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-5", "Parameters:SentrySettings__EventAggregationPeriods:5", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__6", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-6", "Parameters:SentrySettings__EventAggregationPeriods:6", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__7", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-7", "Parameters:SentrySettings__EventAggregationPeriods:7", secret: false))
    .WithEnvironment("SentrySettings__EventAggregationPeriods__8", builder.AddParameterFromConfiguration("sentry-event-aggregation-periods-8", "Parameters:SentrySettings__EventAggregationPeriods:8", secret: false))
    .WithReference(redisCache)
    // .WithReference(blobs)
    .WaitFor(redisCache)
    // .WaitFor(localstack)
    ;

builder.AddProject<Projects.MemQuranDashboard_Api>("memquran-dashboard-api")
    // if you get port conflict, make sure the projects launchSettings.json is using the environmentVariables__ASPNETCORE_URLS (not applicationUrl)
    .WithHttpEndpoint(port: 5147, targetPort: 5147, isProxied: false, name: "memquran-dashboard-api-http")
    .WithHttpsEndpoint(port: 7161, targetPort: 7161, isProxied: false, name: "memquran-dashboard-api-https")
    .WithExternalHttpEndpoints()
    .WithReference(redisCache)
    .WaitFor(redisCache)
    // .WaitFor(localstack)
    ;

builder.Build().Run();
