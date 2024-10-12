using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TerrainMap.Models;
using TerrainMap.Services.Interface;

namespace TerrainMap.Services;

public class TerrainAchievementService(ITerrainApiClient terrainClient) : ITerrainAchievementService
{
    const string GetAchievementUrl = "https://achievements.terrain.scouts.com.au/members/{0}/achievements/{1}";
    const string ActionAchievementUrl = "https://achievements.terrain.scouts.com.au/submissions/{0}/assessments";

    async Task<Achievement> GetAchievement(string memberId, string achievementId)
    {
        var url = string.Format(GetAchievementUrl, memberId, achievementId);
        var response = await terrainClient.SendGet<Achievement>(url);

        return response;
    }

    public async Task<Achievement> GetAchievement(Approval approval)
        => await GetAchievement(approval.Member.Id, approval.Achievement.Id);

    public async Task ApproveSubmission(Submission submission, string comment)
        => await ActionSubmission(submission, "approved", comment);

    public async Task ImproveSubmission(Submission submission, string comment)
        => await ActionSubmission(submission, "rejected", comment);

    public async Task AwardSubmission(Submission submission, DateTime awardedAt)
    {
        var awardAssessment = new AwardAssessment("awarded", awardedAt.ToString("yyyy-MM-dd"));
        var url = string.Format(ActionAchievementUrl, submission.Id);

        await terrainClient.SendPostVoid(url, awardAssessment);
    }

    async Task ActionSubmission(Submission submission, string outcome, string comment)
    {
        var assessment = new Assessment(outcome, comment);
        var url = string.Format(ActionAchievementUrl, submission.Id);
        
        await terrainClient.SendPostVoid(url, assessment);
    }

    record Assessment(
        [property: JsonPropertyName("outcome")] string Outcome,
        [property: JsonPropertyName("comment")] string Comment);

    record AwardAssessment(
        [property: JsonPropertyName("outcome")] string Outcome,
        [property: JsonPropertyName("date_awarded")] string DateAwarded);
}
