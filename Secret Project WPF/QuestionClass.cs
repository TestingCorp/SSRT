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
            if (Answers != null) // If answers list is not empty just add an answer
                Answers.Add(new AnswerClass(value, isRightAnswer));
            // If it is, init it and then add an answer
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
                for (int nRightAnswerNum = 0; nRightAnswerNum < Answers.Count; nRightAnswerNum++) // For every answer
                {
                    if (Answers[nRightAnswerNum].IsRightAnswer == true) // If the answer is the right one
                    {
                        return nRightAnswerNum;
                    }
                }
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
            int? nCheckedIndex = lrbAnswers.GetCheckedIndex(); // Get the number of the checked RadioButton
            if (nCheckedIndex != null) // If there is something checked
            {
                return (this.Answers[(int)nCheckedIndex].IsRightAnswer == true); // Return if the checked answer is the right one
            }
            else // If nothing is checked yet
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
            if (!String.IsNullOrEmpty(Question)) // IF the question is empty
            {
                return false;
            }

            for (int i = 0; i < Answers.Count; i++) // For each answer
            {
                if (Answers[i] != null && !Answers[i].IsEmpty) // If answer is not null and not empty
                {
                    return false;
                }
            }
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
            if (String.IsNullOrEmpty(Question)) // If question is empty
            {
                return TestErrorCode.NoQuestion;
            }

            int numberOfAnswers = 0; // Count of the answers
            for (int i = 0; i < this.Answers.Count; i++) // For every answer
            {
                if (!this.Answers[i].IsEmpty) // If it is not null
                {
                    numberOfAnswers++; // Increase count
                }
            }

            if (numberOfAnswers == 0)
            {
                return TestErrorCode.NoAnswers;
            }
            else if (numberOfAnswers == 1)
            {
                return TestErrorCode.TooFewAnswers;
            }

            // If there are duplicate answers
            for (int i = 0; i < this.Answers.Count; i++)
            {
                for (int j = i + 1; j < this.Answers.Count; j++)
                {
                    if (!this.Answers[i].IsEmpty &&
                       !this.Answers[j].IsEmpty &&
                        this.Answers[i].Value == this.Answers[j].Value)
                        return TestErrorCode.DuplicateAnswers;
                }
            }

            if (this.Points <= 0)
            {
                return TestErrorCode.NoPoints;
            }

            if (lrbAnswers.GetCheckedIndex() != null) // If nothing is wrong and there is an answer checked
            {
                return TestErrorCode.AllFine;
            }
            else
            {
                return TestErrorCode.NoRightAnswer;
            }
        }

        /// <summary>
        /// Represents the time remaining in the test
        /// </summary>
        public static TimeSpan Time { get; set; }
        private static System.Timers.Timer timer = new System.Timers.Timer(1000);

        /// <summary>
        /// Sets a handler to be executed every second
        /// </summary>
        /// <param name="handler"></param>
        public static void SetTimerElapsedEventHandler(System.Timers.ElapsedEventHandler handler)
        {
            timer.Elapsed += handler;
        }

        //Methods to execute when the time is over
        public static Action onTimeOutExecute = null;

        //Methods to execute on each elapsed time interval
        public static Action onTimerElapsedExecute = null;

        /// <summary>
        /// Run/Start the timer
        /// </summary>
        public static void RunTimer()
        {
            timer.Start();
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        private static void StopTimer()
        {
            timer.Stop();
        }

        /// <summary>
        /// Stop the timer and nullify delegates
        /// </summary>
        public static void ResetTimer()
        {
            StopTimer();
            NulliftDelegates();
        }

        // Sets the delegates to null
        private static void NulliftDelegates()
        {
            onTimeOutExecute = null;
            onTimerElapsedExecute = null;
        }

        public static void timer_Elapsed(System.Windows.Threading.Dispatcher dispatcher,
                                         object sender,
                                         System.Timers.ElapsedEventArgs e)
        {
            Time = Time.Subtract(TimeSpan.FromSeconds(1)); // time - 1
            dispatcher.Invoke(new Action(() =>
            {
                if (onTimerElapsedExecute != null) onTimerElapsedExecute(); // execute all methods on elapsed
                if (Time <= TimeSpan.Zero)
                {
                    StopTimer();
                    if (onTimeOutExecute != null) onTimeOutExecute(); // execute all methods on time out
                    NulliftDelegates();
                    return;
                }
            }));
        }
    }
}