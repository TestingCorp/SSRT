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
using System.Windows.Markup;

namespace Secret_Project_WPF
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// While creating a test when the last tab AddQuestion is selected this method creates and adds a new question to the TabControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTabItemAdd_GotFocus(object sender, RoutedEventArgs e)
        {
            g_lQCQuestions.AddQuestionIfNotExist();
            TabItem ti = new TabItem();
            ti.Content = CreateAddQuestion(new string[] { "[отговор 1]", "[отговор 2]", "[отговор 3]", "[отговор 4]" });
            ti.Header = String.Format("Въпрос {0}", (g_lTITabs.Count - 1 <= 1) ? 1 : (g_lTITabs.Count - 1));
            ti.GotFocus += CreateTabItem_GotFocus;
            (ti.Content as Grid).Width = tabControl.Width;
            g_lTITabs.Add(ti);
            g_lTITabs.Remove(sender as TabItem);
            g_lTITabs.Add(sender as TabItem);
            ResizeAndAdjust();
            tabControl.SelectedIndex = tabControl.Items.Count - 2;
        }

        /// <summary>
        /// Creates a grid with a question with all its parts - textBoxes and Buttons.
        /// </summary>
        /// <param name="tc"></param>
        /// <param name="asAnswers"></param>
        private Grid CreateAddQuestion(string[] asAnswers)
        {
            Grid gr = new Grid();
            gr.Width = tabControl.Width;
            gr.Margin = new Thickness(0, -5, 0, 0);

            Button btReady = new Button();
            AutomationProperties.SetAutomationId(btReady, "button_ready");
            btReady.HorizontalAlignment = HorizontalAlignment.Left;
            btReady.VerticalAlignment = VerticalAlignment.Top;
            btReady.Width = 87;
            btReady.Height = 25;
            btReady.Content = "Готов съм";
            btReady.Click += CreateButtonReady_Click;
            gr.Children.Add(btReady);

            Button btRemove = new Button();
            AutomationProperties.SetAutomationId(btRemove, "button_remove");
            btRemove.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btRemove.VerticalAlignment = VerticalAlignment.Top;
            btRemove.Width = 87;
            btRemove.Height = 25;
            btRemove.Content = "Изтриване";
            btRemove.Click += CreateButtonRemove_Click;
            gr.Children.Add(btRemove);

            TextBox tbQuestion = new TextBox();
            AutomationProperties.SetAutomationId(tbQuestion, "textbox_question");
            //tbQuestion.Template = (ControlTemplate)XamlReader.Parse(Properties.Resources.CreateTextBoxQuestionTemplate);
            tbQuestion.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbQuestion.SelectionStart = tbQuestion.Text.Length;
            tbQuestion.HorizontalAlignment = HorizontalAlignment.Left;
            tbQuestion.VerticalAlignment = VerticalAlignment.Top;
            tbQuestion.AcceptsReturn = true;
            tbQuestion.TextWrapping = TextWrapping.Wrap;
            tbQuestion.Height = 70;
            tbQuestion.Margin = new Thickness(10, 8, 0, 0);
            tbQuestion.Text = "[въпрос]";
            tbQuestion.GotFocus += CreateTextBox_GotFocus;
            tbQuestion.LostFocus += CreateTextBox_LostFocus;
            gr.Children.Add(tbQuestion);

            List<TextBox> ltbAnswers = new List<TextBox>();
            g_l2rbAnswers.Add(new List<RadioButton>());
            for (int i = 0; i < Math.Min(asAnswers.Count(), 4); i++)
            {
                g_lQCQuestions[g_lQCQuestions.Count-1].AddAnswer("");
                ltbAnswers.Add(new TextBox());
                int nLastIndex = ltbAnswers.Count - 1;

                string sID = String.Format("textbox_answer_{0}", i);
                AutomationProperties.SetAutomationId(ltbAnswers[nLastIndex], sID);
                //ltbAnswers[nLastIndex].Template = (ControlTemplate)XamlReader.Parse(Properties.Resources.CreateTextBoxAnswerTemplate);
                ltbAnswers[nLastIndex].Text = asAnswers[i];
                ltbAnswers[nLastIndex].HorizontalAlignment = HorizontalAlignment.Left;
                ltbAnswers[nLastIndex].HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                //ltbAnswers[nLastIndex].Style
                ltbAnswers[nLastIndex].VerticalAlignment = VerticalAlignment.Top;
                ltbAnswers[nLastIndex].Height = 25;
                ltbAnswers[nLastIndex].Margin = new Thickness(13 + 10, 90 + i * 30, 0, 0);
                ltbAnswers[nLastIndex].GotFocus += CreateTextBox_GotFocus;
                ltbAnswers[nLastIndex].LostFocus +=
                    CreateTextBox_LostFocus;
                gr.Children.Add(ltbAnswers[nLastIndex]);

                nLastIndex = g_l2rbAnswers.Count - 1;
                g_l2rbAnswers[nLastIndex].Add(new RadioButton());
                g_l2rbAnswers[nLastIndex][i].Width = 13;
                g_l2rbAnswers[nLastIndex][i].Height = 13;
                g_l2rbAnswers[nLastIndex][i].Margin = new Thickness(10, 90 + i * 30 + 5, 0, 0);
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

                    string sTimerText = "[време (мин:сек)]";
                    if (i != 0 && g_lTITabs.Count != 2) sTimerText = (GetObjectById("textbox_timer", 1) as TextBox).Text as String;
                    timer.Text = sTimerText;

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
            gr.Children.Add(g_lICImages[g_lICImages.Count - 1].wfh);
            return gr;
        }

        private void CreateTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            currentQuestionNum = tabControl.SelectedIndex - 1;
        }

        private void CreateButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var browseBut = sender as Button;
            if (browseBut.Content.Equals("-Изображение"))
            {
                if (MessageBox.Show(String.Format("В момента имате добавено изображение! " +
                                                  "Сигурни ли сте, че желаете да го изтриете?"),
                                                  "Внимание!",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    g_lICImages[currentQuestionNum].picBox.Image.Dispose();
                    g_lICImages[currentQuestionNum].picBox.Image = null;
                    g_lICImages[currentQuestionNum].SetSize(0, 0);
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

            ImageClass currentImage = g_lICImages[currentQuestionNum];
            currentImage.picBox.Image = img;
            currentImage.SetSize(controlsWidth, controlsHeight);
            currentImage.Show();

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

            ResizeAndAdjust();
        }

        /// <summary>
        /// If the remove button is clicked this method asks if the user is sure about deleting the question
        /// and if yes - does just that and moves to the next question (if any)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Сигурни ли сте, че искате да изтриете този въпрос?",
                                "Внимание!",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            g_lQCQuestions.RemoveAt(currentQuestionNum);
            g_l2rbAnswers.RemoveAt(currentQuestionNum);

            tabControl.SelectedIndex = 0;
            g_lTITabs.RemoveAt(currentQuestionNum + 1);
            g_lICImages.RemoveAt(currentQuestionNum);
            if (g_lTITabs.Count == 2)
            {
                tabControl.SelectedIndex = g_lTITabs.Count - 1;
            }
            else if (currentQuestionNum > 0)
            {
                currentQuestionNum--;
            }
            tabControl.SelectedIndex = currentQuestionNum + 1;

            for (int i = tabControl.SelectedIndex; i < g_lTITabs.Count - 1; i++)
            {
                g_lTITabs[i].Header = String.Format("Въпрос {0}", i);
            }
        }

        /// <summary>
        /// If the button "Готов съм" is clicked this method checks if there are invalid textboxes or radio buttons.
        /// If there are any - shows an error. If everything is fine - asks for the location of the output file and saves the encoded and encrypted output there.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButtonReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < g_lQCQuestions.Count; i++)
                {
                    TestErrorCode errorCode = g_lQCQuestions[i].IsSomethingWrong(g_l2rbAnswers[i]);
                    if (errorCode == TestErrorCode.AllFine) continue;

                    string sError = String.Empty;
                    switch (errorCode)
                    {
                        case TestErrorCode.NoQuestion:
                            sError = "Не сте въвели въпрос при";
                            break;
                        case TestErrorCode.NoAnswers:
                            sError = "Не сте въвели нито един отговор при";
                            break;
                        case TestErrorCode.TooFewAnswers:
                            sError = "Въвели сте прекалено малко отговори при ";
                            break;
                        case TestErrorCode.DuplicateAnswers:
                            sError = "Въвели сте два еднакви отговора при ";
                            break;
                        case TestErrorCode.NoRightAnswer:
                            sError = "Не сте избрали верен отговор при";
                            break;
                        case TestErrorCode.NoPoints:
                            sError = "Не сте въвели брой точки за верен отговор на";
                            break;
                    }
                    MessageBox.Show(String.Format(sError + " въпрос {0}!", i + 1), "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (QuestionClass.Time == TimeSpan.Zero)
                {
                    if (MessageBox.Show("Не сте избрали време за решаване на теста! Сигурни ли " +
                                        "сте, че не искате да задавате време?",
                                        "Внимание",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "MGTest files (.mgt)|*.mgt";
                saveFileDialog1.FilterIndex = 1;

                bool? nbClickedOK = saveFileDialog1.ShowDialog();
                if (nbClickedOK != true) return;

                List<string> lsOutput;
                if (!Encode(out lsOutput, g_lQCQuestions))
                {
                    throw new OperationFailedException("Кодирането на данните се провали.");
                }

                List<string> enc = new List<string>();
                for (int i = 0; i < lsOutput.Count; i++)
                {
                    string sOutput = lsOutput[i];
                    enc.Add(Encrypt(ref sOutput, "I like spagetti!"));
                }

                using (Stream fileStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        binaryWriter.Write(g_lQCQuestions.Count);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            long bytesCount = -1;
                            for (int i = 0; i < enc.Count; i++)
                            {
                                binaryWriter.Write(enc[i]);
                                if (g_lICImages[i].picBox.Image != null)
                                {
                                    System.Drawing.Bitmap resizedImage;
                                    System.Drawing.Size newSize = new System.Drawing.Size();
                                    newSize.Height = 500;
                                    double imageRatio = (double)(g_lICImages[i].picBox.Image.Width) /
                                                                 g_lICImages[i].picBox.Image.Height;
                                    newSize.Width = (int)(imageRatio * newSize.Height);
                                    resizedImage = new System.Drawing.Bitmap(g_lICImages[i].picBox.Image, newSize);
                                    resizedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    bytesCount = ms.Length;
                                    ms.Position = 0;
                                }
                                binaryWriter.Write(bytesCount);
                                if (bytesCount != -1) { ms.CopyTo(fileStream); ms.Position = 0; }
                            }
                            binaryWriter.Write(QuestionClass.Time.Minutes);
                            binaryWriter.Write(QuestionClass.Time.Seconds);
                        }
                    }

                    state = TestState.DoingNothing;
                    ResetTest();
                }
            }
            catch (OperationFailedException exc)
            {
                MessageBox.Show("Възникна грешка при записването на теста. Грешката показва следното съобщение:{0}",
                                exc.Message);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Възникна непозната грешка при записването на теста. " +
                    "Грешката показва следното съобщение:{0}", exc.Message);
            }
        }

        /// <summary>
        /// When a radio button of an answer is unchecked updates the status of the QuestionClass object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateRadioButtonAnswer_CheckedChanged(object sender, RoutedEventArgs e)
        {
            int nAnswerNum = -1, nQuestionNum = -1;
            for (nQuestionNum = 0; nQuestionNum < g_l2rbAnswers.Count; nQuestionNum++)
                for (nAnswerNum = 0; nAnswerNum < g_l2rbAnswers[nQuestionNum].Count; nAnswerNum++)
                    if (Object.ReferenceEquals(sender, g_l2rbAnswers[nQuestionNum][nAnswerNum]))
                        goto next;
        next:
            if ((sender as RadioButton).IsChecked == true)
            {
                string correspondingTextboxId = String.Format("textbox_answer_" + nAnswerNum);
                TextBox correspondingTextbox = GetObjectById(correspondingTextboxId, currentQuestionNum + 1) as TextBox;
                if (correspondingTextbox.Text == String.Empty ||
                    correspondingTextbox.Text.Length > 8 &&
                    correspondingTextbox.Text.Substring(0, 9) == "[отговор ")
                {
                    MessageBox.Show("Моля въведете отговор преди да сте го отбелязали като верен!",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    (sender as RadioButton).IsChecked = false;
                }
                g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].IsRightAnswer = true;
            }
            else
                g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].IsRightAnswer = false;
        }

        /// <summary>
        /// Clears any textbox starting with "[отговор " or being "[въпрос]" or "[брой точки]" and gets focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateTextBox_GotFocus(object sender, RoutedEventArgs e)
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
        private void CreateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //Get ID of text box (format is "textbox_answer_{number}")
            string sID = AutomationProperties.GetAutomationId(sender as TextBox);

            //Set the question number from the global variable
            int nQuestionNum = currentQuestionNum;

            string sContent = (sender as TextBox).Text;
            //Get the answer number from the ID and assign it to nAnswerNum

            if (sID.Length > 14 && sID.Substring(0, 14) == "textbox_answer")
            {
                int nAnswerNum = Int32.Parse(sID.Substring(15));
                //Check if the string in the textbox is empty
                if (sContent == String.Empty)
                {
                    //Set the answer in the global list of questions to be empty as well
                    g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].Value = String.Empty;
                    //and set the textbox'es text equal to the appropriate default value
                    (sender as TextBox).Text = String.Format("[отговор {0}]", nAnswerNum + 1);

                    RadioButton correspondingRadioButton = g_l2rbAnswers[nQuestionNum][nAnswerNum];
                    correspondingRadioButton.IsChecked = false;
                }
                else
                    //If the textbox is not empty set the answer to its content
                    g_lQCQuestions[nQuestionNum].Answers[nAnswerNum].Value = sContent;
            }
            else if (sID == "textbox_question")
            {
                //Check if the string in the textbox is empty
                if (sContent == String.Empty)
                {
                    //Set the question in the global list of questions to be empty as well
                    g_lQCQuestions[currentQuestionNum].Question = String.Empty;
                    //and set the textbox'es text the appropriate default value
                    (sender as TextBox).Text = String.Format("[въпрос]");
                }
                else
                {
                    //If the textbox is not empty set the question equal to its content
                    g_lQCQuestions[currentQuestionNum].Question = sContent;
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
                g_lQCQuestions[currentQuestionNum].Points = nRes;
            }
            else if (sID == "textbox_timer")
            {
                //Regex r = new Regex("\\d\\d\\:\\d\\d");
                //Match m = r.Match(sContent);
                //if (sContent.Length == 5 && m.Success && !sContent.Equals("00:00"))
                int a;
                if (sContent.Contains(':') &&
                    Int32.TryParse(sContent.SubstringCharToChar(':', 0, true), out a) &&
                    Int32.TryParse(sContent.Substring(sContent.IndexOf(':')+1), out a))
                {
                    string sMinutes = sContent.SubstringCharToChar(':', 0, true);
                    int minutes = Int32.Parse(sMinutes);
                    string sSeconds = sContent.Substring(sContent.IndexOf(':') + 1);
                    int seconds = Int32.Parse(sSeconds);
                    
                    QuestionClass.Time = new TimeSpan(0, minutes, seconds);
                    (sender as TextBox).Text = String.Format("{0:00}:{1:00}",
                                               QuestionClass.Time.Hours*60+
                                               QuestionClass.Time.Minutes,
                                               QuestionClass.Time.Seconds);
                }
                else
                {
                    QuestionClass.Time = TimeSpan.Zero;
                    (sender as TextBox).Text = "[време (мин:сек)]";
                }
                for (int i = 1; i < g_lTITabs.Count-1; i++)
                {
                    TextBox textBox = GetObjectById("textbox_timer", i) as TextBox;
                    textBox.Text = (sender as TextBox).Text;
                }
            }
        }
    }
}