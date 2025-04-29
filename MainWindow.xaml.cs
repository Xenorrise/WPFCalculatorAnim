using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFCalculatorAnim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const byte MAX_NUM_LENGTH = 32;
        readonly string ZeroDividingExceptionStr = "Деление на ноль невозможно";
        List<string> operations = new List<string>();
        string[] problemElements;
        string curNum = "";
        bool isNumChanged = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddDigit(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            ClearException(sender, e);
            if (ProblemBlock.Text.Length > 0 && ProblemBlock.Text[ProblemBlock.Text.Length - 1] == '=')
            {
                ProblemBlock.Text = "";
                curNum = "";
            }
            if (sender is Button btn && curNum.Length < MAX_NUM_LENGTH)
            {
                if (NumBlock.Text == "0" && btn.Content.ToString() == "0")
                    return;
                if (btn.Content.ToString() == ",")
                {
                    if (curNum.Contains(","))
                        return;
                    if (NumBlock.Text == "0")
                        curNum = "0";
                }
                curNum += btn.Content;
            }
            NumBlock.Text = curNum;
            isNumChanged = true;
        }

        public void RemoveDigit(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            if (ClearException(sender, e))
                return;
            if (curNum.Length - 1 > 0)
            {
                curNum = curNum.Substring(0, curNum.Length - 1);
                NumBlock.Text = curNum;
            }
            else
            {
                curNum = "";
                NumBlock.Text = "0";
            }
        }

        public void ChangeNumberSign(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            if (ClearException(sender, e))
                return;
            if (NumBlock.Text[0] == '-')
            {
                NumBlock.Text = NumBlock.Text.Substring(1);
                curNum = curNum.Substring(1);
            }
            else
            {
                NumBlock.Text = "-" + NumBlock.Text;
                curNum = "-" + curNum;
            }

        }

        public void AddOperation(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            if (ClearException(sender, e))
                return;
            if (sender is Button btn)
                if (operations.Count > 0 && operations[operations.Count - 1] != btn.Content.ToString())
                {
                    operations.Add(btn.Content.ToString());
                }
                else if (operations.Count == 0)
                    operations.Add(btn.Content.ToString());

            if (ProblemBlock.Text.Length > 0 && ProblemBlock.Text[ProblemBlock.Text.Length - 1] == '=')
                ProblemBlock.Text = NumBlock.Text + $" {operations[operations.Count - 1]} ";
            else
            {
                if (ProblemBlock.Text.Length > 0 && !isNumChanged && ProblemBlock.Text.Length - NumBlock.Text.Length - 3 >= 0 && NumBlock.Text == ProblemBlock.Text.Substring(ProblemBlock.Text.Length - NumBlock.Text.Length - 3, NumBlock.Text.Length))
                    ProblemBlock.Text = ProblemBlock.Text.Substring(0, ProblemBlock.Text.Length - 2) + $"{operations[operations.Count - 1]} ";
                else
                    ProblemBlock.Text += NumBlock.Text + $" {operations[operations.Count - 1]} ";
            }
            curNum = "";
            isNumChanged = false;
        }

        private List<string> RPNGenerate()
        {
            Stack<string> operationsStack = new Stack<string>();
            List<string> resultList = new List<string>();

            foreach (string problemElement in problemElements)
            {
                if (double.TryParse(problemElement, out _))
                    resultList.Add(problemElement);
                else
                {
                    while (operationsStack.Count > 0 && (operationsStack.Peek() == "*" || operationsStack.Peek() == "/"))
                    {
                        resultList.Add(operationsStack.Pop());
                    }
                    operationsStack.Push(problemElement);
                }
            }
            while (operationsStack.Count > 0)
            {
                resultList.Add(operationsStack.Pop());
            }
            Console.WriteLine(resultList);
            return resultList;
        }

        public void ResultCalculating(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            if (ProblemBlock.Text.Length > 0 && ProblemBlock.Text[ProblemBlock.Text.Length - 1] != '=')
                ProblemBlock.Text += NumBlock.Text;
            problemElements = ProblemBlock.Text.Split(' ');
            if (problemElements.Length == 1 || problemElements.Length == 2)
            {
                ProblemBlock.Text = NumBlock.Text + " =";
                return;
            }
            else if (ClearException(sender, e))
                return;
            string lastAction = "";
            double lastActionResult = 0;
            int lastIndex = Array.FindLastIndex(problemElements, x => x == "+" || x == "-");
            for (int i = lastIndex + 2; i < problemElements.Length; i += 2)
            {
                if (lastIndex != -1 && problemElements[i] == "*" || problemElements[i] == "/")
                {
                    switch (problemElements[i])
                    {
                        case "*":
                            if (lastActionResult == 0)
                                lastActionResult = Convert.ToDouble(problemElements[i - 1]) * Convert.ToDouble(problemElements[i + 1]);
                            else
                                lastActionResult *= Convert.ToDouble(problemElements[i + 1]);
                            break;
                        case "/":
                            if (lastActionResult == 0)
                                lastActionResult = Convert.ToDouble(problemElements[i - 1]) / Convert.ToDouble(problemElements[i + 1]);
                            else
                                lastActionResult /= Convert.ToDouble(problemElements[i + 1]);
                            break;
                    }
                }
                else if (lastIndex == -1)
                {
                    lastIndex = Array.FindLastIndex(problemElements, x => x == "*" || x == "/");
                    lastAction = problemElements[lastIndex] + " " + problemElements[lastIndex + 1];
                    break;
                }
                else if (lastActionResult != 0)
                {
                    lastAction = problemElements[lastIndex] + " " + lastActionResult.ToString();
                    break;
                }
                else
                {
                    lastAction = problemElements[lastIndex] + " " + problemElements[lastIndex + 1];
                    break;
                }
            }
            if (ProblemBlock.Text.Length > 0 && ProblemBlock.Text[ProblemBlock.Text.Length - 1] == '=')
            {
                ProblemBlock.Text = NumBlock.Text + " " + lastAction;
                problemElements = ProblemBlock.Text.Split(' ');
            }
            curNum = "";

            Stack<double> resultStack = new Stack<double>();

            foreach (string problemElement in RPNGenerate())
            {
                if (double.TryParse(problemElement, out double num))
                    resultStack.Push(num);
                else
                {
                    double a = resultStack.Pop(), b = resultStack.Pop();
                    double curResult = 0;
                    switch (problemElement)
                    {
                        case "+":
                            curResult = a + b;
                            break;
                        case "-":
                            curResult = b - a;
                            break;
                        case "*":
                            curResult = a * b;
                            break;
                        case "/":
                            if (a != 0)
                                curResult = b / a;
                            else
                            {
                                NumBlock.Text = ZeroDividingExceptionStr;
                                return;
                            }
                            break;
                    }
                    resultStack.Push(curResult);
                }
            }
            ProblemBlock.Text += " =";
            AnimateResultChange(resultStack.Pop().ToString());
        }

        public bool ClearException(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(NumBlock.Text, out _))
            {
                ClearAll(sender, e);
                return true;
            }
            return false;
        }

        public void ClearAll(object sender, RoutedEventArgs e)
        {
            PlayClickSound();
            curNum = "";
            AnimateClear();
        }

        private void AnimateResultChange(string newResult)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));

            fadeOut.Completed += (s, e) => {
                NumBlock.Text = newResult;
                NumBlock.BeginAnimation(OpacityProperty, fadeIn);
            };

            NumBlock.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void AnimateClear()
        {
            DoubleAnimation clearAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            clearAnim.Completed += (s, e) => {
                NumBlock.Text = "0";
                ProblemBlock.Text = "";
                NumBlock.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));
            };
            NumBlock.BeginAnimation(OpacityProperty, clearAnim);
        }

        private void PlayClickSound()
        {
            var player = new System.Media.SoundPlayer("Sounds/click.wav");
            player.Play();
        }
    }
}

