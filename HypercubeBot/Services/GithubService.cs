using System.Text.Json;
using Hypercube.Dependencies;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class GithubService
{
    [Dependency] private HttpService _httpService = default!;
    private static readonly string _githubUrl = "https://api.github.com/repos/";
    
    public async Task<Contributor[]?> GetContributors(string owner, string repo)
    {
        var response = await _httpService.GetAsync($"{_githubUrl}{owner}/{repo}/contributors");
        Contributor[]? contributors = null;

        try
        {
            contributors = JsonSerializer.Deserialize<Contributor[]>(response) ?? [];
        }
        catch (Exception e)
        {}
        
        return contributors;
    }
}