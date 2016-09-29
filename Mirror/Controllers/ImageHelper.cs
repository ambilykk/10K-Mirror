using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Mirror.Controllers
{
    public class ImageHelper
    {
        static string textSubscriptionKey = "23bf8fa933b04bcfac457c2299499c15";
        static string emotionSubscriptionKey = "7f3fe88d5e424a22855caa1472625c0e";
        static string visionSubscriptionKey = "ada889d064b04f42ad12f1b5c6ee6e6b";
              

        public static async Task<string> GetSentiments(string message)
        {
            try
            {
                var client = new HttpClient();
                // Request headers
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", textSubscriptionKey);

                byte[] msg = Encoding.UTF8.GetBytes("{\"documents\":[" +
                    "{\"id\":\"1\",\"text\":\"" + message + "\"}]}");

                var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";

                HttpResponseMessage response;

                using (var content = new ByteArrayContent(msg))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("text/json");
                    response = await client.PostAsync(uri, content);
                }
                var scoreData = response.Content.ReadAsStringAsync().Result;

                var scoreInfo = scoreData.Substring(scoreData.IndexOf(":", scoreData.IndexOf("score"))+1);
                scoreInfo = scoreInfo.Substring(0, scoreInfo.IndexOf(","));

                float score = float.Parse(scoreInfo) * 100;
                if (score > 45)
                {
                    return "Awesome. You sounds so happy. You have used many positive words in the message. Good to see that. Always be Positive.";
                }
                else
                {
                    return "Oh Dear! What happened? You sounds so unhappy. A Negative mind will never give you a Positive Life. Think Positive Be Positive.";
                }
            }catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public static async Task<string> AnalyzeEmotion(string fileName)
        {
            string result = string.Empty;

            try
            {
                AnalysisResult analyzeresult = null;
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    VisionServiceClient VisionServiceClient = new VisionServiceClient(visionSubscriptionKey);
                    VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Description, VisualFeature.Faces };
                    analyzeresult = await VisionServiceClient.AnalyzeImageAsync(stream, visualFeatures);
                }

                if (analyzeresult != null)
                {
                    if (analyzeresult.Faces.Count() == 0)
                    {
                        result = "Oh!  This doesn't look like your photo. I think the snap is  " + analyzeresult.Description.Captions.First().Text;
                    }
                    else if (analyzeresult.Faces.Count() == 1)
                    {
                        result = "You are a " + analyzeresult.Faces.First().Gender + " with approximately " + analyzeresult.Faces.First().Age + " years old. ";

                        EmotionServiceClient emotionServiceClient = new EmotionServiceClient(emotionSubscriptionKey);
                        FileStream fileStream = new FileStream(fileName, FileMode.Open);
                        Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(fileStream);
                        if (emotionResult.Count() > 0)
                        {
                            var scores = emotionResult.First().Scores;
                            result += " You looks so " + GetEmotion(scores) + "  today.";
                        }
                        fileStream.Close();
                    }
                    else
                    {
                        result = "How can I identify you from the group of  " + analyzeresult.Faces.Count() + " people?  ";
                        if (analyzeresult.Faces.Count(p => p.Gender == "Male") > 0)
                            result += " You have " + analyzeresult.Faces.Count(p => p.Gender == "Male") + " Male ";
                        if (analyzeresult.Faces.Count(p => p.Gender == "Female") > 0)
                        {
                            if (analyzeresult.Faces.Count(p => p.Gender == "Male") > 0)
                                result += " and ";
                            result += analyzeresult.Faces.Count(p => p.Gender == "Female") + " Female ";
                        }
                        result += " friends in the snap. Share only your snap.";
                       
                    }
                }
                return result;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        
        private static string GetEmotion(Scores scores)
        {
            var imgName = "Angry";
            if (scores.Anger > scores.Contempt && scores.Anger > scores.Disgust && scores.Anger > scores.Fear && scores.Anger > scores.Happiness &&
               scores.Anger > scores.Neutral && scores.Anger > scores.Sadness && scores.Anger > scores.Surprise)
            {
                imgName = "Angry";
            }
            else if (scores.Contempt > scores.Anger && scores.Contempt > scores.Disgust && scores.Contempt > scores.Fear && scores.Contempt > scores.Happiness &&
                       scores.Contempt > scores.Neutral && scores.Contempt > scores.Sadness && scores.Contempt > scores.Surprise)
            {
                imgName = "Contempt";
            }
            else if (scores.Disgust > scores.Anger && scores.Disgust > scores.Contempt && scores.Disgust > scores.Fear && scores.Disgust > scores.Happiness &&
                scores.Disgust > scores.Neutral && scores.Disgust > scores.Sadness && scores.Disgust > scores.Surprise)
            {
                imgName = "Disgust";
            }
            else if (scores.Fear > scores.Anger && scores.Fear > scores.Contempt && scores.Fear > scores.Disgust && scores.Fear > scores.Happiness &&
                scores.Fear > scores.Neutral && scores.Fear > scores.Sadness && scores.Fear > scores.Surprise)
            {
                imgName = "Fearful";
            }
            else if (scores.Happiness > scores.Anger && scores.Happiness > scores.Contempt && scores.Happiness > scores.Disgust && scores.Happiness > scores.Fear &&
                scores.Happiness > scores.Neutral && scores.Happiness > scores.Sadness && scores.Happiness > scores.Surprise)
            {
                imgName = "Happy";
            }
            else if (scores.Sadness > scores.Anger && scores.Sadness > scores.Contempt && scores.Sadness > scores.Disgust && scores.Sadness > scores.Fear &&
                scores.Sadness > scores.Neutral && scores.Sadness > scores.Happiness && scores.Sadness > scores.Surprise)
            {
                imgName = "Sad";
            }
            else if (scores.Surprise > scores.Anger && scores.Surprise > scores.Contempt && scores.Surprise > scores.Disgust && scores.Surprise > scores.Fear &&
                scores.Surprise > scores.Neutral && scores.Surprise > scores.Happiness && scores.Surprise > scores.Sadness)
            {
                imgName = "Surprised";
            }
            else
            {
                imgName = "Neutral";
            }
            return imgName;
        }
    }
    
}