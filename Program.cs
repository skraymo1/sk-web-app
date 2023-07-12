global using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("plugins/{pluginName}/invoke/{functionName}", async (HttpContext context, Query query, string pluginName, string functionName) =>
        {
            try
            {

                var headers = context.Request.Headers;
                var endpoint = headers["x-sk-web-app-endpoint"];
                var model = headers["x-sk-web-app-model"];
                var key = headers["x-sk-web-app-key"];
                if (String.IsNullOrEmpty(model) || String.IsNullOrEmpty(key))
                {
                    throw new Exception("Missing required headers");
                }

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
