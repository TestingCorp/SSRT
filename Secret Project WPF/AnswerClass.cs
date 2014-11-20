    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Secret_Project_WPF
    {
        /// <summary>
        /// The class used to store the answers. It includes a boolean to know if the answers is the right one,
        /// the answer itself, an initialization constructor and a method to check if the answers is invalid.
        /// </summary>
        public class AnswerClass
        {
            /// <summary>
            /// The answer
            /// </summary>
            public string Value { set; get; }

            /// <summary>
            /// A boolean representing whether the answer is the righ one
            /// </summary>
            public bool IsRightAnswer { set; get; }

            /// <summary>
            /// Sets sAnswer to an empty string and the boolean for the right answer to false
            /// </summary>
            public AnswerClass()
            {
                IsRightAnswer = false; Value = String.Empty;
            }

            public AnswerClass(string answer, bool isRightAnswer = false)
            {
                Value = answer;
                IsRightAnswer = isRightAnswer;
            }

            /// <summary>
            /// Checks if the answer given is null or an empty string
            /// </summary>
            /// <returns></returns>
            public bool IsEmpty {
                get
                {
                    return String.IsNullOrEmpty(Value);
                }
            }
        }
    }
