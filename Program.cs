global using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("plugins/{pluginName}/invoke/{functionName}", async (HttpContext context, Query query, string pluginName, string functionName) =>
        {
            try
            {
                var (endpoint, model, key) = LoadHeaders(context);

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

                var pluginDirectory = "Plugins";

                var plugInFunctions = kernel!.ImportSemanticSkillFromDirectory(pluginDirectory, pluginName);

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

app.Run();


static (string? model, string? endpoint, string? key) LoadHeaders(HttpContext context)
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
        return (endpoint, model, key);
    }
}