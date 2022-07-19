using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Test
{
    public partial class MainWindow : Window
    {
        string ActionLeft = "";
        string ActionRight = "";
        string EqFirstPart = "";
        string LastNumber = "";
        int Mode = 0;
        int CurrentBase = 10;

        static GridLength Zero = new GridLength(0, GridUnitType.Star);
        static GridLength One = new GridLength(1, GridUnitType.Star);
        static Dictionary<string, int> Bases = new Dictionary<string, int>
        {
            { "HEX", 16 },
            { "DEC", 10 },
            { "OCT", 8 },
            { "BIN", 2 },
        };
        public MainWindow()
        {
            InitializeComponent();  
        }

        void Update(bool ignore_base = false)
        {
            if (CurrentBase != 10 && !ignore_base)
            {
                LastNumber = ToBase10(LastNumber);
            }
            SecondaryText.Text = EqFirstPart + ActionLeft + LastNumber + ActionRight;
        }

        string ToBase10(string i) 
        {
            return ToBase(i, CurrentBase, 10);
        }

        string ToBase(string i, int a, int b)
        {
            return(i == "" || i == "ERROR") ? "" : Convert.ToString(Convert.ToInt64(i.ToLower().Trim(), a), b);
        }

        void DisplayPowers(string dec)
        {
            int n = Convert.ToInt32(Convert.ToDouble(dec.ToLower().Trim()));
            TextHex.Text = Convert.ToString(n, 16).ToUpper();
            TextDec.Text = n.ToString();
            TextOct.Text = Convert.ToString(n, 8);
            TextBin.Text = Convert.ToString(n, 2);
        }

        // NUMBER KEYS + DOT
        void ProcessNumber(dynamic sender, RoutedEventArgs e)
        {
            if (MainText.Text == "0") MainText.Text = "";
            string val = sender.Content;
            if (val == ".")
                MainText.Text = MainText.Text.Replace(".", "");
            MainText.Text += val;
            if (Mode == 2) DisplayPowers(ToBase10(MainText.Text));
        }

        void ActionUpdate()
        {
            if (MainText.Text != "")
            {
                LastNumber = MainText.Text;
                MainText.Text = "";
                EqFirstPart = SecondaryText.Text;
            }
        }

        // +-*/
        void ProcessAction(dynamic sender, RoutedEventArgs e)
        {
            bool ignore_base = MainText.Text == "" && Mode == 2;
            if (ActionLeft != "")
            {
                LastNumber = ActionLeft + LastNumber + ActionRight;
                ActionLeft = "";
                MainText.Text = "";
            }
            ActionUpdate();
            ActionRight = sender.Content.ToString();
            Update(ignore_base);
        }

        void BracketLeft(dynamic sender, RoutedEventArgs e)
        {
            EqFirstPart += "(";
            Update();
        }

        void BracketRight(dynamic sender, RoutedEventArgs e)
        {
            if (MainText.Text != "")
                Update();
            if (Mode == 2) MainText.Text = ToBase10(MainText.Text);
            SecondaryText.Text += MainText.Text + ")";
            EqFirstPart = SecondaryText.Text;
            ActionRight = ""; ActionLeft = ""; LastNumber = "";
            MainText.Text = "";
        }

        // SIN COS TAN ABS
        void ProcessAdvanced(dynamic sender, RoutedEventArgs e)
        {
            ActionUpdate();
            ActionLeft = sender.Content + "("; ActionRight = ")";
            Update();
        }

        // +/-
        void Invert(dynamic sender, RoutedEventArgs e)
        {
            if (MainText.Text.Length > 0 && MainText.Text[0] == '-')
                MainText.Text = MainText.Text.Substring(1);
            else
                MainText.Text = "-" + MainText.Text;
        }

        // SQRT
        void SquareRoot(object sender, RoutedEventArgs e)
        {
            ActionUpdate();
            ActionLeft = "Sqrt("; ActionRight = ")";
            Update();
        }

        // SQUARE
        void Pow2(object sender, RoutedEventArgs e)
        {
            ActionUpdate();
            ActionLeft = "Pow("; ActionRight = ", 2)";
            Update();
        }

        // 1/X
        void OneOver(object sender, RoutedEventArgs e)
        {
            if (MainText.Text.Length > 2 && MainText.Text[1] == '/')
                MainText.Text = MainText.Text.Substring(2);
            else
                MainText.Text = "1/" + MainText.Text;
        }

        // SOLVE
        void Equals(object sender, RoutedEventArgs e)
        {
            Update(true);
            if (MainText.Text != "ERROR" || MainText.Text != "")
            {
                SecondaryText.Text += (Mode == 2) ? ToBase10(MainText.Text) : MainText.Text;
            }
                if (SecondaryText.Text != "")
                {
                    NCalc.Expression exp = new NCalc.Expression(SecondaryText.Text);
                    string res = String.Format("{0:G12}", exp.Evaluate()).Replace(',', '.');
                    if (Mode == 2) 
                    {
                        res = String.Format("{0:0}", Math.Floor(Convert.ToDouble(res)));
                        DisplayPowers(res);
                        res = ToBase(res, 10, CurrentBase).ToUpper();
                    }
                    MainText.Text = res;
                    SecondaryText.Text = "";
                    ActionRight = ""; ActionLeft = ""; LastNumber = "";
                }
        }

        void Clear(object sender=null, RoutedEventArgs e=null)
        {
            SecondaryText.Text = "";
            MainText.Text = "0";
            SecondaryText.Text = "";
            EqFirstPart = ""; ActionRight = ""; ActionLeft = ""; LastNumber = "";
            TextHex.Text = ""; TextDec.Text = ""; TextOct.Text = ""; TextBin.Text = "";
        }

        void Backspace(object sender, RoutedEventArgs e)
        {
            int l = MainText.Text.Length;
            if (l > 0)
            {
                MainText.Text = MainText.Text.Substring(0, l - 1);
                return;
            }
            else if (ActionLeft != "" || ActionRight != "")
            {
                ActionLeft = ""; ActionRight = "";
            }
            else if (LastNumber.Length > 0) LastNumber = LastNumber.Substring(0, LastNumber.Length - 1);
            else if (EqFirstPart.Length > 0)
                EqFirstPart = EqFirstPart.Substring(0, EqFirstPart.Length - 1);
            Update();
        }

        void SwitchMode(object sender, RoutedEventArgs e)
        {
            Clear();
            Mode = (Mode + 1) % 3;
            switch (Mode)
            {
                case 0:
                    SetBase(btn_dec);
                    ModeText.Text = "Обычный";
                    HexColumn.Width = Zero;
                    AdvancedRow.Height = Zero;
                    TextBoxRow.Height = One;
                    BinaryText.Height = Zero;
                    btn_pow2.IsEnabled = true; btn_invx.IsEnabled = true; btn_sqrt.IsEnabled = true; btn_dot.IsEnabled = true;
                    break;
                case 1:
                    ModeText.Text = "Инженерный";
                    HexColumn.Width = Zero;
                    AdvancedRow.Height = One;
                    break;
                case 2:
                    ModeText.Text = "Программистический";
                    HexColumn.Width = One;
                    AdvancedRow.Height = Zero;
                    BinaryText.Height = One;
                    SetBase(btn_hex);
                    btn_pow2.IsEnabled = false; btn_invx.IsEnabled = false; btn_sqrt.IsEnabled = false; btn_dot.IsEnabled = false;
                    TextBoxRow.Height = new GridLength(2, GridUnitType.Star);
                    break;
            }
        }

        void SetBase(dynamic sender=null, RoutedEventArgs e=null)
        {
            int new_base = Bases[sender.Content.ToString()];
            MainText.Text = ToBase(MainText.Text, CurrentBase, new_base).ToUpper();

            
            CurrentBase = new_base;
            MainText.FontSize = CurrentBase * 2.2 + 10;

            Button[] NumberButtons = new Button[] { btn_0, btn_1, btn_2, btn_3,
                btn_4, btn_5, btn_6, btn_7, btn_8, btn_9, btn_a, btn_b, btn_c,
                btn_d, btn_e, btn_f};

            foreach (dynamic b in BinaryGrid.Children)
                b.IsEnabled = true;

            for (int i = 0; i < CurrentBase; i++)
                NumberButtons[i].IsEnabled = true;
            for (int i = CurrentBase; i < 16; i++)
                NumberButtons[i].IsEnabled = false;

            btn_inv.IsEnabled = CurrentBase == 10;
            sender.IsEnabled = false;
        }
    }
}