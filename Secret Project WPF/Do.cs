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
        private void DoAddQuestion(ref TabControl tc, QuestionClass QCQuestion, int nQuestionNumber, bool bAddReadyButton = false)
        {
            Grid gr = new Grid();

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
                btReady.Click += DoButtonReady_Click;
                gr.Children.Add(btReady);
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
            gr.Children.Add(sv);

            //List<RadioButton> lrbAnswers = new List<RadioButton>(4);
            if (g_l2rbAnswers[nQuestionNumber].Count != 0)
            {
                MessageBox.Show("DoAddQuestion() Error: RadioButton list not empty!");
                return;
            }
            for (int i = 0; i < Math.Min(QCQuestion.Answers.Count, 4); i++)
            {
                g_l2rbAnswers[nQuestionNumber].Add(new RadioButton());
                g_l2rbAnswers[nQuestionNumber][i].Content = QCQuestion.Answers[i].Value;
                g_l2rbAnswers[nQuestionNumber][i].Checked += rbAnswer_Checked;
                ScrollViewer svv = new ScrollViewer();
                svv.Height = 40;
                svv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                svv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                svv.Focusable = false;
                svv.Content = g_l2rbAnswers[nQuestionNumber][i];
                AutomationProperties.SetAutomationId(svv, "scrollview_answer");
                svv.HorizontalAlignment = HorizontalAlignment.Left;
                svv.VerticalAlignment = VerticalAlignment.Top;
                svv.Height = 23;
                svv.Width = 420;
                svv.Margin = new Thickness(10, 70 + i * 25, 0, 0);
                gr.Children.Add(svv);
            }

            g_lLblTimeLeft.Add(new Label());
            Label label = g_lLblTimeLeft[g_lLblTimeLeft.Count - 1];
            AutomationProperties.SetAutomationId(label, "label_timeLeft");
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            label.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            
            label.Content = QuestionClass.Time.ToString();
            label.FontSize = 15;

            gr.Children.Add(label);
            gr.Children.Add(g_lICImages[nQuestionNumber].wfh);
            g_lICImages[nQuestionNumber].Show();
            TabItem ti = new TabItem();
            ti.Content = gr;
            g_lTITabs.Add(ti);
            ti.GotFocus += DoTabItem_GotFocus;
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

        private void rbAnswer_Checked(object sender, RoutedEventArgs e)
        {
                for (int j = 0; j < g_l2rbAnswers[currentQuestionNum].Count; j++)
                {
                    if(g_l2rbAnswers[currentQuestionNum][j].IsChecked == true &&
                        !Object.ReferenceEquals(g_l2rbAnswers[currentQuestionNum][j], sender))
                    {
                        g_l2rbAnswers[currentQuestionNum][j].IsChecked = false;
                    }
                }
        }

        private void DoTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            currentQuestionNum = tabControl.SelectedIndex - 1;
        }

        private void DoButtonReady_Click(object sender, RoutedEventArgs e)
        {
            Ready();
        }

        private void Ready()
        {
            QuestionClass.ResetTimer();

            int score = 0, totalScore = 0, nNumberOfRightAnswers = 0;
            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                bool bIsRightAnswerChecked = g_lQCQuestions[i].IsRightAnswerChecked(g_l2rbAnswers[i]);
                score += Convert.ToInt32(bIsRightAnswerChecked) * g_lQCQuestions[i].Points;
                totalScore += g_lQCQuestions[i].Points;
                nNumberOfRightAnswers += Convert.ToInt32(bIsRightAnswerChecked);
            }

            if ((g_lTITabs[g_lTITabs.Count - 1].Content as Grid) != null)
            {
                UIElementCollection children = (g_lTITabs[g_lTITabs.Count - 1].Content as Grid).Children;
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] is Button &&
                        AutomationProperties.GetAutomationId(children[i] as Button) == "button_ready")
                    {
                        (children[i] as Button).Content = String.Format("{0} точки", score);
                        (children[i] as Button).IsEnabled = false;
                        break;
                    }
                }
            }

            MessageBox.Show(String.Format("{0}/{1} верни отговора ({2}/{3} точки)", nNumberOfRightAnswers, g_lQCQuestions.Count, score.ToString(), totalScore.ToString()), "Резултат", MessageBoxButton.OK, MessageBoxImage.Information);

            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                int? nnChecked = g_l2rbAnswers[i].GetCheckedIndex(), nnRight = g_lQCQuestions[i].GetRightAnswerIndex();
                if (nnChecked != nnRight && nnChecked != null && nnRight != null)
                {
                    g_l2rbAnswers[i][(int)nnChecked].Foreground = Brushes.Red;
                }
                if (nnRight != null)
                {
                    g_l2rbAnswers[i][(int)nnRight].Foreground = Brushes.Green;
                }
            }

            for (int i = 0; i < g_l2rbAnswers.Count; i++)
            {
                for (int j = 0; j < g_l2rbAnswers[i].Count; j++)
                {
                    g_l2rbAnswers[i][j].IsEnabled = false;
                }
            }

            state = TestState.DoingNothing;
            ResizeAndAdjust();
        }

        private void TimeOutLabelManage()
        {
            for (int i = 1; i < g_lTITabs.Count; i++)
            {
                var children = (g_lTITabs[i].Content as Grid).Children;
                for (int j = 0; j < children.Count; j++)
                {
                    string sID = AutomationProperties.GetAutomationId(children[j]);
                    if (sID == "label_timeLeft")
                    {
                        (children[j] as Label).Foreground = Brushes.Red;
                        break;
                    }
                }
            }
        }

        private void TimerElapsedLabelManage()
        {
            for (int i = 0; i < g_lLblTimeLeft.Count; i++)
			{
                g_lLblTimeLeft[i].Content = QuestionClass.Time.ToString();
			}
        }

        private void HideTimerLabels()
        {
            for (int i = 1; i < g_lTITabs.Count; i++)
            {
                var children = (g_lTITabs[i].Content as Grid).Children;
                for (int j = 0; j < children.Count; j++)
                {
                    string sID = AutomationProperties.GetAutomationId(children[j]);
                    if (sID == "label_timeLeft")
                    {
                        (children[j] as Label).Visibility = System.Windows.Visibility.Hidden;
                        break;
                    }
                }
            }
        }
    }
}
