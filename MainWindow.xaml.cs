﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Automation;
using System.Collections.ObjectModel;
//using System.Drawing;
using System.Windows.Interop;
using System.Windows.Forms.Integration;

namespace Secret_Project_WPF
{
    /// <summary>
    /// An error code representing whether something is wrong with the creation of the questions.
    /// </summary>
    public enum ISE_ErrorCode
    {
        NoQuestion,
        NoAnswers,
        NoRightAnswer,
        NoPoints,
        AllFine
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Contains all possible states of a test.
        /// </summary>
        public enum TestState
        {
            DoingNothing,
            DoingTest,
            CreatingTest
        }
        /// <summary>
        /// The current state of the test.
        /// </summary>
        TestState state = TestState.DoingNothing;
        /// <summary>
        /// A global list of QuestionClass objects that represent the questions currently in use.
        /// </summary>
        public List<QuestionClass> g_lQCQuestions = null;
        /// <summary>
        /// A global 2-dimentional list of RadioButton objects that represent the answers in the GUI.
        /// </summary>
        public List<List<RadioButton>> g_l2rbAnswers = null;
        /// <summary>
        /// A global list of Buttons that represent the "Ready" button when doing test. It is used to be made
        /// inactive after the test is done.
        /// </summary>
        //public List<Button> g_lbtReady = null;
        public ObservableCollection<TabItem> g_lTITabs = null;
        public Window g_wImageWindow = null;
        public List<ImageClass> g_lICImages = null;
        /// <summary>
        /// The number of the current question being done.
        /// </summary>
        public int g_nCurrQuestion = 0;

        public MainWindow()
        {
            tabControl = new TabControl();
            g_lTITabs = new ObservableCollection<TabItem>();
            g_lICImages = new List<ImageClass>();
            g_wImageWindow = new Window();

            InitializeComponent();
            CreateAndAddMainTab();

            tabControl.ItemsSource = g_lTITabs;

            window1.KeyDown += window1_KeyDown;
            window1.KeyUp += window1_KeyUp;
            window1.SizeChanged += delegate { ResizeAndAdjust(); };
            window1.Closing += delegate { g_wImageWindow.Close(); };
            window1.StateChanged += delegate { ResizeAndAdjust(); };
        }

        /// <summary>
        /// Creates the main tab that we see when we open the program.
        /// </summary>
        /// <returns>The main tab as an TabItem object.</returns>
        void CreateAndAddMainTab()
        {
            TabItem ti = new TabItem();
            ti.Header = "Начало";
            ti.Content = new Grid();

            Button[] bt = new Button[2];
            for (int i = 0; i < 2; i++)
            {
                bt[i] = new Button();
                bt[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                bt[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                bt[i].Width = 138;
                bt[i].Height = 40;
                (ti.Content as Grid).Children.Add(bt[i]);
            }
            AutomationProperties.SetAutomationId(bt[0], "button_create");
            AutomationProperties.SetAutomationId(bt[1], "button_do");

            bt[0].Content = "Създай тест";
            bt[1].Content = "Реши тест";

            bt[0].Click += CreateButton_Click;
            bt[1].Click += DoButton_Click;

            g_lTITabs.Add(ti);
        }

        /// <summary>
        /// Event handler for the Create button at the main tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (state != TestState.DoingNothing)
            {
                string message = String.Format("В момента {0} тест! Искате ли да изтриете данните си от теста и да създадете друг?", ((state == TestState.CreatingTest) ? "правите" : "решавате"));
                string title = "Внимание! Недовършен тест!";
                MessageBoxButton options = MessageBoxButton.YesNo;
                MessageBoxImage msgIcon = MessageBoxImage.Question;
                if (MessageBox.Show(message, title, options, msgIcon) == MessageBoxResult.No) return;
            }

            while (g_lTITabs.Count > 1)
            {
                g_lTITabs.RemoveAt(g_lTITabs.Count - 1);
            }

            g_l2rbAnswers = new List<List<RadioButton>>();
            g_lQCQuestions = new List<QuestionClass>();

            TabItem tiAdd = new TabItem();
            tiAdd.Header = "+Въпрос";
            tiAdd.GotFocus += CreateTabItemAdd_GotFocus;
            g_lTITabs.Add(tiAdd);

            state = TestState.CreatingTest;

            tabControl.SelectedIndex = tabControl.Items.Count - 1;
        }

        void DoButton_Click(object sender, RoutedEventArgs e)
        {
            if (state != TestState.DoingNothing)
            {
                string message = String.Format("В момента {0} тест! Искате ли да изтриете данните си от теста и да решите друг?", ((state == TestState.CreatingTest) ? "правите" : "решавате"));
                string title = "Внимание! Недовършен тест!";
                MessageBoxButton options = MessageBoxButton.YesNo;
                MessageBoxImage msgIcon = MessageBoxImage.Question;
                if (MessageBox.Show(message, title, options, msgIcon) == MessageBoxResult.No) return;
            }

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = "MGTest files (.mgt)|*.mgt";
            openFileDialog1.FilterIndex = 1;

            bool? nbClickedOK = openFileDialog1.ShowDialog();
            if (nbClickedOK == false) return;

            while (g_lTITabs.Count > 1)
                g_lTITabs.RemoveAt(g_lTITabs.Count - 1);

            g_l2rbAnswers = new List<List<RadioButton>>();
            g_lQCQuestions = new List<QuestionClass>();
            g_lICImages = new List<ImageClass>();

            string sInput = "";
            using (FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader sr = new BinaryReader(fs))
                {
                    MemoryStream ms = new MemoryStream();
                    int nQuestionsCount = sr.ReadInt32(); //sr.ReadInt32() = number of questions
                    for (int i = 0; i < nQuestionsCount; i++)
                    {
                        g_lICImages.Add(new ImageClass());
                        sInput += sr.ReadString();
                        long bytesCount = sr.ReadInt64();
                        if (bytesCount != -1)
                        {
                            ms = new MemoryStream(sr.ReadBytes((int)bytesCount));
                            g_lICImages[i].picBox.Image = System.Drawing.Image.FromStream(ms);
                            g_lICImages[i].picBox.Image = System.Drawing.Image.FromStream(ms);
                            g_lICImages[i].SetSize((int)((double)(g_lICImages[i].picBox.Image.Width) /
                                                   (g_lICImages[i].picBox.Image.Height) *
                                                   (int)(tabControl.ActualHeight - 95)),
                                                   (int)(tabControl.ActualHeight - 95));
                        }
                    }
                    ms.Dispose();
                }
            }
            sInput = Decrypt(sInput, "I like spagetti!");

            if (!Decode(sInput, out g_lQCQuestions)) return;

            while (g_l2rbAnswers.Count < g_lQCQuestions.Count)
                g_l2rbAnswers.Add(new List<RadioButton>());

            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                if (i == g_lQCQuestions.Count - 1) DoAddQuestion(ref tabControl, g_lQCQuestions[i], i, true);
                else DoAddQuestion(ref tabControl, g_lQCQuestions[i], i);
            }
            tabControl.SelectedIndex = 1;
            g_nCurrQuestion = 0;
            state = TestState.DoingTest;
            ResizeAndAdjust();
        }

        bool rightCtrlPressed = false;

        /// <summary>
        /// A keyUp event handler. Its current function is to be used for the cheating option.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void window1_KeyUp(object sender, KeyEventArgs e)
        {
            if (rightCtrlPressed && e.Key == Key.RightCtrl) rightCtrlPressed = false;
        }

        /// <summary>
        /// A keyDown event handler. Its current function is to be used for the cheating option.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void window1_KeyDown(object sender, KeyEventArgs e)
        {
            if (state == TestState.DoingTest)
            {
                if (e.Key == Key.RightCtrl) rightCtrlPressed = true;
                else if (e.Key == Key.NumPad0 && rightCtrlPressed) ShowRightAnswer();
            }
        }

        /// <summary>
        /// Differentiates the right answer from the others by adding a space in the beginning. Used for the cheating option.
        /// </summary>
        void ShowRightAnswer()
        {
            int? nnRightAnswerNum = g_lQCQuestions[g_nCurrQuestion].GetRightAnswerIndex();
            if (nnRightAnswerNum == null) return;
            int nRightAnswerNum = (int)(nnRightAnswerNum);

            string sContent = g_l2rbAnswers[g_nCurrQuestion][nRightAnswerNum].Content as string;
            if (sContent.Length == 0) return;
            sContent = (sContent.Substring(0, 1) == " ") ? (sContent.Substring(1)) : (sContent.Insert(0, " "));
            g_l2rbAnswers[g_nCurrQuestion][nRightAnswerNum].Content = sContent;
        }

        /// <summary>
        /// Transforms the global list of QuestionClass objects into a string of format [Q{0},{1}:{2}] for questions
        /// (where {0} is the question's number, {1} is the number of points, {2} is the quesion itself)
        /// and [A{0},{1}:{2}{3}] for answers (where {0} is the number of the question associated with the answer,
        /// {1} is the answer's number, {2} it either "{}" or "" (if the answer is the right one or not),
        /// {3} is the answer itself).
        /// </summary>
        /// <param name="lsOutput"></param>
        /// <param name="lQCQuestions"></param>
        /// <returns></returns>
        bool Encode(out List<string> lsOutput, List<QuestionClass> lQCQuestions)
        {
            if (lQCQuestions == null)
            {
                lsOutput = null;
                return false;
            }

            lQCQuestions.Compress();
            lsOutput = new List<string>();
            for (int i = 0; i < lQCQuestions.Count; i++)
                if (lQCQuestions[i] != null)
                {
                    lsOutput.Add("");
                    if (!String.IsNullOrEmpty(lQCQuestions[i].sQuestion))
                    {
                        lsOutput[lsOutput.Count - 1] += String.Format("[Q{0},{1}:{2}]",
                                                                      i,
                                                                      lQCQuestions[i].nPoints,
                                                                      lQCQuestions[i].sQuestion);
                    }
                    for (int j = 0; j < lQCQuestions[i].AnswersCount(); j++)
                    {
                        if (!lQCQuestions[i].isAnswerNull(j))
                        {
                            lsOutput[lsOutput.Count - 1] += String.Format("[A{0},{1}:{2}{3}]",
                                                                          i,
                                                                          j,
                                                                          (lQCQuestions[i].IsRightAnswer(j)) ?
                                                                          "{}" : "",
                                                                          lQCQuestions[i].GetAnswer(j));
                        }
                    }
                }
            return true;
        }

        /// <summary>
        /// Transforms the input string from the input file from the string format [Q{0},{1}:{2}] for questions
        /// and [A{0},{1}:{2}{3}] for answers into a list of QuestionClass objects.
        /// </summary>
        /// <param name="sInput"></param>
        /// <param name="lQCInput"></param>
        /// <returns></returns>
        bool Decode(string sInput, out List<QuestionClass> lQCInput)
        {
            List<string> lsArgs = new List<string>();
            try
            {
                List<QuestionClass> l_lQCInput = new List<QuestionClass>();
                if (sInput.Length == 0) { lQCInput = null; return false; }
                for (int l_index = 0; l_index < sInput.Length - 1; )
                {
                    lsArgs.Add(sInput.SubstringCharToChar('[', ']', l_index + 1 * Convert.ToInt32(l_index != 0), true, true));
                    l_index = sInput.IndexOf(']', l_index + 1);
                    //sInput = sInput.Remove(0, sInput.IndexOf(']') + 1);
                }
                for (int i = 0; i < lsArgs.Count; i++)
                {
                    if (lsArgs[i][1] == 'Q')
                    {
                        int nQuestionNum = Int32.Parse(lsArgs[i].SubstringCharToChar(',', 1, false, false));
                        l_lQCInput.AddQuestionIfNotExist(nQuestionNum);
                        string sQuestion = String.Format("{0} ({1} точки)", lsArgs[i].SubstringCharToChar(':', ']'), lsArgs[i].SubstringCharToChar(',', ':'));
                        //sQuestion = ManageStringLines(sQuestion);
                        l_lQCInput[nQuestionNum].UpdateStatus(sQuestion, Int32.Parse(lsArgs[i].SubstringCharToChar(',', ':')), -1, null, null);
                    }
                    else if (lsArgs[i][1] == 'A')
                    {
                        int nQuestionNum = Int32.Parse(lsArgs[i].SubstringCharToChar('A', ',')),
                            nAnswerNum = Int32.Parse(lsArgs[i].SubstringCharToChar(',', ':'));
                        bool bIsRightAnswer = lsArgs[i].Contains("{}");
                        string sAnswer = lsArgs[i].SubstringCharToChar((bIsRightAnswer ? '}' : ':'), ']');

                        l_lQCInput.AddQuestionIfNotExist(nQuestionNum);
                        l_lQCInput[nQuestionNum].UpdateStatus(null, null, nAnswerNum, sAnswer, bIsRightAnswer);
                    }
                }
                lQCInput = l_lQCInput;
                return true;
            }
            catch (Exception e)
            {
                string message = String.Format("Изглежда файлът е повреден!\nПрограмата получава следната грешка:\n{0}",
                    e.Message);
                string title = "Грешка!";
                MessageBox.Show(message, title);
                //MessageBox.Show(sInput);
                MessageBox.Show(e.StackTrace);
                lQCInput = null;
                return false;
            }
        }

        //public string ManageStringLines(string text, int nMaxLineLength = 73)
        //{
        //    if (text.Length >= nMaxLineLength)
        //        for (int j = 0, nLineBeginning = 0; j < text.Length; j++)
        //        {
        //            if (text[j] == '\n') nLineBeginning = j;
        //            if (j == nLineBeginning + nMaxLineLength)
        //                for (int i = j; i > nLineBeginning; i--)
        //                {
        //                    if (text[i] == ' ')
        //                    {
        //                        text = text.Insert(i, "\n");
        //                        text = text.Remove(i + 1, 1);
        //                        nLineBeginning = i + 1;
        //                        if (i + nMaxLineLength + 1 > text.Length) return text;
        //                        else j = i + nMaxLineLength + 1;
        //                    }
        //                }
        //        }
        //    return text;
        //}

        /// <summary>
        /// Encrypts a text with a password. The passCode is the sum of all the password's chars as numbers.
        /// The passcode is then added to each char of the text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Encrypt(ref string text, string password)
        {
            string sRes = "";
            int nPassSize = password.Length,
                nPassCode = 0;
            for (int i = 0; i < nPassSize; i++)
                nPassCode += (int)(password[i]);
            for (int i = 0; i < text.Length; i++)
                sRes = sRes.Insert(sRes.Length, ((char)((int)(text[i]) + nPassCode + nPassSize)).ToString());
            return sRes;
        }

        /// <summary>
        /// Decrypts the input text with the password given.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Decrypt(string text, string password)
        {
            string sRes = "";
            int nPassSize = password.Length,
                nPassCode = 0;
            for (int i = 0; i < nPassSize; i++)
                nPassCode += (int)(password[i]);
            for (int i = 0; i < text.Length; i++)
                sRes = sRes.Insert(sRes.Length, ((char)(((int)(text[i])) - nPassCode - nPassSize)).ToString());
            return sRes;
        }

        private void ResizeAndAdjust()
        {
            try
            {
                if (window1.ActualHeight < 300) window1.Height = 300;
                if (window1.ActualWidth < 455) window1.Width = 455;
                tabControl.Height = window1.ActualHeight - 45;
                tabControl.Width = window1.ActualWidth - 30;
                for (int i = 0; i < g_lTITabs.Count; i++)
                {
                    tabControl.Items.MoveCurrentToPosition(i);
                    (g_lTITabs[i].Content as Grid).Width = window1.ActualWidth;
                    (g_lTITabs[i].Content as Grid).Height = tabControl.Height - 40;
                    (g_lTITabs[i].Content as Grid).Margin = new Thickness(0, -5, 0, 0);
                    UIElementCollection children = (g_lTITabs[i].Content as Grid).Children;
                    double nImgWidth = 0.0d;
                    for (int j = 0; j < children.Count; j++)
                    {
                        if (children[j] is WindowsFormsHost)
                            if ((children[j] as WindowsFormsHost).Child.Size.IsEmpty == false)
                            {
                                nImgWidth = (children[j] as WindowsFormsHost).Child.Size.Width;
                                (children[j] as WindowsFormsHost).Margin = new Thickness(tabControl.Width - nImgWidth - 15, 9, 0, 0);
                                //(children[j] as Image).Margin = new Thickness(tabControl.Width - nImgWidth - 15, 9, 0, 0);
                            }

                    }
                    for (int j = 0; j < children.Count; j++)
                    {
                        if (i == 0)
                        {
                            if (AutomationProperties.GetAutomationId(children[j]) == "button_create")
                                (children[j] as Button).Margin = new Thickness(tabControl.Width / 2 - (children[j] as Button).Width - 10,
                                                                               tabControl.Height / 2 - (children[j] as Button).Height, 0, 0);
                            else if (AutomationProperties.GetAutomationId(children[j]) == "button_do")
                                (children[j] as Button).Margin = new Thickness(tabControl.Width / 2 + 20,
                                                                               tabControl.Height / 2 - (children[j] as Button).Height, 0, 0);
                        }
                        else if (state == TestState.CreatingTest)
                        {
                            double dValue = 0;
                            double dValue2 = 0;
                            if (AutomationProperties.GetAutomationId(children[j]) == "textbox_question")
                            {
                                dValue = tabControl.Width - 30 - nImgWidth;
                                if (dValue < 0) dValue = 0;
                                (children[j] as TextBox).Width = dValue;
                            }
                            else if (AutomationProperties.GetAutomationId(children[j]) == "textbox_points")
                            {
                                dValue = tabControl.Height - 70;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                (children[j] as TextBox).Margin = new Thickness(10, dValue, 0, 0);
                            }
                            else if (AutomationProperties.GetAutomationId(children[j]) == "textbox_answer")
                            {
                                dValue = tabControl.Width - 30 - nImgWidth - 13;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                (children[j] as TextBox).Width = dValue;
                            }
                            else if (AutomationProperties.GetAutomationId(children[j]) == "button_ready")
                            {
                                dValue = tabControl.Width - 100;
                                dValue2 = tabControl.Height - 70;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                if (dValue2 < 0)
                                {
                                    dValue2 = 0;
                                }
                                else
                                {
                                    (children[j] as Button).Margin = new Thickness(dValue, dValue2, 0, 0);
                                }
                            }
                            else if (AutomationProperties.GetAutomationId(children[j]) == "button_remove")
                            {
                                dValue = tabControl.Width - 100 - 87 - 5;
                                dValue2 = tabControl.Height - 70;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                if (dValue2 < 0)
                                {
                                    dValue2 = 0;
                                }
                                (children[j] as Button).Margin = new Thickness(dValue, dValue2, 0, 0);
                            }
                            else if (AutomationProperties.GetAutomationId(children[j]) == "button_browse")
                            {
                                dValue = tabControl.Width - (children[j] as Button).Width - 10 - 187;
                                dValue2 = tabControl.Height - 70;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                if (dValue2 < 0)
                                {
                                    dValue2 = 0;
                                }
                                (children[j] as Button).Margin = new Thickness(dValue,
                                                                               dValue2, 0, 0);
                            }
                        }
                        else if (state == TestState.DoingTest)
                        {
                            if (children[j] is TextBlock)
                            {
                                if ((children[j] as TextBlock).Margin.Top == 10)
                                {
                                    (children[j] as TextBlock).Width = tabControl.Width - 30;
                                }
                            }
                            else if (children[j] is Button)
                            {
                                if (AutomationProperties.GetAutomationId(children[j] as Button) == "button_ready")
                                {

                                    (children[j] as Button).Margin = new Thickness(tabControl.Width - 100, tabControl.Height - 70, 0, 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException) return;
                else
                {
                    string message = String.Format("Имаше грешка при оразмеряването! {0}", ex.Message);
                    MessageBox.Show(message);
                    MessageBox.Show(ex.StackTrace);
                }
            }
        }
    }


}
