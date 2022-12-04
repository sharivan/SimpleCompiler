
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSimpleCompiler));
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainerCode = new System.Windows.Forms.SplitContainer();
            this.tcSources = new System.Windows.Forms.TabControl();
            this.tcRight = new System.Windows.Forms.TabControl();
            this.tpAssembly = new System.Windows.Forms.TabPage();
            this.wpfAssemblyHost = new System.Windows.Forms.Integration.ElementHost();
            this.tpLocals = new System.Windows.Forms.TabPage();
            this.dgvLocals = new System.Windows.Forms.DataGridView();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnValue = new System.Windows.Forms.DataGridViewButtonColumn();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvWatch = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewButtonColumn1 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtConsole = new System.Windows.Forms.RichTextBox();
            this.dgvStack = new System.Windows.Forms.DataGridView();
            this.mnuStackView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuStackViewAlign16 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStackViewAlign8 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStackViewAlign4 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.progress = new System.Windows.Forms.ToolStripProgressBar();
            this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnReload = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.btnCompile = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRun = new System.Windows.Forms.ToolStripButton();
            this.btnPause = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnStepOver = new System.Windows.Forms.ToolStripButton();
            this.btnStepInto = new System.Windows.Forms.ToolStripButton();
            this.btnStepReturn = new System.Windows.Forms.ToolStripButton();
            this.btnRunToCursor = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnToggleBreakpoint = new System.Windows.Forms.ToolStripButton();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.bwCompiler = new System.ComponentModel.BackgroundWorker();
            this.splitContainerData = new System.Windows.Forms.SplitContainer();
            this.gbStack = new System.Windows.Forms.GroupBox();
            this.gbStrings = new System.Windows.Forms.GroupBox();
            this.dgvStrings = new System.Windows.Forms.DataGridView();
            this.RA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewButtonColumn();
            this.View = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StringHA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StringRefCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StringLen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StringValue = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCode)).BeginInit();
            this.splitContainerCode.Panel1.SuspendLayout();
            this.splitContainerCode.Panel2.SuspendLayout();
            this.splitContainerCode.SuspendLayout();
            this.tcRight.SuspendLayout();
            this.tpAssembly.SuspendLayout();
            this.tpLocals.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocals)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStack)).BeginInit();
            this.mnuStackView.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.toolBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerData)).BeginInit();
            this.splitContainerData.Panel1.SuspendLayout();
            this.splitContainerData.Panel2.SuspendLayout();
            this.splitContainerData.SuspendLayout();
            this.gbStack.SuspendLayout();
            this.gbStrings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStrings)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 27);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainerCode);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainerMain.Size = new System.Drawing.Size(1462, 569);
            this.splitContainerMain.SplitterDistance = 359;
            this.splitContainerMain.TabIndex = 12;
            // 
            // splitContainerCode
            // 
            this.splitContainerCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerCode.Location = new System.Drawing.Point(0, 0);
            this.splitContainerCode.Name = "splitContainerCode";
            // 
            // splitContainerCode.Panel1
            // 
            this.splitContainerCode.Panel1.Controls.Add(this.tcSources);
            // 
            // splitContainerCode.Panel2
            // 
            this.splitContainerCode.Panel2.Controls.Add(this.tcRight);
            this.splitContainerCode.Size = new System.Drawing.Size(1462, 359);
            this.splitContainerCode.SplitterDistance = 793;
            this.splitContainerCode.SplitterWidth = 10;
            this.splitContainerCode.TabIndex = 8;
            // 
            // tcSources
            // 
            this.tcSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcSources.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tcSources.Location = new System.Drawing.Point(0, 0);
            this.tcSources.Name = "tcSources";
            this.tcSources.SelectedIndex = 0;
            this.tcSources.Size = new System.Drawing.Size(793, 359);
            this.tcSources.TabIndex = 2;
            this.tcSources.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tcSources_DrawItem);
            this.tcSources.SelectedIndexChanged += new System.EventHandler(this.tcSources_SelectedIndexChanged);
            this.tcSources.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tcSources_MouseDown);
            // 
            // tcRight
            // 
            this.tcRight.Controls.Add(this.tpAssembly);
            this.tcRight.Controls.Add(this.tpLocals);
            this.tcRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcRight.Location = new System.Drawing.Point(0, 0);
            this.tcRight.Name = "tcRight";
            this.tcRight.SelectedIndex = 0;
            this.tcRight.Size = new System.Drawing.Size(659, 359);
            this.tcRight.TabIndex = 4;
            // 
            // tpAssembly
            // 
            this.tpAssembly.Controls.Add(this.wpfAssemblyHost);
            this.tpAssembly.Location = new System.Drawing.Point(4, 22);
            this.tpAssembly.Name = "tpAssembly";
            this.tpAssembly.Padding = new System.Windows.Forms.Padding(3);
            this.tpAssembly.Size = new System.Drawing.Size(651, 333);
            this.tpAssembly.TabIndex = 0;
            this.tpAssembly.Text = "Assembly";
            this.tpAssembly.UseVisualStyleBackColor = true;
            // 
            // wpfAssemblyHost
            // 
            this.wpfAssemblyHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wpfAssemblyHost.Location = new System.Drawing.Point(3, 3);
            this.wpfAssemblyHost.Name = "wpfAssemblyHost";
            this.wpfAssemblyHost.Size = new System.Drawing.Size(645, 327);
            this.wpfAssemblyHost.TabIndex = 0;
            this.wpfAssemblyHost.Child = null;
            // 
            // tpLocals
            // 
            this.tpLocals.Controls.Add(this.dgvLocals);
            this.tpLocals.Controls.Add(this.dgvWatch);
            this.tpLocals.Location = new System.Drawing.Point(4, 22);
            this.tpLocals.Name = "tpLocals";
            this.tpLocals.Padding = new System.Windows.Forms.Padding(3);
            this.tpLocals.Size = new System.Drawing.Size(651, 333);
            this.tpLocals.TabIndex = 1;
            this.tpLocals.Text = "Dados Locais";
            this.tpLocals.UseVisualStyleBackColor = true;
            // 
            // dgvLocals
            // 
            this.dgvLocals.AllowUserToAddRows = false;
            this.dgvLocals.AllowUserToDeleteRows = false;
            this.dgvLocals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLocals.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnName,
            this.columnValue,
            this.columnType});
            this.dgvLocals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLocals.Location = new System.Drawing.Point(3, 3);
            this.dgvLocals.Name = "dgvLocals";
            this.dgvLocals.ReadOnly = true;
            this.dgvLocals.Size = new System.Drawing.Size(645, 188);
            this.dgvLocals.TabIndex = 0;
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnName.HeaderText = "Nome";
            this.columnName.Name = "columnName";
            this.columnName.ReadOnly = true;
            this.columnName.Width = 60;
            // 
            // columnValue
            // 
            this.columnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnValue.HeaderText = "Valor";
            this.columnValue.Name = "columnValue";
            this.columnValue.ReadOnly = true;
            this.columnValue.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.columnValue.Width = 56;
            // 
            // columnType
            // 
            this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnType.HeaderText = "Tipo";
            this.columnType.Name = "columnType";
            this.columnType.ReadOnly = true;
            this.columnType.Width = 53;
            // 
            // dgvWatch
            // 
            this.dgvWatch.AllowUserToAddRows = false;
            this.dgvWatch.AllowUserToDeleteRows = false;
            this.dgvWatch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWatch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewButtonColumn1,
            this.dataGridViewTextBoxColumn2});
            this.dgvWatch.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvWatch.Location = new System.Drawing.Point(3, 191);
            this.dgvWatch.Name = "dgvWatch";
            this.dgvWatch.ReadOnly = true;
            this.dgvWatch.Size = new System.Drawing.Size(645, 139);
            this.dgvWatch.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn1.HeaderText = "Nome";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 60;
            // 
            // dataGridViewButtonColumn1
            // 
            this.dataGridViewButtonColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewButtonColumn1.HeaderText = "Valor";
            this.dataGridViewButtonColumn1.Name = "dataGridViewButtonColumn1";
            this.dataGridViewButtonColumn1.ReadOnly = true;
            this.dataGridViewButtonColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewButtonColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.dataGridViewButtonColumn1.Width = 56;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridViewTextBoxColumn2.HeaderText = "Tipo";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 53;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtConsole);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainerData);
            this.splitContainer1.Size = new System.Drawing.Size(1462, 206);
            this.splitContainer1.SplitterDistance = 793;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 7;
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConsole.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConsole.ForeColor = System.Drawing.Color.Gainsboro;
            this.txtConsole.Location = new System.Drawing.Point(0, 0);
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.Size = new System.Drawing.Size(793, 206);
            this.txtConsole.TabIndex = 5;
            this.txtConsole.Text = "";
            this.txtConsole.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtConsole_KeyPress);
            // 
            // dgvStack
            // 
            this.dgvStack.AllowUserToAddRows = false;
            this.dgvStack.AllowUserToDeleteRows = false;
            this.dgvStack.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStack.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RA,
            this.HA,
            this.Value,
            this.View});
            this.dgvStack.ContextMenuStrip = this.mnuStackView;
            this.dgvStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStack.Location = new System.Drawing.Point(3, 16);
            this.dgvStack.Name = "dgvStack";
            this.dgvStack.ReadOnly = true;
            this.dgvStack.Size = new System.Drawing.Size(394, 187);
            this.dgvStack.TabIndex = 1;
            this.dgvStack.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dgvStack_Scroll);
            this.dgvStack.Resize += new System.EventHandler(this.dgvStack_Resize);
            // 
            // mnuStackView
            // 
            this.mnuStackView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStackViewAlign16,
            this.mnuStackViewAlign8,
            this.mnuStackViewAlign4});
            this.mnuStackView.Name = "mnuStackView";
            this.mnuStackView.Size = new System.Drawing.Size(193, 70);
            // 
            // mnuStackViewAlign16
            // 
            this.mnuStackViewAlign16.Name = "mnuStackViewAlign16";
            this.mnuStackViewAlign16.Size = new System.Drawing.Size(192, 22);
            this.mnuStackViewAlign16.Text = "Alinhamento: 16 bytes";
            this.mnuStackViewAlign16.Click += new System.EventHandler(this.mnuStackViewAlign16_Click);
            // 
            // mnuStackViewAlign8
            // 
            this.mnuStackViewAlign8.Name = "mnuStackViewAlign8";
            this.mnuStackViewAlign8.Size = new System.Drawing.Size(192, 22);
            this.mnuStackViewAlign8.Text = "Alinhamento: 8 bytes";
            this.mnuStackViewAlign8.Click += new System.EventHandler(this.mnuStackViewAlign8_Click);
            // 
            // mnuStackViewAlign4
            // 
            this.mnuStackViewAlign4.Name = "mnuStackViewAlign4";
            this.mnuStackViewAlign4.Size = new System.Drawing.Size(192, 22);
            this.mnuStackViewAlign4.Text = "Alinhamento: 4 bytes";
            this.mnuStackViewAlign4.Click += new System.EventHandler(this.mnuStackViewAlign4_Click);
            // 
            // statusBar
            // 
            this.statusBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progress,
            this.statusText});
            this.statusBar.Location = new System.Drawing.Point(0, 596);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(1462, 22);
            this.statusBar.TabIndex = 13;
            // 
            // progress
            // 
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(100, 16);
            this.progress.Step = 1;
            this.progress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progress.Visible = false;
            // 
            // statusText
            // 
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(0, 17);
            // 
            // toolBar
            // 
            this.toolBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.btnReload,
            this.btnSave,
            this.toolStripSeparator,
            this.btnCompile,
            this.toolStripSeparator2,
            this.btnRun,
            this.btnPause,
            this.btnStop,
            this.toolStripSeparator3,
            this.btnStepOver,
            this.btnStepInto,
            this.btnStepReturn,
            this.btnRunToCursor,
            this.toolStripSeparator4,
            this.btnToggleBreakpoint});
            this.toolBar.Location = new System.Drawing.Point(0, 0);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(1462, 27);
            this.toolBar.TabIndex = 14;
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(24, 24);
            this.btnNew.Text = "&Novo";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(24, 24);
            this.btnOpen.Text = "&Abrir";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnReload
            // 
            this.btnReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnReload.Image = ((System.Drawing.Image)(resources.GetObject("btnReload.Image")));
            this.btnReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(24, 24);
            this.btnReload.Text = "&Recarregar";
            this.btnReload.ToolTipText = "Recarregar";
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(24, 24);
            this.btnSave.Text = "&Salvar";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 27);
            // 
            // btnCompile
            // 
            this.btnCompile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCompile.Enabled = false;
            this.btnCompile.Image = ((System.Drawing.Image)(resources.GetObject("btnCompile.Image")));
            this.btnCompile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(24, 24);
            this.btnCompile.Text = "&Compilar";
            this.btnCompile.ToolTipText = "Compilar";
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRun
            // 
            this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRun.Enabled = false;
            this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
            this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(24, 24);
            this.btnRun.Text = "&Executar";
            this.btnRun.ToolTipText = "Executar";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnPause
            // 
            this.btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPause.Enabled = false;
            this.btnPause.Image = ((System.Drawing.Image)(resources.GetObject("btnPause.Image")));
            this.btnPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(24, 24);
            this.btnPause.Text = "&Pausar";
            this.btnPause.ToolTipText = "Pausar";
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStop.Enabled = false;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(24, 24);
            this.btnStop.Text = "Parar";
            this.btnStop.ToolTipText = "Parar";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnStepOver
            // 
            this.btnStepOver.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepOver.Enabled = false;
            this.btnStepOver.Image = ((System.Drawing.Image)(resources.GetObject("btnStepOver.Image")));
            this.btnStepOver.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepOver.Name = "btnStepOver";
            this.btnStepOver.Size = new System.Drawing.Size(24, 24);
            this.btnStepOver.Text = "Pular Função";
            this.btnStepOver.ToolTipText = "Pular Função";
            this.btnStepOver.Click += new System.EventHandler(this.btnStepOver_Click);
            // 
            // btnStepInto
            // 
            this.btnStepInto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepInto.Enabled = false;
            this.btnStepInto.Image = ((System.Drawing.Image)(resources.GetObject("btnStepInto.Image")));
            this.btnStepInto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepInto.Name = "btnStepInto";
            this.btnStepInto.Size = new System.Drawing.Size(24, 24);
            this.btnStepInto.Text = "Entrar na Função";
            this.btnStepInto.ToolTipText = "Entrar na Função";
            this.btnStepInto.Click += new System.EventHandler(this.btnStepInto_Click);
            // 
            // btnStepReturn
            // 
            this.btnStepReturn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStepReturn.Enabled = false;
            this.btnStepReturn.Image = ((System.Drawing.Image)(resources.GetObject("btnStepReturn.Image")));
            this.btnStepReturn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStepReturn.Name = "btnStepReturn";
            this.btnStepReturn.Size = new System.Drawing.Size(24, 24);
            this.btnStepReturn.Text = "Sair da Função";
            this.btnStepReturn.ToolTipText = "Sair da Função";
            this.btnStepReturn.Click += new System.EventHandler(this.btnStepReturn_Click);
            // 
            // btnRunToCursor
            // 
            this.btnRunToCursor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRunToCursor.Enabled = false;
            this.btnRunToCursor.Image = ((System.Drawing.Image)(resources.GetObject("btnRunToCursor.Image")));
            this.btnRunToCursor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRunToCursor.Name = "btnRunToCursor";
            this.btnRunToCursor.Size = new System.Drawing.Size(24, 24);
            this.btnRunToCursor.Text = "Executar até o Cursor";
            this.btnRunToCursor.ToolTipText = "Executar até o Cursor";
            this.btnRunToCursor.Click += new System.EventHandler(this.btnRunToCursor_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // btnToggleBreakpoint
            // 
            this.btnToggleBreakpoint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnToggleBreakpoint.Image = ((System.Drawing.Image)(resources.GetObject("btnToggleBreakpoint.Image")));
            this.btnToggleBreakpoint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnToggleBreakpoint.Name = "btnToggleBreakpoint";
            this.btnToggleBreakpoint.Size = new System.Drawing.Size(24, 24);
            this.btnToggleBreakpoint.Text = "Alternar Ponto de Interrupção";
            this.btnToggleBreakpoint.ToolTipText = "Alternar Ponto de Interrupção";
            this.btnToggleBreakpoint.Click += new System.EventHandler(this.btnToggleBreakpoint_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Arquivos de código fonte|*.sl|Todos os arquivos|*.*";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Arquivos de código fonte|*.sl|Todos os arquivos|*.*";
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "BreakpointEnabled");
            this.imageList.Images.SetKeyName(1, "BreakpointOff");
            // 
            // bwCompiler
            // 
            this.bwCompiler.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwCompiler_DoWork);
            // 
            // splitContainerData
            // 
            this.splitContainerData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerData.Location = new System.Drawing.Point(0, 0);
            this.splitContainerData.Name = "splitContainerData";
            // 
            // splitContainerData.Panel1
            // 
            this.splitContainerData.Panel1.Controls.Add(this.gbStack);
            // 
            // splitContainerData.Panel2
            // 
            this.splitContainerData.Panel2.Controls.Add(this.gbStrings);
            this.splitContainerData.Size = new System.Drawing.Size(659, 206);
            this.splitContainerData.SplitterDistance = 400;
            this.splitContainerData.TabIndex = 3;
            // 
            // gbStack
            // 
            this.gbStack.Controls.Add(this.dgvStack);
            this.gbStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbStack.Location = new System.Drawing.Point(0, 0);
            this.gbStack.Name = "gbStack";
            this.gbStack.Size = new System.Drawing.Size(400, 206);
            this.gbStack.TabIndex = 0;
            this.gbStack.TabStop = false;
            this.gbStack.Text = "Pilha";
            // 
            // gbStrings
            // 
            this.gbStrings.Controls.Add(this.dgvStrings);
            this.gbStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbStrings.Location = new System.Drawing.Point(0, 0);
            this.gbStrings.Name = "gbStrings";
            this.gbStrings.Size = new System.Drawing.Size(255, 206);
            this.gbStrings.TabIndex = 0;
            this.gbStrings.TabStop = false;
            this.gbStrings.Text = "Textos";
            // 
            // dgvStrings
            // 
            this.dgvStrings.AllowUserToAddRows = false;
            this.dgvStrings.AllowUserToDeleteRows = false;
            this.dgvStrings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStrings.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.StringHA,
            this.StringRefCount,
            this.StringLen,
            this.StringValue});
            this.dgvStrings.ContextMenuStrip = this.mnuStackView;
            this.dgvStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvStrings.Location = new System.Drawing.Point(3, 16);
            this.dgvStrings.Name = "dgvStrings";
            this.dgvStrings.ReadOnly = true;
            this.dgvStrings.Size = new System.Drawing.Size(249, 187);
            this.dgvStrings.TabIndex = 3;
            // 
            // RA
            // 
            this.RA.HeaderText = "RA";
            this.RA.Name = "RA";
            this.RA.ReadOnly = true;
            this.RA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.RA.ToolTipText = "Endereço Residente";
            // 
            // HA
            // 
            this.HA.HeaderText = "HA";
            this.HA.Name = "HA";
            this.HA.ReadOnly = true;
            this.HA.ToolTipText = "Endereço Host";
            // 
            // Value
            // 
            this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.Value.HeaderText = "Valor";
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            this.Value.Width = 37;
            // 
            // View
            // 
            this.View.HeaderText = "Visualização em caracteres";
            this.View.Name = "View";
            this.View.ReadOnly = true;
            this.View.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.View.Width = 200;
            // 
            // StringHA
            // 
            this.StringHA.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringHA.HeaderText = "HA";
            this.StringHA.Name = "StringHA";
            this.StringHA.ReadOnly = true;
            this.StringHA.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.StringHA.ToolTipText = "Endereço Host";
            this.StringHA.Width = 28;
            // 
            // StringRefCount
            // 
            this.StringRefCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringRefCount.HeaderText = "#Referências";
            this.StringRefCount.Name = "StringRefCount";
            this.StringRefCount.ReadOnly = true;
            this.StringRefCount.ToolTipText = "Número de Referências";
            this.StringRefCount.Width = 96;
            // 
            // StringLen
            // 
            this.StringLen.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringLen.HeaderText = "Tamanho";
            this.StringLen.Name = "StringLen";
            this.StringLen.ReadOnly = true;
            this.StringLen.ToolTipText = "Comprimento do Texto";
            this.StringLen.Width = 77;
            // 
            // StringValue
            // 
            this.StringValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.StringValue.HeaderText = "Valor";
            this.StringValue.Name = "StringValue";
            this.StringValue.ReadOnly = true;
            this.StringValue.Width = 37;
            // 
            // FrmSimpleCompiler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1462, 618);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.toolBar);
            this.Controls.Add(this.statusBar);
            this.DoubleBuffered = true;
            this.Name = "FrmSimpleCompiler";
            this.Text = "Simple Compiler";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSimpleCompiler_FormClosing);
            this.Load += new System.EventHandler(this.FrmSimpleCompiler_Load);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainerCode.Panel1.ResumeLayout(false);
            this.splitContainerCode.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerCode)).EndInit();
            this.splitContainerCode.ResumeLayout(false);
            this.tcRight.ResumeLayout(false);
            this.tpAssembly.ResumeLayout(false);
            this.tpLocals.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocals)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWatch)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStack)).EndInit();
            this.mnuStackView.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.splitContainerData.Panel1.ResumeLayout(false);
            this.splitContainerData.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerData)).EndInit();
            this.splitContainerData.ResumeLayout(false);
            this.gbStack.ResumeLayout(false);
            this.gbStrings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStrings)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerCode;
        private System.Windows.Forms.TabControl tcSources;
        private System.Windows.Forms.TabControl tcRight;
        private System.Windows.Forms.TabPage tpAssembly;
        private System.Windows.Forms.TabPage tpLocals;
        private System.Windows.Forms.DataGridView dgvWatch;
        private System.Windows.Forms.DataGridView dgvLocals;
        private System.Windows.Forms.RichTextBox txtConsole;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripProgressBar progress;
        private System.Windows.Forms.ToolStripStatusLabel statusText;
        private System.Windows.Forms.ToolStrip toolBar;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnReload;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton btnCompile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnRun;
        private System.Windows.Forms.ToolStripButton btnPause;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnStepOver;
        private System.Windows.Forms.ToolStripButton btnStepInto;
        private System.Windows.Forms.ToolStripButton btnStepReturn;
        private System.Windows.Forms.ToolStripButton btnRunToCursor;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnToggleBreakpoint;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ImageList imageList;
        private System.ComponentModel.BackgroundWorker bwCompiler;
        private System.Windows.Forms.Integration.ElementHost wpfAssemblyHost;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewButtonColumn dataGridViewButtonColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.DataGridViewButtonColumn columnValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvStack;
        private System.Windows.Forms.ContextMenuStrip mnuStackView;
        private System.Windows.Forms.ToolStripMenuItem mnuStackViewAlign16;
        private System.Windows.Forms.ToolStripMenuItem mnuStackViewAlign8;
        private System.Windows.Forms.ToolStripMenuItem mnuStackViewAlign4;
        private System.Windows.Forms.SplitContainer splitContainerData;
        private System.Windows.Forms.GroupBox gbStack;
        private System.Windows.Forms.GroupBox gbStrings;
        private System.Windows.Forms.DataGridView dgvStrings;
        private System.Windows.Forms.DataGridViewTextBoxColumn RA;
        private System.Windows.Forms.DataGridViewTextBoxColumn HA;
        private System.Windows.Forms.DataGridViewButtonColumn Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn View;
        private System.Windows.Forms.DataGridViewTextBoxColumn StringHA;
        private System.Windows.Forms.DataGridViewTextBoxColumn StringRefCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn StringLen;
        private System.Windows.Forms.DataGridViewButtonColumn StringValue;
    }
}

