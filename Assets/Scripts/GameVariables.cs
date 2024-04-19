using UnityEngine;
using UnityEngine.UI;

public class GameVariables
{
    public static string CategroryName;
    public static int UserScore;
    public static int NumberOfQuestions;
    public static QuestionBank QuizQuestionBank;
    public static FillInTheBlankBank FillInTheBlankBank;
    public static bool PlaySounds = true;
    public static float DifficultyLevel = 0;
    public static string SettingsNumberOfQuestions = "5";
    public static GameTypes GameType = GameTypes.Quiz;
}
