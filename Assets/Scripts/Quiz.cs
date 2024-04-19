using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Quiz : MonoBehaviour
{
    [SerializeField] private Button buttonValue1;
    [SerializeField] private Button buttonValue2;
    [SerializeField] private Button buttonValue3;
    [SerializeField] private Button buttonValue4;
    [SerializeField] private Button goBack;
    [SerializeField] private TMPro.TMP_Text question;
    [SerializeField] private TMPro.TMP_Text value1;
    [SerializeField] private TMPro.TMP_Text value2;
    [SerializeField] private TMPro.TMP_Text value3;
    [SerializeField] private TMPro.TMP_Text value4;
    [SerializeField] private TMPro.TMP_Text userScore;
    [SerializeField] private TMPro.TMP_Text next;
    [SerializeField] private AudioSource correctAnswerAudioSource;
    [SerializeField] private AudioSource wrongAnswerAudioSource;
    [SerializeField] private Button nextQuestion;
    
    private int round = 0;
    private int correctAnswer = 0;
    private int userCorrectAnswer = 0;
    private bool firstAnswer = true;

    private QuestionBank quiz;
    
    void Start()
    {
        goBack.onClick.AddListener(GoBack);
        nextQuestion.onClick.AddListener(NextQuestion);

        buttonValue1.onClick.AddListener(delegate { ButtonValueClick(1); });
        buttonValue2.onClick.AddListener(delegate { ButtonValueClick(2); });
        buttonValue3.onClick.AddListener(delegate { ButtonValueClick(3); });
        buttonValue4.onClick.AddListener(delegate { ButtonValueClick(4); });
        CallOpenAiToGetValues();
    }


    // Update is called once per frame
    void Update()
    {
    }
    private void CallOpenAiToGetValues()
    {
        quiz = GameVariables.QuizQuestionBank;
        if (quiz == null)
        {
            SceneManager.LoadScene("Choose Category");
            return;
        }
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        question.text = quiz.questions[round].query;
        value1.text = quiz.questions[round].choices[0];
        value2.text = quiz.questions[round].choices[1];
        value3.text = quiz.questions[round].choices[2];
        value4.text = quiz.questions[round].choices[3];
        for (int i = 0; i < quiz.questions[round].choices.Count; i++)
        {
            if (quiz.questions[round].answer == quiz.questions[round].choices[i])
            {
                correctAnswer = i + 1;
                break;
            }
        }

        if (round == quiz.questions.Count - 1)
        {
            next.text = "Next";
        }
    }

    private void GoBack()
    {
        SceneManager.LoadScene("Choose Category");
    }

    private void ButtonValueClick(int buttonNumber)
    {
        if (buttonNumber == correctAnswer)
        {
            if (firstAnswer)
            {
                userCorrectAnswer++;
            }
            
            string correctAnswer = "<color=green>You are correct!</color> <color=black>" + quiz.questions[round].explanation;     
            userScore.text = quiz.questions[round].explanation.Length > 145 ? correctAnswer.Substring(0, 179)  + "..." : correctAnswer;
            
            if (GameVariables.PlaySounds)
            {
                correctAnswerAudioSource.Play();
            }
        }
        else {
            userScore.text = "Sorry, Please try again!";
            userScore.color = Color.red;
            if (GameVariables.PlaySounds)
            {
                wrongAnswerAudioSource.Play();
            }
        }

        firstAnswer = false;

        if (round == quiz.questions.Count - 1)
        {
            GameVariables.UserScore = userCorrectAnswer;
            GameVariables.NumberOfQuestions = quiz.questions.Count;
        }
    }   

    private void NextQuestion()
    {
        round++;
        if (round < quiz.questions.Count)
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
