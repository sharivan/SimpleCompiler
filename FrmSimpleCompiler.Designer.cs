
namespace SimpleCompiler
{
    partial class FrmSimpleCompiler
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtSource = new System.Windows.Forms.RichTextBox();
            this.lbConsole = new System.Windows.Forms.ListBox();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClearConsole = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.mnuConsoleContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuConsoleCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuConsoleSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuConsoleContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(2, 0);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(796, 274);
            this.txtSource.TabIndex = 0;
            this.txtSource.Text = "";
            // 
            // lbConsole
            // 
            this.lbConsole.ContextMenuStrip = this.mnuConsoleContext;
            this.lbConsole.FormattingEnabled = true;
            this.lbConsole.Location = new System.Drawing.Point(2, 314);
            this.lbConsole.Name = "lbConsole";
            this.lbConsole.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbConsole.Size = new System.Drawing.Size(796, 277);
            this.lbConsole.TabIndex = 1;
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(144, 280);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(120, 28);
            this.btnCompile.TabIndex = 2;
            this.btnCompile.Text = "Compilar Expressão";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // btnRun
            // 
            this.btnRun.Enabled = false;
            this.btnRun.Location = new System.Drawing.Point(396, 280);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(120, 28);
            this.btnRun.TabIndex = 3;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClearConsole
            // 
            this.btnClearConsole.Enabled = false;
            this.btnClearConsole.Location = new System.Drawing.Point(522, 280);
            this.btnClearConsole.Name = "btnClearConsole";
            this.btnClearConsole.Size = new System.Drawing.Size(120, 28);
            this.btnClearConsole.TabIndex = 4;
            this.btnClearConsole.Text = "Limpar Console";
            this.btnClearConsole.UseVisualStyleBackColor = true;
            this.btnClearConsole.Click += new System.EventHandler(this.btnClearConsole_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(270, 280);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 28);
            this.button1.TabIndex = 5;
            this.button1.Text = "Compilar Programa";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // mnuConsoleContext
            // 
            this.mnuConsoleContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuConsoleSelectAll,
            this.mnuConsoleCopy});
            this.mnuConsoleContext.Name = "mnuConsoleContext";
            this.mnuConsoleContext.Size = new System.Drawing.Size(201, 70);
            // 
            // mnuConsoleCopy
            // 
            this.mnuConsoleCopy.Name = "mnuConsoleCopy";
            this.mnuConsoleCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mnuConsoleCopy.Size = new System.Drawing.Size(200, 22);
            this.mnuConsoleCopy.Text = "Copiar";
            this.mnuConsoleCopy.Click += new System.EventHandler(this.mnuConsoleCopy_Click);
            // 
            // mnuConsoleSelectAll
            // 
            this.mnuConsoleSelectAll.Name = "mnuConsoleSelectAll";
            this.mnuConsoleSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.mnuConsoleSelectAll.Size = new System.Drawing.Size(200, 22);
            this.mnuConsoleSelectAll.Text = "Selecionar Tudo";
            this.mnuConsoleSelectAll.Click += new System.EventHandler(this.mnuConsoleSelectAll_Click);
            // 
            // FrmSimpleCompiler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 593);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnClearConsole);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.lbConsole);
            this.Controls.Add(this.txtSource);
            this.Name = "FrmSimpleCompiler";
            this.Text = "Simple Compiler";
            this.mnuConsoleContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtSource;
        private System.Windows.Forms.ListBox lbConsole;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClearConsole;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip mnuConsoleContext;
        private System.Windows.Forms.ToolStripMenuItem mnuConsoleCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuConsoleSelectAll;
    }
}

