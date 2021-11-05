using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    public class UnitySystem
    {
        public static readonly Dictionary<string, VM.ExternalFunctionHandler> FUNCTIONS;

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void KernelCopyMemory(IntPtr dest, IntPtr src, int count);

        public static void CopyMemory(VM vm)
        {
            int src = vm.LoadParam(2);
            int dst = vm.LoadParam(1);
            int len = vm.LoadParam(0);

            vm.MoveBlock(src, dst, len);
        }

        public static void StringLength(VM vm)
        {
            int str = vm.LoadParam(0);

            string s = vm.ReadStackString(str);

            vm.SetParam(1, s.Length);
        }

        public static void CopyString(VM vm)
        {
            int src = vm.LoadParam(1);
            int dst = vm.LoadParam(0);

            string s = vm.ReadStackString(src);

            vm.WriteStack(dst, s);
        }

        public static void ConcatenateStrings(VM vm)
        {
            int src = vm.LoadParam(1);
            int dst = vm.LoadParam(0);

            string s2 = vm.ReadStackString(dst);
            string s1 = vm.ReadStackString(src);
            
            vm.WriteStack(dst, s2 + s1);
        }

        public static void CompareStyrings(VM vm)
        {
            int str1 = vm.LoadParam(1);
            int str2 = vm.LoadParam(0);

            string s1 = vm.ReadStackString(str1);
            string s2 = vm.ReadStackString(str2);

            vm.SetParam(2, s1 == s2 ? 1 : 0);
        }

        public static void StringToInt(VM vm)
        {
            int str = vm.LoadParam(0);

            string s = vm.ReadStackString(str);

            try
            {
                vm.SetParam(1, int.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(1, 0);
            }
        }

        public static void StringToLong(VM vm)
        {
            int str = vm.LoadParam(0);

            string s = vm.ReadStackString(str);

            try
            {
                vm.SetParam(2, long.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(2, 0L);
            }
        }

        public static void StringToFloat(VM vm)
        {
            int str = vm.LoadParam(0);

            string s = vm.ReadStackString(str);

            try
            {
                vm.SetParam(1, float.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(1, 0F);
            }
        }

        public static void StringToDouble(VM vm)
        {
            int str = vm.LoadParam(0);

            string s = vm.ReadStackString(str);

            try
            {
                vm.SetParam(2, double.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(2, 0.0);
            }
        }

        static UnitySystem()
        {
            FUNCTIONS = new Dictionary<string, VM.ExternalFunctionHandler>();
            FUNCTIONS.Add("CopyMemory", CopyMemory);
            FUNCTIONS.Add("StringLength", StringLength);
            FUNCTIONS.Add("CopyString", CopyString);
            FUNCTIONS.Add("ConcatenateStrings", ConcatenateStrings);
            FUNCTIONS.Add("CompareStyrings", CompareStyrings);
            FUNCTIONS.Add("StringToInt", StringToInt);
            FUNCTIONS.Add("StringToLong", StringToLong);
            FUNCTIONS.Add("StringToFloat", StringToFloat);
            FUNCTIONS.Add("StringToDouble", StringToDouble);
        }
    }
}
