using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using vm;

namespace units
{
    public class UnitySystem
    {
        public static readonly Dictionary<string, VM.ExternalFunctionHandler> FUNCTIONS;

        private static readonly int COPY_MEMORY_PARM_SIZE = 2 * IntPtr.Size + sizeof(int);
        private static readonly int STRING_LENGTH_PARM_SIZE = IntPtr.Size + sizeof(int);
        private static readonly int COPY_STRING_PARM_SIZE = 2 * IntPtr.Size;
        private static readonly int CONCATENATE_STRING_PARM_SIZE = 3 * IntPtr.Size;
        private static readonly int STRING_TO_LONG_PARM_SIZE = IntPtr.Size + sizeof(long);

        private static readonly int INT_PTR_COUNT = IntPtr.Size / sizeof(int);

        public static void CopyMemory(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(COPY_MEMORY_PARM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(COPY_MEMORY_PARM_SIZE, INT_PTR_COUNT);
            int len = vm.LoadParam(COPY_MEMORY_PARM_SIZE, 2 * INT_PTR_COUNT);

            vm.MovePointerBlock(src, dst, len);
        }

        public static void StringLength(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(STRING_LENGTH_PARM_SIZE, 1);

            string s = vm.ReadPointerString(str);

            vm.SetParam(STRING_LENGTH_PARM_SIZE, 0, s.Length);
        }

        public static void CopyString(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(COPY_STRING_PARM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(COPY_STRING_PARM_SIZE, INT_PTR_COUNT);           

            string s = vm.ReadPointerString(src);

            vm.WritePointer(dst, s);
        }

        public static void ConcatenateStrings(VM vm)
        {
            IntPtr dst = vm.LoadParamPtr(CONCATENATE_STRING_PARM_SIZE, 0);
            IntPtr src1 = vm.LoadParamPtr(CONCATENATE_STRING_PARM_SIZE, INT_PTR_COUNT);
            IntPtr src2 = vm.LoadParamPtr(CONCATENATE_STRING_PARM_SIZE, 2 * INT_PTR_COUNT);
            
            string s1 = vm.ReadPointerString(src1);
            string s2 = vm.ReadPointerString(src2);

            vm.WritePointer(dst, s1 + s2);
        }

        public static void CompareStyrings(VM vm)
        {
            IntPtr str1 = vm.LoadParamPtr(COPY_MEMORY_PARM_SIZE, 1);
            IntPtr str2 = vm.LoadParamPtr(COPY_MEMORY_PARM_SIZE, 1 + INT_PTR_COUNT);

            string s1 = vm.ReadPointerString(str1);
            string s2 = vm.ReadPointerString(str2);

            vm.SetParam(COPY_MEMORY_PARM_SIZE, 0, s1 == s2 ? 1 : 0);
        }

        public static void StringToInt(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(STRING_LENGTH_PARM_SIZE, 1);

            string s = vm.ReadPointerString(str);

            try
            {
                vm.SetParam(STRING_LENGTH_PARM_SIZE, 0, int.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(STRING_LENGTH_PARM_SIZE, 0, 0);
            }
        }

        public static void StringToLong(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(STRING_TO_LONG_PARM_SIZE, 2);

            string s = vm.ReadPointerString(str);

            try
            {
                vm.SetParam(STRING_TO_LONG_PARM_SIZE, 0, long.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(STRING_TO_LONG_PARM_SIZE, 0, 0L);
            }
        }

        public static void StringToFloat(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(STRING_LENGTH_PARM_SIZE, 1);

            string s = vm.ReadPointerString(str);

            try
            {
                vm.SetParam(STRING_LENGTH_PARM_SIZE, 0, float.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(STRING_LENGTH_PARM_SIZE, 0, 0F);
            }
        }

        public static void StringToDouble(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(STRING_TO_LONG_PARM_SIZE, 2);

            string s = vm.ReadPointerString(str);

            try
            {
                vm.SetParam(STRING_TO_LONG_PARM_SIZE, 0, double.Parse(s));
            }
            catch (Exception)
            {
                vm.SetParam(STRING_TO_LONG_PARM_SIZE, 0, 0.0);
            }
        }

        static UnitySystem()
        {
            FUNCTIONS = new Dictionary<string, VM.ExternalFunctionHandler>();
            FUNCTIONS.Add("CopiaMemória", CopyMemory);
            FUNCTIONS.Add("ComprimentoString", StringLength);
            FUNCTIONS.Add("CopiaString", CopyString);
            FUNCTIONS.Add("ConcatenaStrings", ConcatenateStrings);
            FUNCTIONS.Add("CompareStrings", CompareStyrings);
            FUNCTIONS.Add("StringParaInt", StringToInt);
            FUNCTIONS.Add("StringParaLong", StringToLong);
            FUNCTIONS.Add("StringParaFloat", StringToFloat);
            FUNCTIONS.Add("StringParaReal", StringToDouble);
        }
    }
}
