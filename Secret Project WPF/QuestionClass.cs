using System;
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
        public string sQuestion { internal set; get; }

        /// <summary>
        /// The points given if the right answer is selected.
        /// </summary>
        public int nPoints { internal set; get; }

        /// <summary>
        /// A private list of AnswerClass objects to store all the answers.
        /// </summary>
        private List<AnswerClass> lACAnswers;

        /// <summary>
        /// An initialization constructor
        /// </summary>
        public QuestionClass()
        {
            sQuestion = null;
            lACAnswers = null;
            nPoints = 0;
        }

        /// <summary>
        /// Sets the question equal to the given string
        /// </summary>
        /// <param name="question"></param>
        public void SetQuestion(string question)
        {
            sQuestion = question;
        }

        /// <summary>
        /// Gets the question
        /// </summary>
        /// <param name="question"></param>
        public string GetQuestion()
        {
            return sQuestion;
        }

        /// <summary>
        /// Sets the points to the given int
        /// </summary>
        /// <param name="points"></param>
        public void SetPoints(int points)
        {
            nPoints = points;
        }

        public static System.Timers.Timer timer = new System.Timers.Timer();
        public static void RunTimer()
        {
            timer.Interval = 1000;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {   
            //accessing the object while doing the test
        }

        public static TimeSpan time;

        /// <summary>
        /// Gets the number of answers currently stored in the list of answers.
        /// </summary>
        /// <returns></returns>
        public int AnswersCount()
        {
            return lACAnswers.Count;
        }

        /// <summary>
        /// Gets the selected answer as string.
        /// </summary>
        /// <param name="answerNumber">the number of the answer</param>
        /// <returns></returns>
        public string GetAnswer(int answerNumber)
        {
            return lACAnswers[answerNumber].sAnswer;
        }

        /// <summary>
        /// Sets the selected answer.
        /// </summary>
        /// <param name="answerNumber">the answer number</param>
        /// <param name="answer">the answer as string</param>
        public void SetAnswer(int answerNumber, string answer)
        {
            lACAnswers[answerNumber].sAnswer = answer;
        }

        /// <summary>
        /// Checks if the selected answer is the right answer.
        /// </summary>
        /// <param name="answerNumber">the answer number</param>
        /// <returns></returns>
        public bool IsRightAnswer(int answerNumber)
        {
            return lACAnswers[answerNumber].bIsRightAnswer;
        }

        /// <summary>
        /// Sets the selected answer's right answer property
        /// </summary>
        /// <param name="answerNumber">the answer number</param>
        /// <param name="rightAnswer">a boolean representing whether the answer is the right answer</param>
        public void SetRightAnswer(int answerNumber, bool rightAnswer)
        {
            lACAnswers[answerNumber].bIsRightAnswer = rightAnswer;
        }

        /// <summary>
        /// Checks if the selected answer is empty.
        /// </summary>
        /// <param name="answerNumber">the answer number</param>
        /// <returns></returns>
        public bool IsAnswerEmpty(int answerNumber)
        {
            return lACAnswers[answerNumber].IsEmpty();
        }

        /// <summary>
        /// Makes the selected answer null.
        /// </summary>
        /// <param name="answerNumber">the answer number</param>
        public void NullifyAnswer(int answerNumber)
        {
            lACAnswers[answerNumber] = null;
        }

        /// <summary>
        /// Checks if the selected answer is null.
        /// </summary>
        /// <param name="answerNumber">the answer number</param>
        /// <returns></returns>
        public bool isAnswerNull(int answerNumber)
        {
            return (lACAnswers[answerNumber] == null);
        }

        /// <summary>
        /// Adds 4 answers to the null list of answer objects.
        /// </summary>
        /// <returns></returns>
        public bool InitializeAnswers()
        {
            if (lACAnswers == null)
            {
                lACAnswers = new List<AnswerClass>();
                for (int i = 0; i < 4; i++) lACAnswers.Add(new AnswerClass());
                return true;
            }
            //TODO: // else throw new exception
            else return false;
        }

        /// <summary>
        /// Adds an answer to the list of answer objects.
        /// </summary>
        public void AddAnswer()
        {
            lACAnswers.Add(new AnswerClass());
        }

        /// <summary>
        /// Gets the number of the right answer from the list of answer objects.
        /// </summary>
        /// <returns></returns>
        public int? GetRightAnswerIndex()
        {
            try
            {
                for (int nRightAnswerNum = 0; nRightAnswerNum < lACAnswers.Count(); nRightAnswerNum++)
                    if (lACAnswers[nRightAnswerNum].bIsRightAnswer == true) return nRightAnswerNum;
                throw new Exception("No right answer!");
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("GetRightAnswerIndex() trew an exception: {0}", e.Message));
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
            if (nCheckedIndex == null)
            {
                return false;
            }
            else
            {
                return (this.lACAnswers[(int)nCheckedIndex].bIsRightAnswer == true);
            }
        }

        /// <summary>
        /// Checks if the question is empty by checking the question as string if empty
        /// and each individual answers if null or empty
        /// </summary>
        /// <returns>true if question is empty, false if not</returns>
        public bool IsEmpty()
        {
            if (!String.IsNullOrEmpty(sQuestion)) return false;
            for (int i = 0; i < lACAnswers.Count; i++)
                if (lACAnswers[i] != null && !lACAnswers[i].IsEmpty()) return false;
            return true;
        }

        /// <summary>
        /// Checks the question and answers individually if null or empty,
        /// if no right answer is selected and if no points are specified.
        /// </summary>
        /// <param name="lrbAnswers">a list of RadioButtons (for the right answer checked)</param>
        /// <returns>The corresponding error code.</returns>
        public ISE_ErrorCode IsSomethingWrong(List<RadioButton> lrbAnswers)
        {
            if (String.IsNullOrEmpty(sQuestion)) return ISE_ErrorCode.NoQuestion;
            for (int i = 0; i < this.lACAnswers.Count; i++)
            {
                if (!this.lACAnswers[i].IsEmpty()) break;
                if (i == this.lACAnswers.Count - 1) return ISE_ErrorCode.NoAnswers;
            }
            if (lrbAnswers.GetCheckedIndex() == null) return ISE_ErrorCode.NoRightAnswer;
            if (this.nPoints <= 0) return ISE_ErrorCode.NoPoints;
            return ISE_ErrorCode.AllFine;
        }
    }
}
