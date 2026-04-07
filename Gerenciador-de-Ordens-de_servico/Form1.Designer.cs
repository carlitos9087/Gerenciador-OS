using System.Drawing.Drawing2D;

namespace Gerenciador_de_Ordens_de_servico
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            label4 = new Label();
            label2 = new Label();
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            panel2 = new Panel();
            label6 = new Label();
            label5 = new Label();
            textBox2 = new TextBox();
            label7 = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.BackColor = Color.FromArgb(15, 28, 46);
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(20, 0, 20, 0);
            panel1.RightToLeft = RightToLeft.No;
            panel1.Size = new Size(333, 686);
            panel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.management_service;
            pictureBox1.InitialImage = (Image)resources.GetObject("pictureBox1.InitialImage");
            pictureBox1.Location = new Point(75, 346);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(154, 148);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label4.AutoSize = true;
            label4.ForeColor = SystemColors.AppWorkspace;
            label4.Location = new Point(23, 662);
            label4.MaximumSize = new Size(300, 0);
            label4.Name = "label4";
            label4.Size = new Size(287, 15);
            label4.TabIndex = 4;
            label4.Text = "Automatize seu fluxo de aprovações com a GestãoOS";
            label4.Click += label4_Click;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 20F);
            label2.ForeColor = Color.White;
            label2.Location = new Point(45, 150);
            label2.Name = "label2";
            label2.Size = new Size(231, 37);
            label2.TabIndex = 2;
            label2.Text = "Ordens de Serviço";
            label2.Click += label2_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.ForeColor = Color.LightSteelBlue;
            label1.Location = new Point(88, 32);
            label1.Name = "label1";
            label1.Size = new Size(164, 15);
            label1.TabIndex = 1;
            label1.Text = "DOCUMENTO CONTROLADO";
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(27, 63);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(335, 23);
            textBox1.TabIndex = 2;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            button1.BackColor = SystemColors.HotTrack;
            button1.ForeColor = Color.Transparent;
            button1.Location = new Point(105, 257);
            button1.Name = "button1";
            button1.Size = new Size(187, 32);
            button1.TabIndex = 3;
            button1.Text = "Logar";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click_1;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.White;
            panel2.Controls.Add(label6);
            panel2.Controls.Add(label5);
            panel2.Controls.Add(textBox2);
            panel2.Controls.Add(textBox1);
            panel2.Controls.Add(button1);
            panel2.ForeColor = SystemColors.ControlLightLight;
            panel2.Location = new Point(564, 150);
            panel2.Margin = new Padding(300, 3, 3, 3);
            panel2.MaximumSize = new Size(400, 400);
            panel2.MinimumSize = new Size(300, 300);
            panel2.Name = "panel2";
            panel2.Size = new Size(396, 331);
            panel2.TabIndex = 5;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.ForeColor = SystemColors.ActiveCaptionText;
            label6.Location = new Point(27, 119);
            label6.Name = "label6";
            label6.Size = new Size(39, 15);
            label6.TabIndex = 5;
            label6.Text = "Senha";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.ForeColor = SystemColors.ActiveCaptionText;
            label5.Location = new Point(27, 35);
            label5.Name = "label5";
            label5.Size = new Size(41, 15);
            label5.TabIndex = 5;
            label5.Text = "E-mail";
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBox2.Location = new Point(27, 153);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(335, 23);
            textBox2.TabIndex = 4;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label7.AutoSize = true;
            label7.ForeColor = SystemColors.AppWorkspace;
            label7.Location = new Point(729, 629);
            label7.MaximumSize = new Size(300, 0);
            label7.Name = "label7";
            label7.Size = new Size(85, 15);
            label7.TabIndex = 4;
            label7.Text = "GestãoOS 2026";
            label7.Click += label4_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1107, 686);
            Controls.Add(label7);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private Panel panel2;
        private Label label2;
        private TextBox textBox2;
        private Label label5;
        private Label label6;
        private Label label7;

        // ── Pintura do botão arredondado ──────────────────────────────
        private void Button1_Paint(object? sender, PaintEventArgs e)
        {
            var btn = (Button)sender!;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
            using var path = GetRoundedRectanglePath(rect, 10);

            // Fundo
            using var brush = new SolidBrush(btn.BackColor);
            e.Graphics.FillPath(brush, path);

            // Define a região clicável como arredondada
            btn.Region = new Region(GetRoundedRectanglePath(new Rectangle(0, 0, btn.Width, btn.Height), 10));

            // Texto
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, rect, btn.ForeColor, flags);
        }

        // ── Pintura do panel2 arredondado com sombra suave ────────────
        private void Panel2_Paint(object? sender, PaintEventArgs e)
        {
            var p = (Panel)sender!;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(1, 1, p.Width - 3, p.Height - 3);
            using var path = GetRoundedRectanglePath(rect, 18);

            // Define a região visual como arredondada
            p.Region = new Region(GetRoundedRectanglePath(new Rectangle(0, 0, p.Width, p.Height), 18));

            // Fundo branco
            using var bgBrush = new SolidBrush(Color.White);
            e.Graphics.FillPath(bgBrush, path);

            // Borda sutil
            using var pen = new Pen(Color.FromArgb(220, 225, 235), 1.5f);
            e.Graphics.DrawPath(pen, path);
        }

        // ── Cria um painel wrapper arredondado ao redor de um TextBox ──
        private void AplicarWrapperArredondado(TextBox txt, int raio)
        {
            var parent = txt.Parent;
            if (parent == null) return;

            // Painel que envolve o TextBox
            var wrapper = new Panel
            {
                BackColor = Color.White,
                Location = new Point(txt.Left - 10, txt.Top - 8),
                Size = new Size(txt.Width + 20, txt.Height + 16),
                BorderStyle = BorderStyle.None,
            };

            // Move o TextBox para dentro do wrapper
            parent.Controls.Remove(txt);
            wrapper.Controls.Add(txt);
            txt.Location = new Point(10, (wrapper.Height - txt.Height) / 2);
            txt.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txt.Width = wrapper.Width - 20;
            parent.Controls.Add(wrapper);

            // Pintura da borda arredondada
            wrapper.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, wrapper.Width - 1, wrapper.Height - 1);
                using var path = GetRoundedRectanglePath(rect, raio);

                wrapper.Region = new Region(path);

                using var bgBrush = new SolidBrush(Color.White);
                e.Graphics.FillPath(bgBrush, path);

                // Borda azul quando focado, cinza quando não
                bool focado = txt.Focused;
                var corBorda = focado
                    ? Color.FromArgb(30, 80, 160)
                    : Color.FromArgb(200, 210, 230);
                using var pen = new Pen(corBorda, focado ? 2f : 1.5f);
                e.Graphics.DrawPath(pen, path);
            };

            // Redesenha a borda ao focar/desfocar
            txt.GotFocus += (s, e) => wrapper.Invalidate();
            txt.LostFocus += (s, e) => wrapper.Invalidate();
        }
        private Label label4;
        private PictureBox pictureBox1;
    }
}