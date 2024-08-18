using System.Text.Json;
using Hypercube.Dependencies;
using HypercubeBot.Schemas;
using HypercubeBot.ServiceRealisation;

namespace HypercubeBot.Services;

[Service]
public class GithubService
{
    [Dependency] private readonly HttpService _httpService = default!;
    private const string GithubUrl = "https://api.github.com/repos/";

    public async Task<Contributor[]?> GetContributors(string uri)
    {
        var response = await _httpService.GetAsync($"{GithubUrl}{uri}/contributors");
        Contributor[]? contributors = null;

        try
        {
            contributors = JsonSerializer.Deserialize<Contributor[]>(response) ?? [];
        }
        catch (Exception)
        {
            // ignored
        }

        return contributors;
    }
}