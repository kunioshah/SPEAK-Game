using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using OpenAI;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
public class PlayAudio : MonoBehaviour
{
    [SerializeField] private Button buttonValue1;
    [SerializeField] private Button buttonValue2;
    [SerializeField] private Button buttonValue3;
    [SerializeField] private Button buttonValue4;
    [SerializeField] private TMPro.TMP_Text value1;
    [SerializeField] private TMPro.TMP_Text value2;
    [SerializeField] private TMPro.TMP_Text value3;
    [SerializeField] private TMPro.TMP_Text value4;
    [SerializeField] private Image image;
    [SerializeField] private bool useAI = false;
    [SerializeField] private TMPro.TMP_Text userScore;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Toggle audioEnabled;
    [SerializeField] private Toggle imageEnabled;
    [SerializeField] private Button playAudio;
    [SerializeField] private Button playAgain;
    [SerializeField] private GameObject valuesContainer;
    [SerializeField] private GameObject playAgainContainer;
    [SerializeField] private GameObject playAudioContainer;
    [SerializeField] private AudioSource correctAnswerAudioSource;
    [SerializeField] private AudioSource wrongAnswerAudioSource;
    private int randomNumber = 1;
    private static readonly string apiKey = APIKeys.OpenAIKey;
    private OpenAIApi openai = new OpenAIApi(apiKey);
    private List<ChatMessage> messages = new List<ChatMessage>();
    private string prompt = "You are a API. Answer with json list of strings without root node. Do not include any explanation";
    private string filePath = "";
    private string speechUri = "https://api.openai.com/v1/audio/speech";
    private readonly string mp3Format = "mp3";
    private readonly string wavFormat = "wav";
    private string outputFormat;
    private byte[] audioByteArray;

    // Start is called before the first frame update
    void Start()
    {
        outputFormat = mp3Format;
#if UNITY_WEBGL
        {
            outputFormat = wavFormat;
        }
#endif

        filePath = Path.Combine(Application.persistentDataPath, "audio." + outputFormat);
        playAudio.onClick.AddListener(delegate { LoadAndPlayAudio(filePath); });
        buttonValue1.onClick.AddListener(delegate { ButtonValueClick(1); });
        buttonValue2.onClick.AddListener(delegate { ButtonValueClick(2); });
        buttonValue3.onClick.AddListener(delegate { ButtonValueClick(3); });
        buttonValue4.onClick.AddListener(delegate { ButtonValueClick(4); });
        playAgain.onClick.AddListener(PlayAgain);

        valuesContainer.SetActive(false);
        playAgainContainer.SetActive(false);
        playAudioContainer.SetActive(false);

        CallOpenAiToGetValues();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void ButtonValueClick(int buttonNumber)
    {
        if (buttonNumber == randomNumber)
        {
            userScore.text = "You are correct!";
            userScore.color = Color.green;
            correctAnswerAudioSource.Play();
        }
        else
        {
            userScore.text = "Sorry, Please try again!";
            userScore.color = Color.red;
            wrongAnswerAudioSource.Play();
        }

        playAgainContainer.SetActive(true);
    }

    private async void CallOpenAiToGetValues()
    {
        string messageValue = null;
        string inputValue = GameVariables.CategroryName;

        if (string.IsNullOrEmpty(inputValue))
        {
            Debug.Log("Category=" + inputValue);
            SceneManager.LoadScene("Choose Category");
            return;
        }

        if (useAI)
        {
            randomNumber = new System.Random().Next(1, 5);
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputValue
            };

            if (messages.Count == 0) newMessage.Content = prompt + "\n Give me 4 values of " + inputValue;

            messages.Add(newMessage);

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                messageValue = message.Content = message.Content.Trim();
                Debug.Log(messageValue);
                messages.Add(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
        else
        {
            messageValue = "[\"lion\", \"tiger\", \"giraffe\", \"goat\"]";

            ChatMessage chatMessage = new ChatMessage();
            chatMessage.Content = messageValue;
            messages.Add(chatMessage);
            var sprite = Resources.Load<Sprite>("Images/lion");
            image.sprite = sprite;
        }

        List<string> options = JsonConvert.DeserializeObject<List<string>>(messageValue);
        value1.text = options[0];
        value2.text = options[1];
        value3.text = options[2];
        value4.text = options[3];
        Debug.Log("random numer=" + randomNumber);
        if (useAI)
        {
            if (imageEnabled.isOn)
            {
                SendImageRequest(options[randomNumber - 1]);
            }

            if (audioEnabled.isOn)
            {
                StartCoroutine(GenerateSpeech(options[randomNumber - 1]));
            }
        }
    }


    private async void SendImageRequest(string prompt)
    {
        image.sprite = null;
        Debug.Log(prompt);

        var response = await openai.CreateImage(new CreateImageRequest
        {
            Prompt = prompt,
            Size = ImageSize.Size256
        });

        if (response.Data != null && response.Data.Count > 0)
        {
            using (var request = new UnityWebRequest(response.Data[0].Url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                request.SendWebRequest();

                while (!request.isDone) await Task.Yield();

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(request.downloadHandler.data);
                var sprite = Sprite.Create(texture, new Rect(0, 0, 256, 256), Vector2.zero, 1f);
                image.sprite = sprite;
            }
        }
        else
        {
            Debug.LogWarning("No image was created from this prompt.");
        }
    }

    private IEnumerator GenerateSpeech(string text)
    {
        var payload = new
        {
            model = "tts-1",
            input = text,
            voice = "echo",
            response_format = outputFormat,
            speed = 1f
        };

        // Convert the payload to a JSON string.
        string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.None);

        using (UnityWebRequest www = new UnityWebRequest(speechUri, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {

                byte[] audioData = www.downloadHandler.data;
                if (audioData != null)
                {
                    Debug.Log("filepath=" + filePath + ", Text= " + text);
                    File.WriteAllBytes(filePath, audioData);
                    Debug.Log("After Saving file");
                    playAudioContainer.SetActive(true);
                    audioByteArray = audioData;

                    //AWSManager.Instance.UpLoadToS3(filePath, "audio.mp3");
                }
            }
            else
            {
                Debug.LogError("Failed to generate speech: " + www.error);
            }
        }
    }

    //https://github.com/mapluisch/OpenAI-Text-To-Speech-for-Unity/blob/main/Assets/Scripts/Core/AudioPlayer.cs
    //https://github.com/srcnalt/OpenAI-Unity
    private void LoadAndPlayAudio(string filePath)
    {
#if UNITY_WEBGL
        Debug.Log("Playing WebGL");
        CreateAudioClip(audioByteArray);
#else
            StartCoroutine(LoadAndPlayAudioForNonWebGL(filePath));
#endif

        valuesContainer.SetActive(true);
    }

    private IEnumerator LoadAndPlayAudioForNonWebGL(string filePath)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Audio file loading error: " + www.error);
        }
    }

    private void PlayAgain()
    {
        SceneManager.LoadScene("Choose Category");
    }

    private AudioClip ConvertToWav(byte[] audioData, int sampleRate, int bitsPerSample, int channels)
    {
        int numSamples = audioData.Length / (bitsPerSample / 8);
        float[] audioSamples = new float[numSamples];

        // Convertissez les données audio brutes en float[]
        for (int i = 0; i < numSamples; i++)
        {
            audioSamples[i] = BitConverter.ToInt16(audioData, i * (bitsPerSample / 8)) / 32768f;
        }
        AudioClip clip = AudioClip.Create("MyAudioClip", numSamples, channels, sampleRate, false);

        clip.SetData(audioSamples, 0);

        return clip;
    }

    private void CreateAudioClip(byte[] audioData)
    {
        var clip = ConvertToWav(audioData, 44100, 16, 1);
        audioSource.clip = clip;
        audioSource.pitch = 0.5f;
        audioSource.Play();
    }
}
