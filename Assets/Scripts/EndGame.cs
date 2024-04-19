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

public class EndGame : MonoBehaviour
{
    [SerializeField] private Button playAgain;
    [SerializeField] private Button playFillBlanks;
    [SerializeField] private TMPro.TMP_Text userScore;

    void Start()
    {
        playAgain.onClick.AddListener(PlayAgain);
        playFillBlanks.onClick.AddListener(PlayFillBlanks);
        DisplayUserScore();
    }

    private void DisplayUserScore()
    {
        int userScorePercentage = (GameVariables.UserScore / GameVariables.NumberOfQuestions) * 100;

        if (userScorePercentage > 80){
            userScore.text = string.Format("Wow! You got {0} out of {1} correct. That was Amazing!", 
            GameVariables.UserScore, GameVariables.NumberOfQuestions);

            return;
        }
        
        if (userScorePercentage > 50){
            userScore.text = string.Format("Nice! You got {0} out of {1} correct. Practice again!", 
            GameVariables.UserScore, GameVariables.NumberOfQuestions);

            return;
        }

        userScore.text = string.Format("You got {0} out of {1} correct. Do another round to get better!", 
            GameVariables.UserScore, GameVariables.NumberOfQuestions);

    }

    private void PlayAgain()
    {
        SceneManager.LoadScene("Choose Category");
    }

    private void PlayFillBlanks()
    {
        GameVariables.GameType = GameTypes.FillBlanks;
        SceneManager.LoadScene("Choose Category");
    }
}