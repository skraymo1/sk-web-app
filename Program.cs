
global using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("plugins/{pluginName}/invoke/{functionName}", async (HttpContext context, Query query, string pluginName, string functionName) =>
        {
            try
            {
                var headers = context.Request.Headers;
                var model = headers["x-sk-web-app-model"];
                var endpoint = headers["x-sk-web-app-endpoint"];
                var key = headers["x-sk-web-app-key"];
                if (String.IsNullOrEmpty(model) || String.IsNullOrEmpty(endpoint) || String.IsNullOrEmpty(key))
                {
                    throw new Exception("Missing required headers");
                }

                var kernel =  new KernelBuilder()
                    .WithAzureTextCompletionService(model!, endpoint!, key!)
                    .Build();


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
