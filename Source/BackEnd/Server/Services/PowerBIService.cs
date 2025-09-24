using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class PowerBIService
{
    private readonly string tenantId = "YOUR_TENANT_ID";
    private readonly string clientId = "YOUR_CLIENT_ID";
    private readonly string clientSecret = "YOUR_CLIENT_SECRET";
    private readonly string workspaceId = "YOUR_WORKSPACE_ID";
    private readonly string powerBiApiUrl = "https://api.powerbi.com/";
    private readonly string authorityUrl = "https://login.microsoftonline.com/YOUR_TENANT_ID/oauth2/token";

    public async Task<string> GetAccessTokenAsync()
    {
        using (var client = new HttpClient())
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("resource", powerBiApiUrl)
            });

            var response = await client.PostAsync(authorityUrl, content);
            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return tokenResponse["access_token"];
        }
    }

    public async Task<EmbedToken> GetReportEmbedTokenAsync(string reportId)
    {
        string accessToken = await GetAccessTokenAsync();

        using (var client = new PowerBIClient(new Uri(powerBiApiUrl), new TokenCredentials(accessToken, "Bearer")))
        {
            
            var report = await client.Reports.GetReportInGroupAsync(Guid.Parse(workspaceId), Guid.Parse(reportId));
            var datasetId = report.DatasetId;

            var embedRequest = new GenerateTokenRequestV2
            {
                Reports = new List<GenerateTokenRequestV2Report> { new GenerateTokenRequestV2Report(report.Id) },
                Datasets = new List<GenerateTokenRequestV2Dataset> { new GenerateTokenRequestV2Dataset(datasetId) },
                Identities = new List<EffectiveIdentity>()
            };

            var embedToken = await client.EmbedToken.GenerateTokenAsync(embedRequest);
            return embedToken;
        }
    }
}
