using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class TwentyQuestions : MonoBehaviour
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button dontKnowButton;
    [SerializeField] private Button restart;
    [SerializeField] private TMPro.TMP_Text questionLabel;
    [SerializeField] private TMPro.TMP_Text roundLabel;
    [SerializeField] private TMPro.TMP_Text userScore;
    [SerializeField] private AudioSource correctAnswerAudioSource;
    [SerializeField] private AudioSource wrongAnswerAudioSource;

    private static string apiKey = APIKeys.OpenAIKey;
    private OpenAIApi openai = new OpenAIApi(apiKey);
    private List<ChatMessage> messages = new List<ChatMessage>();
    private static string prompt = "You are a game host. Play 20 questions with me. Return your answer entirely in the form of a JSON object. The JSON object should have a key named \"question\".  Don't include anything other than the JSON. If you the user guessed it the value of  \"question\" should be \"correct\". Do not include any explanation.";
    private string question;

    private int round = 0;

    async void Start()
    {
        restart.onClick.AddListener(Restart);
        yesButton.onClick.AddListener(delegate { ButtonClick("yes"); });
        noButton.onClick.AddListener(delegate { ButtonClick("no"); });
        dontKnowButton.onClick.AddListener(delegate { ButtonClick("I don't know"); });
        await CallOpenAiToGetValues(prompt);
    }

    private async void ButtonClick(string value)
    {
        await CallOpenAiToGetValues(value);
    }


    private void Restart()
    {
        SceneManager.LoadScene("Twenty Questions");
    }

    private async Task CallOpenAiToGetValues(string content)
    {
        if (round == 20)
        {
            questionLabel.text = "Sorry I couldn't get your quess in 20 questions!";
            return;
        }
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = content
        };

        if (messages.Count == 0) newMessage.Content = prompt;
        messages.Add(newMessage);
        Debug.Log("prompt=" + newMessage.Content);
        //https://github.com/quentin-mckay/AI-Quiz-Generator?tab=readme-ov-file
        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo-0613",
            Messages = messages
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            question = message.Content = message.Content.Trim();

            Debug.Log(question);
            messages.Add(message);
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
        var twentyQuestionData = JsonConvert.DeserializeObject<TwentyQuestionData>(question);
        if (twentyQuestionData.question.ToLower() == "correct")
        {
            questionLabel.text = "WOOHOO!";
            return;
        }

        questionLabel.text = twentyQuestionData.question;

        round++;
        roundLabel.text = string.Format("Question {0}:", round);
    }
}
