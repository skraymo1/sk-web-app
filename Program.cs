global using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;

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

app.MapPost("chain", async (HttpContext httpContext, Query query) =>
        {
            try
            {
                var context = new SKContext();
                context["input"] = query.Value!;
                context["options"] = "Sqrt, Add";

                var kernel = LoadKernel(httpContext);
                var pluginDirectory = "Plugins";
                // var departmentFunction = kernel.Skills.GetFunction(pluginDirectory, "DepartmentRouter");
                // var detectIntentFunction = kernel.Skills.GetFunction(pluginDirectory, "DetectIntent");
                // string prompt1 = @"Translate {{$input}} to Engligh";

                // string prompt2 = @"{{$input}}
                //         DETERMINE THE INTENT OF THE TEXT
                //         RETURN VALUES ARE POSITIVE, NEGATIVE, OR NEUTRAL";

                // string prompt3 = @"{{$input}}
                //         DETERMINE THE DEPARTMENT FROM THE INPUT
                //         POSSIBLE DEPARTMENT ARE:
                //         HOUSEWARES
                //         FOOD
                //         BOOKS
                //         ELECTRONICS
                //         RETURN 
                //         Message: {{$original_input}}
                //         Department:"; 

                var plugInFunctions = kernel.ImportSemanticSkillFromDirectory(pluginDirectory, "OrchestratorPlugin");
                var result = await plugInFunctions["GetIntent"].InvokeAsync(context);
                
                // string intent = context["input"].Trim();
                // return Results.Json(new SKResponse { Value = intent });




                // var result = await kernel.RunAsync(context, prompt2Function, prompt3Function);
                SKResponse response = new SKResponse();
                response.Value = result.Result.Trim();
                return Results.Json(response);

            }
            catch (Exception ex)
            {
                return Results.Json(new SKResponse { Value = ex.Message });
            }

        });



//This is a good example of calling skills from within skills
app.MapPost("variables", async (HttpContext context, Query query) =>
        {
            try
            {
                var kernel = LoadKernel(context);
                // var pluginDirectory = "Plugins";
                // var departmentFunction = kernel.Skills.GetFunction(pluginDirectory, "DepartmentRouter");
                // var detectIntentFunction = kernel.Skills.GetFunction(pluginDirectory, "DetectIntent");
                string departmentPrompt = @"{{$input}}
                        DETERMINE THE DEPARTMENT FROM THE INPUT
                        POSSIBLE DEPARTMENT ARE:
                        HOUSEWARES
                        FOOD
                        BOOKS
                        ELECTRONICS";

                string detectIntentPrompt = @"{{$input}}
                        DETERMINE THE INTENT OF THE TEXT
                        RETURN VALUES ARE POSITIVE, NEGATIVE, OR NEUTRAL";

                var departmentRouter = kernel.CreateSemanticFunction(departmentPrompt);
                var detectIntent = kernel.CreateSemanticFunction(detectIntentPrompt);


                var result = await kernel.RunAsync(query.Value!, departmentRouter, detectIntent);
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