namespace Gerenciador_de_Ordens_de_servico
{
    partial class Form3
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelCabecalho = new System.Windows.Forms.Panel();
            this.panelLateral = new System.Windows.Forms.Panel();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.btnCriarRelatorio = new System.Windows.Forms.Button();
            this.btnAssinar = new System.Windows.Forms.Button();
            this.btnAdmin = new System.Windows.Forms.Button();
            this.panelConteudo = new System.Windows.Forms.Panel();

            this.SuspendLayout();
            this.panelCabecalho.SuspendLayout();
            this.panelLateral.SuspendLayout();

            // ── panelCabecalho ──────────────────────────────────────────
            this.panelCabecalho.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCabecalho.Name = "panelCabecalho";
            this.panelCabecalho.Size = new System.Drawing.Size(1100, 60);
            this.panelCabecalho.TabIndex = 0;

            // ── panelLateral ────────────────────────────────────────────
            this.panelLateral.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.panelLateral.Controls.Add(this.btnDashboard);
            this.panelLateral.Controls.Add(this.btnCriarRelatorio);
            this.panelLateral.Controls.Add(this.btnAssinar);
            this.panelLateral.Controls.Add(this.btnAdmin);
            this.panelLateral.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLateral.Name = "panelLateral";
            this.panelLateral.Size = new System.Drawing.Size(200, 590);
            this.panelLateral.TabIndex = 1;

            // Linha separadora direita da lateral
            this.panelLateral.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new System.Drawing.Pen(System.Drawing.Color.FromArgb(210, 215, 225), 1),
                    this.panelLateral.Width - 1, 0,
                    this.panelLateral.Width - 1, this.panelLateral.Height);
            };

            // ── btnDashboard ────────────────────────────────────────────
            this.btnDashboard.Location = new System.Drawing.Point(12, 20);
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.Size = new System.Drawing.Size(176, 44);
            this.btnDashboard.TabIndex = 0;

            // ── btnCriarRelatorio ───────────────────────────────────────
            this.btnCriarRelatorio.Location = new System.Drawing.Point(12, 74);
            this.btnCriarRelatorio.Name = "btnCriarRelatorio";
            this.btnCriarRelatorio.Size = new System.Drawing.Size(176, 44);
            this.btnCriarRelatorio.TabIndex = 1;

            // ── btnAssinar ──────────────────────────────────────────────
            this.btnAssinar.Location = new System.Drawing.Point(12, 128);
            this.btnAssinar.Name = "btnAssinar";
            this.btnAssinar.Size = new System.Drawing.Size(176, 44);
            this.btnAssinar.TabIndex = 2;

            // ── btnAdmin ────────────────────────────────────────────────
            // Começa oculto — só aparece se o usuário logado for Administrador
            this.btnAdmin.Location = new System.Drawing.Point(12, 182);
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(176, 44);
            this.btnAdmin.TabIndex = 3;
            this.btnAdmin.Visible = false;

            // ── panelConteudo ───────────────────────────────────────────
            this.panelConteudo.BackColor = System.Drawing.Color.White;
            this.panelConteudo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelConteudo.Name = "panelConteudo";
            this.panelConteudo.Padding = new System.Windows.Forms.Padding(24);
            this.panelConteudo.TabIndex = 2;

            // ── Form3 ───────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.MinimumSize = new System.Drawing.Size(860, 500);
            this.Text = "Gerenciador de Ordens de Serviço";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // ATENÇÃO: Fill deve ser adicionado antes para funcionar corretamente
            this.Controls.Add(this.panelConteudo);
            this.Controls.Add(this.panelLateral);
            this.Controls.Add(this.panelCabecalho);

            this.panelCabecalho.ResumeLayout(false);
            this.panelLateral.ResumeLayout(false);
            this.ResumeLayout(false);

            this.Load += new System.EventHandler(this.Form3_Load);
        }

        private System.Windows.Forms.Panel panelCabecalho;
        private System.Windows.Forms.Panel panelLateral;
        private System.Windows.Forms.Panel panelConteudo;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnCriarRelatorio;
        private System.Windows.Forms.Button btnAssinar;
        private System.Windows.Forms.Button btnAdmin;
    }
}