
public class KernelHelper
{
    public IKernel? CreateAzureOpenAIKernel(IHeaderDictionary headers)
    {
        try
        {
            var model = headers["x-sk-web-app-model"];
            var endpoint = headers["x-sk-web-app-endpoint"];
            var key = headers["x-sk-web-app-key"];
            if (String.IsNullOrEmpty(model) || String.IsNullOrEmpty(endpoint) || String.IsNullOrEmpty(key))
            {
                throw new Exception("Missing required headers");
            }

            return new KernelBuilder()
                .WithAzureTextCompletionService(model!, endpoint!, key!)
                .Build();
        }
        catch
        {
            throw new Exception("Error creating kernel");
        }

    }
}

public class SKResponse
{
    public string? Value { get; set; }
}

public class Query
{
    public string? Value { get; set; }
}
