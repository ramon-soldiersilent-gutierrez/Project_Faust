using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Faust.AI
{
    [Serializable]
    public class GeminiRequest
    {
        public GeminiContent[] contents;
        public GeminiSystemInstruction systemInstruction;
        public GeminiGenerationConfig generationConfig;
    }

    [Serializable]
    public class GeminiContent
    {
        public string role;
        public GeminiPart[] parts;
    }

    [Serializable]
    public class GeminiSystemInstruction
    {
        public GeminiPart[] parts;
    }

    [Serializable]
    public class GeminiPart
    {
        public string text;
    }

    [Serializable]
    public class GeminiGenerationConfig
    {
        public float temperature;
        public string responseMimeType;
    }

    [Serializable]
    public class GeminiResponse
    {
        public GeminiCandidate[] candidates;
    }

    [Serializable]
    public class GeminiCandidate
    {
        public GeminiContent content;
    }

    public class GeminiClient
    {
        private const string API_URL_TEMPLATE = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={0}";
        private readonly string _apiKey;

        public GeminiClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> SendPromptAsync(string systemPrompt, string userPrompt, int timeoutSeconds)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini API Key is missing.");
            }

            var requestPayload = new GeminiRequest
            {
                contents = new[]
                {
                    new GeminiContent
                    {
                        role = "user",
                        parts = new[] { new GeminiPart { text = userPrompt } }
                    }
                },
                systemInstruction = new GeminiSystemInstruction
                {
                    parts = new[] { new GeminiPart { text = systemPrompt } }
                },
                generationConfig = new GeminiGenerationConfig
                {
                    temperature = 0.7f,
                    responseMimeType = "application/json"
                }
            };

            string jsonPayload = JsonUtility.ToJson(requestPayload);
            string url = string.Format(API_URL_TEMPLATE, _apiKey);

            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"Gemini API error: {request.error}");
                }

                // Parse the response
                string responseJson = request.downloadHandler.text;
                var geminiResponse = JsonUtility.FromJson<GeminiResponse>(responseJson);

                if (geminiResponse != null && geminiResponse.candidates != null && geminiResponse.candidates.Length > 0)
                {
                    return geminiResponse.candidates[0].content.parts[0].text;
                }

                throw new Exception("Gemini API returned an empty or invalid response structure.");
            }
        }
    }
}
