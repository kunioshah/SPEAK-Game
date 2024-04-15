using System.Collections.Generic;

public class QuestionBank
{
    public List<Quiz> questions;
    public class Quiz
    {
        public string query;
        public List<string> choices;
        public string answer;
        public string explanation;
    }
}