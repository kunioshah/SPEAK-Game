using System.Collections.Generic;
using OpenAI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections;

public class ChooseCategory : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private bool useAI;
    [SerializeField] string difficultyLevel = "easy";
    [SerializeField] private TMPro.TMP_Text loading;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsClose;
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private TMPro.TMP_Text sliderValue;
    [SerializeField] private TMPro.TMP_InputField numberOfQuestions;
    [SerializeField] private Toggle playSounds;
    private static string apiKey = APIKeys.OpenAIKey;
    private OpenAIApi openai = new OpenAIApi(apiKey);
   

    private List<ChatMessage> messages = new List<ChatMessage>();
    //https://github.com/quentin-mckay/AI-Quiz-Generator?tab=readme-ov-file
    private string prompt =  "Give me {0} multiple choice questions about a random topic in {1}. The questions should be at an {2} level. Return your answer entirely in the form of a JSON object. The JSON object should have a key named \"questions\" which is an array of the questions. Each quiz question should include the choices, the answer, and a brief explanation of why the answer is correct. Don't include anything other than the JSON. The JSON properties of each question should be \"query\" (which is the question), \"choices\", \"answer\", and \"explanation\". The choices shouldn't have any ordinal value like A, B, C, D or a number like 1, 2, 3, 4. Please make sure the correct answer is random. Make sure one answer includes the word panda. Do not include any explanation.";
   
    void Start()
    {
        numberOfQuestions.text = "5";
        button.onClick.AddListener(SubmitForm);
        settingsClose.onClick.AddListener(CloseSettings);
        //difficultySlider.onClick.AddListener(DifficultySliderClick);
        loading.gameObject.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }
    private void CloseSettings()
    {
        GameVariables.PlaySounds = playSounds.isOn;
        settingsPanel.SetActive(false);
    }

    public void DifficultySliderClick()
    {
        switch (difficultySlider.value)
        {
            case 1:
                difficultyLevel = sliderValue.text = "Medium";
                break;
            case 2:
                difficultyLevel = sliderValue.text = "Difficult";
                break;
            default:
                difficultyLevel = sliderValue.text = "Easy";
                break;
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return)) { SubmitForm(); }
    }

    private void SubmitForm()
    {
        GameVariables.CategroryName = inputField.text;
        var task = CallOpenAiToGetValues();
    }
    private async Task CallOpenAiToGetValues()
    {
        string questions = null;
        string inputValue = inputField.text;
        
        if (useAI)
        {
            loading.gameObject.SetActive(true);
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputValue
            };
            prompt = string.Format(prompt, numberOfQuestions.text, inputValue, difficultyLevel);
            Debug.Log("prompt=" + prompt);
            if (messages.Count == 0) newMessage.Content = prompt;

            messages.Add(newMessage);

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                questions = message.Content = message.Content.Trim();
                Debug.Log(questions);
                messages.Add(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
        else
        {
            questions = "{\"questions\":[{\"query\":\"Which player has won the most Ballon d\'Or awards?\",\"choices\":[\"Lionel Messi\",\"Cristiano Ronaldo\",\"Diego Maradona\",\"Pele\"],\"answer\":\"Lionel Messi\",\"explanation\":\"Lionel Messi has won the Ballon d\'Or award 6 times, which is the most by any player in history.\"},{\"query\":\"Who holds the record for the most goals scored in a single FIFA World Cup tournament?\",\"choices\":[\"Just Fontaine\",\"Pele\",\"Gerd Muller\",\"Ronaldo\"],\"answer\":\"Just Fontaine\",\"explanation\":\"Just Fontaine of France holds the record for the most goals scored in a single FIFA World Cup tournament. He scored 13 goals in the 1958 World Cup.\"},{\"query\":\"Which club has won the most UEFA Champions League titles?\",\"choices\":[\"Real Madrid\",\"AC Milan\",\"Bayern Munich\",\"Liverpool\"],\"answer\":\"Real Madrid\",\"explanation\":\"Real Madrid has won the UEFA Champions League title 13 times, making them the most successful club in the history of the competition.\"},{\"query\":\"Who has scored the fastest hat-trick in Premier League history?\",\"choices\":[\"Sadio Mane\",\"Sergio Aguero\",\"Robbie Fowler\",\"Jermain Defoe\"],\"answer\":\"Sadio Mane\",\"explanation\":\"Sadio Mane of Liverpool holds the record for the fastest hat-trick in Premier League history, scoring three goals in just 2 minutes and 56 seconds against Aston Villa in 2015.\"},{\"query\":\"Which national team has won the most FIFA World Cup titles?\",\"choices\":[\"Brazil\",\"Germany\",\"Italy\",\"Argentina\"],\"answer\":\"Brazil\",\"explanation\":\"Brazil has won the FIFA World Cup title a record 5 times, making them the most successful national team in the history of the tournament.\"}]}";
        }

        GameVariables.QuizQuestionBank = JsonConvert.DeserializeObject<QuestionBank>(questions);
        SceneManager.LoadScene("Quiz Game");
        loading.gameObject.SetActive(false);
    }
}
