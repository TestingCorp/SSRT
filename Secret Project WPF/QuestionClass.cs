﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Secret_Project_WPF
{
    /// <summary>
    /// Stores information about the questions.
    /// </summary>
    public class QuestionClass
    {
        /// <summary>
        /// The question
        /// </summary>
        public string Question { set; get; }

        /// <summary>
        /// The points given if the right answer is selected.
        /// </summary>
        public int Points { set; get; }

        /// <summary>
        /// A private list of AnswerClass objects to store all the answers.
        /// </summary>
        public List<AnswerClass> Answers { get; private set; }

        /// <summary>
        /// An initialization constructor
        /// </summary>
        public QuestionClass()
        {
            Question = null;
            Answers = null;
            Points = 0;
        }

        /// <summary>
        /// Makes the selected answer null.
        /// </summary>
        /// <param name="answerNumber">the answer number</param>
        public void NullifyAnswer(int answerNumber)
        {
            Answers[answerNumber] = null;
        }

        /// <summary>
        /// Adds an answer to the list of answer objects.
        /// </summary>
        public void AddAnswer(string value, bool isRightAnswer = false)
        {
            if (Answers != null)
                Answers.Add(new AnswerClass(value, isRightAnswer));
            else
            {
                Answers = new List<AnswerClass>();
                Answers.Add(new AnswerClass(value, isRightAnswer));
            }
        }

        /// <summary>
        /// Gets the number of the right answer from the list of answer objects.
        /// </summary>
        /// <returns></returns>
        public int? GetRightAnswerIndex()
        {
            try
            {
                for (int nRightAnswerNum = 0; nRightAnswerNum < Answers.Count; nRightAnswerNum++)
                    if (Answers[nRightAnswerNum].IsRightAnswer == true) return nRightAnswerNum;
                throw new Exception("No right answer!");
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("GetRightAnswerIndex() trew an exception: {0}", e.Message), "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Checks if the right answer from a list of RadioButtons is checked
        /// </summary>
        /// <param name="lrbAnswers"></param>
        /// <param name="nQuestionNumber">the list of RadioButtons</param>
        /// <returns>true if the right answer is checked</returns>
        public bool IsRightAnswerChecked(List<RadioButton> lrbAnswers)
        {
            int? nCheckedIndex = lrbAnswers.GetCheckedIndex();
            if (nCheckedIndex != null)
            {
                return (this.Answers[(int)nCheckedIndex].IsRightAnswer == true);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the question is empty by checking the question as string if empty
        /// and each individual answers if null or empty
        /// </summary>
        /// <returns>true if question is empty, false if not</returns>
        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(Question)) return false;
            for (int i = 0; i < Answers.Count; i++)
                if (Answers[i] != null && !Answers[i].IsEmpty) return false;
            return true;
        }

        /// <summary>
        /// Checks the question and answers individually if null or empty,
        /// if no right answer is selected and if no points are specified.
        /// </summary>
        /// <param name="lrbAnswers">a list of RadioButtons (for the right answer checked)</param>
        /// <returns>The corresponding error code.</returns>
        public TestErrorCode IsSomethingWrong(List<RadioButton> lrbAnswers)
        {
            if (String.IsNullOrEmpty(Question)) return TestErrorCode.NoQuestion;
            for (int i = 0; i < this.Answers.Count; i++)
            {
                if (!this.Answers[i].IsEmpty) break;
                if (i == this.Answers.Count - 1) return TestErrorCode.NoAnswers;
            }
            if (this.Points <= 0) return TestErrorCode.NoPoints;
            if (lrbAnswers.GetCheckedIndex() != null)
            {
                return TestErrorCode.AllFine;
            }
            else
            {
                return TestErrorCode.NoRightAnswer;
            }
        }

        public static TimeSpan Time { get; set; }
        private static System.Timers.Timer timer = new System.Timers.Timer(1000);

        public static void SetTimerElapsedEventHandler(System.Timers.ElapsedEventHandler handler)
        {
            timer.Elapsed += handler;
        }

        public static void RunTimer()
        {
            timer.Start();
        }

        private static void StopTimer()
        {
            timer.Stop();
        }

        public static void ResetTimer()
        {
            StopTimer();
            NulliftDelegates();
        }

        private static void NulliftDelegates()
        {
            onTimeOutExecute = null;
            onTimerElapsedExecute = null;
        }

        //Methods to execute when the time is over
        public delegate void OnTimeOutExecute();
        public static OnTimeOutExecute onTimeOutExecute = null;

        //Methods to execute when the time interval has elapsed
        public delegate void OnTimerElapsedExecute();
        public static OnTimerElapsedExecute onTimerElapsedExecute = null;

        public static void timer_Elapsed(System.Windows.Threading.Dispatcher dispatcher,
                                         object sender,
                                         System.Timers.ElapsedEventArgs e)
        {
            Time = Time.Subtract(TimeSpan.FromSeconds(1));
            dispatcher.Invoke(new Action(() =>
            {
                if (onTimerElapsedExecute != null) onTimerElapsedExecute();
                if (Time <= TimeSpan.Zero)
                {
                    StopTimer();
                    if (onTimeOutExecute != null) onTimeOutExecute();
                    NulliftDelegates();
                    return;
                }
            }));
        }
    }
}