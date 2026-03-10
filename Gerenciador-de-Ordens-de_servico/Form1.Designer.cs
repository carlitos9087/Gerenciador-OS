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
            panel1 = new Panel();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            panel2 = new Panel();
            textBox2 = new TextBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.BackColor = Color.FromArgb(15, 28, 46);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(320, 676);
            panel1.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = SystemColors.AppWorkspace;
            label4.Location = new Point(29, 251);
            label4.MaximumSize = new Size(280, 0);
            label4.Name = "label4";
            label4.Size = new Size(265, 45);
            label4.TabIndex = 4;
            label4.Text = "Plataforma digital para gestão, aprovação e\\nrastreamento de Ordens de Serviço Crítico — 100% sem\\npapel.";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.DarkGoldenrod;
            label3.Location = new Point(52, 178);
            label3.Name = "label3";
            label3.Size = new Size(42, 15);
            label3.TabIndex = 3;
            label3.Text = "Crítico";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.White;
            label2.Location = new Point(29, 131);
            label2.Name = "label2";
            label2.Size = new Size(101, 15);
            label2.TabIndex = 2;
            label2.Text = "Ordem de Serviço";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.Gray;
            label1.Location = new Point(29, 61);
            label1.Name = "label1";
            label1.Size = new Size(251, 15);
            label1.TabIndex = 1;
            label1.Text = "DOCUMENTO CONTROLADO • USO INTERNO";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(27, 55);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(247, 23);
            textBox1.TabIndex = 2;
            textBox1.Text = "E-mail";
            // 
            // button1
            // 
            button1.ForeColor = Color.Black;
            button1.Location = new Point(105, 249);
            button1.Name = "button1";
            button1.Size = new Size(99, 23);
            button1.TabIndex = 3;
            button1.Text = "Logar";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(textBox2);
            panel2.Controls.Add(textBox1);
            panel2.Controls.Add(button1);
            panel2.ForeColor = SystemColors.ControlLightLight;
            panel2.Location = new Point(377, 131);
            panel2.Name = "panel2";
            panel2.Size = new Size(308, 333);
            panel2.TabIndex = 5;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(27, 145);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(247, 23);
            textBox2.TabIndex = 4;
            textBox2.Text = "Senha";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(765, 662);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
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
        private Label label4;
        private Label label3;
        private TextBox textBox2;
    }
}
