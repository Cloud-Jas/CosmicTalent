var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer("cache");

var talentprocessor = builder.AddProject<Projects.CosmicTalent_TalentProcessor>("talentprocessor");

builder.AddProject<Projects.CosmicTalent_ChatApp>("chatapp")
    .WithReference(cache)
    .WithReference(talentprocessor.GetEndpoint("http"));

builder.AddProject<Projects.CosmicTalent_WorkerService>("workerservice");

builder.Build().Run();
