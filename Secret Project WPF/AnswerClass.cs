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
        class AnswerClass
        {
            public bool bIsRightAnswer { internal set; get; }
            public string sAnswer { internal set; get; }
            public AnswerClass() { bIsRightAnswer = false; sAnswer = String.Empty; }
            //public AnswerClass(string str, bool rightAnswer = false) { sAnswer = str; this.bIsRightAnswer = rightAnswer; }
            public bool IsEmpty()
            { 
                return String.IsNullOrEmpty(sAnswer);
            }
        }
    }
