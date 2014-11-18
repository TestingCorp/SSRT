﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Forms.Integration;
using System.Text.RegularExpressions;

namespace Secret_Project_WPF
{
    public partial class MainWindow : Window
    {


        public static List<Label> listOfLabels = new List<Label>();


        /// <summary>
        /// While creating a test when the last tab AddQuestion is selected this method creates and adds a new question to the TabControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTabItemAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            //AddQuestion(ref tabControl, ManageStringLines("How longdlkasmdv;alsmd;lcakmds ;lakmdsc lakmds;clkasdlcmaslkm asdc?How lpo nmklmlmlkkd v;alsmd;lcakmds ;lakmdsc lakmds;clkasdlcmaslkm asdc?"), new string[] { "No", "Idea", "What", "To do" });
            g_lQCQuestions.AddQuestionIfNotExist();
            TabItem ti = new TabItem();
            ti.Content = CreateAddQuestion(new string[] { "[отговор 1]", "[отговор 2]", "[отговор 3]", "[отговор 4]" });
            ti.Header = String.Format("Въпрос {0}", (g_lTITabs.Count - 1 <= 1) ? 1 : (g_lTITabs.Count - 1));
            ti.GotFocus += CreateTabItem_GotFocus;
            (ti.Content as Grid).Width = window1.ActualWidth;
            g_lTITabs.Add(ti);
            g_lTITabs.Remove(sender as TabItem);
            g_lTITabs.Add(sender as TabItem);
            ResizeAndAdjust();
            //tabControl.Items.Remove(sender);
            //tabControl.Items.Add(sender);
            tabControl.SelectedIndex = tabControl.Items.Count - 2;

        }

        /// <summary>
        /// Creates a grid with a question with all its parts - textBoxes and Buttons.
        /// </summary>
        /// <param name="tc"></param>
        /// <param name="asAnswers"></param>
        Grid CreateAddQuestion(string[] asAnswers)
        {
            Grid gr = new Grid();
            gr.Width = tabControl.Width;
            //gr.Background = Brushes.Aqua;
            //gr.Height = tabControl.Height-40;
            gr.Margin = new Thickness(0, -5, 0, 0);

            Button btReady = new Button();
            AutomationProperties.SetAutomationId(btReady, "button_ready");
            btReady.HorizontalAlignment = HorizontalAlignment.Left;
            btReady.VerticalAlignment = VerticalAlignment.Top;
            btReady.Width = 87;
            btReady.Height = 25;
            //btReady.Margin = new Thickness(tabControl.Width - 115, tabControl.Height - 70, 0, 0);
            btReady.Content = "Готов съм";
            btReady.Click += CreateButtonReady_Click;
            gr.Children.Add(btReady);

            Button btRemove = new Button();
            AutomationProperties.SetAutomationId(btRemove, "button_remove");
            btRemove.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btRemove.VerticalAlignment = VerticalAlignment.Top;
            btRemove.Width = 87;
            btRemove.Height = 25;
            //btRemove.Margin = new Thickness(tabControl.Width - 115 - 87 - 10, tabControl.Height - 70, 0, 0);
            btRemove.Content = "Изтриване";
            btRemove.Click += CreateButtonRemove_Click;
            gr.Children.Add(btRemove);

            TextBox tbQuestion = new TextBox();
            AutomationProperties.SetAutomationId(tbQuestion, "textbox_question");
            tbQuestion.HorizontalAlignment = HorizontalAlignment.Left;
            tbQuestion.VerticalAlignment = VerticalAlignment.Top;
            tbQuestion.AcceptsReturn = true;
            tbQuestion.TextWrapping = TextWrapping.Wrap;
            //tbQuestion.Width = tabControl.Width-30-150;
            tbQuestion.Height = 50;
            tbQuestion.Margin = new Thickness(10, 8, 0, 0);
            tbQuestion.Text = "[въпрос]";
            tbQuestion.GotFocus += CreateTextBox_GotFocus;
            tbQuestion.LostFocus += CreateTextBox_LostFocus;
            gr.Children.Add(tbQuestion);

            List<TextBox> ltbAnswers = new List<TextBox>();
            g_l2rbAnswers.Add(new List<RadioButton>());
            for (int i = 0; i < Math.Min(asAnswers.Count(), 4); i++)
            {
                ltbAnswers.Add(new TextBox());
                int nLastIndex = ltbAnswers.Count - 1;

                string sID = String.Format("textbox_answer_{0}", i);
                AutomationProperties.SetAutomationId(ltbAnswers[nLastIndex], sID);
                ltbAnswers[nLastIndex].Text = asAnswers[i];
                ltbAnswers[nLastIndex].HorizontalAlignment = HorizontalAlignment.Left;
                ltbAnswers[nLastIndex].VerticalAlignment = VerticalAlignment.Top;
                ltbAnswers[nLastIndex].Height = 20;
                //ltbAnswers[nLastIndex].Width = tabControl.Width - 30 -150-13;
                ltbAnswers[nLastIndex].Margin = new Thickness(13 + 10, 70 + i * 25, 0, 0);
                ltbAnswers[nLastIndex].GotFocus += CreateTextBox_GotFocus;
                ltbAnswers[nLastIndex].LostFocus +=
                    CreateTextBox_LostFocus;
                gr.Children.Add(ltbAnswers[nLastIndex]);

                nLastIndex = g_l2rbAnswers.Count - 1;
                g_l2rbAnswers[nLastIndex].Add(new RadioButton());
                g_l2rbAnswers[nLastIndex][i].Width = 13;
                g_l2rbAnswers[nLastIndex][i].Height = 13;
                g_l2rbAnswers[nLastIndex][i].Margin = new Thickness(10, 73 + i * 25, 0, 0);
                g_l2rbAnswers[nLastIndex][i].HorizontalAlignment = HorizontalAlignment.Left;
                g_l2rbAnswers[nLastIndex][i].VerticalAlignment = VerticalAlignment.Top;
                g_l2rbAnswers[nLastIndex][i].Checked += CreateRadioButtonAnswer_CheckedChanged;
                g_l2rbAnswers[nLastIndex][i].Unchecked += CreateRadioButtonAnswer_CheckedChanged;
                gr.Children.Add(g_l2rbAnswers[nLastIndex][i]);

                if (i == Math.Min(asAnswers.Count(), 4) - 1)
                {
                    TextBox tbPoints = new TextBox();
                    AutomationProperties.SetAutomationId(tbPoints, "textbox_points");
                    tbPoints.Width = 87;
                    tbPoints.Height = 25;
                    tbPoints.HorizontalAlignment = HorizontalAlignment.Left;
                    tbPoints.VerticalAlignment = VerticalAlignment.Top;
                    tbPoints.Text = "[брой точки]";
                    tbPoints.Margin = new Thickness(10, tabControl.Height - 70, 0, 0);
                    tbPoints.GotFocus += CreateTextBox_GotFocus;
                    tbPoints.LostFocus += CreateTextBox_LostFocus;
                    gr.Children.Add(tbPoints);

                    TextBox timer = new TextBox();
                    AutomationProperties.SetAutomationId(timer, "textbox_timer");
                    timer.Width = 100;
                    timer.Height = 25;
                    timer.HorizontalAlignment = HorizontalAlignment.Left;
                    timer.VerticalAlignment = VerticalAlignment.Top;
                    timer.Text = "[време (мин:сек)]";
                    timer.GotFocus += CreateTextBox_GotFocus;
                    timer.LostFocus += CreateTextBox_LostFocus;
                    gr.Children.Add(timer);
                }
            }

            Button browse = new Button();
            AutomationProperties.SetAutomationId(browse, "button_browse");
            browse.HorizontalAlignment = HorizontalAlignment.Left;
            browse.VerticalAlignment = VerticalAlignment.Top;
            browse.Width = 95;
            browse.Height = 25;
            browse.Content = "+Изображение";
            browse.Click += new RoutedEventHandler(CreateButtonBrowse_Click);
            gr.Children.Add(browse);

            g_lICImages.Add(new ImageClass());
            //WindowsFormsHost wfh = new WindowsFormsHost();

            /*wfh.Width = tabControl.ActualWidth - 350;
            wfh.Height = tabControl.ActualHeight - 95;*/

            //wfh.Margin = new Thickness(tabControl.Width - wfh.Width - 15, 9, 0, 0);
            //wfh.Child = g_lICImages[g_lICImages.Count - 1].picBox;
            gr.Children.Add(g_lICImages[g_lICImages.Count - 1].wfh);
            return gr;
        }

        void CreateTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            g_nCurrQuestion = tabControl.SelectedIndex - 1;
        }

        void CreateButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var browseBut = sender as Button;
            if (browseBut.Content.Equals("-Изображение"))
            {
                if (MessageBox.Show(String.Format("В момента имате добавено изображение! Сигурни ли сте, че желаете да го изтриете?"), "Внимание!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //g_lICImages[g_nCurrQuestion].img.Source = null;
                    g_lICImages[g_nCurrQuestion].picBox.Image.Dispose();
                    g_lICImages[g_nCurrQuestion].picBox.Image = null;
                    g_lICImages[g_nCurrQuestion].SetSize(0, 0);
                    ResizeAndAdjust();
                    browseBut.Content = "+Изображение";
                }
                return;
            }

            OpenFileDialog imageDialog = new OpenFileDialog();
            imageDialog.Multiselect = false;
            imageDialog.Filter = "Image File (.jpg, .jpeg, .bmp, .png)|*.jpg; *.jpeg; *.bmp; *.png";
            imageDialog.FilterIndex = 1;

            bool? nbClickedOK = imageDialog.ShowDialog();
            if (nbClickedOK == false || nbClickedOK == null) return;
            browseBut.Content = "-Изображение";

            System.Drawing.Image img = System.Drawing.Image.FromFile(imageDialog.FileName);

            double imageRatio = (double)(img.Width) / img.Height;
            int controlsHeight = (int)(tabControl.ActualHeight - 95);
            int controlsWidth = (int)(imageRatio * controlsHeight);

            ImageClass currentImage = g_lICImages[g_nCurrQuestion];
            currentImage.picBox.Image = img;
            currentImage.SetSize(controlsWidth, controlsHeight);
            currentImage.Show();

            currentImage.picBox.MouseDown += ImageClass.picBox_MouseDown;
            currentImage.picBox.MouseUp += /*picBox_MouseUp*/
                (object mouseUpSender, System.Windows.Forms.MouseEventArgs mouseUpE) =>
                {
                    ImageClass.pictureOpenMethods += ResizeAndAdjust;
                    ImageClass.pictureOpenMethods += DisableControls;

                    ImageClass.pictureCloseMethods += ResizeAndAdjust;
                    ImageClass.pictureCloseMethods += EnableControls;

                    currentImage.picBox_MouseUp(mouseUpSender, mouseUpE);
                };

            ResizeAndAdjust();
        }

        /// <summary>
        /// If the remove button is clicked this method asks if the user is sure about deleting the question
        /// and if yes - does just that and moves to the next question (if any)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Сигурни ли сте, че искате да изтриете този въпрос?", "Внимание!",
                MessageBoxButton.YesNo) == MessageBoxResult.No) return;
            g_lQCQuestions.RemoveAt(g_nCurrQuestion);
            g_l2rbAnswers.RemoveAt(g_nCurrQuestion);
            tabControl.SelectedIndex = 0;
            g_lTITabs.RemoveAt(g_nCurrQuestion + 1);
            g_lICImages.RemoveAt(g_nCurrQuestion);
            if (g_lTITabs.Count == 2)
                tabControl.SelectedIndex = g_lTITabs.Count - 1;
            else if (g_nCurrQuestion > 0)
                g_nCurrQuestion--;
            tabControl.SelectedIndex = g_nCurrQuestion + 1;
            for (int i = g_nCurrQuestion + 1; i < g_lTITabs.Count - 1; i++)
                g_lTITabs[i].Header = String.Format("Въпрос {0}", i);
        }

        /// <summary>
        /// If the button "Готов съм" is clicked this method checks if there are invalid textboxes or radio buttons.
        /// If there are any - shows an error. If everything is fine - asks for the location of the output file and saves the encoded and encrypted output there.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateButtonReady_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < g_lQCQuestions.Count; i++)
            {
                switch (g_lQCQuestions[i].IsSomethingWrong(g_l2rbAnswers[i]))
                {
                    case ISE_ErrorCode.NoAnswers:
                        MessageBox.Show(String.Format("Не сте въвели нито един отговор при въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.NoQuestion:
                        MessageBox.Show(String.Format("Не сте въвели въпрос при въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.NoRightAnswer:
                        MessageBox.Show(String.Format("Не сте избрали верен отговор при въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.NoPoints:
                        MessageBox.Show(String.Format("Не сте въвели брой точки за верен отговор на въпрос {0}!", i + 1));
                        return;
                    case ISE_ErrorCode.AllFine:
                        break;
                }
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "MGTest files (.mgt)|*.mgt";
            saveFileDialog1.FilterIndex = 1;
            bool? nbClickedOK = saveFileDialog1.ShowDialog();
            if (nbClickedOK != true) return;
            QuestionClass.StopTimer();
            List<string> lsOutput;
            Encode(out lsOutput, g_lQCQuestions);
            List<string> enc = new List<string>();
            for (int i = 0; i < lsOutput.Count; i++)
            {
                string sOutput = lsOutput[i];
                enc.Add(Encrypt(ref sOutput, "I like spagetti!"));
            }
            using (Stream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(g_lQCQuestions.Count);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        long bytesCount = -1;
                        for (int i = 0; i < enc.Count; i++)
                        {
                            bw.Write(enc[i]);
                            if (g_lICImages[i].picBox.Image != null)
                            {
                                g_lICImages[i].picBox.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                bytesCount = ms.Length;
                                ms.Position = 0;
                            }
                            bw.Write(bytesCount);
                            if (bytesCount != -1) { ms.CopyTo(fs); ms.Position = 0; }
                        }
                        bw.Write(QuestionClass.time.Minutes);
                        bw.Write(QuestionClass.time.Seconds);

                    }
                }

                /*using (StreamWriter stream = new StreamWriter(saveFileDialog1.FileName))
                    stream.WriteLine(enc);*/
                while (g_lTITabs.Count > 1)
                    g_lTITabs.RemoveAt(g_lTITabs.Count - 1);
                g_lQCQuestions = null;
                g_l2rbAnswers = null;
                //btCreate.IsEnabled = true;
                state = TestState.DoingNothing;
                /*StreamReader streamR = new StreamReader("output.mgt");
                string dec = Decrypt(streamR.ReadLine(), "I like spagetti!");
                streamR.Close();*/
            }
        }

        /// <summary>
        /// When a radio button of an answer is unchecked updates the status of the QuestionClass object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateRadioButtonAnswer_CheckedChanged(object sender, RoutedEventArgs e)
        {
            int nAnswerNum = -1, nQuestionNum = -1;
            for (nQuestionNum = 0; nQuestionNum < g_l2rbAnswers.Count; nQuestionNum++)
                for (nAnswerNum = 0; nAnswerNum < g_l2rbAnswers[nQuestionNum].Count; nAnswerNum++)
                    if (Object.ReferenceEquals(sender, g_l2rbAnswers[nQuestionNum][nAnswerNum]))
                        goto next;
        next:
            if ((sender as RadioButton).IsChecked == true)
                g_lQCQuestions[nQuestionNum].SetRightAnswer(nAnswerNum, true);
            else
                g_lQCQuestions[nQuestionNum].SetRightAnswer(nAnswerNum, false);
        }

        /// <summary>
        /// Clears any textbox starting with "[отговор " or being "[въпрос]" or "[брой точки]" and gets focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string sText = (sender as TextBox).Text;
            if (sText == "[въпрос]" ||
                sText == "[брой точки]" ||
                sText == "[време (мин:сек)]" ||
                sText.Length > 8 &&
                sText.Substring(0, 9) == "[отговор ")
                (sender as TextBox).Text = String.Empty;
        }

        /// <summary>
        /// Whenever a textbox loses focus if it's empty this method makes its content equal to the appropriate default value.
        /// If not it updates the list of questions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CreateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //Get ID of text box (format is "textbox_answer_{number}")
            string sID = AutomationProperties.GetAutomationId(sender as TextBox);

            //Set the question number from the global variable
            int nQuestionNum = g_nCurrQuestion;

            string sContent = (sender as TextBox).Text;
            //Get the answer number from the ID and assign it to nAnswerNum

            if (sID.Length > 14 && sID.Substring(0, 14) == "textbox_answer")
            {
                int nAnswerNum = Int32.Parse(sID.Substring(15));
                //Check if the string in the textbox is empty
                if (sContent == String.Empty)
                {
                    //Set the answer in the global list of questions to be empty as well
                    g_lQCQuestions[nQuestionNum].SetAnswer(nAnswerNum, String.Empty);
                    //and set the textbox'es text equal to the appropriate default value
                    (sender as TextBox).Text = String.Format("[отговор {0}]", nAnswerNum + 1);
                }
                else
                    //If the textbox is not empty set the answer to its content
                    g_lQCQuestions[nQuestionNum].SetAnswer(nAnswerNum, sContent);
            }
            else if (sID == "textbox_question")
            {
                //Check if the string in the textbox is empty
                if (sContent == String.Empty)
                {
                    //Set the question in the global list of questions to be empty as well
                    g_lQCQuestions[g_nCurrQuestion].SetQuestion(String.Empty);
                    //and set the textbox'es text the appropriate default value
                    (sender as TextBox).Text = String.Format("[въпрос]");
                }
                else
                {
                    //If the textbox is not empty set the question equal to its content
                    g_lQCQuestions[g_nCurrQuestion].SetQuestion(sContent);
                }
            }
            else if (sID == "textbox_points")
            {
                int nRes = -1;
                //Try to convert the content of the textbox into a number
                if (!Int32.TryParse(sContent, out nRes) || nRes <= 0)
                {
                    //if it fails or the number is not positive set the textbox'es content the appropriate default value
                    (sender as TextBox).Text = "[брой точки]";
                }
                //Set the points equal to the texbox'es content
                g_lQCQuestions[g_nCurrQuestion].SetPoints(nRes);
            }
            else if (sID == "textbox_timer")
            {
                Regex r = new Regex("\\d\\d\\:\\d\\d");
                Match m = r.Match(sContent);
                if (m.Success)
                {
                    int minutes = Int32.Parse(sContent.SubstringCharToChar(':', 0, true));
                    int seconds = Int32.Parse(sContent.Substring(sContent.IndexOf(':') + 1));

                    QuestionClass.time = new TimeSpan(0, minutes, seconds);
                }
                else
                {
                    //if it fails or the number is not positive set the textbox'es content the appropriate default value
                    (sender as TextBox).Text = "[време (мин:сек)]";

                    //Set the points equal to the texbox'es content
                    // g_lQCQuestions[g_nCurrQuestion].SetPoints(nRes);
                }
            }
        }
    }
}