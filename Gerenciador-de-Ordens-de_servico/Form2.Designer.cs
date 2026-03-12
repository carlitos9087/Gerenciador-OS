namespace Gerenciador_de_Ordens_de_servico
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            contextMenuStrip1 = new ContextMenuStrip(components);
            contextMenuStrip2 = new ContextMenuStrip(components);
            tabControl1 = new TabControl();
            Dashboard = new TabPage();
            Criar_Ordens = new TabPage();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            tabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.Name = "contextMenuStrip2";
            contextMenuStrip2.Size = new Size(61, 4);
            // 
            // tabControl1
            // 
            tabControl1.Alignment = TabAlignment.Left;
            tabControl1.Controls.Add(Dashboard);
            tabControl1.Controls.Add(Criar_Ordens);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Multiline = true;
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1272, 776);
            tabControl1.TabIndex = 3;
            // 
            // Dashboard
            // 
            Dashboard.Location = new Point(27, 4);
            Dashboard.Name = "Dashboard";
            Dashboard.Padding = new Padding(3);
            Dashboard.Size = new Size(1241, 768);
            Dashboard.TabIndex = 0;
            Dashboard.Text = "Dashboard";
            Dashboard.UseVisualStyleBackColor = true;
            // 
            // Criar_Ordens
            // 
            Criar_Ordens.Location = new Point(27, 4);
            Criar_Ordens.Name = "Criar_Ordens";
            Criar_Ordens.Padding = new Padding(3);
            Criar_Ordens.Size = new Size(1241, 768);
            Criar_Ordens.TabIndex = 1;
            Criar_Ordens.Text = "Novas Ordens";
            Criar_Ordens.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(27, 4);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(1241, 768);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "tabPage3";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(27, 4);
            tabPage4.Name = "tabPage4";
            tabPage4.RightToLeft = RightToLeft.No;
            tabPage4.Size = new Size(1241, 768);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "tabPage4";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1272, 776);
            Controls.Add(tabControl1);
            Name = "Form2";
            Text = "Form2";
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private ContextMenuStrip contextMenuStrip1;
        private ContextMenuStrip contextMenuStrip2;
        private TabControl tabControl1;
        private TabPage Dashboard;
        private TabPage Criar_Ordens;
        private TabPage tabPage3;
        private TabPage tabPage4;
    }
}