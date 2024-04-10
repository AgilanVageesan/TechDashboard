using System.Text.Json;

static async Task Main(string[] args)
{
    string orgName = "your_organization_name";
    string apiUrl = $"https://your-github-enterprise-url/api/v3/orgs/{orgName}/repos";
    string gitHubToken = "your_github_token";

    try
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", gitHubToken);
            client.DefaultRequestHeaders.Add("User-Agent", "C# Console App");

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                List<GitHubRepo> repos = JsonSerializer.Deserialize<List<GitHubRepo>>(responseBody);

                foreach (GitHubRepo repo in repos)
                {
                    Console.WriteLine($"Repo: {repo.Name}");

                    // Get repository information to find the default branch
                    string repoInfoUrl = $"https://your-github-enterprise-url/api/v3/repos/{orgName}/{repo.Name}";
                    HttpResponseMessage infoResponse = await client.GetAsync(repoInfoUrl);

                    if (infoResponse.IsSuccessStatusCode)
                    {
                        string infoBody = await infoResponse.Content.ReadAsStringAsync();
                        GitHubRepoInfo repoInfo = JsonSerializer.Deserialize<GitHubRepoInfo>(infoBody);
                        string defaultBranch = repoInfo.DefaultBranch;

                        // Get repository contents for the default branch
                        string repoContentsUrl = $"https://your-github-enterprise-url/api/v3/repos/{orgName}/{repo.Name}/contents/?ref={defaultBranch}";
                        HttpResponseMessage contentsResponse = await client.GetAsync(repoContentsUrl);

                        if (contentsResponse.IsSuccessStatusCode)
                        {
                            string contentsBody = await contentsResponse.Content.ReadAsStringAsync();
                            List<GitHubFile> files = JsonSerializer.Deserialize<List<GitHubFile>>(contentsBody);

                            // Filter for .csproj file
                            GitHubFile csprojFile = files.FirstOrDefault(f => f.Name.EndsWith(".csproj"));

                            if (csprojFile != null)
                            {
                                // Get .csproj file content
                                string csprojUrl = csprojFile.DownloadUrl;
                                HttpResponseMessage csprojResponse = await client.GetAsync(csprojUrl);

                                if (csprojResponse.IsSuccessStatusCode)
                                {
                                    string csprojContent = await csprojResponse.Content.ReadAsStringAsync();
                                    string frameworkVersion = ExtractFrameworkVersion(csprojContent);
                                    Console.WriteLine($".NET Framework Version: {frameworkVersion}");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to fetch .csproj file content for {repo.Name}. Status code: {csprojResponse.StatusCode}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No .csproj file found.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to fetch repository contents for {repo.Name}. Status code: {contentsResponse.StatusCode}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to fetch repository information for {repo.Name}. Status code: {infoResponse.StatusCode}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch repos for {orgName}. Status code: {response.StatusCode}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
    static string ExtractFrameworkVersion(string csprojContent)
    {
        // Implement logic to extract .NET framework version from .csproj file content
        // Example: search for <TargetFramework> tag and extract the value
        // This part depends on the structure of your .csproj files
        // For example:
        int startTagIndex = csprojContent.IndexOf("<TargetFramework>");
        if (startTagIndex == -1)
        {
            return "Unknown";
        }
        int endTagIndex = csprojContent.IndexOf("</TargetFramework>", startTagIndex);
        if (endTagIndex == -1)
        {
            return "Unknown";
        }
        string frameworkVersion = csprojContent.Substring(startTagIndex + "<TargetFramework>".Length, endTagIndex - (startTagIndex + "<TargetFramework>".Length)).Trim();
        return frameworkVersion;
    }


}
public class GitHubRepo
{
    public string Name { get; set; }
    public string HtmlUrl { get; set; } // URL to the repository on GitHub
    public string Description { get; set; } // Description of the repository
}

public class GitHubRepoInfo
{
    public string DefaultBranch { get; set; }
    public string HtmlUrl { get; set; } // URL to the repository on GitHub
    public string Description { get; set; } // Description of the repository
    // You can add more properties here if needed
}

public class GitHubFile
{
    public string Name { get; set; }
    public string DownloadUrl { get; set; }
    public string HtmlUrl { get; set; } // URL to view the file on GitHub
    public string Path { get; set; } // Path of the file in the repository
    public string Type { get; set; } // Type of the file (e.g., "file" or "dir")
    public int Size { get; set; } // Size of the file in bytes
    // You can add more properties here if needed
}



