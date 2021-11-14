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
        private static readonly int STRING_TO_INT_PARAM_SIZE = sizeof(int) + 2 * IntPtr.Size;
        private static readonly int INT_TO_STRING_PARAM_SIZE = sizeof(int) + IntPtr.Size;
        private static readonly int LONG_TO_STRING_PARAM_SIZE = sizeof(long) + IntPtr.Size;
        private static readonly int ALLOC_PARAM_SIZE = sizeof(int) + IntPtr.Size;
        private static readonly int FREE_PARAM_SIZE = IntPtr.Size;

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
            IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + INT_PTR_COUNT);

            string s = vm.ReadPointerString(src);

            try
            {
                int value = int.Parse(s);
                vm.WritePointer(dst, value);
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
            }
            catch (Exception)
            {
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            }
        }

        public static void StringToLong(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + INT_PTR_COUNT);

            string s = vm.ReadPointerString(src);

            try
            {
                long value = long.Parse(s);
                vm.WritePointer(dst, value);
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
            }
            catch (Exception)
            {
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            }
        }

        public static void StringToFloat(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + INT_PTR_COUNT);

            string s = vm.ReadPointerString(src);

            try
            {
                float value = float.Parse(s);
                vm.WritePointer(dst, value);
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
            }
            catch (Exception)
            {
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            }
        }

        public static void StringToDouble(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + INT_PTR_COUNT);

            string s = vm.ReadPointerString(src);

            try
            {
                double value = double.Parse(s);
                vm.WritePointer(dst, value);
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
            }
            catch (Exception)
            {
                vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            }
        }

        public static void IntToString(VM vm)
        {
            int src = vm.LoadParam(INT_TO_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(INT_TO_STRING_PARAM_SIZE, 1);

            string s = src.ToString();
            vm.WritePointer(dst, s);
        }

        public static void LongToString(VM vm)
        {
            long src = vm.LoadParamLong(LONG_TO_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

            string s = src.ToString();
            vm.WritePointer(dst, s);
        }

        public static void FloatToString(VM vm)
        {
            float src = vm.LoadParamFloat(INT_TO_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(INT_TO_STRING_PARAM_SIZE, 1);

            string s = src.ToString();
            vm.WritePointer(dst, s);
        }

        public static void DoubleToString(VM vm)
        {
            double src = vm.LoadParamDouble(LONG_TO_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

            string s = src.ToString();
            vm.WritePointer(dst, s);
        }

        public static void Alloc(VM vm)
        {
            int len = vm.LoadParam(ALLOC_PARAM_SIZE, INT_PTR_COUNT);

            try
            {
                IntPtr result = Marshal.AllocHGlobal(len);
                vm.SetParam(ALLOC_PARAM_SIZE, 0, result);
            }
            catch (OutOfMemoryException)
            {
                vm.SetParam(ALLOC_PARAM_SIZE, 0, IntPtr.Zero);
            }
        }

        public static void Free(VM vm)
        {
            IntPtr ptr = vm.LoadParamPtr(FREE_PARAM_SIZE, 0);
            Marshal.FreeHGlobal(ptr);
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
            FUNCTIONS.Add("IntParaString", IntToString);
            FUNCTIONS.Add("LongParaString", LongToString);
            FUNCTIONS.Add("FloatParaString", FloatToString);
            FUNCTIONS.Add("RealParaString", DoubleToString);
            FUNCTIONS.Add("AlocarMemória", Alloc);
            FUNCTIONS.Add("DesalocarMemória", Free);
        }
    }
}
