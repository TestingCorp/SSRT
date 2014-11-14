using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Secret_Project_WPF
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Retrieves a char-to-char substring from this instance. The method provides a starting search index and the
        /// option to include in the substring the start and end chars.
        /// </summary>
        /// <param name="chStart">the starting char</param>
        /// <param name="chEnd">the ending char</param>
        /// <param name="nStartAt">the starting search index</param>
        /// <param name="bWithStart">include starting char?</param>
        /// <param name="bWithEnd">include endind char?</param>
        /// <returns></returns>
        public static string SubstringCharToChar(this string str, char chStart, char chEnd, int nStartAt = 0, bool bWithStart = false, bool bWithEnd = false)
        {
            return str.Substring(str.IndexOf(chStart, nStartAt) + Convert.ToInt32(!bWithStart), str.IndexOf(chEnd, nStartAt) - str.IndexOf(chStart, nStartAt) - Convert.ToInt32(!bWithStart) + Convert.ToInt32(bWithEnd));
        }

        /// <summary>
        /// Retrieves a start-to-char substring from this instance. The method provides a starting search index and the
        /// option to include in the substring the start and end chars.
        /// </summary>
        /// <param name="chEnd">the ending char</param>
        /// <param name="nStartAt">the starting search index</param>
        /// <param name="bWithStart">include starting char?</param>
        /// <param name="bWithEnd">include endind char?</param>
        /// <returns></returns>
        public static string SubstringCharToChar(this string str, char chEnd, int nStartAt = 0, bool bWithStart = false, bool bWithEnd = false)
        {
            return str.Substring(nStartAt + Convert.ToInt32(!bWithStart), str.IndexOf(chEnd, nStartAt) - nStartAt - Convert.ToInt32(!bWithStart) + Convert.ToInt32(bWithEnd));
        }

        /// <summary>
        /// Adds a question to the list. If the question's number is specified, checks whether
        /// the list already contains the question and adds a new one only if it doesn't.
        /// </summary>
        /// <param name="nQuestionNum">the question's number</param>
        public static void AddQuestionIfNotExist(this List<QuestionClass> list, int nQuestionNum = -1)
        {
            if (list.Count - 1 < nQuestionNum || nQuestionNum == -1)
            {
                if (nQuestionNum == -1) nQuestionNum = list.Count;
                for (int i = 0; i < nQuestionNum - list.Count + 1; i++)
                {
                    list.Add(new QuestionClass());
                    if (!list[list.Count - 1].InitializeAnswers()) MessageBox.Show("Initialization of answers failed!");
                }
            }
        }

        /// <summary>
        /// Compresses the list making all empty answers null and if all answers are null
        /// makes the whole question associated with them null.
        /// </summary>
        public static void Compress(this List<QuestionClass> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].AnswersCount(); j++)
                    if (list[i].IsAnswerEmpty(j)) list[i].NullifyAnswer(j);
                if (list[i].IsEmpty()) list[i] = null;
            }
        }

        /// <summary>
        /// Gets the index of the checked RadioButton from a list
        /// </summary>
        /// <param name="lrbAnswers"></param>
        /// <returns></returns>
        public static int? GetCheckedIndex(this List<RadioButton> lrbAnswers)
        {
            for (int i = 0; i < lrbAnswers.Count; i++)
                if (lrbAnswers[i] != null && lrbAnswers[i].IsChecked == true) return i;
            return null;
        }
    }
}
