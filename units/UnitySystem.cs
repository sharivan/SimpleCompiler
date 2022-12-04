using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using vm;

using static vm.VM;

namespace units
{
    public class UnitySystem
    {
        public static readonly Dictionary<string, VM.ExternalFunctionHandler> FUNCTIONS;

        private static readonly int COPY_MEMORY_PARAM_SIZE = 2 * POINTER_SIZE + sizeof(int);
        private static readonly int STRING_LENGTH_PARAM_SIZE = POINTER_SIZE + sizeof(int);
        private static readonly int COPY_STRING_PARAM_SIZE = 2 * POINTER_SIZE;
        private static readonly int CONCATENATE_STRING_PARAM_SIZE = 3 * POINTER_SIZE;
        private static readonly int STRING_TO_INT_PARAM_SIZE = sizeof(int) + 2 * POINTER_SIZE;
        private static readonly int INT_TO_STRING_PARAM_SIZE = sizeof(int) + POINTER_SIZE;
        private static readonly int LONG_TO_STRING_PARAM_SIZE = sizeof(long) + POINTER_SIZE;
        private static readonly int ALLOC_PARAM_SIZE = sizeof(int) + POINTER_SIZE;
        private static readonly int FREE_PARAM_SIZE = POINTER_SIZE;
        private static readonly int NEW_STRING_PARAM_SIZE = STRING_SIZE + POINTER_SIZE;
        private static readonly int NEW_STRING2_PARAM_SIZE = 2 * POINTER_SIZE;
        private static readonly int COPY_STRING2_PARAM_SIZE = STRING_SIZE + POINTER_SIZE;
        private static readonly int STRING_LENGTH2_PARAM_SIZE = sizeof(int) + STRING_SIZE;
        private static readonly int CONCATENATE_STRING2_PARAM_SIZE = 3 * STRING_SIZE;
        private static readonly int CONCATENATE_STRING3_PARAM_SIZE = POINTER_SIZE + 2 * STRING_SIZE;
        private static readonly int STRING_STORE_PARAM_SIZE = POINTER_SIZE + STRING_SIZE;
        private static readonly int STRING_ADDREF_PARAM_SIZE = STRING_SIZE;
        private static readonly int STRING_RELEASE_PARAM_SIZE = STRING_SIZE;

        public static readonly int POINTER_COUNT = POINTER_SIZE / sizeof(int);
        public static readonly int STRING_COUNT = POINTER_COUNT;

        public static void CopyMemory(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, POINTER_COUNT);
            int len = vm.LoadParam(COPY_MEMORY_PARAM_SIZE, 2 * POINTER_COUNT);

            MovePointerBlock(src, dst, len);
        }

        public static void StringLength(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(STRING_LENGTH_PARAM_SIZE, 1);

            string s = ReadPointerString(str);

            vm.SetParam(STRING_LENGTH_PARAM_SIZE, 0, s.Length);
        }

        public static void CopyString(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(COPY_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(COPY_STRING_PARAM_SIZE, POINTER_COUNT);

            string s = ReadPointerString(src);

            WritePointer(dst, s);
        }

        public static void ConcatenateStrings(VM vm)
        {
            IntPtr dst = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, 0);
            IntPtr src1 = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, POINTER_COUNT);
            IntPtr src2 = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, 2 * POINTER_COUNT);

            string s1 = ReadPointerString(src1);
            string s2 = ReadPointerString(src2);

            WritePointer(dst, s1 + s2);
        }

        public static void CompareStyrings(VM vm)
        {
            IntPtr str1 = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 1);
            IntPtr str2 = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 1 + POINTER_COUNT);

            string s1 = ReadPointerString(str1);
            string s2 = ReadPointerString(str2);

            vm.SetParam(COPY_MEMORY_PARAM_SIZE, 0, s1 == s2 ? 1 : 0);
        }

        public static void StringToInt(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

            string s = ReadPointerString(src);

            try
            {
                int value = int.Parse(s, CultureInfo.InvariantCulture);
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
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

            string s = ReadPointerString(src);

            try
            {
                long value = long.Parse(s, CultureInfo.InvariantCulture);
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
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

            string s = ReadPointerString(src);

            try
            {
                float value = float.Parse(s, CultureInfo.InvariantCulture);
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
            IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

            string s = ReadPointerString(src);

            try
            {
                double value = double.Parse(s, CultureInfo.InvariantCulture);
                WritePointer(dst, value);
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

            string s = src.ToString(CultureInfo.InvariantCulture);
            WritePointer(dst, s);
        }

        public static void LongToString(VM vm)
        {
            long src = vm.LoadParamLong(LONG_TO_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

            string s = src.ToString(CultureInfo.InvariantCulture);
            WritePointer(dst, s);
        }

        public static void FloatToString(VM vm)
        {
            float src = vm.LoadParamFloat(INT_TO_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(INT_TO_STRING_PARAM_SIZE, 1);

            string s = src.ToString(CultureInfo.InvariantCulture);
            WritePointer(dst, s);
        }

        public static void DoubleToString(VM vm)
        {
            double src = vm.LoadParamDouble(LONG_TO_STRING_PARAM_SIZE, 0);
            IntPtr dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

            string s = src.ToString(CultureInfo.InvariantCulture);
            WritePointer(dst, s);
        }

        public static void Alloc(VM vm)
        {
            int len = vm.LoadParam(ALLOC_PARAM_SIZE, POINTER_COUNT);

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

        public static void NewString(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(NEW_STRING_PARAM_SIZE, POINTER_COUNT);
            string s = ReadPointerString(str);

            IntPtr result = vm.NewString(s);

            vm.SetParam(NEW_STRING_PARAM_SIZE, 0, result);
        }

        public static void NewString2(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(NEW_STRING2_PARAM_SIZE, POINTER_COUNT);
            string s = ReadPointerString(str);

            IntPtr result = vm.NewString(s);

            IntPtr dstAddr = vm.LoadParamPtr(NEW_STRING2_PARAM_SIZE, 0);

            IntPtr dst = ReadPointerPtr(dstAddr);
            vm.StringRelease(dst);

            WritePointer(dstAddr, result);
        }

        public static void CopyString2(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(COPY_STRING2_PARAM_SIZE, 0);
            string s = src != IntPtr.Zero ? ReadPointerString(src) : "";

            IntPtr dst = vm.SetStringLength(src, s.Length);
            vm.SetParam(COPY_STRING2_PARAM_SIZE, STRING_COUNT, dst);
        }

        public static void StringLength2(VM vm)
        {
            IntPtr src = vm.LoadParamPtr(STRING_LENGTH2_PARAM_SIZE, 1);
            int len = VM.StringLength(src);
            vm.SetParam(STRING_LENGTH2_PARAM_SIZE, 0, len);
        }

        public static void ConcatenateStrings2(VM vm)
        {
            IntPtr str1 = vm.LoadParamPtr(CONCATENATE_STRING2_PARAM_SIZE, STRING_COUNT);
            IntPtr str2 = vm.LoadParamPtr(CONCATENATE_STRING2_PARAM_SIZE, 2 *STRING_COUNT);

            string s1 = str1 != IntPtr.Zero ? ReadPointerString(str1) : "";
            string s2 = str2 != IntPtr.Zero ? ReadPointerString(str2) : "";
            string s = s1 + s2;

            IntPtr result = vm.NewString(s);

            vm.SetParam(CONCATENATE_STRING2_PARAM_SIZE, 0, result);
        }

        public static void ConcatenateStrings3(VM vm)
        {
            IntPtr str1 = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, POINTER_COUNT);
            IntPtr str2 = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, POINTER_COUNT + STRING_COUNT);

            string s1 = str1 != IntPtr.Zero ? ReadPointerString(str1) : "";
            string s2 = str2 != IntPtr.Zero ? ReadPointerString(str2) : "";
            string s = s1 + s2;

            IntPtr result = vm.NewString(s);

            IntPtr dstAddr = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, 0);

            IntPtr dst = ReadPointerPtr(dstAddr);
            vm.StringRelease(dst);

            WritePointer(dstAddr, result);
        }

        public static void StringStore(VM vm)
        {
            IntPtr dstAddr = vm.LoadParamPtr(STRING_STORE_PARAM_SIZE, 0);
            IntPtr dst = ReadPointerPtr(dstAddr);
            IntPtr src = vm.LoadParamPtr(STRING_STORE_PARAM_SIZE, POINTER_COUNT);

            VM.StringAddRef(src);
            vm.StringRelease(dst);

            WritePointer(dstAddr, src);
        }

        public static void StringAddRef(VM vm)
        {
            IntPtr str = vm.LoadParamPtr(STRING_ADDREF_PARAM_SIZE, 0);
            VM.StringAddRef(str);
        }

        public static void StringRelease(VM vm)
        {
            IntPtr strAddr = vm.LoadParamPtr(STRING_RELEASE_PARAM_SIZE, 0);
            IntPtr str = ReadPointerPtr(strAddr);
            str = vm.StringRelease(str);
            WritePointer(strAddr, str);
        }

        static UnitySystem() => FUNCTIONS = new Dictionary<string, VM.ExternalFunctionHandler>
            {
                { "CopiaMemória", CopyMemory },
                { "ComprimentoString", StringLength },
                { "CopiaString", CopyString },
                { "ConcatenaStrings", ConcatenateStrings },
                { "CompareStrings", CompareStyrings },
                { "StringParaInt", StringToInt },
                { "StringParaLong", StringToLong },
                { "StringParaFloat", StringToFloat },
                { "StringParaReal", StringToDouble },
                { "IntParaString", IntToString },
                { "LongParaString", LongToString },
                { "FloatParaString", FloatToString },
                { "RealParaString", DoubleToString },
                { "AlocarMemória", Alloc },
                { "DesalocarMemória", Free },
                { "NovoTexto", NewString },
                { "NovoTexto2", NewString2 },
                { "CopiaTexto", CopyString2 },
                { "ComprimentoTexto", StringLength2 },
                { "ConcatenaTextos", ConcatenateStrings2 },
                { "ConcatenaTextos2", ConcatenateStrings3 },
                { "AtribuiTexto", StringStore },
                { "IncrementaReferenciaTexto", StringAddRef },
                { "DecrementaReferenciaTexto", StringRelease }
            };
    }
}
