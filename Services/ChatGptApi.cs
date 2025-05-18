using JsonConvert = Newtonsoft.Json.JsonConvert; // to avoid nullability problem
using System.Text;
using CommunityToolkit.Maui.Alerts;

using StudyApp.Models;
using StudyApp.Data;
using System.Net.Http.Headers;


namespace StudyApp.Services
{
    public class ChatGptService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

        public ChatGptService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserSettingsCopy.ApiKey);
        }

        public void UpdateApiKey(string newApiKey)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", newApiKey);
        }

        // Function to convert image to Base64
        public string ConvertImageToBase64(string imagePath)
        {
            byte[] imageArray = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageArray);
        }

        public async Task<string> FileResultToBase64(FileResult imageFile)
        {
            if (imageFile != null)
            {
                using var stream = await imageFile.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
            return string.Empty;
        }

        private object PreparePayload(string requestType, string userInput, string model, string? base64Image = null)
        {
            // Prepare content array for "user" message
            var userContent = new List<object>
            {
                new { type = "text", text = userInput }
            };

            // If a Base64 image is provided, add it as a separate object within the content
            if (!string.IsNullOrEmpty(base64Image))
            {
                userContent.Add(new
                {
                    type = "image_url",
                    image_url = new { url = $"data:image/jpeg;base64,{base64Image}" }
                });
            }

            // Create messages list with the system request type and user input encapsulated in appropriate structure
            var messages = new List<object>
            {
                new { role = "system", content = requestType },
                new { role = "user", content = userContent }
            };

            // Create and return the request payload
            var requestBody = new
            {
                model, // The model to be used (e.g., "gpt-4o-mini", "gpt-3.5-turbo")
                messages = messages.ToArray(),
                max_tokens = 100
            };

            return requestBody;
        }


        public async Task<HttpResponseMessage> GenerateResponseAsync(string requestType, string userInput, string model, string? base64Image = null)
        { // or argument //string? imagePath = null ?
            var requestBody = PreparePayload(requestType, userInput, model, base64Image);
            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            try
            {
                return await _httpClient.PostAsync(ApiUrl, jsonContent);
            }
            catch (HttpRequestException httpEx)
            {
                throw new HttpRequestException($"Network error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception occurred: {ex.Message}");
            }
        }

        public async Task<string> GenerateAiAnswerAsync(string requestType, string userInput, string model, string? base64Image = null)
        {
            try
            {
                var response = await GenerateResponseAsync(requestType, userInput, model, base64Image);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ChatGptResponse>(jsonResponse);
                    if (!string.IsNullOrWhiteSpace(responseObject?.choices?[0]?.message?.content))
                    {
                        return responseObject.choices[0].message.content;
                    }
                    else
                    {
                        await Toast.Make("No response from GPT.").Show();
                        return "";
                    }
                }
                else
                {
                    await Toast.Make($"Error: {response.StatusCode} - Could not retrieve response.").Show();
                    return "";
                }
            }
            catch (HttpRequestException httpEx)
            {
                await Toast.Make(httpEx.Message).Show();
                return "";
            }
            catch (Exception ex)
            {
                await Toast.Make(ex.Message).Show();
                return "";
            }
        }

        public async Task<(string originalText, string translatedText)> GenerateAiTranslationAsync(string userInput, string model, string? base64Image = null, string? requestType = null)
        {
            if (requestType == null)
                requestType =
                    "You are a translation assistant. Your task is to translate **only** the underlined words and nothing else. " +
                    $"Ignore everything except the text underlined in red. Translate it into **{UserSettingsCopy.NativeLanguage}**. " +
                    "Respond in strict JSON format with exactly two messages: " +
                    "{\"messages\": [{\"role\": \"assistant\", \"content\": \"<original text>\"}, " +
                    "{\"role\": \"assistant\", \"content\": \"<translated text>\"}]}. " +
                    "Do not add explanations, do not translate anything else, do not alter the format.";

            try
            {
                var response = await GenerateResponseAsync(requestType, userInput, model, base64Image);

                if (!response.IsSuccessStatusCode)
                {
                    await Toast.Make($"Translation failed due to a network or API error. Error: {response.StatusCode}").Show();
                    return ("Translation failed.", $"");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Try parsing the response
                var responseObject = JsonConvert.DeserializeObject<ChatGptResponse>(jsonResponse);
                if (responseObject?.choices == null || responseObject.choices.Length == 0 || responseObject.choices[0]?.message?.content == null)
                {
                    await Toast.Make("Unexpected API response format received from GPT.").Show();
                    return ("Translation failed.", $"");
                }

                string messageContent = responseObject.choices[0].message.content;
                var result = JsonConvert.DeserializeObject<ChatGptTranslation>(messageContent);

                if (result?.messages != null && result.messages.Count >= 2)
                {
                    string originalText = result.messages[0].content ?? "";
                    string translatedText = result.messages[1].content ?? "";
                    return (originalText, translatedText);
                }
                else
                {
                    await Toast.Make($"Unexpected API response format received from GPT. Error: {messageContent}").Show();
                    return ("Translation failed.", $"");
                }
            }

            catch (HttpRequestException httpEx)
            {
                await Toast.Make($"Translation failed due to network error. Error: {httpEx.Message}").Show();
                return ("Translation failed.", $"");
            }
            catch (Exception ex)
            {
                await Toast.Make($"Translation failed due to an unexpected error. Error: {ex.Message}").Show();
                return ("Translation failed.", $"");
            }
        }


    }
}
