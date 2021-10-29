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

using compiler;

namespace SimpleCompiler
{
    public partial class FrmSimpleCompiler : Form
    {
        private AbstractType type;
        private Compiler compiler;
        private Assembler assembler;
        private VM vm;

        public FrmSimpleCompiler()
        {
            InitializeComponent();

            type = null;
            compiler = new Compiler();
            assembler = new Assembler();
            vm = new VM();
            vm.OnConsoleRead += ConsoleRead;
            vm.OnConsolePrint += ConsolePrint;
        }

        private string ConsoleRead()
        {
            // TODO Implementar
            return null;
        }

        private void ConsolePrint(string message)
        {
            lbConsole.Items.Add(message);
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

        private void btnRun_Click(object sender, EventArgs e)
        {
            vm.Run();
            if (type is PrimitiveType)
            {
                PrimitiveType p = (PrimitiveType)type;
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            assembler.Reset();

            try
            {
                compiler.CompileProgram(txtSource.Text, assembler);
                vm.Initialize(assembler);
                vm.Print();
                btnRun.Enabled = true;
            }
            catch (ParserException ex)
            {
                ConsolePrintLn(ex.Message);
            }
        }

        private void mnuConsoleCopy_Click(object sender, EventArgs e)
        {
            string text = "";
            for (int i = 0; i < lbConsole.SelectedItems.Count; i++)
                text += lbConsole.SelectedItems[i].ToString() + '\n';

            Clipboard.SetText(text);
        }

        private void mnuConsoleSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbConsole.Items.Count; i++)
                lbConsole.SelectedItems.Add(lbConsole.Items[i]);
        }
    }
}
