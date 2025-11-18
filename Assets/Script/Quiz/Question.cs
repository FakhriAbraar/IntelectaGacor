using System;

[Serializable]
public class Question
{
    public string questionText;     
    public string[] answers;        
    public int correctAnswerIndex;  
}

[Serializable]
public class QuestionList
{
    public Question[] questions;
}