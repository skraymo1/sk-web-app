# sk-web-app
Semantic Kernel using minimal API

The Sematic Kernel is a great tool to help developers quickly integrate their codebases with OpenAI or Azure OpenAI models.  The SK team has provided wonderful samples in C# and python using functions, web apps and notebooks.  

[Semantic Kernel Overview](https://learn.microsoft.com/en-us/semantic-kernel/overview/)   
[Semantic Kernel Repo](https://github.com/microsoft/semantic-kernel)


The goal of this repository is to implement a C# minimal API using the Semantic Kernel SDK. 

## Getting Started

### Prerequisites
  
* API key from the [Azure OpenAI portal](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/how-to/create-resource?pivots=web-portal#create-a-resource)   
OR
* An [Azure subscription](https://azure.microsoft.com/free/)
* Azure OpenAI subscription.  Access to the Azure OpenAI portal is by application only.  
Apply for access with this [form](https://aka.ms/oai/access?azure-portal=true)  
* Deployed model to reference in [Azure OpenAI Studio](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/how-to/create-resource?pivots=web-portal#deploy-a-model)   
* Postman or other application to call endpoints.

## Usage

Clone this repository to your local machine.

Open the solution in your developer environment ([Visual Studio Code](https://code.visualstudio.com/), Visual Studio, etc.).

With a minimal API, the POST endpoint is simply defined in the program.cs file.
```C#
app.MapPost("plugins/{pluginName}/invoke/{functionName}", async (HttpContext context, Query query, string pluginName, string functionName) =>
```   
The path specifiesis a "pluginName" and a "functionName" to execute.   
As an example, a call to this endpoint would be "/plugins/FunPlugin/invoke/Joke".  In the project FunPlugin is the directory which contains the Joke plugin.   

So... what are plugins?   
Plugins are interoperable, [Open AI standards based](https://platform.openai.com/docs/plugins/getting-started/), encapsulated AI capabilities.

Clear?   

How about, a plugin is a simple implementation of an AI task that can be shared in the future with other copilots.   
Within the Semantic Kernel repo are several examples of [plugins](https://github.com/microsoft/semantic-kernel/tree/main/samples/skills) (note: plugins used to be skills).   

Here is an example of the Joke plugin from this repo.
```Text
WRITE EXACTLY ONE JOKE or HUMOROUS STORY ABOUT THE TOPIC BELOW

JOKE MUST BE:
- G RATED
- WORKPLACE/FAMILY SAFE
NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY

BE CREATIVE AND FUNNY. I WANT TO LAUGH.

+++++

{{$input}}
+++++
```

The skprompt.txt file a simple text file defining the natural language prmopt that will be sent to the AI service.   Pair that with the config.json file which provides configuration information to the [planner](https://learn.microsoft.com/en-us/semantic-kernel/ai-orchestration/planner?tabs=Csharp) and you have created a [semantic function](https://learn.microsoft.com/en-us/semantic-kernel/ai-orchestration/semantic-functions?tabs=Csharp).   



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

