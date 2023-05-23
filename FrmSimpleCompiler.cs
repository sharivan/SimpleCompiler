using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using assembler;

using compiler;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

using vm;

namespace SimpleCompiler;

public partial class FrmSimpleCompiler : Form
{
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

    public sealed class DocumentConfigElement : ConfigurationElement
    {
        public DocumentConfigElement()
        {
        }

        public DocumentConfigElement(string fileName)
        {
            FileName = fileName;
        }

        [ConfigurationProperty("filename", IsRequired = true, IsKey = true)]
        public string FileName
        {
            get => (string) this["filename"];
            set => this["filename"] = value;
        }

        [ConfigurationProperty("tabindex",
            IsRequired = false,
            DefaultValue = -1
            )]
        public int TabIndex
        {
            get => (int) this["tabindex"];
            set => this["tabindex"] = value;
        }

        [ConfigurationProperty("selected",
            IsRequired = false,
            DefaultValue = false
            )]
        public bool Selected
        {
            get => (bool) this["selected"];
            set => this["selected"] = value;
        }

        [ConfigurationProperty("focused",
            IsRequired = false,
            DefaultValue = false
            )]
        public bool Focused
        {
            get => (bool) this["focused"];
            set => this["focused"] = value;
        }
    }

    public sealed class DocumentCollection : ConfigurationElementCollection
    {
        public new DocumentConfigElement this[string filename]
        {
            get
            {
                DocumentConfigElement element;
                if (IndexOf(filename) < 0)
                {
                    element = new DocumentConfigElement(filename);
                    BaseAdd(element);
                }
                else
                {
                    element = (DocumentConfigElement) BaseGet(filename);
                }

                return element;
            }
        }

        public DocumentConfigElement this[int index] => (DocumentConfigElement) BaseGet(index);

        protected override string ElementName => "document";

        public int IndexOf(string name)
        {
            name = name.ToLower();

            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].FileName.ToLower() == name)
                    return idx;
            }

            return -1;
        }

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;

        protected override ConfigurationElement CreateNewElement()
        {
            return new DocumentConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DocumentConfigElement) element).FileName;
        }

        public void Clear()
        {
            BaseClear();
        }
    }

    public sealed class ProgramConfiguratinSection : ConfigurationSection
    {
        public ProgramConfiguratinSection()
        {
        }

        [ConfigurationProperty("left",
            DefaultValue = -1,
            IsRequired = false
            )]
        public int Left
        {
            get => (int) this["left"];

            set => this["left"] = value;
        }

        [ConfigurationProperty("top",
            DefaultValue = -1,
            IsRequired = false
            )]
        public int Top
        {
            get => (int) this["top"];

            set => this["top"] = value;
        }

        [ConfigurationProperty("width",
            DefaultValue = -1,
            IsRequired = false
            )]
        public int Width
        {
            get => (int) this["width"];

            set => this["width"] = value;
        }

        [ConfigurationProperty("height",
            DefaultValue = -1,
            IsRequired = false
            )]
        public int Height
        {
            get => (int) this["height"];

            set => this["height"] = value;
        }

        [ConfigurationProperty("maximized",
            DefaultValue = false,
            IsRequired = false
            )]
        public bool Maximized
        {
            get => (bool) this["maximized"];

            set => this["maximized"] = value;
        }

        [ConfigurationProperty("stackviewalignsize",
            DefaultValue = 16,
            IsRequired = false
            )]
        public int StackViewAlignSize
        {
            get => (int) this["stackviewalignsize"];

            set => this["stackviewalignsize"] = value;
        }

        [ConfigurationProperty("documents", IsDefaultCollection = true)]
        public DocumentCollection Documents => (DocumentCollection) base["documents"];
    }

    private delegate void ParsingDelegate(int lineNumber);

    internal class Line
    {
        internal int m_iNumber;
        internal int m_iStartPos;
        internal int m_iEndPos;

        internal Line(int number, int start, int end)
        {
            m_iNumber = number;
            m_iStartPos = start;
            m_iEndPos = end;
        }
    }

    private readonly struct Interval
    {
        public int Start
        {
            get;
        }

        public int End
        {
            get;
        }

        public Interval(int start, int end)
        {
            Start = start;
            End = end;
        }

        public override int GetHashCode()
        {
            return (Start << 16) + End;
        }

        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            if (ReferenceEquals(this, 0))
                return true;

            var interval = o as Interval?;
            return interval != null && this == interval;
        }

        public static bool operator ==(Interval left, Interval right)
        {
            return left.Start == right.Start && left.End == right.End;
        }

        public static bool operator !=(Interval left, Interval right)
        {
            return left.Start != right.Start || left.End != right.End;
        }

        public static bool operator <(Interval left, Interval right)
        {
            return left.Start > right.Start && left.End < right.End;
        }

        public static bool operator >(Interval left, Interval right)
        {
            return right.Start > left.Start && right.End < left.End;
        }

        public static bool operator <=(Interval left, Interval right)
        {
            return left.Start >= right.Start && left.End <= right.End;
        }

        public static bool operator >=(Interval left, Interval right)
        {
            return right.Start >= left.Start && right.End <= left.End;
        }

        public static Interval operator &(Interval left, Interval right)
        {
            int min = Math.Max(left.Start, right.Start);
            int max = Math.Min(left.End, right.End);

            return new Interval(min, max);
        }

        public static Interval operator |(Interval left, Interval right)
        {
            int min = Math.Min(left.Start, right.Start);
            int max = Math.Max(left.End, right.End);

            return new Interval(min, max);
        }
    }

    internal class ErrorRenderer : IBackgroundRenderer
    {
        private static readonly System.Windows.Media.Pen pen;
        private static readonly SolidColorBrush errorBackground;

        static ErrorRenderer()
        {
            errorBackground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, 0xff, 0x00, 0x00));
            errorBackground.Freeze();

            var blackBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x00, 0x00, 0x00));
            blackBrush.Freeze();
            pen = new System.Windows.Media.Pen(blackBrush, 0.0);
        }

        private readonly TextView view;
        private bool enabled;
        private int start;
        private int end;
        private string message;

        public bool Enabled
        {
            get => enabled;

            set
            {
                enabled = value;
                view.InvalidateLayer(KnownLayer.Background);
            }
        }

        public int Start
        {
            get => start;

            set
            {
                start = value;
                view.InvalidateLayer(KnownLayer.Background);
            }
        }

        public int End
        {
            get => end;

            set
            {
                end = value;
                view.InvalidateLayer(KnownLayer.Background);
            }
        }

        public string Message
        {
            get => message;

            set
            {
                message = value;
                view.InvalidateLayer(KnownLayer.Background);
            }
        }

        public KnownLayer Layer => KnownLayer.Background;

        public ErrorRenderer(TextView view, bool enabled = false, int start = -1, int end = -1)
        {
            this.view = view;
            this.enabled = enabled;
            this.start = start;
            this.end = end;
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!enabled)
                return;

            foreach (var v in textView.VisualLines)
            {
                int s = -1;
                int e = -1;

                if (v.FirstDocumentLine.Offset <= start && start < v.FirstDocumentLine.EndOffset)
                {
                    s = start;
                    e = end <= v.FirstDocumentLine.EndOffset ? end : v.FirstDocumentLine.EndOffset;
                }
                else if (v.FirstDocumentLine.Offset >= start && v.FirstDocumentLine.Offset < end)
                {
                    s = v.FirstDocumentLine.Offset;
                    e = end <= v.FirstDocumentLine.EndOffset ? end : v.FirstDocumentLine.EndOffset;
                }

                if (s != -1)
                {
                    var rcs = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, v, s - v.FirstDocumentLine.Offset, e - v.FirstDocumentLine.Offset);
                    foreach (var rc in rcs)
                    {
                        var brush = errorBackground;
                        drawingContext.DrawRectangle(brush, pen, new Rect(rc.Left, rc.Top, rc.Width, rc.Height));
                    }
                }
            }
        }
    }

    internal class SteppingRenderer : IBackgroundRenderer
    {
        private static readonly System.Windows.Media.Pen pen;
        private static readonly SolidColorBrush steppingBackground;

        private readonly TextView view;
        private bool enabled;
        private int lineNumber;

        static SteppingRenderer()
        {
            steppingBackground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xbe, 0x00, 0x00, 0xff));
            steppingBackground.Freeze();

            var blackBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xff, 0x00, 0x00, 0x00));
            blackBrush.Freeze();
            pen = new System.Windows.Media.Pen(blackBrush, 0.0);
        }

        public bool Enabled
        {
            get => enabled;

            set
            {
                enabled = value;
                view.InvalidateLayer(KnownLayer.Background);
            }
        }

        public int LineNumber
        {
            get => lineNumber;

            set
            {
                lineNumber = value;
                view.InvalidateLayer(KnownLayer.Background);
            }
        }

        public KnownLayer Layer => KnownLayer.Background;

        public SteppingRenderer(TextView view, bool enabled = false, int lineNumber = 1)
        {
            this.view = view;

            this.enabled = enabled;
            this.lineNumber = lineNumber;
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!enabled)
                return;

            foreach (var v in textView.VisualLines)
            {
                if (v.FirstDocumentLine.LineNumber != lineNumber)
                    continue;

                var rc = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, v, 0, 1000).First();

                var brush = steppingBackground;
                drawingContext.DrawRectangle(brush, pen, new Rect(0, rc.Top, textView.ActualWidth, rc.Height));

                break;
            }
        }
    }

    internal class BreakPointMargin : AbstractMargin
    {
        private const int margin = 20;

        private readonly FrmSimpleCompiler form;
        private readonly Dictionary<int, Breakpoint> breakpoints;

        public KnownLayer Layer => KnownLayer.Background;

        public BreakPointMargin(FrmSimpleCompiler form)
        {
            this.form = form;

            breakpoints = new Dictionary<int, Breakpoint>();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            return new(margin, 0);
        }

        #region OnTextViewChanged
        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= OnRedrawRequested;
            }

            base.OnTextViewChanged(oldTextView, newTextView);

            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += OnRedrawRequested;
            }

            InvalidateVisual();
        }

        void OnRedrawRequested(object sender, EventArgs e)
        {
            // Don't invalidate the IconBarMargin if it'll be invalidated again once the
            // visual lines become valid.
            if (TextView != null && TextView.VisualLinesValid)
            {
                InvalidateVisual();
            }
        }

        public virtual void Dispose()
        {
            TextView = null; // detach from TextView (will also detach from manager)
        }
        #endregion

        protected override void OnRender(DrawingContext drawingContext)
        {
            TextView textView = TextView;
            System.Windows.Size renderSize = RenderSize;
            drawingContext.DrawRectangle(System.Windows.SystemColors.ControlBrush, null, new Rect(0, 0, renderSize.Width, renderSize.Height));

            if (textView != null && textView.VisualLinesValid)
            {
                foreach (VisualLine line in textView.VisualLines)
                {
                    if (breakpoints.ContainsKey(line.FirstDocumentLine.LineNumber))
                        drawingContext.DrawImage(form.enabledImage, new Rect(0, line.VisualTop - textView.VerticalOffset, form.enabledImage.Width, form.enabledImage.Height));
                }
            }
        }

        public void AddBreakpoint(int line, Breakpoint breakpoint)
        {
            breakpoints[line] = breakpoint;
            InvalidateVisual();
        }

        public void ToggleBreakpoint(int line, Breakpoint breakpoint)
        {
            if (!breakpoints.ContainsKey(line))
                breakpoints[line] = breakpoint;
            else if (breakpoints[line] != breakpoint)
                breakpoints[line] = breakpoint;
            else
                breakpoints.Remove(line);

            InvalidateVisual();
        }

        public void RemoveBreakpoint(int line)
        {
            breakpoints.Remove(line);
            InvalidateVisual();
        }

        public void ClearBreakpoints()
        {
            breakpoints.Clear();
            InvalidateVisual();
        }
    }

    public class SourceTab
    {
        internal FrmSimpleCompiler form;
        internal TabPage page;
        internal int id;
        internal string fileName;
        internal bool saved;

        internal string m_SourceCode;
        internal List<Line> m_Lines;

        internal ElementHost wpfHost;
        internal TextEditor txtSource;

        internal ErrorRenderer errorBackgroundRenderer;
        internal SteppingRenderer lineBackgroundRenderer;
        internal BreakPointMargin breakpointMargin;

        public int ID => id;

        public string FileName => fileName;

        public bool Saved => saved;

        public SourceTab(FrmSimpleCompiler form)
        {
            Initialize(form);
        }

        public SourceTab(FrmSimpleCompiler form, string fileName)
        {
            Initialize(form);
            Load(fileName);
        }

        private void Initialize(FrmSimpleCompiler form)
        {
            this.form = form;

            saved = false;
            id = form.sourceTabNextID++;

            m_Lines = new List<Line>();

            wpfHost = new ElementHost
            {
                Dock = DockStyle.Fill
            };

            page = new TabPage("Sem nome   ");
            form.tcSources.TabPages.Add(page);

            page.Controls.Add(wpfHost);

            form.enabledImage ??= new BitmapImage(new Uri(@"pack://application:,,,/resources/img/BreakpointEnabled_6584_16x.png"));

            form.disabledImage ??= new BitmapImage(new Uri(@"pack://application:,,,/resources/img/breakpoint_Off_16xMD.png"));

            txtSource = new TextEditor();
            wpfHost.Child = txtSource;
            txtSource.ShowLineNumbers = true;
            txtSource.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(txtSource.Options);
            txtSource.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".sl");
            txtSource.FontSize = 14;
            txtSource.Foreground = new SolidColorBrush((System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#DCDCDC"));
            txtSource.Background = new SolidColorBrush((System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#1E1E1E"));
            txtSource.Document.Changed += txtSource_Document_OnChanged;

            errorBackgroundRenderer = new ErrorRenderer(txtSource.TextArea.TextView);
            txtSource.TextArea.TextView.BackgroundRenderers.Add(errorBackgroundRenderer);

            lineBackgroundRenderer = new SteppingRenderer(txtSource.TextArea.TextView);
            txtSource.TextArea.TextView.BackgroundRenderers.Add(lineBackgroundRenderer);

            breakpointMargin = new BreakPointMargin(form);
            txtSource.TextArea.LeftMargins.Insert(0, breakpointMargin);
        }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
        private void txtSource_Document_OnChanged(object sender, DocumentChangeEventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
        {
            int delta = e.InsertionLength - e.RemovalLength;
            Interval intersection = new Interval(e.Offset, e.Offset + delta) & new Interval(errorBackgroundRenderer.Start, errorBackgroundRenderer.End);

            if (intersection.Start < intersection.End)
            {
                errorBackgroundRenderer.Enabled = false;
            }
            else if (e.Offset + delta < errorBackgroundRenderer.Start)
            {
                errorBackgroundRenderer.Start += delta;
                errorBackgroundRenderer.End += delta;
            }

            saved = false;
        }

        internal void SetSource(string sSource)
        {
            m_Lines.Clear();

            int number = 0;
            int start = 0;
            int n = sSource.Length;

            char c0 = '\0';

            for (int i = 0; i < n; i++)
            {
                char c = sSource[i];

                if (c == '\n')
                {
                    m_Lines.Add(new Line(++number, start, i));
                    start = i + 1;
                }
                else if (c == '\r')
                {
                    if (c0 == '\n')
                        start = i + 1;
                }

                c0 = c;
            }

            m_Lines.Add(new Line(++number, start, n));
        }

        public void Load(string fileName)
        {
            this.fileName = fileName;
            if (File.Exists(fileName))
            {
                page.Text = Path.GetFileName(fileName) + "    ";
                txtSource.Load(fileName);
                saved = true;
            }
            else
            {
                System.Windows.MessageBox.Show($"Não foi possível recarregar o arquivo '{fileName}'.");
            }
        }

        public void Reload()
        {
            if (fileName != null)
                Load(fileName);
        }

        public void Save()
        {
            if (fileName != null)
            {
                page.Text = $"{Path.GetFileName(fileName)}    ";
                txtSource.Save(fileName);
                saved = true;
            }
            else
            {
                OpenSaveDialog();
            }
        }

        public void OpenSaveDialog()
        {
            DialogResult result = form.saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string sSourceFileName = form.saveFileDialog.FileName;

                if (File.Exists(sSourceFileName))
                {
                    fileName = sSourceFileName;
                    page.Text = $"{Path.GetFileName(fileName)}    ";
                    txtSource.Save(sSourceFileName);
                    saved = true;
                }
                else
                {
                    System.Windows.MessageBox.Show($"Arquivo '{sSourceFileName}' não existe.");
                }
            }
        }

        public void Clear()
        {
            fileName = null;
            page.Text = "Sem nome    ";
            txtSource.Clear();
            saved = false;
        }

        public void Focus()
        {
            form.tcSources.SelectedTab = page;
            txtSource.Focus();
        }

        public void Close()
        {
            form.tcSources.TabPages.Remove(page);

            form.sourceTabs.Remove(this);
            form.sourceTabsMapByID.Remove(id);

            if (fileName != null)
                form.sourceTabsMapByFileName.Remove(fileName);
        }
    }

    private string sourceFile = null;
    private readonly Compiler compiler;
    private readonly Assembler assembler;
    private bool vmRunning = false;
    private bool paused = false;
    private readonly VM vm;
    private Thread vmThread;
    private readonly object inputLock = new();
    private bool scanning = false;
    private int inputPos = 0;
    private string input = null;

    internal ImageSource enabledImage;
    internal ImageSource disabledImage;

    private int sourceTabNextID;
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

        compiler = new Compiler("examples");
        compiler.OnCompileError += OnCompileError;

        assembler = new Assembler();

        vm = new VM();
        vm.OnDisassemblyLine += DisassemblyLine;
        vm.OnConsoleRead += ConsoleRead;
        vm.OnConsolePrint += ConsolePrint;
        vm.OnPause += OnPause;
        vm.OnStep += OnStep;
        vm.OnBreakpoint += OnBreakpoint;

        sourceTabNextID = 0;
        sourceTabs = new List<SourceTab>();
        sourceTabsMapByID = new Dictionary<int, SourceTab>();
        sourceTabsMapByFileName = new Dictionary<string, SourceTab>();

        sourceCache = new Dictionary<string, string[]>();

        IHighlightingDefinition customHighlighting;

        string resourceName = "SimpleCompiler.resources.slHighlighting.xshd";
        Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Could not find embedded resource");

        using (s)
        {
            using XmlReader reader = new XmlTextReader(s);
            customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        // and register it in the HighlightingManager
        HighlightingManager.Instance.RegisterHighlighting("Custom Highlighting", new string[] { ".sl" }, customHighlighting);

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

        lineNumberToIP = new Dictionary<int, int>();
        ipToLineNumber = new Dictionary<int, int>();

        addImage = Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("SimpleCompiler.resources.img.5700.add.png"));
        closeImage = Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("SimpleCompiler.resources.img.5657.close.png"));

        tcSources.HandleCreated += tcSources_HandleCreated;
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
                SourceTab tab = SelectSourceTab(interval.FileName, true);
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
            BeginInvoke(new Action(() => ConsolePrint(message)));
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

        SourceTab tab = SelectSourceTab(filename, true, false);
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

        btnRun.Enabled = true;
        btnPause.Enabled = false;
        btnStop.Enabled = true;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = true;
        btnStepReturn.Enabled = true;
        btnRunToCursor.Enabled = true;

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

        btnRun.Enabled = true;
        btnPause.Enabled = false;
        btnStop.Enabled = true;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = true;
        btnStepReturn.Enabled = true;
        btnRunToCursor.Enabled = true;

        ShowStepCursor(ip);
    });
    }

    private void OnBreakpoint(Breakpoint bp)
    {
        BeginInvoke((MethodInvoker) delegate
    {
        paused = true;

        statusBar.Items["statusText"].Text = "Ponto de interrupção encontrado.";

        btnRun.Enabled = true;
        btnPause.Enabled = false;
        btnStop.Enabled = true;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = true;
        btnStepReturn.Enabled = true;
        btnRunToCursor.Enabled = true;

        ShowStepCursor(bp.IP);
    });
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnClearConsole_Click(object sender, EventArgs e)
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

        btnCompile.Enabled = true;
        btnRun.Enabled = true;
        btnPause.Enabled = false;
        btnStop.Enabled = false;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = false;
        btnStepReturn.Enabled = false;
        btnRunToCursor.Enabled = true;
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
        dgvStrings.RowCount = Math.Max(vm.StringCount, 50);
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
                var rec = (VM.StringRec*) (str - VM.STRING_REC_SIZE).ToPointer();

                dgvStrings[0, row].Value = str.ToString("x8");
                dgvStrings[1, row].Value = rec->refCount.ToString();
                dgvStrings[2, row].Value = rec->len.ToString();
                dgvStrings[3, row].Value = VM.ReadPointerString(str);
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

        FetchStackData();
        FetchStringData();

        lblRegisters.Text = $"IP: {vm.IP:x8} - BP: {vm.BP:x8} - SP: {vm.SP:x8}";
    }

    private void FetchStackData()
    {
        if (!paused)
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
            System.Windows.Forms.Application.DoEvents();
        }

        dgvStack.ResumeLayout();
    }

    private void FetchStringData()
    {
        if (!paused)
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

        IntPtr str = vm.LastString;
        for (int rowIndex = 0; str != IntPtr.Zero && rowIndex < firstDisplayedRowIndex; rowIndex++)
        {
            unsafe
            {
                var rec = (VM.StringRec*) (str - VM.STRING_REC_SIZE).ToPointer();
                str = rec->previous;
            }
        }

        for (int rowIndex = firstDisplayedRowIndex; rowIndex <= lastvisibleRowIndex; rowIndex++)
        {
            FetchStringView(rowIndex, str);
            System.Windows.Forms.Application.DoEvents();

            if (str != IntPtr.Zero)
            {
                unsafe
                {
                    var rec = (VM.StringRec*) (str - VM.STRING_REC_SIZE).ToPointer();
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

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnRun_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        paused = false;

        foreach (var sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        btnCompile.Enabled = false;
        btnRun.Enabled = false;
        btnPause.Enabled = true;
        btnStop.Enabled = true;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = false;
        btnStepReturn.Enabled = false;
        btnRunToCursor.Enabled = false;

        if (!vmRunning)
            StartVM();
        else
            vm.Resume();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnCompile_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex == -1)
            return;

        SourceTab currentTab = sourceTabs[currentPageIndex];
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

        foreach (SourceTab tab in sourceTabs)
        {
            tab.breakpointMargin.ClearBreakpoints();
            tab.SetSource(tab.txtSource.Text);
            tab.m_SourceCode = tab.txtSource.Text;
        }

        txtAssembly.Clear();
        lineNumberToIP.Clear();
        ipToLineNumber.Clear();
        breakpointMargin.ClearBreakpoints();

        btnCompile.Enabled = false;
        statusBar.Items["statusText"].Text = "Compilando...";

        bwCompiler.RunWorkerAsync();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void mnuConsoleCopy_Click(object sender, EventArgs e)
    {
        System.Windows.Clipboard.SetText(txtConsole.Text);
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void mnuConsoleSelectAll_Click(object sender, EventArgs e)
    {
        txtConsole.SelectAll();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void txtConsole_KeyPress(object sender, KeyPressEventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
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
        btnCompile.Enabled = true;
        SourceTab result = new(this);
        sourceTabs.Add(result);
        sourceTabsMapByID[result.ID] = result;
        return result;
    }

    public SourceTab NewSourceTab(string fileName, bool selected = false, bool focused = false)
    {
        if (sourceTabsMapByFileName.TryGetValue(fileName, out SourceTab tab))
            return tab;

        if (!File.Exists(fileName))
            return null;

        btnCompile.Enabled = true;
        tab = new SourceTab(this, fileName);
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
        SourceTab tab = sourceTabs[index];
        tab.Focus();
        return tab;
    }

    public SourceTab SelectSourceTab(string fileName, bool openIfNotExist = false, bool focused = true)
    {
        if (sourceTabsMapByFileName.TryGetValue(fileName, out SourceTab tab))
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
        SourceTab tab = sourceTabs[index];
        tab.Close();

        btnCompile.Enabled = sourceTabs.Count > 0;
    }

    public void CloseSourceTab(string fileName)
    {
        if (sourceTabsMapByFileName.TryGetValue(fileName, out SourceTab tab))
            tab.Close();

        btnCompile.Enabled = sourceTabs.Count > 0;
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnNew_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        SourceTab tab = NewSourceTab();
        tab.Focus();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnOpen_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        DialogResult result = openFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            string sSourceFileName = openFileDialog.FileName;

            if (File.Exists(sSourceFileName))
            {
                if (sourceTabsMapByFileName.ContainsKey(sSourceFileName))
                {
                    SourceTab sourceTab = sourceTabsMapByFileName[sSourceFileName];
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
                System.Windows.MessageBox.Show("Arquivo não existe.");
            }
        }
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnReload_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex != -1)
        {
            SourceTab sourceTab = sourceTabs[currentPageIndex];
            sourceTab.Reload();
        }
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnSave_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        int currentPageIndex = tcSources.SelectedIndex;
        if (currentPageIndex != -1)
        {
            SourceTab sourceTab = sourceTabs[currentPageIndex];
            sourceTab.Save();
        }
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnPause_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        btnPause.Enabled = false;
        vm.Pause();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnStop_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        for (int i = 0; i < sourceTabs.Count; i++)
        {
            SourceTab sourceTab = sourceTabs[i];
            sourceTab.lineBackgroundRenderer.Enabled = false;
        }

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Terminando o programa...";

        btnRun.Enabled = false;
        btnPause.Enabled = false;
        btnStop.Enabled = false;
        btnStepOver.Enabled = false;
        btnStepInto.Enabled = false;
        btnStepReturn.Enabled = false;
        btnRunToCursor.Enabled = false;

        vmThread.Abort();
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
                SourceTab sourceTab = sourceTabs[index];
                if (sourceTab.wpfHost.Focused)
                    assemblyFocused = false;
            }
        }

        return assemblyFocused;
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnStepOver_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        paused = false;

        foreach (SourceTab sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        btnCompile.Enabled = false;
        btnRun.Enabled = false;
        btnPause.Enabled = true;
        btnStop.Enabled = true;
        btnStepOver.Enabled = false;
        btnStepInto.Enabled = false;
        btnStepReturn.Enabled = false;
        btnRunToCursor.Enabled = false;

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

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnStepInto_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        paused = false;

        foreach (SourceTab sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        btnRun.Enabled = false;
        btnPause.Enabled = true;
        btnStop.Enabled = true;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = false;
        btnStepReturn.Enabled = false;
        btnRunToCursor.Enabled = false;

        vm.StepInto(!FetchAndGetAssemblyFocused());
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnStepReturn_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        paused = false;

        foreach (SourceTab sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        btnPause.Enabled = true;
        btnStop.Enabled = true;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = false;
        btnStepReturn.Enabled = false;
        btnRunToCursor.Enabled = false;

        vm.StepReturn(!FetchAndGetAssemblyFocused());
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnRunToCursor_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        paused = false;

        foreach (SourceTab sourceTab in sourceTabs)
            sourceTab.lineBackgroundRenderer.Enabled = false;

        lineBackgroundRenderer.Enabled = false;

        statusBar.Items["statusText"].Text = "Executando...";

        btnCompile.Enabled = false;
        btnPause.Enabled = true;
        btnStop.Enabled = true;
        btnStepOver.Enabled = true;
        btnStepInto.Enabled = false;
        btnStepReturn.Enabled = false;
        btnRunToCursor.Enabled = false;

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

            SourceTab currentSourceTab = sourceTabs[currentTabIndex];

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

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void btnToggleBreakpoint_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        if (wpfAssemblyHost.Focused)
        {
            var line = txtAssembly.Document.GetLineByOffset(txtAssembly.SelectionStart);
            if (lineNumberToIP.TryGetValue(line.LineNumber, out int ip))
            {
                Breakpoint bp = vm.ToggleBreakPoint(ip);
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

            SourceTab sourceTab = sourceTabs[selectedPageIndex];

            var line = sourceTab.txtSource.Document.GetLineByOffset(sourceTab.txtSource.SelectionStart);
            Breakpoint breakpoint = sourceTab.FileName != null
                ? vm.ToggleBreakPoint(sourceTab.FileName, line.LineNumber)
                : vm.ToggleBreakPoint($"#{sourceTab.ID}", line.LineNumber);

            if (breakpoint != null)
                sourceTab.breakpointMargin.ToggleBreakpoint(line.LineNumber, breakpoint);
            else
                sourceTab.breakpointMargin.RemoveBreakpoint(line.LineNumber);
        }
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void bwCompiler_DoWork(object sender, DoWorkEventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
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

                btnCompile.Enabled = true;
                btnRun.Enabled = true;
                btnPause.Enabled = false;
                btnStop.Enabled = false;
                btnStepOver.Enabled = true;
                btnStepInto.Enabled = false;
                btnStepReturn.Enabled = false;
                btnRunToCursor.Enabled = true;
            });
        }
        else
        {
            BeginInvoke((MethodInvoker) delegate
            {
                statusBar.Items["statusText"].Text = "Erro ao compilar.";

                compiled = false;

                btnCompile.Enabled = true;
            });
        }
    }

    private void FrmSimpleCompiler_Load(object sender, EventArgs e)
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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

        DocumentCollection collection = section.Documents;
        foreach (DocumentConfigElement element in collection)
            NewSourceTab(element.FileName, element.Selected, element.Focused);
    }

    private void FrmSimpleCompiler_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (vmRunning)
            vmThread.Abort();

        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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

        DocumentCollection collection = section.Documents;
        collection.Clear();
        for (int i = 0; i < sourceTabs.Count; i++)
        {
            SourceTab tab = sourceTabs[i];
            if (tab.FileName != null)
            {
                DocumentConfigElement element = collection[tab.FileName];
                element.TabIndex = i;
                element.Selected = tcSources.SelectedTab == tab.page;
                element.Focused = tab.txtSource.IsFocused;
            }
        }

        config.Save();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void dgvStack_Scroll(object sender, ScrollEventArgs e)
    {
        FetchStackData();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void dgvStack_Resize(object sender, EventArgs e)
    {
        FetchStackData();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void mnuStackViewAlign16_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        stackViewAlignSize = 16;
        FetchData();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void mnuStackViewAlign8_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        stackViewAlignSize = 8;
        FetchData();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void mnuStackViewAlign4_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        stackViewAlignSize = 4;
        FetchData();
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void tcSources_DrawItem(object sender, DrawItemEventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
        try
        {
            TabPage tabPage = tcSources.TabPages[e.Index];
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

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void tcSources_SelectedIndexChanged(object sender, EventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
    {
    }

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void tcSources_MouseDown(object sender, MouseEventArgs e)
#pragma warning restore IDE1006 // Estilos de Nomenclatura
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

#pragma warning disable IDE1006 // Estilos de Nomenclatura
    private void tcSources_HandleCreated(object sender, EventArgs e)
    {
        SendMessage(tcSources.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) 16);
    }
}