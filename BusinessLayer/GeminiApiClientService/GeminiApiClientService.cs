using BusinessLayer.Models.ContentRequest;
using BusinessLayer.Models.ContentResponse;
using DataAccessLayer;
using Newtonsoft.Json;
using System.Text;

namespace BusinessLayer.Client;

public class GeminiApiClientService
{
    private readonly ISkillsRepository _skillsRepository;
    private readonly IRoomsRepository _roomsRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly HttpClient _httpClient;
    // this the user id of the AI user in the database 
    // please don't change it
    private readonly int AI_USER_ID = 9;
    string _apiKey;

    public GeminiApiClientService(
        ISkillsRepository skillsRepository,
        IRoomsRepository roomsRepository,
        IMessagesRepository messagesRepository)
    {
        //_apiKey = key;
        _httpClient = new HttpClient();
        _skillsRepository = skillsRepository;
        _roomsRepository = roomsRepository;
        _messagesRepository = messagesRepository;
    }

    public async Task<string> SuggestOpenSourceProjectsAsync(int userId, string message = null)
    {
        var prompt = await GeneratePrompt(message, userId);

        var aiResponse = await SendToAi(prompt);

        var newMessage = new MessageDto(1, AI_USER_ID, userId, aiResponse, DateTime.Now, null);
        await _messagesRepository.AddNewMessageAsync(newMessage);

        return aiResponse;
    }

    private async Task<string> GeneratePrompt(string message, int userId)
    {
        if (message != null)
        {
            var newMessage = new MessageDto(1, userId, AI_USER_ID, message, DateTime.Now, null);
            await _messagesRepository.AddNewMessageAsync(newMessage);
            return message;
        }

        var prompt = "Given the following developer profile, suggest suitable open-source projects for contribution. " +
            "Consider factors like skill level, preferred languages, technologies, areas of interest, " +
            "and available time commitment.\"\r\n\r\n" +
            "Developer Profile:\r\n\r\n" +
            "Skill Level: <skill_level>\r\n" +
            "Preferred Languages & Technologies: <preferred_languages>\r\n" +
            "Areas of Interest: <areas_of_interest>\r\n" +
            "Time Commitment: <time_commitment>\r\n" +
            "Provide project recommendations in HTML format " +
            "with details like project name, description, GitHub repository link, difficulty level," +
            " and how the developer can get started." +
            "do what did ask with no fluff";

        var skills = await _skillsRepository.GetSkillsByUserIdAsync(userId);
        var skillsNames = string.Join(", ", skills.Select(x => x.Name));

        var newPrompt = prompt.Replace("<skill_level>", "Beginner")
        .Replace("<preferred_languages>", skillsNames)
        .Replace("<areas_of_interest>", "Web Development")
        .Replace("<time_commitment>", "2-4 hours per week");


        return newPrompt;
    }

    private async Task<string> SendToAi(string message)
    {
        var key = "AIzaSyCt5d2Jzwr25ihK4y-8dIj0nlsMZb-jqSM";

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={key}";

        var request = new ContentRequest
        {
            contents =
            [
                new Models.ContentRequest.Content
                {
                    parts =
                    [
                        new Models.ContentRequest.Part
                        {
                            text = message
                        }
                    ]
                }
            ]
        };

        string jsonRequest = JsonConvert.SerializeObject(request);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var geminiResponse = JsonConvert.DeserializeObject<ContentResponse>(responseContent);
            return geminiResponse.Candidates[0].Content.Parts[0].Text;
        }
        else
        {
            throw new Exception($"Failed to generate content. Status code: {response.StatusCode}");
        }
    }

    public async Task<string> SuggestProjectForRoomAsync(int userId, string message = null)
    {
        var prompt = await GenerateRoomPrompt(message, userId);

        var aiResponse = await SendToAi(prompt);

        var newMessage = new MessageDto(1, AI_USER_ID, userId, aiResponse, DateTime.Now, null);
        await _messagesRepository.AddNewMessageAsync(newMessage);

        return aiResponse;
    }
    private async Task<string> GenerateRoomPrompt(string message, int userId)
    {
        if (message != null)
        {
            var newMessage = new MessageDto(1, userId, AI_USER_ID, message, DateTime.Now, null);
            await _messagesRepository.AddNewMessageAsync(newMessage);
            return message;
        }

        var users = await _roomsRepository.GetRoomUsersAsync(userId);
        var prompt = "Given the following team profile, suggest an appropriate open-source project for contribution. Analyze the team members' skills, preferred languages, technologies, and interests to determine suitable roles for each member. Additionally, create a realistic time plan with milestones for project completion.\r\n\r\n";

        int memberIndex = 1;
        foreach (var user in users)
        {
            var userSkills = await _skillsRepository.GetSkillsByUserIdAsync(user.UserId);
            var skillsNames = string.Join(", ", userSkills.Select(x => x.Name));

            var tempPrompt = $"Team Member {memberIndex}:\r\n\r\n" +
                      $"Skill Level: <skill_level>\r\n" +
                      $"Preferred Languages & Technologies: <preferred_languages>\r\n" +
                      $"Areas of Interest: <areas_of_interest>\r\n" +
                      $"Time Commitment: <time_commitment>\r\n\r\n";


            prompt += tempPrompt
                .Replace("<skill_level>", "Beginner")
                .Replace("<preferred_languages>", skillsNames)
                .Replace("<areas_of_interest>", "Web Development")
                .Replace("<time_commitment>", "2-4 hours per week");

            memberIndex++;
        }

        prompt += "Provide project recommendations in HTML format";

        return prompt;
    }
}