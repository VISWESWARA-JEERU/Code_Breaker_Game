using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CodeBreakerGUI
{
    public class Form1 : Form
    {
        private string secretCode;
        private int attemptsLeft;
        private const int maxAttempts = 8;
        private bool gameOver;
        private int codeLength = 3;

        private TextBox txtGuess;
        private Button btnSubmit, btnRestart, btnRules;
        private Label lblAttempts, lblTitle, lblStatus;
        private RichTextBox rtbFeedback;
        private RadioButton rbEasy, rbMedium, rbHard;

        public Form1()
        {
            InitializeComponent();
            ResetGame();
        }

        // 🎨 Gradient Background
        protected override void OnPaint(PaintEventArgs e)
        {
            LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(200, 220, 255),
                Color.FromArgb(240, 250, 255),
                45F);

            e.Graphics.FillRectangle(brush, this.ClientRectangle);
        }

        private void InitializeComponent()
        {
            this.Text = "🔐 Code Breaker Pro";
            this.Size = new Size(750, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 11);

            lblTitle = new Label()
            {
                Text = "🔐 Code Breaker",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 10),
                AutoSize = true
            };

            btnRules = CreateButton("📖 Rules", 580, 15, Color.Goldenrod);
            btnRules.Click += (s, e) => new RulesForm().ShowDialog();

            GroupBox gb = new GroupBox()
            {
                Text = "Difficulty",
                Location = new Point(20, 70),
                Size = new Size(680, 60)
            };

            rbEasy = new RadioButton() { Text = "Easy (3)", Location = new Point(20, 25), Checked = true };
            rbMedium = new RadioButton() { Text = "Medium (4)", Location = new Point(200, 25) };
            rbHard = new RadioButton() { Text = "Hard (5)", Location = new Point(400, 25) };

            rbEasy.CheckedChanged += ChangeDifficulty;
            rbMedium.CheckedChanged += ChangeDifficulty;
            rbHard.CheckedChanged += ChangeDifficulty;

            gb.Controls.AddRange(new Control[] { rbEasy, rbMedium, rbHard });

            txtGuess = new TextBox()
            {
                Location = new Point(20, 150),
                Width = 250,
                Font = new Font("Consolas", 18, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center
            };

            btnSubmit = CreateButton("Submit", 300, 150, Color.Green);
            btnSubmit.Click += BtnSubmit_Click;

            lblAttempts = new Label()
            {
                Location = new Point(20, 200),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true
            };

            rtbFeedback = new RichTextBox()
            {
                Location = new Point(20, 230),
                Size = new Size(680, 250),
                ReadOnly = true,
                Font = new Font("Consolas", 13),
                BackColor = Color.White
            };

            btnRestart = CreateButton("🔄 Restart", 280, 500, Color.DodgerBlue);
            btnRestart.Click += (s, e) => ResetGame();

            lblStatus = new Label()
            {
                Location = new Point(20, 540),
                AutoSize = true
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle, btnRules, gb, txtGuess, btnSubmit,
                lblAttempts, rtbFeedback, btnRestart, lblStatus
            });
        }

        // 🔵 Rounded Button Creator
        private Button CreateButton(string text, int x, int y, Color color)
        {
            Button btn = new Button()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 45),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btn.FlatAppearance.BorderSize = 0;

            btn.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                path.AddArc(0, 0, 20, 20, 180, 90);
                path.AddArc(btn.Width - 20, 0, 20, 20, 270, 90);
                path.AddArc(btn.Width - 20, btn.Height - 20, 20, 20, 0, 90);
                path.AddArc(0, btn.Height - 20, 20, 20, 90, 90);
                path.CloseAllFigures();
                btn.Region = new Region(path);
            };

            return btn;
        }

        private string GenerateCode()
        {
            Random rand = new Random();
            return string.Concat(Enumerable.Range(0, codeLength)
                .Select(_ => rand.Next(0, 10)));
        }

        private void ResetGame()
        {
            secretCode = GenerateCode();
            attemptsLeft = maxAttempts;
            gameOver = false;

            txtGuess.Enabled = true;
            btnSubmit.Enabled = true;

            txtGuess.MaxLength = codeLength;
            txtGuess.Clear();

            lblAttempts.Text = $"Attempts Left: {attemptsLeft}";
            lblStatus.Text = "Game Started!";
            rtbFeedback.Clear();
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (gameOver) return;

            string guess = txtGuess.Text.Trim();

            if (guess.Length != codeLength || !guess.All(char.IsDigit))
            {
                new ResultForm("Enter valid digits!", false).ShowDialog();
                return;
            }

            string result = CheckGuess(guess);
            rtbFeedback.AppendText($"Guess: {guess} → {result}\n");

            if (guess == secretCode)
            {
                new ResultForm("🎉 You cracked the code!", true).ShowDialog();
                EndGame();
                return;
            }

            attemptsLeft--;
            lblAttempts.Text = $"Attempts Left: {attemptsLeft}";

            ShowHint(guess);

            if (attemptsLeft == 0)
            {
                new ResultForm($"Game Over!\nCode: {secretCode}", false).ShowDialog(this);
                EndGame();
            }

            txtGuess.Clear();
        }

        private string CheckGuess(string guess)
        {
            char[] result = new char[codeLength];
            bool[] used = new bool[codeLength];

            for (int i = 0; i < codeLength; i++)
            {
                if (guess[i] == secretCode[i])
                {
                    result[i] = '✔';
                    used[i] = true;
                }
            }

            for (int i = 0; i < codeLength; i++)
            {
                if (result[i] == '✔') continue;

                for (int j = 0; j < codeLength; j++)
                {
                    if (!used[j] && guess[i] == secretCode[j])
                    {
                        result[i] = '~';
                        used[j] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < codeLength; i++)
                if (result[i] == '\0') result[i] = '✖';

            return string.Join(" ", result);
        }

        private void ShowHint(string guess)
        {
            int correct = guess.Count(c => secretCode.Contains(c));
            lblStatus.Text = $"Hint: {correct} correct digits";
        }

        private void ChangeDifficulty(object sender, EventArgs e)
        {
            if (rbEasy.Checked) codeLength = 3;
            else if (rbMedium.Checked) codeLength = 4;
            else codeLength = 5;

            ResetGame();
        }

        private void EndGame()
        {
            gameOver = true;
            txtGuess.Enabled = false;
            btnSubmit.Enabled = false;
        }
    }

    // ✨ Animated Popup Base
    public class FadeForm : Form
    {
        // Timer timer = new Timer();
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public FadeForm()
        {
            this.Opacity = 0;
            timer.Interval = 20;
            timer.Tick += (s, e) =>
            {
                if (this.Opacity < 1) this.Opacity += 0.05;
                else timer.Stop();
            };
            timer.Start();
        }
    }

    // 📖 Rules Window
   public class RulesForm : FadeForm
 {
    public RulesForm()
    {
        this.Text = "📖 Game Rules";
        this.Size = new Size(500, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;

        // Title
        Label title = new Label()
        {
            Text = "🔐 Code Breaker Rules",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.DarkBlue,
            AutoSize = true,
            Location = new Point(70, 20)
        };

        // Rules Content
        RichTextBox txt = new RichTextBox()
        {
            Location = new Point(30, 80),
            Size = new Size(420, 300),
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 13),
            BackColor = Color.White,
            TabStop = false // ✅ prevents cursor focus
        };

        txt.Text =
            "🎯 OBJECTIVE\n" +
            "Guess the secret code within 8 attempts.\n\n" +

            "🎮 HOW TO PLAY\n" +
            "• Enter your guess\n" +
            "• Click Submit\n" +
            "• Use feedback to improve\n\n" +

            "📊 FEEDBACK\n" +
            "✔ Correct digit & position\n" +
            "~ Correct digit wrong position\n" +
            "✖ Not present\n\n" +

            "💡 TIP\n" +
            "Start with different digits for better hints!";

        // Remove blinking cursor completely
        txt.GotFocus += (s, e) => this.ActiveControl = null;

        // Close Button
        Button btnClose = new Button()
        {
            Text = "Close",
            Size = new Size(120, 45),
            Location = new Point(180, 400),
            BackColor = Color.DodgerBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        btnClose.FlatAppearance.BorderSize = 0;

        // Rounded button
        btnClose.Paint += (s, e) =>
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 20, 20, 180, 90);
            path.AddArc(btnClose.Width - 20, 0, 20, 20, 270, 90);
            path.AddArc(btnClose.Width - 20, btnClose.Height - 20, 20, 20, 0, 90);
            path.AddArc(0, btnClose.Height - 20, 20, 20, 90, 90);
            path.CloseAllFigures();
            btnClose.Region = new Region(path);
        };

        btnClose.Click += (s, e) => this.Close();

        // Add Controls
        this.Controls.Add(title);
        this.Controls.Add(txt);
        this.Controls.Add(btnClose);

        // Ensure no control gets focus
        this.ActiveControl = null;
      }
    }


    // 🏆 Result Window
    public class ResultForm : FadeForm
{
    public ResultForm(string message, bool isWin)
    {
        this.Text = "Game Result";
        this.Size = new Size(400, 250);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.White;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Message Label
        Label lbl = new Label()
        {
            Text = message,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(340, 100),
            Location = new Point(30, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = isWin ? Color.Green : Color.Red
        };

        // OK Button
        Button btnOk = new Button()
        {
            Text = "OK",
            Size = new Size(120, 45),
            Location = new Point(140, 150),
            BackColor = isWin ? Color.Green : Color.Red,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        btnOk.FlatAppearance.BorderSize = 0;

        // Rounded Button
        btnOk.Paint += (s, e) =>
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, 20, 20, 180, 90);
            path.AddArc(btnOk.Width - 20, 0, 20, 20, 270, 90);
            path.AddArc(btnOk.Width - 20, btnOk.Height - 20, 20, 20, 0, 90);
            path.AddArc(0, btnOk.Height - 20, 20, 20, 90, 90);
            path.CloseAllFigures();
            btnOk.Region = new Region(path);
        };

        btnOk.Click += (s, e) => this.Close();

        // Add Controls
        this.Controls.Add(lbl);
        this.Controls.Add(btnOk);
    }

    // Extra safety for centering
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        this.CenterToParent();
    }
 }


    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }
    }
}
