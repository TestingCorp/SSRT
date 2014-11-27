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
//using System.Drawing;kk
using System.Windows.Interop;
using System.Windows.Forms.Integration;

namespace Secret_Project_WPF
{
    /// <summary>
    /// An error code representing whether something is wrong with the creation of the questions.
    /// </summary>
    public enum TestErrorCode
    {
        NoQuestion,
        NoAnswers,
        TooFewAnswers,
        DuplicateAnswers,
        NoRightAnswer,
        NoPoints,
        AllFine
    }

    /// <summary>
    /// Contains all possible states of a test.
    /// </summary>
    public enum TestState
    {
        DoingNothing,
        DoingTest,
        CreatingTest
    }

    public class OperationFailedException : SystemException
    {
        private OperationFailedException()
        {
            return;
        }
        public OperationFailedException(string message)
            :base(message)
        { }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The current state of the test.
        /// </summary>
        private TestState state = TestState.DoingNothing;

        /// <summary>
        /// The tabs in the GUI. Used for data bindings with the TabControl
        /// </summary>
        private ObservableCollection<TabItem> g_lTITabs = null;

        /// <summary>
        /// The questions currently in use.
        /// </summary>
        private List<QuestionClass> g_lQCQuestions = null;

        /// <summary>
        /// The RadioButton answers in the GUI.
        /// </summary>
        private List<List<RadioButton>> g_l2rbAnswers = null;

        /// <summary>
        /// The Images used in the questions
        /// </summary>
        private List<ImageClass> g_lICImages = null;

        /// <summary>
        /// The current question being done.
        /// </summary>
        private int currentQuestionNum = 0;

        public MainWindow()
        {
            g_lTITabs = new ObservableCollection<TabItem>();

            InitializeComponent();
            CreateAndAddMainTab();

            tabControl.ItemsSource = g_lTITabs;
            tabControl.SelectionChanged += new SelectionChangedEventHandler(tabControl_SelectionChanged);
            

            window1.SizeChanged += delegate { ResizeAndAdjust(); };
            window1.Closing += window1_Closing;
            window1.StateChanged += delegate { ResizeAndAdjust(); };
            QuestionClass.SetTimerElapsedEventHandler(
                (object ElapsedSender, System.Timers.ElapsedEventArgs ElapsedE) =>
                    QuestionClass.timer_Elapsed(this.Dispatcher, ElapsedSender, ElapsedE));
            ImageClass.InitImageWindow();
        }

        /// <summary>
        /// Event handler for when the user has closed the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ImageClass.CloseWindow(); // Close the image window

            g_lICImages = null;
            g_lTITabs = null;

            Application.Current.Shutdown(); // Close the program
        }

        /// <summary>
        /// Creates the main tab that we see when we open the program.
        /// </summary>
        private void CreateAndAddMainTab()
        {
            TabItem ti = new TabItem(); // Create the TabItem object
            ti.Header = "Начало";
            ti.Content = new Grid();

            Button[] bt = new Button[2]; // Create the two main buttons Create and Do
            for (int i = 0; i < 2; i++)
            {
                bt[i] = new Button();
                bt[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                bt[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                bt[i].Width = 138;
                bt[i].Height = 40;
                (ti.Content as Grid).Children.Add(bt[i]); // Add them to the grid
            }

            // Set their IDs,
            AutomationProperties.SetAutomationId(bt[0], "button_create");
            AutomationProperties.SetAutomationId(bt[1], "button_do");

            // contents
            bt[0].Content = "Създай тест";
            bt[1].Content = "Реши тест";

            // and Click event handlers
            bt[0].Click += ButtonCreate_Click;
            bt[1].Click += ButtonDo_Click;

            g_lTITabs.Add(ti); // Add the new TabItem to the global list of tabs
        }

        /// <summary>
        /// Event handler for the Create button at the main tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            // If the user is currently creating or doing a test show a message
            if (state != TestState.DoingNothing)
            {
                string message = String.Format("В момента {0} тест! Искате ли да изтриете данните си от теста и да създадете друг?", ((state == TestState.CreatingTest) ? "правите" : "решавате"));
                string title = "Внимание! Недовършен тест!";
                if (MessageBox.Show(message,
                                    title,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.No) return;
            }

            state = TestState.CreatingTest; // Set the new state
            ResetTest(); // Reset all global varialbes

            // Create the "Add question" buton and set its GotFocus event handler
            TabItem tiAdd = new TabItem();
            tiAdd.Header = "+Въпрос";
            tiAdd.GotFocus += CreateTabItemAdd_GotFocus;

            g_lTITabs.Add(tiAdd); // Add the new tab to the global list of tabs

            tabControl.SelectedIndex = tabControl.Items.Count - 1; // Select the newly created tab in order for it to create a new question
        }

        /// <summary>
        /// Event handler for the Do button at the main tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDo_Click(object sender, RoutedEventArgs e)
        {
            if (state != TestState.DoingNothing)
            {
                string message = String.Format("В момента {0} тест! Искате ли да изтриете данните си от теста и да решите друг?", ((state == TestState.CreatingTest) ? "правите" : "решавате"));
                string title = "Внимание! Недовършен тест!";
                if (MessageBox.Show(message,
                                    title,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.No) return;
            }

            OpenFileDialog openFileDialog1 = new OpenFileDialog(); // Create an Open dialog for the test
            openFileDialog1.Multiselect = false;
            openFileDialog1.Filter = "SUT Test (.sut)|*.sut";
            openFileDialog1.FilterIndex = 1;

            bool? nbClickedOK = openFileDialog1.ShowDialog(); // Show the dialog and let the user choose a test
            if (nbClickedOK == false) return; // If the user clicked 'Cancel', return
            
            state = TestState.DoingTest; // Set the state of the test
            ResetTest(); // Nullify the varialbes

            string sInput = String.Empty; // Create an empty string for the test data
            using (FileStream fileStream = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    int nQuestionsCount = binaryReader.ReadInt32(); //sr.ReadInt32() = number of questions
                    for (int i = 0; i < nQuestionsCount; i++)
                    {
                        g_lICImages.Add(new ImageClass()); // Add an image to the global list
                        sInput += binaryReader.ReadString(); // Read next question info
                        long bytesCount = binaryReader.ReadInt64(); // Read next image bytes
                        if (bytesCount != -1) // If image available
                        {
                            memoryStream = new MemoryStream(binaryReader.ReadBytes((int)bytesCount)); // Read the image to a memory stream 

                            // Read and resize the image from the memory stream
                            g_lICImages[i].picBox.Image = System.Drawing.Image.FromStream(memoryStream);
                            g_lICImages[i].SetSize((int)((double)(g_lICImages[i].picBox.Image.Width) /
                                                   (g_lICImages[i].picBox.Image.Height) *
                                                   (int)(tabControl.ActualHeight - 95)),
                                                   (int)(tabControl.ActualHeight - 95));
                        }
                    }
                    // Read and set the time info at the end of the file
                    int minutes = binaryReader.ReadInt32();
                    int seconds = binaryReader.ReadInt32();
                    QuestionClass.Time = new TimeSpan(0, minutes, seconds);

                    memoryStream.Dispose();
                }
            }

            sInput = Decrypt(sInput, "I like spagetti!");

            if (!Decode(sInput, out g_lQCQuestions)) return; // Turn the input into question object info

            while (g_l2rbAnswers.Count < g_lQCQuestions.Count) // Equalize the number of questions and radiobutton lists 
                g_l2rbAnswers.Add(new List<RadioButton>());

            // For every question object in the list create and add a new question
            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                if (i == g_lQCQuestions.Count - 1) DoAddQuestion(ref tabControl, g_lQCQuestions[i], i, true);
                else DoAddQuestion(ref tabControl, g_lQCQuestions[i], i);
            }
            tabControl.SelectedIndex = 1; // Select the first question
            currentQuestionNum = 0; // Set the current question to the first

            ResizeAndAdjust();

            // If time has been set, add methods on elapse and timeout and run the timer
            if (QuestionClass.Time != TimeSpan.Zero)
            {
                QuestionClass.onTimeOutExecute += () =>
                    MessageBox.Show("Времето Ви изтече!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuestionClass.onTimeOutExecute += FinishTest;
                QuestionClass.onTimeOutExecute += TimeOutLabelManage;
                QuestionClass.onTimerElapsedExecute += () =>
                        {
                            TimerElapsedLabelManage();
                        };
                QuestionClass.RunTimer(); // Run the timer
            }
            else // If not,
            {
                HideTimerLabels(); // hide the time labels
            }
        }

        /// <summary>
        /// Sets the appropriate global variables to null
        /// </summary>
        private void ResetTest()
        {
            QuestionClass.ResetTimer(); // Reset the timer

            // Leave only the first tab
            while (g_lTITabs.Count > 1)
                g_lTITabs.RemoveAt(g_lTITabs.Count - 1);

            if (state == TestState.DoingNothing)
            {
                g_l2rbAnswers = null;
                g_lQCQuestions = null;
                g_lICImages = null;
            }
            else
            {
                g_l2rbAnswers = new List<List<RadioButton>>();
                g_lQCQuestions = new List<QuestionClass>();
                g_lICImages = new List<ImageClass>();
            }
        }

        /// <summary>
        /// Event handler for when a new tab is active. Sets the global variable for the
        /// current question number based on the new selected tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedIndex != 0 && // If the selected tab is not the first one
                (
                    state == TestState.DoingTest || // and the user is doing a test
                    (
                        state == TestState.CreatingTest && // or creating
                        tabControl.Items.Count != 2 && // and there are not 2 tabs available
                        tabControl.SelectedIndex != tabControl.Items.Count - 1) // and the user is not on the last tab
                    )
                )
            {
                currentQuestionNum = tabControl.SelectedIndex - 1;
            }
        }

        /// <summary>
        /// Sets the tabs and buttons' IsEnabled property to true
        /// </summary>
        private void EnableControls()
        {
            if (state != TestState.DoingNothing)
            {
                // Enable all buttons on current tab
                UIElementCollection children = (g_lTITabs[currentQuestionNum + 1].Content as Grid).Children;
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] is Button) (children[i] as Button).IsEnabled = true;
                }
            }
            // Enable all tabs
            for (int i = 0; i < g_lTITabs.Count; i++)
            {
                g_lTITabs[i].IsEnabled = true;
            }
        }

        /// <summary>
        /// Sets the tabs and buttons' IsEnabled property to false
        /// </summary>
        private void DisableControls()
        {
            // Disable all buttons on current tab
            UIElementCollection children = (g_lTITabs[currentQuestionNum + 1].Content as Grid).Children;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is Button) (children[i] as Button).IsEnabled = false;
            }

            // Disable all tabs
            for (int i = 0; i < g_lTITabs.Count; i++)
            {
                g_lTITabs[i].IsEnabled = false;
            }
        }

        /// <summary>
        /// Checks all the items in the given tab and compares their ID with the given.
        /// Finally, returns the match as object
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="tabNumber"></param>
        /// <returns></returns>
        private object GetObjectById(string ID, int tabNumber)
        {
            var children = (g_lTITabs[tabNumber].Content as Grid).Children; // Get all objects on selected tab
            for (int i = 0; i < children.Count; i++)
            {
                if (AutomationProperties.GetAutomationId(children[i]) == ID) // If the ID matches the current object of the tab
                    return children[i];
            }
            return null;
        }

        /// <summary>
        /// Get the index of the tab, containing the selected object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private int GetTabIndexByObject(object obj)
        {
            for(int i = 0; i < g_lTITabs.Count; i++)
            {
                var children = (g_lTITabs[i].Content as Grid).Children; // Get all objects on current tab
                if (children.Contains(obj as UIElement)) // If the given object is included in the objects of the current tab
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Controls the size and position of the controls in the GUI
        /// </summary>
        private void ResizeAndAdjust()
        {
            try
            {
                tabControl.Height = window1.ActualHeight - 45;
                tabControl.Width = window1.ActualWidth - 30;
                for (int i = 0; i < g_lTITabs.Count; i++)
                {
                    //If creting test, count of tabs is bigger than 1 and on last tab
                    if (state == TestState.CreatingTest &&
                        g_lTITabs.Count > 1 &&
                        i == g_lTITabs.Count - 1) break;

                    // Set up the grid
                    (g_lTITabs[i].Content as Grid).Width = tabControl.Width;
                    (g_lTITabs[i].Content as Grid).Height = tabControl.Height - 40;
                    (g_lTITabs[i].Content as Grid).Margin = new Thickness(0, -5, 0, 0);

                    double nImgWidth = 0.0d; // Variable for the image width
                    if (i != 0 && g_lICImages[i - 1].picBox.Image != null) // If tab is not the first and an image is present
                    {
                        nImgWidth = g_lICImages[i - 1].picBox.Width; // get its width
                        g_lICImages[i - 1].wfh.Margin = new Thickness(tabControl.Width - nImgWidth - 15, 9, 0, 0); // and set it up
                    }

                    UIElementCollection children = (g_lTITabs[i].Content as Grid).Children;
                    for (int j = 0; j < children.Count; j++)
                    {
                        string sID = AutomationProperties.GetAutomationId(children[j]); // Get the ID of the current object from the current tab
                        double dValue2 = 0;
                        double dValue = 0;
                        #region ifFirstTab
                        if (i == 0)
                        {
                            if (sID == "button_create")
                                (children[j] as Button).Margin = new Thickness(tabControl.Width / 2 - (children[j] as Button).Width - 10,
                                                                               tabControl.Height / 2 - (children[j] as Button).Height, 0, 0);
                            else if (sID == "button_do")
                                (children[j] as Button).Margin = new Thickness(tabControl.Width / 2 + 20,
                                                                               tabControl.Height / 2 - (children[j] as Button).Height, 0, 0);
                        }
                        #endregion
                        #region ifCreatingTest
                        else if (state == TestState.CreatingTest)
                        {
                            if (sID == "textbox_question")
                            {
                                dValue = tabControl.Width - 30 - nImgWidth;
                                if (dValue < 0) dValue = 0;
                                (children[j] as TextBox).Width = dValue;
                            }
                            else if (sID == "textbox_points")
                            {
                                dValue = tabControl.Height - 70;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                (children[j] as TextBox).Margin = new Thickness(10, dValue, 0, 0);
                            }
                            else if (sID == "textbox_timer")
                            {
                                dValue = tabControl.Height - 70;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                dValue2 = 10 + 87 + 10;
                                (children[j] as TextBox).Margin = new Thickness(dValue2, dValue, 0, 0);
                            }
                            else if (sID.Length > 14 && sID.Substring(0, 14) == "textbox_answer")
                            {
                                dValue = tabControl.Width - 30 - nImgWidth - 13;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                (children[j] as TextBox).Width = dValue;
                            }
                            else if (sID == "button_ready")
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
                            else if (sID == "button_remove")
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
                            else if (sID == "button_browse")
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
                        #endregion
                        #region ifDoingTestOrDoingNothing
                        else
                        {
                            if (sID == "label_timeLeft")
                            {
                                dValue = tabControl.Width - 175;
                                if (dValue < 0)
                                {
                                    dValue = 0;
                                }
                                dValue2 = tabControl.Height - 70;
                                if (dValue2 < 0)
                                {
                                    dValue2 = 0;
                                }
                                (children[j] as Label).Margin = new Thickness(dValue, dValue2, 0, 0);
                            }
                            if (sID == "scrollviewer_question")
                            {
                                dValue = tabControl.Width - 30 - nImgWidth;
                                if (dValue < 0) dValue = 0;
                                (children[j] as ScrollViewer).Width = dValue;
                                ((children[j] as ScrollViewer).Content as TextBlock).Width = dValue - 8;
                            }
                            else if (sID.Length > 17 && sID.Substring(0, 17) == "scrollview_answer")
                            {
                                dValue = tabControl.Width - 30 - nImgWidth-13;
                                if (dValue < 0) dValue = 0;
                                (children[j] as ScrollViewer).Width = dValue;
                            }
                            else if (sID == "button_ready")
                            {
                                (children[j] as Button).Margin =
                                    new Thickness(tabControl.Width - 100, tabControl.Height - 70, 0, 0);
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                string message = String.Format("Имаше грешка при оразмеряването! {0}", ex.Message);
                MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Transforms the global list of QuestionClass objects into a string of format [Q{0},{1}:{2}] for questions
        /// (where {0} is the question's number, {1} is the number of points, {2} is the quesion itself)
        /// and [A{0}:{2}{3}] for answers (where {0} is the number of the question associated with the answer,
        /// {1} it either "{}" or "" (if the answer is the right one or not),
        /// {2} is the answer itself).
        /// </summary>
        /// <param name="lsOutput"></param>
        /// <param name="lQCQuestions"></param>
        /// <returns></returns>
        private bool Encode(out List<string> lsOutput, List<QuestionClass> lQCQuestions)
        {
            if (lQCQuestions != null)
            {
                lQCQuestions.Compress(); // Nullify empty question and answer objects
                lsOutput = new List<string>(); 
                for (int i = 0; i < lQCQuestions.Count; i++)
                {
                    if (lQCQuestions[i] != null)
                    {
                        lsOutput.Add(String.Empty); // Add an empty string to the list
                        //Add to it the data about the question in the standard format
                        lsOutput[lsOutput.Count - 1] += String.Format("[Q{0},{1}:{2}]",
                                                                      i,
                                                                      lQCQuestions[i].Points,
                                                                      lQCQuestions[i].Question);
                        //For every answer add data about it in the standard format
                        for (int j = 0; j < lQCQuestions[i].Answers.Count; j++)
                        {
                            if (lQCQuestions[i].Answers[j] != null)
                            {
                                lsOutput[lsOutput.Count - 1] += String.Format("[A{0}:{1}{2}]",
                                                                              i,
                                                                              (lQCQuestions[i].Answers[j].IsRightAnswer) ?
                                                                              "{}" : String.Empty,
                                                                              lQCQuestions[i].Answers[j].Value);
                            }
                        }
                    }
                }
                return true;
            }
            else // If global list of questions is null
            {
                lsOutput = null;
                return false;
            }
        }

        /// <summary>
        /// Transforms the input string from the input file from the string format [Q{0},{1}:{2}] for questions
        /// and [A{0}:{2}{3}] for answers into a list of QuestionClass objects.
        /// </summary>
        /// <param name="sInput"></param>
        /// <param name="lQCInput"></param>
        /// <returns></returns>
        private bool Decode(string sInput, out List<QuestionClass> lQCInput)
        {
            List<string> lsArgs = new List<string>();
            try
            {
                List<QuestionClass> l_lQCInput = new List<QuestionClass>();
                if (sInput.Length == 0) // If given string is empty
                {
                    lQCInput = null;
                    return false;
                }

                // Break the input into package -> [*]
                for (int l_index = 0; l_index < sInput.Length - 1; )
                {
                    lsArgs.Add(sInput.SubstringCharToChar('[', ']', l_index + 1 * Convert.ToInt32(l_index != 0), true, true));
                    l_index = sInput.IndexOf(']', l_index + 1);
                }

                // For every package transform the data from the string into question objects
                for (int i = 0; i < lsArgs.Count; i++)
                {
                    if (lsArgs[i][1] == 'Q')
                    {
                        int nQuestionNum = Int32.Parse(lsArgs[i].SubstringCharToChar(',', 1, false, false));
                        l_lQCInput.AddQuestionIfNotExist(nQuestionNum);
                        string sQuestion = String.Format("{0} ({1} точки)", lsArgs[i].SubstringCharToChar(':', ']'), lsArgs[i].SubstringCharToChar(',', ':'));
                        l_lQCInput[nQuestionNum].Question = sQuestion;
                        l_lQCInput[nQuestionNum].Points = Int32.Parse(lsArgs[i].SubstringCharToChar(',', ':'));
                    }
                    else if (lsArgs[i][1] == 'A')
                    {
                        int nQuestionNum = Int32.Parse(lsArgs[i].SubstringCharToChar('A', ':'));
                        bool bIsRightAnswer = lsArgs[i].Contains("{}");
                        string sAnswer = lsArgs[i].SubstringCharToChar((bIsRightAnswer ? '}' : ':'), ']');

                        l_lQCInput.AddQuestionIfNotExist(nQuestionNum);
                        l_lQCInput[nQuestionNum].AddAnswer(sAnswer, bIsRightAnswer);
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
                lQCInput = null;
                return false;
            }
        }

        /// <summary>
        /// Encrypts a text with a password. The passCode is the sum of all the password's chars as numbers.
        /// The passcode is then added to each char of the text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private string Encrypt(ref string text, string password)
        {
            string sRes = String.Empty;
            int nPassSize = password.Length,
                nPassCode = 0;

            for (int i = 0; i < nPassSize; i++)
            {
                nPassCode += (int)(password[i]); // Add to the passcode the integer representation of the current char from the password
            }

            for (int i = 0; i < text.Length; i++)
            {
                sRes = sRes.Insert(sRes.Length, ((char)((int)(text[i]) + nPassCode + nPassSize)).ToString()); // Add to the result string the string representation of the char which code is the code of the current char + passcode + the password's length
            }
            return sRes;
        }

        /// <summary>
        /// Decrypts the input text with the password given.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private string Decrypt(string text, string password)
        {
            string sRes = String.Empty;
            int nPassSize = password.Length,
                nPassCode = 0;

            for (int i = 0; i < nPassSize; i++)
            {
                nPassCode += (int)(password[i]); // Add to the passcode the integer representation of the current char from the password
            }

            for (int i = 0; i < text.Length; i++)
            {
                sRes = sRes.Insert(sRes.Length, ((char)(((int)(text[i])) - nPassCode - nPassSize)).ToString()); // Add to the result string the string representation of the char which code is the code of the current char + the passcode - the password's length
            }
            return sRes;
        }
    }
}