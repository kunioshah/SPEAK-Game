using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using OpenAI;
using System.Collections.Generic;

using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
public class FillBlanks : MonoBehaviour
{

    [SerializeField] private Button goBack;
    [SerializeField] private Button submit;
    [SerializeField] private Button showAnswer;
    [SerializeField] private TMPro.TMP_Text question;
    [SerializeField] private TMPro.TMP_Text answer;
    [SerializeField] private TMPro.TMP_Text userScore;
    [SerializeField] private TMPro.TMP_Text next;
    [SerializeField] private AudioSource correctAnswerAudioSource;
    [SerializeField] private AudioSource wrongAnswerAudioSource;
    [SerializeField] private Button nextQuestion;

    private int round = 0;
    private int userCorrectAnswer = 0;
    private bool firstAnswer = true;
    private string gameAnswer = "";
    private string userAnswer = "";

    private FillInTheBlankBank fillInTheBlankBank;

    void Start()
    {
        goBack.onClick.AddListener(GoBack);
        submit.onClick.AddListener(Submit);
        showAnswer.onClick.AddListener(ShowAnswer);
        nextQuestion.onClick.AddListener(NextQuestion);

        CallOpenAiToGetValues();
    }

    private void ShowAnswer()
    {
        Debug.Log("gameanswer=" + gameAnswer);
        userAnswer = gameAnswer;
    }


    // Update is called once per frame
    void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // has backspace/delete been pressed?
            {
                if (userAnswer.Length != 0)
                {
                    userAnswer = userAnswer.Substring(0, userAnswer.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r')) // enter/return
            {
                Submit();
            }
            else
            {
                if (userAnswer.Length < gameAnswer.Length)
                    userAnswer += c;
            }
        }

        FormatAnswerText();
    }

    private void FormatAnswerText()
    {
        answer.text = (userAnswer + " " + new string('_', gameAnswer.Length - userAnswer.Length)).Replace("_", "_ "); ;
    }

    private void CallOpenAiToGetValues()
    {
        fillInTheBlankBank = GameVariables.FillInTheBlankBank;
        if (fillInTheBlankBank == null)
        {
            SceneManager.LoadScene("Choose Category");
            return;
        }
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        answer.text = "";
        userAnswer = "";
        question.text = fillInTheBlankBank.sentences[round].query;
        gameAnswer = fillInTheBlankBank.sentences[round].answer;
        Debug.Log("Sentence=" + fillInTheBlankBank.sentences[round].query);
        /* 
        for (int i = 0; i < fillInTheBlankBank.questions[round].choices.Count; i++)
        {
            if (fillInTheBlankBank.questions[round].answer == fillInTheBlankBank.questions[round].choices[i])
            {
                correctAnswer = i + 1;
                break;
            } 
        }
        */
        if (round == fillInTheBlankBank.sentences.Count - 1)
        {
            next.text = "Next";
        }
    }

    private void GoBack()
    {
        SceneManager.LoadScene("Choose Category");
    }

    private void Submit()
    {
        if (answer.text.Trim().ToLower() == gameAnswer.ToLower())
        {
            if (firstAnswer)
            {
                userCorrectAnswer++;
            }
            userScore.text = "You are correct!";
            userScore.color = Color.green;
            if (GameVariables.PlaySounds)
            {
                correctAnswerAudioSource.Play();
            }
        }
        else
        {
            userScore.text = "Sorry, Please try again!";
            userScore.color = Color.red;
            if (GameVariables.PlaySounds)
            {
                wrongAnswerAudioSource.Play();
            }
        }

        firstAnswer = false;

        if (round == fillInTheBlankBank.sentences.Count - 1)
        {
            GameVariables.UserScore = userCorrectAnswer;
            GameVariables.NumberOfQuestions = fillInTheBlankBank.sentences.Count;
        }
    }

    private void NextQuestion()
    {
        round++;
        if (round < fillInTheBlankBank.sentences.Count)
        {
            firstAnswer = true;
            userScore.text = "";
            LoadQuestion();
        }
        else
        {
            SceneManager.LoadScene("End Game");
        }
    }
}
