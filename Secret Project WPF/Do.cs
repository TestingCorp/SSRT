using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace Secret_Project_WPF
{
    public partial class MainWindow : Window
    {
        private void DoAddQuestion(ref TabControl tc,
                                   QuestionClass QCQuestion,
                                   int nQuestionNumber,
                                   bool bAddReadyButton = false)
        {
            Grid grid = new Grid();

            if (bAddReadyButton)
            {
                Button btReady = new Button();
                AutomationProperties.SetAutomationId(btReady, "button_ready");
                btReady.HorizontalAlignment = HorizontalAlignment.Left;
                btReady.VerticalAlignment = VerticalAlignment.Top;
                btReady.Width = 87;
                btReady.Height = 25;
                btReady.Content = "Готов съм";
                btReady.Margin = new Thickness(tabControl.Width - 100, tabControl.Height - 70, 0, 0);
                btReady.Click += (object sender, RoutedEventArgs e) => FinishTest();
                grid.Children.Add(btReady);
            }

            ScrollViewer sv = new ScrollViewer();
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            sv.Focusable = false;
            TextBlock tblQuestion = new TextBlock();
            sv.Content = tblQuestion;
            AutomationProperties.SetAutomationId(sv, "scrollviewer_question");
            sv.HorizontalAlignment = HorizontalAlignment.Left;
            sv.VerticalAlignment = VerticalAlignment.Top;
            tblQuestion.Width = 50;
            sv.Height = 50;
            sv.Margin = new Thickness(10, 10, 0, 0);
            tblQuestion.Text = QCQuestion.Question;
            tblQuestion.TextWrapping = TextWrapping.Wrap;
            grid.Children.Add(sv);

            //List<RadioButton> lrbAnswers = new List<RadioButton>(4);
            if (g_l2rbAnswers[nQuestionNumber].Count != 0)
            {
                MessageBox.Show("DoAddQuestion() Error: RadioButton list not empty!");
                return;
            }
            List<Label> llblAnswers = new List<Label>();
            for (int i = 0; i < Math.Min(QCQuestion.Answers.Count, 4); i++)
            {
                g_l2rbAnswers[nQuestionNumber].Add(new RadioButton());
                g_l2rbAnswers[nQuestionNumber][i].Margin = new Thickness(10, 72 + i * 25, 0, 0);
                g_l2rbAnswers[nQuestionNumber][i].HorizontalAlignment = HorizontalAlignment.Left;
                g_l2rbAnswers[nQuestionNumber][i].VerticalAlignment = VerticalAlignment.Top;
                grid.Children.Add(g_l2rbAnswers[nQuestionNumber][i]);

                llblAnswers.Add(new Label());
                llblAnswers[i].Content = QCQuestion.Answers[i].Value;
                llblAnswers[i].VerticalAlignment = VerticalAlignment.Top;

                ScrollViewer svv = new ScrollViewer();
                string sID = String.Format("scrollview_answer_{0}", i);
                AutomationProperties.SetAutomationId(svv, sID);
                svv.Height = 26;
                svv.Width = 420;
                svv.Margin = new Thickness(10 + 13, 65 + i * 25, 0, 0);
                svv.VerticalAlignment = VerticalAlignment.Top;
                svv.HorizontalAlignment = HorizontalAlignment.Left;
                svv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                svv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                svv.Focusable = false;
                svv.Content = llblAnswers[i];
                grid.Children.Add(svv);
            }

            List<Label> llblTimeLeft = new List<Label>();
            llblTimeLeft.Add(new Label());
            Label label = llblTimeLeft[llblTimeLeft.Count - 1];
            AutomationProperties.SetAutomationId(label, "label_timeLeft");
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            label.VerticalAlignment = System.Windows.VerticalAlignment.Top;

            label.Content = QuestionClass.Time.ToString();
            label.FontSize = 15;

            grid.Children.Add(label);
            grid.Children.Add(g_lICImages[nQuestionNumber].wfh);
            g_lICImages[nQuestionNumber].Show();
            TabItem ti = new TabItem();
            ti.Content = grid;
            g_lTITabs.Add(ti);
            ti.Header = String.Format("Въпрос {0}", (tc.Items.Count - 1 < 1) ? 1 : (tc.Items.Count - 1));

            ImageClass currentImage = g_lICImages[nQuestionNumber];
            currentImage.picBox.MouseDown += ImageClass.picBox_MouseDown;
            currentImage.picBox.MouseUp +=
                (object mouseUpSender, System.Windows.Forms.MouseEventArgs mouseUpE) =>
                {
                    ImageClass.pictureOpenMethods += ResizeAndAdjust;
                    ImageClass.pictureOpenMethods += DisableControls;

                    ImageClass.pictureCloseMethods += ResizeAndAdjust;
                    ImageClass.pictureCloseMethods += EnableControls;

                    currentImage.picBox_MouseUp(mouseUpSender, mouseUpE);
                };
        }

        /// <summary>
        /// When the time has ran out or the user has pressed the Ready button
        /// calculates the score, shows a message and takes care of the color
        /// of the text.
        /// </summary>
        private void FinishTest()
        {
            QuestionClass.ResetTimer();

            // Calculate the scores
            int score = 0, totalScore = 0, numberOfRightAnswers = 0;
            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                bool bIsRightAnswerChecked = g_lQCQuestions[i].IsRightAnswerChecked(g_l2rbAnswers[i]);

                // Add the points for the question to the total score
                totalScore += g_lQCQuestions[i].Points;

                // If the right answer has been checked, add the points for the question to the score and increase the number of right answers
                if (bIsRightAnswerChecked)
                {
                    score += g_lQCQuestions[i].Points;
                    numberOfRightAnswers++;
                }
            }

            // Shows a message with an information about the number of right answers and the score
            string message = String.Format("{0}/{1} верни отговора ({2}/{3} точки)", numberOfRightAnswers, g_lQCQuestions.Count, score.ToString(), totalScore.ToString());
            MessageBox.Show(message, "Резултат", MessageBoxButton.OK, MessageBoxImage.Information);

            // Changes the foreground of right answers to green and guessed wrong answers to red
            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                int? nnChecked = g_l2rbAnswers[i].GetCheckedIndex(),
                     nnRight = g_lQCQuestions[i].GetRightAnswerIndex();

                if (nnChecked != nnRight && nnChecked != null && nnRight != null) // If user has guessed the answer wrong
                {
                    string sID = string.Format("scrollview_answer_{0}", (int)nnChecked);
                    object scrollViewer = (GetObjectById(sID, i + 1));
                    if (scrollViewer != null) 
                    {
                        Label label = ((scrollViewer as ScrollViewer).Content as Label);
                        label.Foreground = Brushes.Red;
                    }
                }

                if (nnRight != null)
                {
                    string sID = string.Format("scrollview_answer_{0}", (int)nnRight);
                    object scrollViewer = (GetObjectById(sID, i + 1));
                    if (scrollViewer != null) 
                    {
                        Label label = ((scrollViewer as ScrollViewer).Content as Label);
                        label.Foreground = Brushes.Green;
                    }
                }
            }

            // Set the content of the ready button in the last tab to the score and disable it
            if ((g_lTITabs[g_lTITabs.Count - 1].Content as Grid) != null) // If the grid of the last tab is not null
            {
                object button = GetObjectById("button_ready", g_lTITabs.Count - 1);
                if (button != null)
                {
                    (button as Button).Content = String.Format("{0} точки", score);
                    (button as Button).IsEnabled = false;
                }
            }

            // Disable all the RadioButtons
            for (int i = 0; i < g_l2rbAnswers.Count; i++)
            {
                for (int j = 0; j < g_l2rbAnswers[i].Count; j++)
                {
                    g_l2rbAnswers[i][j].IsEnabled = false;
                }
            }

            state = TestState.DoingNothing; // Reset the state of the test
            ResizeAndAdjust();
        }

        /// <summary>
        /// Changes the foreground of all time labels to red
        /// </summary>
        private void TimeOutLabelManage()
        {
            for (int i = 1; i < g_lTITabs.Count; i++)
            {
                object label = GetObjectById("label_timeLeft", i);
                if (label != null) (label as Label).Foreground = Brushes.Red;
            }
        }

        /// <summary>
        /// Each second updates the time in each of the time labels
        /// </summary>
        private void TimerElapsedLabelManage()
        {
            for (int i = 1; i < g_lTITabs.Count; i++)
            {
                object label = GetObjectById("label_timeLeft", i);
                if (label != null) (label as Label).Content = QuestionClass.Time.ToString();
            }
        }

        /// <summary>
        /// Sets the Visibility property of the time labels to Hidden
        /// </summary>
        private void HideTimerLabels()
        {
            for (int i = 1; i < g_lTITabs.Count; i++)
            {
                object label = GetObjectById("label_timeLeft", i);
                if (label != null) (label as Label).Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
