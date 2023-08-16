global using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("plugins/{pluginName}/invoke/{functionName}", async (HttpContext httpContext, Query query, string pluginName, string functionName) =>
        {
            try
            {
                var kernel = LoadKernel(httpContext);

                var pluginDirectory = "Plugins";
                var plugInFunctions = kernel.ImportSemanticSkillFromDirectory(pluginDirectory, pluginName);

                var result = await plugInFunctions[functionName].InvokeAsync(query.Value);
                SKResponse response = new SKResponse();
                response.Value = result.Result.Trim();
                return Results.Json(response);

            }
            catch (Exception ex)
            {
                return Results.Json(new SKResponse { Value = ex.Message });
            }

        });


app.MapPost("planner", async (HttpContext httpContext, Query query) =>
        {
            try
            {
                var kernel = LoadKernel(httpContext);
                var planner = new SequentialPlanner(kernel);
                var pluginDirectory = "Plugins";
                var plugInFunctions = kernel.ImportSemanticSkillFromDirectory(pluginDirectory, pluginName);



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

static IKernel LoadKernel(HttpContext context)
{
    var headers = context.Request.Headers;
    string? endpoint = headers["x-sk-web-app-endpoint"];
    string? model = headers["x-sk-web-app-model"];
    string? key = headers["x-sk-web-app-key"];
    if (String.IsNullOrEmpty(model) || String.IsNullOrEmpty(key))
    {
        throw new Exception("Missing required headers");
    }
    else
    {
        IKernel kernel;
        //If endpoint is empty, this could be OpenAI
        if (String.IsNullOrEmpty(endpoint))
        {
            kernel = new KernelBuilder()
                .WithOpenAITextCompletionService(model!, key!)
                .Build();
        }
        else
        {
            kernel = new KernelBuilder()
                .WithAzureTextCompletionService(model!, endpoint!, key!)
                .Build();
        }
        return kernel;
    }
}