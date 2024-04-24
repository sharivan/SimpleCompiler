using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;

using Asm;

using Comp;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

using VM;

using MessageBox = System.Windows.Forms.MessageBox;

namespace SimpleCompiler.GUI;

public partial class FrmSimpleCompiler : Form
{
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

    private delegate void ParsingDelegate(int lineNumber);

    private string sourceFile = null;
    private readonly Comp.Compiler compiler;
    private readonly Assembler assembler;
    private bool vmRunning = false;
    private bool paused = false;
    private readonly VirtualMachine vm;
    private Thread vmThread;
    private readonly object inputLock = new();
    private bool scanning = false;
    private int inputPos = 0;
    private string input = null;

    internal ImageSource enabledImage;
    internal ImageSource disabledImage;

    internal int sourceTabNextID;
    internal List<SourceTab> sourceTabs;
    internal Dictionary<int, SourceTab> sourceTabsMapByID;
    internal Dictionary<string, SourceTab> sourceTabsMapByFileName;

    private readonly Dictionary<string, string[]> sourceCache;
    private bool compiled = false;
    private int parsedLines;
    private int totalLines;
    private readonly TextEditor txtAssembly;
    private readonly SteppingRenderer lineBackgroundRenderer;
    private readonly BreakPointMargin breakpointMargin;
    private readonly Dictionary<int, int> lineNumberToIP;
    private readonly Dictionary<int, int> ipToLineNumber;
    private bool assemblyFocused;

    private Rectangle unmaximizedBounds;

    private System.Drawing.Color consoleForeColor;
    private System.Drawing.Color consoleBackColor;

    private int stackViewAlignSize = 16;
    private readonly Image addImage;
    private readonly Image closeImage;

    public FrmSimpleCompiler()
    {
        InitializeComponent();

        compiler = new Comp.Compiler("examples");
        compiler.OnCompileError += OnCompileError;

        assembler = new Assembler();

        vm = new VirtualMachine();
        vm.OnDisassemblyLine += DisassemblyLine;
        vm.OnConsoleRead += ConsoleRead;
        vm.OnConsolePrint += ConsolePrint;
        vm.OnPause += OnPause;
        vm.OnStep += OnStep;
        vm.OnBreakpoint += OnBreakpoint;

        sourceTabNextID = 0;
        sourceTabs = [];
        sourceTabsMapByID = [];
        sourceTabsMapByFileName = [];

        sourceCache = [];

        IHighlightingDefinition customHighlighting;

        string resourceName = "SimpleCompiler.Resources.slHighlighting.xshd";
        var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Could not find embedded resource");

        using (s)
        {
            using XmlReader reader = new XmlTextReader(s);
            customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        // and register it in the HighlightingManager
        HighlightingManager.Instance.RegisterHighlighting("Custom Highlighting", [".sl"], customHighlighting);

        txtAssembly = new TextEditor();
        wpfAssemblyHost.Child = txtAssembly;
        txtAssembly.ShowLineNumbers = false;
        txtAssembly.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(txtAssembly.Options);
        txtAssembly.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".sl");
        txtAssembly.FontSize = 14;
        txtAssembly.Foreground = new SolidColorBrush((System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#DCDCDC"));
        txtAssembly.Background = new SolidColorBrush((System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#1E1E1E"));
        txtAssembly.IsReadOnly = true;
        txtAssembly.Focusable = true;

        lineBackgroundRenderer = new SteppingRenderer(txtAssembly.TextArea.TextView);
        txtAssembly.TextArea.TextView.BackgroundRenderers.Add(lineBackgroundRenderer);

        breakpointMargin = new BreakPointMargin(this);
        txtAssembly.TextArea.LeftMargins.Insert(0, breakpointMargin);

        lineNumberToIP = [];
        ipToLineNumber = [];

        addImage = Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("SimpleCompiler.Resources.Img.5700.add.png"));
        closeImage = Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("SimpleCompiler.Resources.Img.5657.close.png"));

        tcSources.HandleCreated += TcSources_HandleCreated;
    }

    private void OnCompileError(SourceInterval interval, string message)
    {
        BeginInvoke((MethodInvoker) delegate
        {
            if (interval.IsValid())
            {
                ConsolePrintLn($"Erro de compilação na linha {interval.FirstLine}: {message}.");

                if (interval.FileName != null && interval.Start >= 0)
                {
                    var tab = SelectSourceTab(interval.FileName, true);
                    if (tab != null)
                    {
                        tab.txtSource.SelectionStart = interval.Start;
                        tab.txtSource.SelectionLength = interval.Length;
                    }
                }
            }
            else
            {
                ConsolePrintLn($"Erro de compilação: {message}.");
            }
        });
    }

    private void DisassemblyLine(int ip, string lineText)
    {
        if (ip == -1)
        {
            txtAssembly.AppendText("\n");
        }
        else
        {
            var (fileName, line) = vm.GetLineFromIP(ip);
            if (line != -1)
            {
                string sourceLine = GetLineFromFile(fileName, line);
                if (sourceLine != null)
                    txtAssembly.AppendText($"// {sourceLine}\n");
            }

            int lineNumber = txtAssembly.LineCount;
            lineNumberToIP.Add(lineNumber, ip);
            ipToLineNumber.Add(ip, lineNumber);

            txtAssembly.AppendText($"{lineText}\n");
        }
    }

    private string GetLineFromFile(string fileName, int line)
    {
        if (line <= 0)
            return null;

        if (!sourceCache.TryGetValue(fileName, out string[] lines))
        {
            lines = File.ReadAllLines(fileName);
            sourceCache.Add(fileName, lines);
        }

        return line <= lines.Length ? lines[line - 1] : null;
    }

    private string ConsoleRead()
    {
        string result = null;

        lock (inputLock)
        {
            try
            {
                while (scanning)
                    Monitor.Wait(inputLock);

                scanning = true;

                BeginInvoke((MethodInvoker) delegate
                {
                    inputPos = txtConsole.Text.Length;
                    consoleForeColor = txtConsole.ForeColor;
                    consoleBackColor = txtConsole.BackColor;
                    txtConsole.ForeColor = System.Drawing.Color.White;
                    txtConsole.BackColor = System.Drawing.Color.Black;
                    txtConsole.SelectionStart = inputPos;
                    txtConsole.SelectionLength = 0;
                    txtConsole.ReadOnly = false;
                    txtConsole.Focus();
                });

                input = null;
                while (input == null)
                    Monitor.Wait(inputLock);

                result = input;
                input = null;
                scanning = false;
                Monitor.PulseAll(inputLock);
            }
            catch (ThreadAbortException e)
            {
                BeginInvoke((MethodInvoker) delegate
                {
                    txtConsole.ForeColor = consoleForeColor;
                    txtConsole.BackColor = consoleBackColor;
                    txtConsole.ReadOnly = true;
                });

                input = null;
                scanning = false;
                Monitor.PulseAll(inputLock);

                throw e;
            }
        }

        return result;
    }

    private void ConsolePrint(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => ConsolePrint(message));
        }
        else
        {
            lock (inputLock)
            {
                txtConsole.Text += message;
                txtConsole.SelectionStart = txtConsole.Text.Length;
                txtConsole.SelectionLength = 0;
            }
        }
    }

    private void ConsolePrintLn(string message)
    {
        ConsolePrint(message + "\n");
    }

    private void ShowStepCursor(int ip)
    {
        FetchData();

        if (ipToLineNumber.TryGetValue(ip, out int lineNumber))
        {
            lineBackgroundRenderer.Enabled = true;
            lineBackgroundRenderer.LineNumber = lineNumber;
            var visualLine = txtAssembly.TextArea.TextView.VisualLines.FirstOrDefault(line => line.FirstDocumentLine.LineNumber == lineNumber);

            if (visualLine == null)
                txtAssembly.ScrollToLine(lineNumber);
        }

        (string filename, lineNumber) = vm.GetLineFromIP(ip);
        if (lineNumber == -1)
            return;

        var tab = SelectSourceTab(filename, true, false);
        if (tab != null)
        {
            var visualLine = tab.txtSource.TextArea.TextView.VisualLines.FirstOrDefault(line => line.FirstDocumentLine.LineNumber == lineNumber);
            if (visualLine == null)
                tab.txtSource.ScrollToLine(lineNumber);

            tab.lineBackgroundRenderer.Enabled = true;
            tab.lineBackgroundRenderer.LineNumber = lineNumber;
        }
    }

    private void OnPause(int ip)
    {
        BeginInvoke((MethodInvoker) delegate
        {
            paused = true;

            statusBar.Items["statusText"].Text = "Pausado.";

            SetRunEnabled(true);
            SetPauseEnabled(false);
            SetStopEnabled(true);
            SetStepOverEnabled(true);
            SetStepIntoEnabled(true);
            SetStepOutEnabled(true);
            SetRunToCursorEnabled(true);

            ShowStepCursor(ip);
        });
    }

    private void OnStep(int ip, SteppingMode mode)
    {
        BeginInvoke((MethodInvoker) delegate
        {
            paused = true;

            statusBar.Items["statusText"].Text = $"Executando passo a passo ({mode switch
            {
                SteppingMode.RUN => "pausado",
                SteppingMode.INTO => "entrando em funções",
                SteppingMode.OVER => "pulando funções",
                SteppingMode.OUT => "saindo da função",
                _ => throw new NotImplementedException()
            }}).";

            SetRunEnabled(true);
            SetPauseEnabled(false);
            SetStopEnabled(true);
            SetStepOverEnabled(true);
            SetStepIntoEnabled(true);
            SetStepOutEnabled(true);
            SetRunToCursorEnabled(true);

            ShowStepCursor(ip);
        });
    }

    private void OnBreakpoint(Breakpoint bp)
    {
        BeginInvoke((MethodInvoker) delegate
        {
            paused = true;

            statusBar.Items["statusText"].Text = "Ponto de interrupção encontrado.";

            SetRunEnabled(true);
            SetPauseEnabled(false);
            SetStopEnabled(true);
            SetStepOverEnabled(true);
            SetStepIntoEnabled(true);
            SetStepOutEnabled(true);
            SetRunToCursorEnabled(true);

            ShowStepCursor(bp.IP);
        });
    }

    private void BtnClearConsole_Click(object sender, EventArgs e)
    {
        txtConsole.Clear();
    }

    private void PostRun(bool success, bool aborted, Exception exception)
    {
        BeginInvoke((MethodInvoker) delegate
        {
            vmRunning = false;
            paused = true;

            if (success)
            {
                statusBar.Items["statusText"].Text = "Programa terminado com sucesso.";
            }
            else if (aborted)
            {
                statusBar.Items["statusText"].Text = "Programa abortado.";
            }
            else
            {
                statusBar.Items["statusText"].Text = $"Programa terminado com falha: {exception.Message}.";
                ConsolePrintLn(exception.StackTrace);
            }

            SetCompileEnabled(true);
            SetRunEnabled(true);
            SetPauseEnabled(false);
            SetStopEnabled(false);
            SetStepOverEnabled(true);
            SetStepIntoEnabled(false);
            SetStepOutEnabled(false);
            SetRunToCursorEnabled(true);
        });
    }

    public void VMRun(SteppingMode steppingMode, bool onSource, int runToIP)
    {
        try
        {
            vm.Run(steppingMode, onSource, runToIP);
            PostRun(true, false, null);
        }
        catch (ThreadAbortException)
        {
            PostRun(false, true, null);
        }
        catch (Exception e)
        {
            PostRun(false, false, e);
        }
    }

    private void SetupStackView()
    {
        int lines = vm.StackSize / stackViewAlignSize;
        dgvStack.RowCount = lines;
    }

    private void SetupStringView()
    {
        dgvStrings.RowCount = Math.Max(vm.ObjectCount, 50);
    }

    private void FetchStackViewRow(int row)
    {
        int firstAddr = row * stackViewAlignSize;
        int addr = firstAddr;

        string data = "";
        string view = "";
        byte oldValue = 0;

        nint dataValue = 0;
        for (int col = 0; col < stackViewAlignSize; col++)
        {
            if (addr >= 0 && addr < vm.StackSize)
            {
                byte value = vm.ReadStackByte(addr++);
                dataValue |= ((nint) (value & 0xff)) << (8 * col);

                data += string.Format("{0:x2}", value) + " ";

                if ((col & 1) != 0)
                    view += (char) (oldValue | (value << 8)) + " ";

                oldValue = value;
            }
            else
            {
                data += "   ";
                view += "  ";
            }
        }

        // TODO Isso deveria funcionar pra qualquer valor de alinhamento de pilha, não somente para 4 bytes. Corrigir isso!
        if (stackViewAlignSize == 4 && vm.IsStackHostAddr((IntPtr) dataValue))
        {
            int residentAddr = vm.HostToResidentAddr((IntPtr) dataValue);
            view = "=>" + string.Format("{0:x8}", residentAddr);
        }

        string residentAddress = string.Format("{0:x8}", row * stackViewAlignSize);

        if (vm.BP == firstAddr)
            residentAddress += "[BP]";

        if (vm.SP == firstAddr)
        {
            residentAddress += "[SP]";
            dgvStack.Rows[row].Selected = true;
            dgvStack.CurrentCell = dgvStack[0, row];
        }

        string hostAddress = vm.ResidentToHostAddr(row * stackViewAlignSize).ToString("x8");

        dgvStack[0, row].Value = residentAddress;
        dgvStack[1, row].Value = hostAddress;
        dgvStack[2, row].Value = data;
        dgvStack[3, row].Value = view;
    }

    private void FetchStringView(int row, IntPtr str)
    {
        if (str != IntPtr.Zero)
        {
            unsafe
            {
                var rec = (VirtualMachine.ObjectRec*) (str - VirtualMachine.OBJECT_REC_SIZE).ToPointer();

                dgvStrings[0, row].Value = str.ToString("x8");
                dgvStrings[1, row].Value = rec->refCount.ToString();
                dgvStrings[2, row].Value = rec->size.ToString();
                dgvStrings[3, row].Value = VirtualMachine.ReadPointerString(str);
            }
        }
        else
        {
            dgvStrings[0, row].Value = "";
            dgvStrings[1, row].Value = "";
            dgvStrings[2, row].Value = "";
            dgvStrings[3, row].Value = "";
        }
    }

    private void FetchData()
    {
        if (!paused)
            return;

        FetchCallStack();
        FetchVariables();
        FetchStackData();
        FetchStringData();

        lblRegisters.Text = $"IP: {vm.IP:x8} - BP: {vm.BP:x8} - SP: {vm.SP:x8}";
    }

    private void FetchCallStack()
    {
    }

    private static string ToString(object value)
    {
        return value == null
            ? "nulo"
            : value switch
            {
                bool b => b ? "verdade" : "falso",
                float f => f.ToString(CultureInfo.InvariantCulture),
                double d => d.ToString(CultureInfo.InvariantCulture),
                IntPtr ptr => "0x" + string.Format("{0:x8}", ptr.ToInt64()),
                string str => $"\"{Regex.Escape(str)}\"", // TODO : Isto é só um quebra galho temporário.
                _ => value.ToString()
            };
    }

    private void FetchVariables()
    {
        dgvVariables.Rows.Clear();
        dgvVariables.Focus();
        dgvVariables.SuspendLayout();

        var variables = new List<Variable>();
        var (fileName, lineNumber) = vm.GetLineFromIP(vm.IP, false);
        vm.FetchDeclaredVariablesAtLine(fileName, lineNumber, variables);

        foreach (var variable in variables)
            dgvVariables.Rows.Add(variable.Name, ToString(vm.GetVariableValue(variable)), variable.Type.ToString());

        dgvVariables.ResumeLayout();
    }

    private void FetchStackData()
    {
        if (!paused)
            return;

        if (dgvStack.FirstDisplayedCell == null)
            return;

        dgvStack.Focus();
        dgvStack.SuspendLayout();

        int visibleRowsCount = dgvStack.DisplayedRowCount(true);

        int firstDisplayedRowIndex = dgvStack.FirstDisplayedCell.RowIndex - 10;
        if (firstDisplayedRowIndex < 0)
            firstDisplayedRowIndex = 0;

        int lastvisibleRowIndex = firstDisplayedRowIndex + visibleRowsCount + 10;
        if (lastvisibleRowIndex >= dgvStack.RowCount)
            lastvisibleRowIndex = dgvStack.RowCount - 1;

        for (int rowIndex = firstDisplayedRowIndex; rowIndex <= lastvisibleRowIndex; rowIndex++)
        {
            FetchStackViewRow(rowIndex);
            Application.DoEvents();
        }

        dgvStack.ResumeLayout();
    }

    private void FetchStringData()
    {
        if (!paused)
            return;

        if (dgvStrings.FirstDisplayedCell == null)
            return;

        dgvStrings.SuspendLayout();

        SetupStringView();

        int visibleRowsCount = dgvStrings.DisplayedRowCount(true);

        int firstDisplayedRowIndex = dgvStrings.FirstDisplayedCell.RowIndex - 10;
        if (firstDisplayedRowIndex < 0)
            firstDisplayedRowIndex = 0;

        int lastvisibleRowIndex = firstDisplayedRowIndex + visibleRowsCount + 10;
        if (lastvisibleRowIndex >= dgvStrings.RowCount)
            lastvisibleRowIndex = dgvStrings.RowCount - 1;

        var str = vm.LastObject;
        for (int rowIndex = 0; str != IntPtr.Zero && rowIndex < firstDisplayedRowIndex; rowIndex++)
        {
            unsafe
            {
                var rec = (VirtualMachine.ObjectRec*) (str - VirtualMachine.OBJECT_REC_SIZE).ToPointer();
                str = rec->previous;
            }
        }

        for (int rowIndex = firstDisplayedRowIndex; rowIndex <= lastvisibleRowIndex; rowIndex++)
        {
            FetchStringView(rowIndex, str);
            Application.DoEvents();

            if (str != IntPtr.Zero)
            {
                unsafe
                {
                    var rec = (VirtualMachine.ObjectRec*) (str - VirtualMachine.OBJECT_REC_SIZE).ToPointer();
                    str = rec->previous;
                }
            }
        }

        dgvStrings.ResumeLayout();
    }

    private void StartVM(SteppingMode steppingMode = SteppingMode.RUN, bool onSource = false, int runToIP = -1)
    {
        SetupStackView();
        SetupStringView();

        vmRunning = true;
        vmThread = new Thread(() => VMRun(steppingMode, onSource, runToIP));
        vmThread.Start();
    }

    private void SetCompileEnabled(bool value)
    {
        btnCompile.Enabled = value;
    }

    private void SetRunEnabled(bool value)
    {
        btnRun.Enabled = value;
        mnuRun.Enabled = value;
    }

    private void SetPauseEnabled(bool value)
    {
        btnPause.Enabled = value;
        mnuPause.Enabled = value;
    }

    private void SetStopEnabled(bool value)
    {
        btnStop.Enabled = value;
        mnuStop.Enabled = value;
    }

    private void SetStepOverEnabled(bool value)
    {
        btnStepOver.Enabled = value;
        mnuStepOver.Enabled = value;
    }

    private void SetStepIntoEnabled(bool value)
    {
        btnStepInto.Enabled = value;
        mnuStepInto.Enabled = value;
    }

    private void SetStepOutEnabled(bool value)
    {
        btnStepOut.Enabled = value;
        mnuStepOut.Enabled = value;
    }

    private void SetRunToCursorEnabled(bool value)
    {
        btnRunToCursor.Enabled = value;
        mnuRunToCursor.Enabled = value;
    }

    public void Run()
    {
        paused = false;

        foreach (var sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        SetCompileEnabled(false);
        SetRunEnabled(false);
        SetPauseEnabled(true);
        SetStopEnabled(true);
        SetStepOverEnabled(true);
        SetStepIntoEnabled(false);
        SetStepOutEnabled(false);
        SetRunToCursorEnabled(false);

        if (!vmRunning)
            StartVM();
        else
            vm.Resume();
    }

    private void BtnRun_Click(object sender, EventArgs e)
    {
        Run();
    }

    public void Compile()
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var currentTab = sourceTabs[currentPageIndex];
        currentTab.breakpointMargin.ClearBreakpoints();
        currentTab.SetSource(currentTab.txtSource.Text);
        currentTab.m_SourceCode = currentTab.txtSource.Text;

        if (!currentTab.Saved)
        {
            currentTab.Save();
            if (!currentTab.Saved)
                return;
        }

        sourceFile = currentTab.FileName;

        foreach (var tab in sourceTabs)
        {
            tab.breakpointMargin.ClearBreakpoints();
            tab.SetSource(tab.txtSource.Text);
            tab.m_SourceCode = tab.txtSource.Text;
        }

        txtAssembly.Clear();
        lineNumberToIP.Clear();
        ipToLineNumber.Clear();
        breakpointMargin.ClearBreakpoints();

        SetCompileEnabled(false);
        statusBar.Items["statusText"].Text = "Compilando...";

        bwCompiler.RunWorkerAsync();
    }

    private void BtnCompile_Click(object sender, EventArgs e)
    {
        Compile();
    }

    private void MnuConsoleCopy_Click(object sender, EventArgs e)
    {
        System.Windows.Clipboard.SetText(txtConsole.Text);
    }

    private void MnuConsoleSelectAll_Click(object sender, EventArgs e)
    {
        txtConsole.SelectAll();
    }

    private void TxtConsole_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == 13)
        {
            lock (inputLock)
            {
                try
                {
                    input = txtConsole.Text.Substring(inputPos, txtConsole.Text.Length - inputPos - 1);
                }
                catch (ArgumentOutOfRangeException)
                {
                    input = "";
                }

                txtConsole.ReadOnly = true;
                txtConsole.ForeColor = consoleForeColor;
                txtConsole.BackColor = consoleBackColor;
                e.Handled = true;
                Monitor.PulseAll(inputLock);
            }
        }
        else if (txtConsole.SelectionStart < inputPos)
        {
            e.KeyChar = '\0';
            e.Handled = true;
        }
    }

    public SourceTab NewSourceTab()
    {
        SetCompileEnabled(true);
        SourceTab result = new(this);
        sourceTabs.Add(result);
        sourceTabsMapByID[result.ID] = result;
        return result;
    }

    public SourceTab NewSourceTab(string fileName, bool selected = false, bool focused = false, double zoom = 1)
    {
        if (sourceTabsMapByFileName.TryGetValue(fileName, out var tab))
            return tab;

        if (!File.Exists(fileName))
            return null;

        SetCompileEnabled(true);
        tab = new SourceTab(this, fileName, zoom);
        sourceTabs.Add(tab);
        sourceTabsMapByID[tab.ID] = tab;
        sourceTabsMapByFileName[fileName] = tab;

        if (selected)
        {
            tcSources.SelectedTab = tab.page;
            if (focused)
                tab.Focus();
        }

        return tab;
    }

    public SourceTab SelectSourceTab(int index)
    {
        var tab = sourceTabs[index];
        tab.Focus();
        return tab;
    }

    public SourceTab SelectSourceTab(string fileName, bool openIfNotExist = false, bool focused = true)
    {
        if (sourceTabsMapByFileName.TryGetValue(fileName, out var tab))
        {
            tcSources.SelectedTab = tab.page;

            if (focused)
                tab.Focus();

            return tab;
        }

        return openIfNotExist ? NewSourceTab(fileName, true, focused) : null;
    }

    public void CloseSourceTab(int index)
    {
        var tab = sourceTabs[index];
        tab.Close();

        SetCompileEnabled(sourceTabs.Count > 0);
    }

    public void CloseSourceTab(string fileName)
    {
        if (sourceTabsMapByFileName.TryGetValue(fileName, out var tab))
            tab.Close();

        SetCompileEnabled(sourceTabs.Count > 0);
    }

    private void BtnNew_Click(object sender, EventArgs e)
    {
        var tab = NewSourceTab();
        tab.Focus();
    }

    public void OpenFileDialog()
    {
        var result = openFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            string sSourceFileName = openFileDialog.FileName;

            if (File.Exists(sSourceFileName))
            {
                if (sourceTabsMapByFileName.ContainsKey(sSourceFileName))
                {
                    var sourceTab = sourceTabsMapByFileName[sSourceFileName];
                    sourceTab.Load(sSourceFileName);
                    sourceTab.Focus();
                }
                else
                {
                    NewSourceTab(sSourceFileName, true, true);
                }
            }
            else
            {
                MessageBox.Show("Arquivo não existe.");
            }
        }
    }

    private void BtnOpen_Click(object sender, EventArgs e)
    {
        OpenFileDialog();
    }

    public void ReloadCurrentTab()
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex != -1)
        {
            var sourceTab = sourceTabs[currentPageIndex];
            sourceTab.Reload();
        }
    }

    private void BtnReload_Click(object sender, EventArgs e)
    {
        ReloadCurrentTab();
    }

    public void SaveCurrentTab()
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex != -1)
        {
            var sourceTab = sourceTabs[currentPageIndex];
            sourceTab.Save();
        }
    }

    public void SaveCurrentTabAs()
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var sourceTab = sourceTabs[currentPageIndex];
        sourceTab.OpenSaveDialog();
    }

    public void CloseCurrentTab()
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var sourceTab = sourceTabs[currentPageIndex];
        sourceTab.Close();
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        SaveCurrentTab();
    }

    public void Pause()
    {
        SetPauseEnabled(false);
        vm.Pause();
    }

    private void BtnPause_Click(object sender, EventArgs e)
    {
        Pause();
    }

    public void Stop()
    {
        for (int i = 0; i < sourceTabs.Count; i++)
        {
            var sourceTab = sourceTabs[i];
            sourceTab.lineBackgroundRenderer.Enabled = false;
        }

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Terminando o programa...";

        SetRunEnabled(false);
        SetPauseEnabled(false);
        SetStopEnabled(false);
        SetStepOverEnabled(false);
        SetStepIntoEnabled(false);
        SetStepOutEnabled(false);
        SetRunToCursorEnabled(false);

        vmThread.Abort();
    }

    private void BtnStop_Click(object sender, EventArgs e)
    {
        Stop();
    }

    private bool FetchAndGetAssemblyFocused()
    {
        if (wpfAssemblyHost.Focused)
        {
            assemblyFocused = true;
        }
        else
        {
            int index = tcSources.SelectedIndex;
            if (index != -1)
            {
                var sourceTab = sourceTabs[index];
                if (sourceTab.wpfHost.Focused)
                    assemblyFocused = false;
            }
        }

        return assemblyFocused;
    }

    public void StepOver()
    {
        paused = false;

        foreach (var sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        SetCompileEnabled(false);
        SetRunEnabled(false);
        SetPauseEnabled(true);
        SetStopEnabled(true);
        SetStepOverEnabled(false);
        SetStepIntoEnabled(false);
        SetStepOutEnabled(false);
        SetRunToCursorEnabled(false);

        if (FetchAndGetAssemblyFocused())
        {
            if (vmRunning)
                vm.StepOver();
            else
                StartVM(SteppingMode.INTO);
        }
        else
        {
            if (vmRunning)
                vm.StepOver(true);
            else
                StartVM(SteppingMode.INTO, true);
        }
    }

    private void BtnStepOver_Click(object sender, EventArgs e)
    {
        StepOver();
    }

    public void StepInto()
    {
        paused = false;

        foreach (var sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        SetRunEnabled(false);
        SetPauseEnabled(true);
        SetStopEnabled(true);
        SetStepOverEnabled(true);
        SetStepIntoEnabled(false);
        SetStepOutEnabled(false);
        SetRunToCursorEnabled(false);

        vm.StepInto(!FetchAndGetAssemblyFocused());
    }

    private void BtnStepInto_Click(object sender, EventArgs e)
    {
        StepInto();
    }

    public void StepOut()
    {
        paused = false;

        foreach (var sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        SetPauseEnabled(true);
        SetStopEnabled(true);
        SetStepOverEnabled(true);
        SetStepIntoEnabled(false);
        SetStepOutEnabled(false);
        SetRunToCursorEnabled(false);

        vm.StepReturn(!FetchAndGetAssemblyFocused());
    }

    private void BtnStepReturn_Click(object sender, EventArgs e)
    {
        StepOut();
    }

    public void RunToCursor()
    {
        paused = false;

        foreach (var sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        SetCompileEnabled(false);
        SetPauseEnabled(true);
        SetStopEnabled(true);
        SetStepOverEnabled(true);
        SetStepIntoEnabled(false);
        SetStepOutEnabled(false);
        SetRunToCursorEnabled(false);

        if (FetchAndGetAssemblyFocused())
        {
            var line = txtAssembly.Document.GetLineByOffset(txtAssembly.SelectionStart);
            if (lineNumberToIP.TryGetValue(line.LineNumber, out int ip))
            {
                if (!vmRunning)
                    StartVM(SteppingMode.RUN, false, ip);
                else
                    vm.RunToIP(ip);
            }
            else
            {
                if (!vmRunning)
                    StartVM();
                else
                    vm.Resume();
            }
        }
        else
        {
            int currentTabIndex = tcSources.SelectedIndex;
            if (currentTabIndex == -1)
                return;

            var currentSourceTab = sourceTabs[currentTabIndex];

            var line = currentSourceTab.txtSource.Document.GetLineByOffset(currentSourceTab.txtSource.SelectionStart);
            int ip = vm.GetIPFromLine(currentSourceTab.FileName, line.LineNumber);

            if (ip == -1)
            {
                if (!vmRunning)
                    StartVM(SteppingMode.RUN, true);
                else
                    vm.Resume();
            }
            else
            {
                if (!vmRunning)
                    StartVM(SteppingMode.RUN, true, ip);
                else
                    vm.RunToIP(ip, true);
            }
        }
    }

    private void BtnRunToCursor_Click(object sender, EventArgs e)
    {
        RunToCursor();
    }

    public void ToggleBreakpoint()
    {
        if (wpfAssemblyHost.Focused)
        {
            var line = txtAssembly.Document.GetLineByOffset(txtAssembly.SelectionStart);
            if (lineNumberToIP.TryGetValue(line.LineNumber, out int ip))
            {
                var bp = vm.ToggleBreakPoint(ip);
                if (bp != null)
                    breakpointMargin.ToggleBreakpoint(line.LineNumber, bp);
                else
                    breakpointMargin.RemoveBreakpoint(line.LineNumber);
            }
        }
        else
        {
            int selectedPageIndex = tcSources.SelectedIndex;
            if (selectedPageIndex == -1)
                return;

            var sourceTab = sourceTabs[selectedPageIndex];

            var line = sourceTab.txtSource.Document.GetLineByOffset(sourceTab.txtSource.SelectionStart);
            var breakpoint = sourceTab.FileName != null
                ? vm.ToggleBreakPoint(sourceTab.FileName, line.LineNumber)
                : vm.ToggleBreakPoint($"#{sourceTab.ID}", line.LineNumber);

            if (breakpoint != null)
                sourceTab.breakpointMargin.ToggleBreakpoint(line.LineNumber, breakpoint);
            else
                sourceTab.breakpointMargin.RemoveBreakpoint(line.LineNumber);
        }
    }

    private void BtnToggleBreakpoint_Click(object sender, EventArgs e)
    {
        ToggleBreakpoint();
    }

    private void BwCompiler_DoWork(object sender, DoWorkEventArgs e)
    {
        totalLines = 0;
        parsedLines = 0;

        assembler.Clear();

        if (compiler.CompileFromFile(sourceFile, assembler))
        {
            BeginInvoke((MethodInvoker) delegate
            {
                statusBar.Items["statusText"].Text = "Compilado.";

                vm.Free();
                vm.Initialize(assembler);
                vm.Print();

                paused = true;
                compiled = true;

                SetCompileEnabled(true);
                SetRunEnabled(true);
                SetPauseEnabled(false);
                SetStopEnabled(false);
                SetStepOverEnabled(true);
                SetStepIntoEnabled(false);
                SetStepOutEnabled(false);
                SetRunToCursorEnabled(true);
            });
        }
        else
        {
            BeginInvoke((MethodInvoker) delegate
            {
                statusBar.Items["statusText"].Text = "Erro ao compilar.";

                compiled = false;

                SetCompileEnabled(true);
            });
        }
    }

    private void FrmSimpleCompiler_Load(object sender, EventArgs e)
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
            config.Save();
        }

        int left = section.Left;
        int top = section.Top;
        int width = section.Width;
        int height = section.Height;

        unmaximizedBounds = new Rectangle(left >= 0 ? left : Left, top >= 0 ? top : Top, width > 0 ? width : Width, height > 0 ? height : Height);

        bool maximized = section.Maximized;

        Location = unmaximizedBounds.Location;
        Size = unmaximizedBounds.Size;

        if (maximized)
            WindowState = FormWindowState.Maximized;

        stackViewAlignSize = section.StackViewAlignSize;

        SetSourcesEnabled(section.ViewCodeChecked);
        SetConsoleEnabled(section.ViewConsoleChecked);
        SetAssemblyEnabled(section.ViewAssemblyChecked);
        SetVariablesEnabled(section.ViewVariablesChecked);
        SetMemoryEnabled(section.ViewMemoryChecked);

        var collection = section.Documents;
        foreach (DocumentConfigElement element in collection)
            NewSourceTab(element.FileName, element.Selected, element.Focused, element.Zoom);
    }

    private void FrmSimpleCompiler_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (vmRunning)
            vmThread.Abort();

        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.Sections["ProgramConfiguratinSection"] is not ProgramConfiguratinSection section)
        {
            section = new ProgramConfiguratinSection();
            config.Sections.Add("ProgramConfiguratinSection", section);
        }

        if (WindowState == FormWindowState.Maximized)
        {
            section.Left = unmaximizedBounds.Left;
            section.Top = unmaximizedBounds.Top;
            section.Width = unmaximizedBounds.Width;
            section.Height = unmaximizedBounds.Height;
            section.Maximized = true;
        }
        else
        {
            section.Left = Left;
            section.Top = Top;
            section.Width = Width;
            section.Height = Height;
            section.Maximized = false;
        }

        section.StackViewAlignSize = stackViewAlignSize;

        section.ViewCodeChecked = mnuViewCode.Checked;
        section.ViewConsoleChecked = mnuViewConsole.Checked;
        section.ViewAssemblyChecked = mnuViewAssembly.Checked;
        section.ViewVariablesChecked = mnuViewVariables.Checked;
        section.ViewMemoryChecked = mnuViewMemory.Checked;

        var collection = section.Documents;
        collection.Clear();
        for (int i = 0; i < sourceTabs.Count; i++)
        {
            var tab = sourceTabs[i];
            if (tab.FileName != null)
            {
                var element = collection[tab.FileName];
                element.TabIndex = i;
                element.Selected = tcSources.SelectedTab == tab.page;
                element.Focused = tab.txtSource.IsFocused;
                element.Zoom = tab.Zoom;
            }
        }

        config.Save();
    }

    private void DgvStack_Scroll(object sender, ScrollEventArgs e)
    {
        FetchStackData();
    }

    private void DgvStack_Resize(object sender, EventArgs e)
    {
        FetchStackData();
    }

    private void MnuStackViewAlign16_Click(object sender, EventArgs e)
    {
        stackViewAlignSize = 16;
        FetchData();
    }

    private void MnuStackViewAlign8_Click(object sender, EventArgs e)
    {
        stackViewAlignSize = 8;
        FetchData();
    }

    private void MnuStackViewAlign4_Click(object sender, EventArgs e)
    {
        stackViewAlignSize = 4;
        FetchData();
    }

    private void TcSources_DrawItem(object sender, DrawItemEventArgs e)
    {
        try
        {
            var tabPage = tcSources.TabPages[e.Index];
            var tabRect = tcSources.GetTabRect(e.Index);

            tabRect.Inflate(-2, -2);

            TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                tabRect, tabPage.ForeColor, TextFormatFlags.Left);
            e.Graphics.DrawImage(closeImage,
                tabRect.Right - closeImage.Width,
                tabRect.Top + (tabRect.Height - closeImage.Height) / 2);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private void TcSources_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void TcSources_MouseDown(object sender, MouseEventArgs e)
    {
        for (int i = 0; i < tcSources.TabPages.Count; i++)
        {
            var tabRect = tcSources.GetTabRect(i);
            tabRect.Inflate(-2, -2);
            var imageRect = new Rectangle(
                tabRect.Right - closeImage.Width,
                tabRect.Top + (tabRect.Height - closeImage.Height) / 2,
                closeImage.Width,
                closeImage.Height);

            if (imageRect.Contains(e.Location))
            {
                CloseSourceTab(i);
                break;
            }
        }
    }

    private void TcSources_HandleCreated(object sender, EventArgs e)
    {
        SendMessage(tcSources.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) 16);
    }

    private void MnuRun_Click(object sender, EventArgs e)
    {
        Run();
    }

    private void MnuPause_Click(object sender, EventArgs e)
    {
        Pause();
    }

    private void MnuStop_Click(object sender, EventArgs e)
    {
        Stop();
    }

    private void MnuStepOver_Click(object sender, EventArgs e)
    {
        StepOver();
    }

    private void MnuStepInto_Click(object sender, EventArgs e)
    {
        StepInto();
    }

    private void MnuStepOut_Click(object sender, EventArgs e)
    {
        StepOut();
    }

    private void MnuRunToCursor_Click(object sender, EventArgs e)
    {
        RunToCursor();
    }

    private void MnuToggleBreakpoint_Click(object sender, EventArgs e)
    {
        ToggleBreakpoint();
    }

    private void CheckCodeSplitControl()
    {
        if (!tcSources.Visible && !tcRight.Visible)
        {
            scMain.Panel1Collapsed = true;
            scCode.Visible = false;
        }
        else
        {
            scMain.Panel1Collapsed = false;
            scCode.Visible = true;
        }
    }

    private void CheckBottomSplitControl()
    {
        if (!txtConsole.Visible && !tcDebugView.Visible)
        {
            scMain.Panel2Collapsed = true;
            scBottom.Visible = false;
        }
        else
        {
            scMain.Panel2Collapsed = false;
            scBottom.Visible = true;
        }
    }

    public void SetSourcesEnabled(bool enabled)
    {
        if (enabled)
        {
            if (!scCode.Visible)
            {
                scMain.Panel1Collapsed = false;
                scMain.Panel2Collapsed = !txtConsole.Visible && !tcDebugView.Visible;
                scCode.Visible = true;
                scCode.Panel2Collapsed = !tcRight.Visible;
            }

            scCode.Panel1Collapsed = false;
            tcSources.Visible = true;
        }
        else
        {
            scCode.Panel1Collapsed = true;
            tcSources.Visible = false;
            CheckCodeSplitControl();
        }
    }

    public void SetAssemblyEnabled(bool enabled)
    {
        if (enabled)
        {
            mnuViewAssembly.Checked = true;

            if (!tcRight.TabPages.Contains(tpAssembly))
                tcRight.TabPages.Add(tpAssembly);
        }
        else
        {
            mnuViewAssembly.Checked = false;
            tcRight.TabPages.Remove(tpAssembly);
        }

        if (tcRight.TabPages.Count > 0)
        {
            if (!scCode.Visible)
            {
                scMain.Panel1Collapsed = false;
                scMain.Panel2Collapsed = !txtConsole.Visible && !tcDebugView.Visible;
                scCode.Visible = true;
                scCode.Panel1Collapsed = !tcSources.Visible;
            }

            scCode.Panel2Collapsed = false;
            tcRight.Visible = true;
        }
        else
        {
            scCode.Panel2Collapsed = true;
            tcRight.Visible = false;
            CheckCodeSplitControl();
        }
    }

    public void SetConsoleEnabled(bool enabled)
    {
        if (enabled)
        {
            if (!scBottom.Visible)
            {
                scMain.Panel2Collapsed = false;
                scMain.Panel1Collapsed = !tcSources.Visible && !tcRight.Visible;
                scBottom.Visible = true;
                scBottom.Panel2Collapsed = !tcDebugView.Visible;
            }

            mnuViewConsole.Checked = true;
            scBottom.Panel1Collapsed = false;
            txtConsole.Visible = true;
        }
        else
        {
            mnuViewConsole.Checked = false;
            scBottom.Panel1Collapsed = true;
            txtConsole.Visible = false;
            CheckBottomSplitControl();
        }
    }

    public void SetVariablesEnabled(bool enabled)
    {
        if (enabled)
        {
            mnuViewVariables.Checked = true;

            if (!tcDebugView.TabPages.Contains(tpVariables))
                tcDebugView.TabPages.Add(tpVariables);
        }
        else
        {
            mnuViewVariables.Checked = false;
            tcDebugView.TabPages.Remove(tpVariables);
        }

        if (tcDebugView.TabPages.Count > 0)
        {
            if (!scBottom.Visible)
            {
                scMain.Panel2Collapsed = false;
                scMain.Panel1Collapsed = !tcSources.Visible && !tcRight.Visible;
                scBottom.Visible = true;
                scBottom.Panel1Collapsed = !txtConsole.Visible;
            }

            scBottom.Panel2Collapsed = false;
            tcDebugView.Visible = true;
        }
        else
        {
            scBottom.Panel2Collapsed = true;
            tcDebugView.Visible = false;
            CheckBottomSplitControl();
        }
    }

    public void SetMemoryEnabled(bool enabled)
    {
        if (enabled)
        {
            mnuViewMemory.Checked = true;

            if (!tcDebugView.TabPages.Contains(tpMemory))
                tcDebugView.TabPages.Add(tpMemory);
        }
        else
        {
            mnuViewMemory.Checked = false;
            tcDebugView.TabPages.Remove(tpMemory);
        }

        if (tcDebugView.TabPages.Count > 0)
        {
            if (!scBottom.Visible)
            {
                scMain.Panel2Collapsed = false;
                scMain.Panel1Collapsed = !tcSources.Visible && !tcRight.Visible;
                scBottom.Visible = true;
                scBottom.Panel1Collapsed = !txtConsole.Visible;
            }

            scBottom.Panel2Collapsed = false;
            tcDebugView.Visible = true;
        }
        else
        {
            scBottom.Panel2Collapsed = true;
            tcDebugView.Visible = false;
            CheckBottomSplitControl();
        }
    }

    private void MnuViewCode_Click(object sender, EventArgs e)
    {
        mnuViewCode.Checked = !mnuViewCode.Checked;
        SetSourcesEnabled(mnuViewCode.Checked);
    }

    private void MnuViewConsole_Click(object sender, EventArgs e)
    {
        mnuViewConsole.Checked = !mnuViewConsole.Checked;
        SetConsoleEnabled(mnuViewConsole.Checked);
    }

    private void MnuViewAssembly_Click(object sender, EventArgs e)
    {
        mnuViewAssembly.Checked = !mnuViewAssembly.Checked;
        SetAssemblyEnabled(mnuViewAssembly.Checked);
    }

    private void MnuViewVariables_Click(object sender, EventArgs e)
    {
        mnuViewVariables.Checked = !mnuViewVariables.Checked;
        SetVariablesEnabled(mnuViewVariables.Checked);
    }

    private void MnuViewMemory_Click(object sender, EventArgs e)
    {
        mnuViewMemory.Checked = !mnuViewMemory.Checked;
        SetMemoryEnabled(mnuViewMemory.Checked);
    }

    private void MnuNew_Click(object sender, EventArgs e)
    {
        var tab = NewSourceTab();
        tab.Focus();
    }

    private void MnuOpen_Click(object sender, EventArgs e)
    {
        OpenFileDialog();
    }

    private void MnuReload_Click(object sender, EventArgs e)
    {
        ReloadCurrentTab();
    }

    private void MnuSave_Click(object sender, EventArgs e)
    {
        SaveCurrentTab();
    }

    private void MnuSaveAs_Click(object sender, EventArgs e)
    {
        SaveCurrentTabAs();
    }

    private void MnuCloseFile_Click(object sender, EventArgs e)
    {
        CloseCurrentTab();
    }

    private void MnuExit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void MnuEditCut_Click(object sender, EventArgs e)
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var sourceTab = sourceTabs[currentPageIndex];
        sourceTab.Cut();
    }

    private void MnuEditCopy_Click(object sender, EventArgs e)
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var sourceTab = sourceTabs[currentPageIndex];
        sourceTab.Copy();
    }

    private void MnuEditPaste_Click(object sender, EventArgs e)
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var sourceTab = sourceTabs[currentPageIndex];
        sourceTab.Paste();
    }

    private void MnuEditDelete_Click(object sender, EventArgs e)
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var sourceTab = sourceTabs[currentPageIndex];
        sourceTab.EditDelete();
    }

    private void MnuEditSelectAll_Click(object sender, EventArgs e)
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        var sourceTab = sourceTabs[currentPageIndex];
        sourceTab.SelectAll();
    }

    private void MnuConsoleClear_Click(object sender, EventArgs e)
    {
        txtConsole.Clear();
    }

    private void MnuAbout_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Compilador Simples por Sharivan X. Visite https://github.com/sharivan/SimpleCompiler para maiores detalhes.", "Sobre o Programa");
    }
}