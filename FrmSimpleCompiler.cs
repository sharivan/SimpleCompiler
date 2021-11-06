using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

using compiler;

namespace SimpleCompiler
{
    public partial class FrmSimpleCompiler : Form
    {
        private AbstractType type = null;
        private Compiler compiler;
        private Assembler assembler;
        private bool vmRunning = false;
        private VM vm;
        private Thread vmThread;
        private object inputLock = new object ();
        private bool waitingForInput = false;
        private string input = null;

        public FrmSimpleCompiler()
        {
            InitializeComponent();

            compiler = new Compiler();
            assembler = new Assembler();
            vm = new VM();
            vm.OnConsoleRead += ConsoleRead;
            vm.OnConsolePrint += ConsolePrint;
        }

        private string ConsoleRead()
        {
            string result = null;

            lock (inputLock)
            {
                while (waitingForInput)
                    Monitor.Wait(inputLock);

                input = null;

                waitingForInput = true;
                Invoke((MethodInvoker) delegate
                {
                    txtInput.Clear();
                    txtInput.ForeColor = Color.White;
                    txtInput.BackColor = Color.Black;
                    txtInput.Focus();
                });

                while (input == null)
                    Monitor.Wait(inputLock);

                result = input;
                input = null;
            }

            return result;
        }

        private void ConsolePrint(string message)
        {
            if (InvokeRequired)
                Invoke(new Action(() => ConsolePrint(message)));
            else
            {
                string[] lines = message.Split('\n');
                if (lines.Length == 0)
                    return;

                if (lbConsole.Items.Count == 0)
                    lbConsole.Items.Add("");

                lbConsole.Items[lbConsole.Items.Count - 1] += lines[0];

                for (int i = 1; i < lines.Length; i++)
                    lbConsole.Items.Add(lines[i]);

                int visibleItems = lbConsole.ClientSize.Height / lbConsole.ItemHeight;
                lbConsole.TopIndex = Math.Max(lbConsole.Items.Count - visibleItems + 1, 0);
            }
        }

        private void ConsolePrintLn(string message)
        {
            ConsolePrint(message + "\n");
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {
            assembler.Reset();

            type = compiler.CompileAdditiveExpression(txtSource.Text, assembler);

            vm.Initialize(assembler);
            vm.Print();
            btnRun.Enabled = true;
        }

        private void btnClearConsole_Click(object sender, EventArgs e)
        {
            lbConsole.Items.Clear();
        }

        public void VMRun()
        {
            vm.Run();
            Invoke((MethodInvoker) delegate
            {
                btnCompile.Enabled = true;
                btnCompileProgram.Enabled = true;
                btnRun.Text = "Start";
                vmRunning = false;

                if (type is PrimitiveType)
                {
                    PrimitiveType p = (PrimitiveType) type;
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        {
                            bool result = (vm.Pop() & 1) != 0;
                            ConsolePrintLn("Result=" + result);
                            break;
                        }

                        case Primitive.CHAR:
                        {
                            char result = (char) vm.Pop();
                            ConsolePrintLn("Result='" + result + "'");
                            break;
                        }

                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                        {
                            int result = vm.Pop();
                            ConsolePrintLn("Result=" + result);
                            break;
                        }

                        case Primitive.LONG:
                        {
                            long result = vm.PopLong();
                            ConsolePrintLn("Result=" + result);
                            break;
                        }

                        case Primitive.FLOAT:
                        {
                            float result = vm.PopFloat();
                            ConsolePrintLn("Result=" + result.ToString(CultureInfo.InvariantCulture));
                            break;
                        }

                        case Primitive.DOUBLE:
                        {
                            double result = vm.PopDouble();
                            ConsolePrintLn("Result=" + result.ToString(CultureInfo.InvariantCulture));
                            break;
                        }
                    }
                }
            });          
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (vmRunning)
            {
                vmThread.Abort();
                return;
            }

            vmRunning = true;
            btnCompile.Enabled = false;
            btnCompileProgram.Enabled = false;
            btnRun.Text = "Stop";
            vmThread = new Thread(() => VMRun());
            vmThread.Start();
        }

        private void btnCompileProgram_Click(object sender, EventArgs e)
        {
            assembler.Reset();

            try
            {
                compiler.CompileProgram(txtSource.Text, assembler);
                vm.Initialize(assembler);
                vm.Print();
                btnRun.Enabled = true;
            }
            catch (CompilerException ex)
            {
                ConsolePrintLn("Erro de compilação na linha " + ex.Interval.Line + ": " + ex.Message);
                txtSource.Focus();
                txtSource.Select(ex.Interval.Start, ex.Interval.End - ex.Interval.Start);
            }
        }

        private void mnuConsoleCopy_Click(object sender, EventArgs e)
        {
            string text = "";
            for (int i = 0; i < lbConsole.SelectedItems.Count; i++)
                text += lbConsole.SelectedItems[i] + "\n";

            Clipboard.SetText(text);
        }

        private void mnuConsoleSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbConsole.Items.Count; i++)
                lbConsole.SelectedIndices.Add(i);
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                lock (inputLock)
                {
                    input = txtInput.Text;
                    txtInput.Clear();
                    txtInput.ForeColor = Color.Black;
                    txtInput.BackColor = Color.White;
                    waitingForInput = false;
                    e.KeyChar = '\0';
                    e.Handled = true;
                    Monitor.PulseAll(inputLock);
                }
            }
        }
    }
}
