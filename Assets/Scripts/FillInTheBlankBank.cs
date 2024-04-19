using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FillInTheBlankBank
{
    public List<Sentence> sentences;
    public class Sentence
    {
        public string query;
        public string answer;
    }
}