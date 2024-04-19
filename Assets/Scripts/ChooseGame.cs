using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseGame : MonoBehaviour
{
    [SerializeField] private Button quizImageButton;
    [SerializeField] private Button quizButton;
    [SerializeField] private Button fillBlanksImageButton;
    [SerializeField] private Button fillBlanksButton;
    private GameTypes gameType;

    void Start()
    {
        quizButton.onClick.AddListener(delegate { ButtonClicked(GameTypes.Quiz); });
        quizImageButton.onClick.AddListener(delegate { ButtonClicked(GameTypes.Quiz); });
        fillBlanksButton.onClick.AddListener(delegate { ButtonClicked(GameTypes.FillBlanks); });
        fillBlanksImageButton.onClick.AddListener(delegate { ButtonClicked(GameTypes.FillBlanks); });
    }

    private void ButtonClicked(GameTypes gameType)
    {
        GameVariables.GameType = gameType;
        SceneManager.LoadScene("Choose Category");
    }

}