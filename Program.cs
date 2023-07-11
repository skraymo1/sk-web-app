
global using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("plugins/{pluginName}/invoke/{functionName}", async (HttpContext context, Query query, string pluginName, string functionName) =>
        {
            try
            {
                var kernel = new KernelHelper().CreateAzureOpenAIKernel(context.Request.Headers);

                var skillsDirectory = "Plugins";

                var funSkillFunctions = kernel!.ImportSemanticSkillFromDirectory(skillsDirectory, pluginName);

                var result = await funSkillFunctions[functionName].InvokeAsync(query.Value);
                SKResponse response = new SKResponse();
                response.Value = result.Result.Trim();
                return Results.Json(response);
            }
            catch (Exception ex)
            {
                return Results.Json(new SKResponse { Value = ex.Message });
            }

        });

app.Run();
