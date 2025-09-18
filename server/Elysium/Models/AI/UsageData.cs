using Newtonsoft.Json;

namespace Elysium.Models.AI;

public record struct UsageData
{
    public struct PromptTokensDetails
    {
        [JsonProperty("cached_tokens")] 
        public int CachedTokens { get; set; }
    }

    public struct CompletionTokensDetails
    {
        [JsonProperty("reasoning_tokens")] 
        public int ReasoningTokens { get; set; }
    }
        
    [JsonProperty("completion_tokens")] 
    public int CompletionTokens{ get; set; }
        
    [JsonProperty("prompt_tokens")] 
    public int PromptTokens{ get; set; }
        
    [JsonProperty("total_tokens")] 
    public int TotalTokens{ get; set; }

    [JsonProperty("prompt_tokens_details")]
    public PromptTokensDetails PTDetails{ get; set; }

    [JsonProperty("completion_tokens_details")]
    public CompletionTokensDetails CTDetails{ get; set; }
        
    public override string ToString()
    {
        return $"""
                Usage Details:
                Completion Tokens: {CompletionTokens}
                Prompt Tokens: {PromptTokens}
                Total Tokens: {TotalTokens}
                Cached Tokens: {PTDetails.CachedTokens}
                Reasoning Tokens: {CTDetails.ReasoningTokens}
                """;
    }
}