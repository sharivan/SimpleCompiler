using System;
using System.Collections.Generic;
using System.Text;

using compiler.lexer;
using compiler.types;
using assembler;
using System.IO;

namespace compiler
{
    public partial class Compiler
    {
        public static int GetAlignedSize(int sizeInBytes, int alignSize = sizeof(int))
        {
            int r = sizeInBytes % alignSize;
            if (r != 0)
                return sizeInBytes + alignSize - r;

            return sizeInBytes;
        }

        public delegate void CompileError(SourceInterval interval, string message);

        private string unityPath;

        private Lexer lexer;
        private List<Label> labels;
        private List<CompilationUnity> units;
        private Dictionary<string, CompilationUnity> unityTable;
        private CompilationUnity program;
        private List<Tuple<string, int>> externalFunctions;
        private Dictionary<string, int> externalFunctionMap;        

        internal CompilationUnity unity;
        internal CompilationUnity unitySystem;
        internal Function function;

        internal int globalVariableOffset;

        public event CompileError OnCompileError;

        public string UnityPath
        {
            get => unityPath;

            set => unityPath = value;
        }

        public Compiler(string unityPath = null)
        {
            if (unityPath == null)
                unityPath = Directory.GetCurrentDirectory();

            this.unityPath = unityPath;

            labels = new List<Label>();
            units = new List<CompilationUnity>();
            unityTable = new Dictionary<string, CompilationUnity>();
            externalFunctions = new List<Tuple<string, int>>();
            externalFunctionMap = new Dictionary<string, int>();

            unity = null;
            function = null;
        }

        public int AddExternalFunction(string name, int paramSize)
        {
            if (externalFunctionMap.ContainsKey(name))
                throw new Exception("External function '" + name + "' already added.");

            int index = externalFunctions.Count;
            externalFunctions.Add(new Tuple<string, int>(name, paramSize));
            externalFunctionMap.Add(name, index);
            return index;
        }

        public int GetExternalFunctionIndex(string name)
        {
            if (externalFunctionMap.TryGetValue(name, out int index))
                return index;

            return -1;
        }

        public int GetOrAddExternalFunction(string name, int paramSize)
        {
            int index = GetExternalFunctionIndex(name);
            if (index != -1)
                return index;

            index = externalFunctions.Count;
            externalFunctions.Add(new Tuple<string, int>(name, paramSize));
            externalFunctionMap.Add(name, index);
            return index;
        }

        public string GetExternalFunctionName(int index)
        {
            return externalFunctions[index].Item1;
        }

        internal Label CreateLabel()
        {
            Label result = new Label();
            labels.Add(result);
            return result;
        }

        private void CompileInt32Conversion(Assembler assembler, PrimitiveType toType)
        {
            switch (toType.Primitive)
            {
                case Primitive.LONG:
                    assembler.EmitInt32ToInt64();
                    break;

                case Primitive.FLOAT:
                    assembler.EmitInt32ToFloat32();
                    break;

                case Primitive.DOUBLE:
                    assembler.EmitInt32ToFloat64();
                    break;
            }
        }

        private void CompileCast(Assembler assembler, AbstractType fromType, AbstractType toType, bool isExplicit, SourceInterval interval)
        {
            if (fromType is PrimitiveType p)
            {
                if (toType is PrimitiveType tp)
                {
                    switch (p.Primitive)
                    {
                        case Primitive.VOID:
                            throw new CompilerException(interval, "Conversão inválida de 'void' para '" + tp + "'.");

                        case Primitive.BOOL:
                            if (isExplicit ? false : tp.Primitive != Primitive.BOOL)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            // TODO Implementar
                            break;

                        case Primitive.BYTE:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.BYTE || tp.Primitive == Primitive.CHAR)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.CHAR:
                            if (isExplicit ? tp.Primitive != Primitive.BOOL : tp.Primitive != Primitive.CHAR)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.SHORT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.SHORT)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.INT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.INT)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            CompileInt32Conversion(assembler, tp);
                            break;

                        case Primitive.LONG:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.LONG)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            switch (tp.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitInt64ToInt32();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitInt64ToFloat64();
                                    assembler.EmitFloat64ToFloat32();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitInt64ToFloat64();
                                    break;
                            }

                            break;

                        case Primitive.FLOAT:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.FLOAT)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            switch (tp.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitFloat32ToInt32();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitFloat32ToInt64();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFloat32ToFloat64();
                                    break;
                            }

                            break;

                        case Primitive.DOUBLE:
                            if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.DOUBLE)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            switch (tp.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitFloat64ToInt64();
                                    assembler.EmitInt64ToInt32();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitFloat64ToInt64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFloat64ToFloat32();
                                    break;
                            }

                            break;
                    }

                    return;
                }

                if (toType is PointerType)
                {
                    switch (p.Primitive)
                    {
                        case Primitive.VOID:
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                            throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                        case Primitive.INT:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt32ToPointer();
                            break;

                        case Primitive.LONG:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitInt64ToPointer();
                            assembler.EmitInt64ToInt32();
                            break;

                        case Primitive.FLOAT:
                        case Primitive.DOUBLE:
                            throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");
                    }

                    return;
                }

                throw new CompilerException(interval, "Tipo desconhecido: '" + toType + "'.");
            }

            if (fromType is StructType s)
            {
                if (!s.Equals(toType))
                    throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                return;
            }

            if (fromType is ArrayType a)
            {
                if (!a.Equals(toType))
                    throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                return;
            }

            if (fromType is PointerType fptr)
            {
                if (toType is PrimitiveType tp)
                {
                    switch (tp.Primitive)
                    {
                        case Primitive.INT:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitPointerToInt32();
                            return;

                        case Primitive.LONG:
                            if (!isExplicit)
                                throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                            assembler.EmitPointerToInt64();
                            return;

                        default:
                            throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido para o tipo '" + toType + "'.");
                    }

                    throw new CompilerException(interval, "Tipo desconhecido: '" + toType + "'.");
                }

                if (toType is PointerType tptr)
                {
                    if (fptr.Type == null)
                        return;

                    AbstractType otherType = tptr.Type;
                    if (isExplicit ? false : !PrimitiveType.IsPrimitiveVoid(otherType) && fptr.Type != otherType)
                        throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido implicitamente para o tipo '" + toType + "'.");

                    return;
                }

                throw new CompilerException(interval, "Tipo desconhecido: '" + toType + "'.");
            }

            throw new CompilerException(interval, "Tipo desconhecido: '" + fromType + "'.");
        }

        private void CompilePop(Assembler assembler, AbstractType type)
        {
            if (!PrimitiveType.IsPrimitiveVoid(type))
                assembler.EmitSubSP(GetAlignedSize(type.Size()));
        }

        private void CompileArrayIndexer(Context context, Assembler assembler, Expression indexer)
        {
            AbstractType indexerType = CompileExpression(context, assembler, indexer);

            if (indexerType is PrimitiveType pt)
            {
                switch (pt.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.CHAR:
                    case Primitive.LONG:
                    case Primitive.FLOAT:
                    case Primitive.DOUBLE:
                        throw new CompilerException(indexer.Interval, "Tipo de indexador inválido: '" + pt + "'.");
                }
            }
        }

        private AbstractType CompileAssignableExpression(Context context, Assembler assembler, Expression expression, out Variable storeVar, out bool isPointerDeference, bool loadHostAddress = false)
        {
            if (expression is UnaryExpression u)
            {
                Expression operand = u.Operand;
                AbstractType operandType = CompileExpression(context, assembler, operand);
                if (u.Operation != UnaryOperation.POINTER_INDIRECTION)
                    throw new CompilerException(operand.Interval, "A expressão do lado esquerdo não é atribuível.");

                if (!(operandType is PointerType ptr))
                    throw new CompilerException(operand.Interval, "Indireção de ponteiros só pode ser feita com tipos de ponteiros.");

                isPointerDeference = true;
                storeVar = null;
                return ptr.Type;
            }

            if (expression is FieldAcessorExpression f)
            {
                Expression operand = f.Operand;
                string fieldName = f.Field;

                AbstractType operandType = CompileAssignableExpression(context, assembler, operand, out _, out _);

                if (operandType is StructType s)
                {
                    Field field = s.FindField(fieldName);
                    if (field == null)
                        throw new CompilerException(expression.Interval, "Campo '" + fieldName + "' não encontrado na estrutura: '" + s.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitAdd();

                    if (loadHostAddress)
                        assembler.EmitResidentToHostAddress();

                    isPointerDeference = false;
                    storeVar = null;
                    return field.Type;
                }

                if (operandType is PointerType ptr)
                {
                    if (ptr.Type == null || !(ptr.Type is StructType s2))
                        throw new CompilerException(operand.Interval, "Pointeiro de estrutura esperado.");

                    assembler.EmitLoadStackPtr();

                    Field field = s2.FindField(fieldName);
                    if (field == null)
                        throw new CompilerException(expression.Interval, "Campo '" + fieldName + "' não encontrado na estrutura: '" + s2.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitPtrAdd();

                    isPointerDeference = true;
                    storeVar = null;
                    return field.Type;
                }

                throw new CompilerException(operand.Interval, "Acesso de membros em um tipo que não é estrutura: '" + operandType + "'.");
            }

            if (expression is ArrayAccessorExpression a)
            {
                Expression operand = a.Operand;
                AbstractType operandType = CompileAssignableExpression(context, assembler, operand, out _, out _);

                if (operandType is ArrayType at)
                {
                    int rank = at.Rank;

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o array.");

                    if (a.IndexerCount != rank)
                        throw new CompilerException(operand.Interval, "Número de inídices fornecidos é diferente da dimensão do array.");

                    Expression indexer = a[0];
                    CompileArrayIndexer(context, assembler, indexer);

                    for (int i = 1; i < rank; i++)
                    {
                        indexer = a[i];
                        assembler.EmitLoadConst(at[i]);
                        assembler.EmitMul();
                        CompileArrayIndexer(context, assembler, indexer);
                        assembler.EmitAdd();
                    }

                    assembler.EmitLoadConst(at.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitAdd();

                    if (loadHostAddress)
                        assembler.EmitResidentToHostAddress();

                    isPointerDeference = false;
                    storeVar = null;
                    return at.Type;
                }

                if (operandType is PointerType ptr)
                {
                    if (ptr.Type == null)
                        throw new CompilerException(operand.Interval, "Não é possível realizar esta operação em um ponteiro do tipo void.");

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o ponteiro.");

                    if (a.IndexerCount != 1)
                        throw new CompilerException(operand.Interval, "Deve-se fornecer somente um índice para o ponteiro.");

                    assembler.EmitLoadStackPtr();

                    Expression indexer = a[0];
                    CompileArrayIndexer(context, assembler, indexer);
                    assembler.EmitLoadConst(ptr.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitPtrAdd();

                    isPointerDeference = true;
                    storeVar = null;
                    return ptr.Type;
                }

                throw new CompilerException(operand.Interval, "Tipo '" + operandType + "' não é um array.");
            }

            if (expression is PrimaryExpression p)
            {
                if (p.PrimaryType != PrimaryType.IDENTIFIER)
                    throw new CompilerException(expression.Interval, "Tipo de expressão não atribuível.");

                IdentifierExpression id = (IdentifierExpression) p;
                string name = id.Name;

                bool byRef = false;
                Variable var = context.FindVariable(name);
                if (var == null)
                {
                    var = unity.FindGlobalVariable(name);
                    if (var == null)
                        throw new CompilerException(id.Interval, "Identificador'" + name + "' não declarado.");

                    // variável local ou parâmetro
                    int offset = var.Offset;

                    if (loadHostAddress)
                        assembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + offset);
                    else
                        assembler.EmitLoadConst(unity.GlobalStartOffset + offset);
                }
                else
                {
                    // variável local ou parâmetro
                    int offset = var.Offset;

                    if (var is Parameter param && param.ByRef)
                    {
                        byRef = true;
                        assembler.EmitLoadLocalPtr(offset);
                    }
                    else if (loadHostAddress)
                        assembler.EmitLoadLocalHostAddress(offset);
                    else
                        assembler.EmitLoadLocalResidentAddress(offset);
                }

                isPointerDeference = byRef;
                storeVar = !byRef && (var.Type is PrimitiveType || var.Type is PointerType) ? var : null;
                return var.Type;
            }

            throw new CompilerException(expression.Interval, "Tipo de expressão não atribuível.");
        }

        private void CompileStoreExpression(Context context, Assembler assembler, BinaryOperation operation, Expression leftOperand, Expression rightOperand)
        {
            AbstractType leftType;
            AbstractType rightType;

            if (operation == BinaryOperation.STORE)
            {
                Assembler leftAssembler = new Assembler();
                leftType = CompileAssignableExpression(context, leftAssembler, leftOperand, out Variable storeVar, out bool isPointerDeference);
                if (storeVar == null)
                    assembler.Emit(leftAssembler);

                rightType = CompileExpression(context, assembler, rightOperand);
                CompileCast(assembler, rightType, leftType, false, rightOperand.Interval);

                if (storeVar != null)
                    CompileStore(assembler, storeVar, leftOperand.Interval);
                else if (isPointerDeference)
                    CompileStorePointer(assembler, leftType, leftOperand.Interval);
                else
                    CompileStoreStack(assembler, leftType, leftOperand.Interval);

                return;
            }

            Assembler tempAssembler = new Assembler();
            leftType = CompileAssignableExpression(context, tempAssembler, leftOperand, out Variable storeVar2, out bool isPointerDeference2);

            if (storeVar2 == null)
            {
                Assembler tempAssembler2 = new Assembler();
                tempAssembler2.Emit(tempAssembler);

                if (isPointerDeference2)
                    CompileLoadPointer(tempAssembler2, leftType, leftOperand.Interval);
                else
                    CompileLoadStack(tempAssembler2, leftType, leftOperand.Interval);

                assembler.Emit(tempAssembler);
                assembler.Emit(tempAssembler2);
            }
            else
                CompileLoad(assembler, storeVar2, leftOperand.Interval);

            rightType = CompileExpression(context, assembler, rightOperand);
            CompileCast(assembler, rightType, leftType, false, rightOperand.Interval);

            switch (operation)
            {
                case BinaryOperation.STORE_OR:
                {
                    if (storeVar2 == null)
                        CompileStoreStackOr(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerOr(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreOr(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_XOR:
                {
                    if (storeVar2 == null)
                        CompileStoreStackXor(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerXor(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreXor(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_AND:
                {
                    if (storeVar2 == null)
                        CompileStoreStackAnd(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerAnd(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreAnd(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SHIFT_LEFT:
                {
                    if (storeVar2 == null)
                        CompileStoreStackShiftLeft(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerShiftLeft(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreShiftLeft(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SHIFT_RIGHT:
                {
                    if (storeVar2 == null)
                        CompileStoreStackShiftRight(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerShiftRight(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreShiftRight(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_UNSIGNED_SHIFT_RIGHT:
                {
                    if (storeVar2 == null)
                        CompileStoreStackUnsignedShiftRight(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerUnsignedShiftRight(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreUnsignedShiftRight(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_ADD:
                {
                    if (storeVar2 == null)
                        CompileStoreStackAdd(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerAdd(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreAdd(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SUB:
                {
                    if (storeVar2 == null)
                        CompileStoreStackSub(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerSub(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreSub(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_MUL:
                {
                    if (storeVar2 == null)
                        CompileStoreStackMul(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerMul(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreMul(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_DIV:
                {
                    if (storeVar2 == null)
                        CompileStoreStackDiv(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerDiv(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreDiv(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_MOD:
                {
                    if (storeVar2 == null)
                        CompileStoreStackMod(assembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference2)
                        CompileStorePointerMod(assembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreMod(assembler, storeVar2, leftOperand.Interval);

                    break;
                }

                default:
                    throw new CompilerException(leftOperand.Interval, "Operador '" + operation + "' desconhecido.");
            }
        }

        private AbstractType CompileExpression(Context context, Assembler assembler, Expression expression, bool getArrayAddress = false)
        {
            if (expression is UnaryExpression u)
            {
                Expression operand = u.Operand;
                switch (u.Operation)
                {
                    case UnaryOperation.NEGATION:
                    {
                        AbstractType operandType = CompileExpression(context, assembler, operand);
                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitNeg();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitNeg64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFNeg();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFNeg64();
                                    break;
                            }

                            return operandType;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.BITWISE_NOT:
                    {
                        AbstractType operandType = CompileExpression(context, assembler, operand);
                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                case Primitive.FLOAT:
                                case Primitive.DOUBLE:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitNot();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitNot64();
                                    break;
                            }

                            return operandType;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.LOGICAL_NOT:
                    {
                        AbstractType operandType = CompileExpression(context, assembler, operand);
                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                    assembler.EmitNot();
                                    break;

                                default:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");
                            }

                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POINTER_INDIRECTION:
                    {
                        AbstractType operandType = CompileExpression(context, assembler, operand);
                        if (operandType is PointerType ptr)
                        {
                            AbstractType ptrType = ptr.Type;
                            if (ptrType == null)
                                throw new CompilerException(operand.Interval, "Indireção de ponteiros não é valida para literais nulos.");

                            if (ptrType is PrimitiveType pt)
                            {
                                switch (pt.Primitive)
                                {
                                    case Primitive.VOID:
                                        throw new CompilerException(operand.Interval, "Indireção em ponteiros do tipo void não pode ser feita.");

                                    case Primitive.BOOL:
                                    case Primitive.BYTE:
                                        assembler.EmitLoadPointer8();
                                        break;

                                    case Primitive.CHAR:
                                    case Primitive.SHORT:
                                        assembler.EmitLoadPointer16();
                                        break;

                                    case Primitive.INT:
                                    case Primitive.FLOAT:
                                        assembler.EmitLoadPointer32();
                                        break;

                                    case Primitive.LONG:
                                    case Primitive.DOUBLE:
                                        assembler.EmitLoadPointer64();
                                        break;
                                }
                            }
                            else if (ptrType is ArrayType at)
                            {
                                // TODO Implementar
                            }
                            else if (ptrType is StructType st)
                            {
                                // TODO Implementar
                            }
                            else if (ptrType is PointerType)
                                assembler.EmitLoadPointerPtr();

                            return ptrType;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.PRE_INCREMENT: // ++x <=> x = x + 1
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                        if (storeVar == null)
                        {
                            assembler.Emit(tempAssembler);
                            assembler.Emit(tempAssembler);

                            if (isPointerDeference)
                                CompileLoadPointer(assembler, operandType, operand.Interval);
                            else
                                CompileLoadStack(assembler, operandType, operand.Interval);
                        }
                        else
                            CompileLoad(assembler, storeVar, operand.Interval);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer8();
                                        else
                                            assembler.EmitStoreStack8();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer16();
                                        else
                                            assembler.EmitStoreStack16();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitAdd64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFAdd64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            if (storeVar == null)
                            {
                                assembler.Emit(tempAssembler);

                                if (isPointerDeference)
                                    CompileLoadPointer(assembler, operandType, operand.Interval);
                                else
                                    CompileLoadStack(assembler, operandType, operand.Interval);
                            }
                            else
                                CompileLoad(assembler, storeVar, operand.Interval);

                            return operandType;
                        }

                        if (operandType is PointerType ptr)
                        {
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitPtrAdd();

                            if (storeVar == null)
                            {
                                if (isPointerDeference)
                                    assembler.EmitStorePointerPtr();
                                else
                                    assembler.EmitStoreStackPtr();
                            }
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobalPtr(storeVar.Offset);
                            else
                                assembler.EmitStoreLocalPtr(storeVar.Offset);

                            return operandType;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.PRE_DECREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                        if (storeVar == null)
                        {
                            assembler.Emit(tempAssembler);
                            assembler.Emit(tempAssembler);

                            if (isPointerDeference)
                                CompileLoadPointer(assembler, operandType, operand.Interval);
                            else
                                CompileLoadStack(assembler, operandType, operand.Interval);
                        }
                        else
                            CompileLoad(assembler, storeVar, operand.Interval);

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer8();
                                        else
                                            assembler.EmitStoreStack8();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer16();
                                        else
                                            assembler.EmitStoreStack16();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            if (storeVar == null)
                            {
                                assembler.Emit(tempAssembler);

                                if (isPointerDeference)
                                    CompileLoadPointer(assembler, operandType, operand.Interval);
                                else
                                    CompileLoadStack(assembler, operandType, operand.Interval);
                            }
                            else
                                CompileLoad(assembler, storeVar, operand.Interval);

                            return operandType;
                        }

                        if (operandType is PointerType ptr)
                        {
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitPtrSub();

                            if (storeVar == null)
                            {
                                if (isPointerDeference)
                                    assembler.EmitStorePointerPtr();
                                else
                                    assembler.EmitStoreStackPtr();
                            }
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobalPtr(storeVar.Offset);
                            else
                                assembler.EmitStoreLocalPtr(storeVar.Offset);

                            return operandType;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POST_INCREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                        if (storeVar == null)
                        {
                            Assembler tempAssembler2 = new Assembler();
                            tempAssembler2.Emit(tempAssembler);

                            if (isPointerDeference)
                                CompileLoadPointer(tempAssembler2, operandType, operand.Interval);
                            else
                                CompileLoadStack(tempAssembler2, operandType, operand.Interval);

                            assembler.Emit(tempAssembler2);

                            assembler.Emit(tempAssembler);
                            assembler.Emit(tempAssembler2);
                        }
                        else
                        {
                            Assembler tempAssembler2 = new Assembler();
                            CompileLoad(tempAssembler2, storeVar, operand.Interval);

                            assembler.Emit(tempAssembler2);
                            assembler.Emit(tempAssembler2);
                        }

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer8();
                                        else
                                            assembler.EmitStoreStack8();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((short) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer16();
                                        else
                                            assembler.EmitStoreStack16();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst(1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitAdd64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFAdd();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFAdd64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            if (operandType is PointerType ptr)
                            {
                                assembler.EmitLoadConst((byte) 1);
                                assembler.EmitPtrAdd();

                                if (storeVar == null)
                                {
                                    if (isPointerDeference)
                                        assembler.EmitStorePointerPtr();
                                    else
                                        assembler.EmitStoreStackPtr();
                                }
                                else if (storeVar is GlobalVariable)
                                    assembler.EmitStoreGlobalPtr(storeVar.Offset);
                                else
                                    assembler.EmitStoreLocalPtr(storeVar.Offset);

                                return operandType;
                            }

                            return operandType;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POST_DECREMENT:
                    {
                        Assembler tempAssembler = new Assembler();
                        AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                        if (storeVar == null)
                        {
                            Assembler tempAssembler2 = new Assembler();

                            tempAssembler2.Emit(tempAssembler);

                            if (isPointerDeference)
                                CompileLoadPointer(tempAssembler2, operandType, operand.Interval);
                            else
                                CompileLoadStack(tempAssembler2, operandType, operand.Interval);

                            assembler.Emit(tempAssembler2);

                            assembler.Emit(tempAssembler);
                            assembler.Emit(tempAssembler2);
                        }
                        else
                        {
                            Assembler tempAssembler2 = new Assembler();
                            CompileLoad(tempAssembler2, storeVar, operand.Interval);

                            assembler.Emit(tempAssembler2);
                            assembler.Emit(tempAssembler2);
                        }

                        if (operandType is PrimitiveType pt)
                        {
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + pt + "'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer8();
                                        else
                                            assembler.EmitStoreStack8();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);


                                    break;
                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer16();
                                        else
                                            assembler.EmitStoreStack16();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();

                                    if (storeVar == null)
                                    {
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    }
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            return operandType;
                        }

                        if (operandType is PointerType ptr)
                        {
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitPtrSub();

                            if (storeVar == null)
                            {
                                if (isPointerDeference)
                                    assembler.EmitStorePointerPtr();
                                else
                                    assembler.EmitStoreStackPtr();
                            }
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobalPtr(storeVar.Offset);
                            else
                                assembler.EmitStoreLocalPtr(storeVar.Offset);

                            return operandType;
                        }

                        throw new CompilerException(operand.Interval, "Operação não definida para o tipo '" + operandType + "'.");
                    }

                    case UnaryOperation.POINTER_TO:
                    {
                        AbstractType operandType = CompileAssignableExpression(context, assembler, operand, out _, out _, true);

                        if (operandType is ArrayType at)
                            return new PointerType(at.Type, true);

                        return new PointerType(operandType);
                    }

                    default:
                        throw new CompilerException(expression.Interval, "Operador '" + u.Operation + "' desconhecido.");
                }
            }

            if (expression is BinaryExpression b)
            {
                Expression leftOperand = b.LeftOperand;
                Expression rightOperand = b.RightOperand;

                if (b.Operation <= BinaryOperation.STORE_MOD)
                {
                    CompileStoreExpression(context, assembler, b.Operation, leftOperand, rightOperand);
                    return PrimitiveType.VOID;
                }

                Assembler leftAssembler = new Assembler();
                AbstractType leftType = CompileExpression(context, leftAssembler, leftOperand);

                Assembler rightAssembler = new Assembler();
                AbstractType rightType = CompileExpression(context, rightAssembler, rightOperand);

                switch (b.Operation)
                {
                    case BinaryOperation.LOGICAL_OR:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType))
                            throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        if (!PrimitiveType.IsPrimitiveBool(rightType))
                            throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitOr();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.LOGICAL_XOR:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType))
                            throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        if (!PrimitiveType.IsPrimitiveBool(rightType))
                            throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitXor();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.LOGICAL_AND:
                    {
                        if (!PrimitiveType.IsPrimitiveBool(leftType))
                            throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        if (!PrimitiveType.IsPrimitiveBool(rightType))
                            throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitAnd();
                        return PrimitiveType.BOOL;
                    }

                    case BinaryOperation.SHIFT_LEFT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new CompilerException(rightOperand.Interval, "Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitShl();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitShl64();
                        else
                            throw new CompilerException(leftOperand.Interval, "Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.SHIFT_RIGHT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new CompilerException(rightOperand.Interval, "Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitShr();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitShr64();
                        else
                            throw new CompilerException(leftOperand.Interval, "Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.UNSIGNED_SHIFT_RIGHT:
                    {
                        if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                            throw new CompilerException(rightOperand.Interval, "Tipo inválido para o operando 2: '" + rightType + "'.");

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        if (PrimitiveType.IsUpTo32BitsInt(leftType))
                            assembler.EmitUShr();
                        else if (PrimitiveType.Is64BitsInt(leftType))
                            assembler.EmitUShr64();
                        else
                            throw new CompilerException(leftOperand.Interval, "Tipo inválido para o operando 1: '" + leftType + "'.");

                        return leftType;
                    }

                    case BinaryOperation.EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveBool(leftType) && PrimitiveType.IsPrimitiveBool(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitAnd();
                            assembler.Emit(rightAssembler);
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitAnd();
                            assembler.EmitCompareEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitComparePointerEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType == rightType)
                        {
                            // TODO Implementar
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.NOT_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveBool(leftType) && PrimitiveType.IsPrimitiveBool(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitAnd();
                            assembler.Emit(rightAssembler);
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitAnd();
                            assembler.EmitCompareNotEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareNotEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareNotEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareNotEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareNotEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareNotEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitComparePointerNotEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType == rightType)
                        {
                            // TODO Implementar
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.GREATER:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareGreater();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareGreater64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareGreater();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareGreater64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareGreater();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitComparePointerGreater();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.GREATER_OR_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareGreaterOrEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareGreaterOrEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareGreaterOrEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareGreaterOrEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareGreaterOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitComparePointerGreaterOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.LESS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareLess();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareLess64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareLess();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareLess64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareLess();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitComparePointerLess();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.LESS_OR_EQUALS:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitCompareLessOrEquals();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitCompareLessOrEquals64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFCompareLessOrEquals();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFCompareLessOrEquals64();
                                    break;
                            }

                            return PrimitiveType.BOOL;
                        }

                        if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitCompareLessOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        if (leftType is PointerType && rightType is PointerType)
                        {
                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitComparePointerLessOrEquals();
                            return PrimitiveType.BOOL;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_OR:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitOr();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitOr64();
                                    break;
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_XOR:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitXor();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitXor64();
                                    break;
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.BITWISE_AND:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitAnd();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitAnd64();
                                    break;
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.ADD:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitAdd();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitAdd64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFAdd();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFAdd64();
                                    break;
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        if (leftType is PointerType ptr)
                        {
                            if (PrimitiveType.IsPrimitiveVoid(ptr.Type))
                                throw new CompilerException(leftOperand.Interval, "Operação aritimética com ponteiros não permitida para ponteiros do tipo void.");

                            if (!(rightType is PrimitiveType rp) || !PrimitiveType.IsPrimitiveInteger(rp))
                                throw new CompilerException(rightOperand.Interval, "Operando direito de uma operação de deslocamento de ponteiros deve ser um inteiro.");

                            int size = ptr.Type.Size();

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);
                            assembler.EmitLoadConst(size);
                            assembler.EmitMul();

                            if (rp.Primitive == Primitive.INT)
                                assembler.EmitPtrAdd();
                            else
                                assembler.EmitPtrAdd64();

                            return leftType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.SUB:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitSub();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitSub64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFSub();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFSub64();
                                    break;
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        if (leftType is PointerType ptr)
                        {
                            if (rightType is PrimitiveType rp && PrimitiveType.IsPrimitiveInteger(rp))
                            {
                                if (PrimitiveType.IsPrimitiveVoid(ptr.Type))
                                    throw new CompilerException(leftOperand.Interval, "Operação aritimética com ponteiros não permitida para ponteiros do tipo void.");

                                int size = ptr.Type.Size();

                                assembler.Emit(leftAssembler);
                                assembler.Emit(rightAssembler);
                                assembler.EmitLoadConst(size);
                                assembler.EmitMul();

                                if (rp.Primitive == Primitive.INT)
                                    assembler.EmitPtrSub();
                                else
                                    assembler.EmitPtrSub64();

                                return leftType;
                            }

                            throw new CompilerException(rightOperand.Interval, "Operando direito de uma operação de subtração envolvendo ponteiros deve ser um inteiro ou um ponteiro.");
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.MUL:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitMul();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitMul64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFMul();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFMul64();
                                    break;

                                default:
                                    throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.DIV:
                    {
                        if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitDiv();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitDiv64();
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitFDiv();
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitFDiv64();
                                    break;

                                default:
                                    throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    case BinaryOperation.MOD:
                    {
                        if (PrimitiveType.IsPrimitiveInteger(leftType) && PrimitiveType.IsPrimitiveInteger(rightType))
                        {
                            PrimitiveType resultType = null;
                            if (leftType.CoerceWith(rightType, false))
                            {
                                resultType = (PrimitiveType) rightType;
                                CompileCast(leftAssembler, leftType, rightType, false, leftOperand.Interval);
                            }
                            else if (rightType.CoerceWith(leftType, false))
                            {
                                resultType = (PrimitiveType) leftType;
                                CompileCast(rightAssembler, rightType, leftType, false, rightOperand.Interval);
                            }

                            assembler.Emit(leftAssembler);
                            assembler.Emit(rightAssembler);

                            switch (resultType.Primitive)
                            {
                                case Primitive.BYTE:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitMod();
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitMod64();
                                    break;

                                default:
                                    throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                            }

                            return resultType.Size() < sizeof(int) ? PrimitiveType.INT : resultType;
                        }

                        throw new CompilerException(expression.Interval, "Tipos imcompatíveis: '" + leftType + "' e '" + rightType + "'.");
                    }

                    default:
                        throw new CompilerException(expression.Interval, "Operador '" + b.Operation + "' desconhecido.");
                }
            }

            if (expression is FieldAcessorExpression f)
            {
                Expression operand = f.Operand;
                string fieldName = f.Field;

                AbstractType operandType = CompileAssignableExpression(context, assembler, operand, out _, out _);

                if (operandType is StructType s)
                {
                    Field field = s.FindField(fieldName);
                    if (field == null)
                        throw new CompilerException(expression.Interval, "Campo '" + fieldName + "' não encontrado na estrutura: '" + s.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitAdd();

                    if (getArrayAddress && field.Type is ArrayType at2)
                    {
                        assembler.EmitResidentToHostAddress();
                        return new PointerType(at2.Type, true);
                    }

                    CompileLoadStack(assembler, field.Type, expression.Interval);

                    return field.Type;
                }

                if (operandType is PointerType ptr)
                {
                    if (ptr.Type == null)
                        throw new CompilerException(operand.Interval, "Não é possível realizar essa operação em um ponteiro do tipo void.");

                    if (!(ptr.Type is StructType s2))
                        throw new CompilerException(operand.Interval, "Pointeiro de estrutura esperado.");

                    assembler.EmitLoadStackPtr();

                    Field field = s2.FindField(fieldName);
                    if (field == null)
                        throw new CompilerException(expression.Interval, "Campo '" + fieldName + "' não encontrado na estrutura: '" + s2.Name + "'.");

                    assembler.EmitLoadConst(field.Offset);
                    assembler.EmitPtrAdd();

                    if (getArrayAddress && field.Type is ArrayType at2)
                        return new PointerType(at2.Type, true);

                    CompileLoadPointer(assembler, field.Type, expression.Interval);

                    return field.Type;
                }

                throw new CompilerException(operand.Interval, "Acesso de membros em um tipo que não é estrutura: '" + operandType + "'.");
            }

            if (expression is ArrayAccessorExpression a)
            {
                Expression operand = a.Operand;
                AbstractType operandType = CompileAssignableExpression(context, assembler, operand, out _, out _);

                if (operandType is ArrayType at)
                {
                    int rank = at.Rank;

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o array.");

                    if (a.IndexerCount != rank)
                        throw new CompilerException(operand.Interval, "Número de inídices fornecidos é diferente da dimensão do array.");

                    Expression indexer = a[0];
                    CompileArrayIndexer(context, assembler, indexer);

                    for (int i = 1; i < rank; i++)
                    {
                        indexer = a[i];
                        assembler.EmitLoadConst(at[i]);
                        assembler.EmitMul();
                        CompileArrayIndexer(context, assembler, indexer);
                        assembler.EmitAdd();
                    }

                    assembler.EmitLoadConst(at.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitAdd();

                    if (getArrayAddress && at.Type is ArrayType at2)
                    {
                        assembler.EmitResidentToHostAddress();
                        return new PointerType(at2.Type, true);
                    }

                    CompileLoadStack(assembler, at.Type, expression.Interval);

                    return at.Type;
                }

                if (operandType is PointerType ptr)
                {
                    if (ptr.Type == null)
                        throw new CompilerException(operand.Interval, "Não é possível realizar essa operação em um ponteiro do tipo void.");

                    if (a.IndexerCount == 0)
                        throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o ponteiro.");

                    if (a.IndexerCount != 1)
                        throw new CompilerException(operand.Interval, "Deve-se fornecer somente um índice único para o ponteiro.");

                    assembler.EmitLoadStackPtr();

                    Expression indexer = a[0];
                    CompileArrayIndexer(context, assembler, indexer);
                    assembler.EmitLoadConst(ptr.Type.Size());
                    assembler.EmitMul();
                    assembler.EmitPtrAdd();

                    if (getArrayAddress && ptr.Type is ArrayType at2)
                        return new PointerType(at2.Type, true);

                    CompileLoadPointer(assembler, ptr.Type, expression.Interval);

                    return ptr.Type;
                }

                throw new CompilerException(operand.Interval, "Tipo '" + operandType + "' não é um array.");
            }

            if (expression is PrimaryExpression p)
            {
                switch (p)
                {
                    case BoolLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.BOOL;

                    case ByteLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.BYTE;

                    case CharLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.CHAR;

                    case ShortLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.SHORT;

                    case IntLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.INT;

                    case LongLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.LONG;

                    case FloatLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.FLOAT;

                    case DoubleLiteralExpression l:
                        assembler.EmitLoadConst(l.Value);
                        return PrimitiveType.DOUBLE;

                    case StringLiteralExpression l:
                    {
                        int offset = unity.GetStringOffset(l.Value);
                        assembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + offset);
                        return PointerType.STRING;
                    }

                    case NullLiteralExpression l:
                        assembler.EmitLoadConst((byte) 0);
                        assembler.EmitInt32ToPointer();
                        return PointerType.NULL;

                    case IdentifierExpression id:
                    {
                        string name = id.Name;

                        Variable var = context.FindVariable(name);
                        if (var == null)
                        {
                            var = unity.FindGlobalVariable(name);
                            if (var == null)
                                throw new CompilerException(id.Interval, "Identificador'" + name + "' não declarado.");
                        }

                        if (getArrayAddress && var.Type is ArrayType at2)
                        {
                            if (var is GlobalVariable)
                                assembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + var.Offset);
                            else
                                assembler.EmitLoadLocalHostAddress(var.Offset);

                            return new PointerType(at2.Type, true);
                        }

                        CompileLoad(assembler, var, id.Interval);

                        return var.Type;
                    }
                }

                throw new CompilerException(expression.Interval, "Tipo de expressão primária desconhecido: '" + p + "'");
            }

            if (expression is CallExpression c)
            {
                Expression operand = c.Operand;
                if (operand is IdentifierExpression id)
                {
                    string functionName = id.Name;
                    Function func = unity.FindFunction(functionName);
                    if (func == null)
                        throw new CompilerException(id.Interval, "Função '" + functionName + "' não declarada.");

                    AbstractType returnType = func.ReturnType;
                    if (!PrimitiveType.IsPrimitiveVoid(returnType))
                        assembler.EmitAddSP(GetAlignedSize(returnType.Size()));

                    if (func.ParamCount != c.ParameterCount)
                        throw new CompilerException(id.Interval, "A quantidade de parâmetros fornecido é diferente da quantidade total de parâmetros esperada.");

                    for (int j = 0; j < func.ParamCount; j++)
                    {
                        Parameter parameter = func[j];
                        Expression expressionParameter = c[j];

                        if (parameter.ByRef)
                        {
                            AbstractType paramType = CompileAssignableExpression(context, assembler, expressionParameter, out _, out _, true);
                            if (paramType != parameter.Type)
                            {
                                if (paramType is ArrayType at)
                                    paramType = new PointerType(at.Type, true);

                                if (paramType != parameter.Type)
                                    throw new CompilerException(expressionParameter.Interval, "Parâmetro passado por referência deve ser do mesmo tipo que o parâmetro correspondente da função a ser chamada.");
                            }
                        }
                        else
                        {
                            AbstractType paramType = CompileExpression(context, assembler, expressionParameter, parameter.Type is PointerType);
                            CompileCast(assembler, paramType, parameter.Type, false, expressionParameter.Interval);
                        }
                    }

                    if (func.IsExtern)
                    {
                        int index = GetOrAddExternalFunction(functionName, func.ParameterSize);
                        assembler.EmitExternCall(index);
                    }
                    else
                        assembler.EmitCall(func.EntryLabel);

                    return func.ReturnType;
                }

                throw new CompilerException(c.Interval, "Expressão de nome de função inválida.");
            }

            if (expression is CastExpression cs)
            {
                Expression operand = cs.Operand;
                AbstractType type = cs.Type;

                AbstractType operandType = CompileExpression(context, assembler, operand);
                CompileCast(assembler, operandType, type, true, operand.Interval);
                return type;
            }

            throw new CompilerException(expression.Interval, "Tipo desconhecido de expressão: " + expression);
        }

        private Variable CheckVariable(Context context, string name, SourceInterval interval)
        {
            Variable var = context.FindVariable(name);
            if (var == null)
            {
                var = unity.FindGlobalVariable(name);
                if (var == null)
                    throw new CompilerException(interval, "Variável '" + name + "' não declarada.");
            }

            return var;
        }

        private void CompileStatement(Context context, Assembler assembler, Statement statement)
        {
            if (statement is ExpressionStatement e)
            {
                Expression expression = e.Expression;
                AbstractType type = CompileExpression(context, assembler, expression);
                CompilePop(assembler, type);
            }
            else if (statement is DeclarationStatement decl)
                CompileLocalVariableDeclaration(context, assembler, decl);
            else if (statement is ReturnStatement r)
            {
                Expression expr = r.Expression;

                if (expr != null && PrimitiveType.IsPrimitiveVoid(function.ReturnType))
                    throw new CompilerException(expr.Interval, "A função não possui tipo de retorno.");

                if (expr == null && PrimitiveType.IsPrimitiveVoid(function.ReturnType))
                    throw new CompilerException(r.Interval, "Expressão de retorno esperada.");

                if (expr != null)
                {
                    if (function.ReturnType is PrimitiveType || function.ReturnType is PointerType)
                    {
                        AbstractType returnType = CompileExpression(context, assembler, expr);
                        CompileCast(assembler, returnType, function.ReturnType, false, expr.Interval);
                        CompileStoreLocal(assembler, function.ReturnType, function.ReturnOffset, expr.Interval);
                    }
                    else
                    {
                        assembler.EmitLoadLocalResidentAddress(function.ReturnOffset);
                        AbstractType returnType = CompileExpression(context, assembler, expr);
                        CompileCast(assembler, returnType, function.ReturnType, false, expr.Interval);
                        CompileStoreStack(assembler, function.ReturnType, expr.Interval);
                    }
                }

                assembler.EmitJump(function.ReturnLabel);
            }
            else if (statement is BreakStatement b)
            {
                Label breakLabel = context.FindNearestBreakLabel();
                if (breakLabel == null)
                    throw new CompilerException(b.Interval, "Instrução 'quebra' deve estar dentro de um loop.");

                assembler.EmitJump(breakLabel);
            }
            else if (statement is ReadStatement rd)
            {
                for (int j = 0; j < rd.ExpressionCount; j++)
                {
                    Expression expr = rd[j];
                    AbstractType exprType = CompileAssignableExpression(context, assembler, expr, out _, out _, true);
                    if (exprType is PrimitiveType p)
                    {
                        switch (p.Primitive)
                        {
                            case Primitive.BOOL:
                                assembler.EmitScanB();
                                break;

                            case Primitive.BYTE:
                                assembler.EmitScan8();
                                break;

                            case Primitive.CHAR:
                                assembler.EmitScanC();
                                break;

                            case Primitive.SHORT:
                                assembler.EmitScan16();
                                break;

                            case Primitive.INT:
                                assembler.EmitScan32();
                                break;

                            case Primitive.LONG:
                                assembler.EmitScan64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFScan();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFScan64();
                                break;

                            default:
                                throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                        }
                    }
                    else if (exprType is PointerType ptr && ptr.IsString)
                    {
                        assembler.EmitScanString();
                    }
                    else if (exprType is ArrayType at && PrimitiveType.IsPrimitiveChar(at.Type))
                    {
                        assembler.EmitScanString();
                    }
                    else
                        throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                }
            }
            else if (statement is PrintStatement p)
            {
                for (int j = 0; j < p.ExpressionCount; j++)
                {
                    Expression expr = p[j];

                    AbstractType exprType = CompileExpression(context, assembler, expr, true);
                    if (exprType is PrimitiveType pt)
                    {
                        switch (pt.Primitive)
                        {
                            case Primitive.BOOL:
                                assembler.EmitPrintB();
                                break;

                            case Primitive.BYTE:
                                assembler.EmitPrint32();
                                break;

                            case Primitive.CHAR:
                                assembler.EmitPrintC();
                                break;

                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitPrint32();
                                break;

                            case Primitive.LONG:
                                assembler.EmitPrint64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFPrint();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFPrint64();
                                break;

                            default:
                                throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                        }
                    }
                    else if (exprType is PointerType ptr && ptr.IsString)
                    {
                        assembler.EmitPrintString();
                    }
                    else
                        throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                }

                if (p.LineBreak)
                {
                    int lineBreakOffset = unity.GetStringOffset("\n");
                    assembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + lineBreakOffset);
                    assembler.EmitPrintString();
                }
            }
            else if (statement is IfStatement i)
            {
                Expression expression = i.Expression;
                Statement thenStatement = i.ThenStatement;
                Statement elseStatement = i.ElseStatement;

                AbstractType exprType = CompileExpression(context, assembler, expression);

                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new CompilerException(expression.Interval, "Expressão do tipo bool experada.");

                Label lblElse = CreateLabel();
                assembler.EmitJumpIfFalse(lblElse);

                CompileStatement(context, assembler, thenStatement);

                Label lblEnd = CreateLabel();
                assembler.EmitJump(lblEnd);

                assembler.BindLabel(lblElse);
                if (elseStatement != null)
                    CompileStatement(context, assembler, elseStatement);

                assembler.BindLabel(lblEnd);
            }
            else if (statement is WhileStatement w)
            {
                Expression expression = w.Expression;
                Statement stm = w.Statement;

                Label lblLoop = CreateLabel();
                assembler.BindLabel(lblLoop);

                AbstractType exprType = CompileExpression(context, assembler, expression);

                Label lblEnd = CreateLabel();
                context.PushBreakLabel(lblEnd);

                assembler.EmitJumpIfFalse(lblEnd);

                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new CompilerException(expression.Interval, "Expressão do tipo bool experada.");

                CompileStatement(context, assembler, stm);

                assembler.EmitJump(lblLoop);
                assembler.BindLabel(lblEnd);

                context.DropBreakLabel();
            }
            else if (statement is DoStatement d)
            {
                Statement stm = d.Statement;
                Expression expr = d.Expression;

                Label lblLoop = CreateLabel();
                assembler.BindLabel(lblLoop);

                Label lblEnd = CreateLabel();
                context.PushBreakLabel(lblEnd);

                CompileStatement(context, assembler, stm);

                AbstractType exprType = CompileExpression(context, assembler, expr);
                if (!PrimitiveType.IsPrimitiveBool(exprType))
                    throw new CompilerException(expr.Interval, "Expressão do tipo bool experada.");

                assembler.EmitJumpIfFalse(lblLoop);

                assembler.BindLabel(lblEnd);
                context.DropBreakLabel();
            }
            else if (statement is ForStatement f)
            {
                Context forContext = new Context(function, context);

                // inicializadores
                for (int j = 0; j < f.InitializerCount; j++)
                {
                    InitializerStatement initializer = f.GetInitializer(j);
                    CompileStatement(forContext, assembler, initializer);
                }

                Label lblLoop = CreateLabel();
                assembler.BindLabel(lblLoop);

                // expressão de controle
                Expression expression = f.Expression;
                if (expression != null)
                {
                    AbstractType expressionType = CompileExpression(forContext, assembler, expression);
                    if (!PrimitiveType.IsPrimitiveBool(expressionType))
                        throw new CompilerException(expression.Interval, "Expressão do tipo bool esperada.");
                }
                else
                    assembler.EmitLoadConst(true);

                Label lblEnd = CreateLabel();
                forContext.PushBreakLabel(lblEnd);

                assembler.EmitJumpIfFalse(lblEnd);

                Statement stm = f.Statement;
                CompileStatement(forContext, assembler, stm);

                // atualizadores
                for (int j = 0; j < f.UpdaterCount; j++)
                {
                    Expression updater = f.GetUpdater(j);
                    AbstractType updaterType = CompileExpression(forContext, assembler, updater);
                    CompilePop(assembler, updaterType);
                }

                assembler.EmitJump(lblLoop);
                assembler.BindLabel(lblEnd);

                forContext.DropBreakLabel();
            }
            else if (statement is BlockStatement bl)
            {
                Context blockContext = new Context(function, context);
                for (int j = 0; j < bl.StatementCount; j++)
                {
                    Statement stm = bl[j];
                    CompileStatement(blockContext, assembler, stm);
                }
            }
            else
                throw new CompilerException(statement.Interval, "Tipo desconhecido de statement: " + statement);
        }

        private void CompileLocalVariableDeclaration(Context context, Assembler assembler, DeclarationStatement statement)
        {
            statement.Resolve();
            AbstractType type = statement.Type;
            for (int i = 0; i < statement.VariableCount; i++)
            {
                Tuple<string, Expression> tuple = statement[i];
                string name = tuple.Item1;
                Expression initializer = tuple.Item2;

                Variable var = context.DeclareLocalVariable(function, name, type, statement.Interval);
                if (var == null)
                    throw new CompilerException(statement.Interval, "Variável local '" + name + "' já declarada.");

                if (initializer != null)
                {
                    bool useVar = false;
                    if (var is GlobalVariable)
                        assembler.EmitLoadConst(var.Offset);
                    else
                    {
                        useVar = var.Type is PrimitiveType || var.Type is PointerType;
                        if (!useVar)
                            assembler.EmitLoadLocalResidentAddress(var.Offset);
                    }

                    AbstractType initializerType = CompileExpression(context, assembler, initializer);
                    CompileCast(assembler, initializerType, type, false, initializer.Interval);

                    if (useVar)
                        CompileStore(assembler, var, initializer.Interval);
                    else
                        CompileStoreStack(assembler, type, initializer.Interval);
                }
            }
        }

        internal void CompileFunction(Assembler assembler)
        {
            if (function.IsExtern)
                return;

            Context context = new Context(function);
            Assembler tempAssembler = new Assembler();

            CompileStatement(context, tempAssembler, function.Block);
            function.BindReturnLabel(tempAssembler);

            function.BindEntryLabel(assembler);
            function.BeginBlock(assembler);
            assembler.Emit(tempAssembler);
            function.EndBlock(assembler);
        }

        internal CompilationUnity OpenUnity(string name)
        {
            if (unityTable.TryGetValue(name, out CompilationUnity result))
                return result;

            string path = unityPath + '\\' + name + ".sl";
            if (!File.Exists(path))
                return null;

            CompilationUnity unity = ParseCompilationUnityFromFile(path, false);
            units.Add(unity);
            unityTable.Add(name, unity);
            return unity;
        }

        public bool CompileFromSource(int sourceID, string source, Assembler assembler)
        {
            using (var input = new StringReader(source))
            {
                return Compile("#" + sourceID, input, assembler);
            }
        }

        public bool CompileFromSources(Tuple<int, string>[] sources, Assembler assembler)
        {
            try
            {
                Initialize();

                foreach (Tuple<int, string> source in sources)
                {
                    int sourceID = source.Item1;
                    string sourceText = source.Item2;
                    string fileName = "#" + sourceID;

                    using (var input = new StringReader(sourceText))
                    {
                        Parse(fileName, input);
                    }
                }

                Compile(assembler);

                return true;
            }
            catch (CompilerException e)
            {
                OnCompileError?.Invoke(e.Interval, e.Message);
            }

            return false;
        }

        public bool CompileFromFile(string fileName, Assembler assembler)
        {
            using (var input = File.OpenText(fileName))
            {
                return Compile(fileName, input, assembler);
            }
        }

        public bool CompileFromFiles(string[] fileNames, Assembler assembler)
        {
            try
            {
                Initialize();

                foreach (string fileName in fileNames)
                {
                    using (var input = File.OpenText(fileName))
                    {
                        Parse(fileName, input);
                    }
                }

                Compile(assembler);

                return true;
            }
            catch (CompilerException e)
            {
                OnCompileError?.Invoke(e.Interval, e.Message);
            }

            return false;
        }

        public bool CompileFromReader(int sourceID, TextReader input, Assembler assembler)
        {
            return Compile("#" + sourceID, input, assembler);
        }

        private void Initialize()
        {
            globalVariableOffset = sizeof(int);
            function = null;
            unity = null;
            program = null;
            lexer = null;

            labels.Clear();
            units.Clear();
            unityTable.Clear();
            externalFunctions.Clear();
            externalFunctionMap.Clear();

            unitySystem = OpenUnity("System");
        }

        private void Parse(string fileName, TextReader input)
        {
            if (fileName == unitySystem.FileName)
                return;

            lexer = null;

            ParseCompilationUnity(fileName, input);

            globalVariableOffset = sizeof(int);
        }

        private void Compile(Assembler assembler)
        {
            if (program == null)
                throw new CompilerException("Não foi encontrado nenhum programa.");

            globalVariableOffset = sizeof(int);

            foreach (CompilationUnity u in units)
                u.Resolve();

            program.Resolve();

            Assembler tempAssembler = new Assembler();
            foreach (CompilationUnity u in units)
                u.Compile(tempAssembler);

            program.Compile(tempAssembler);

            foreach (CompilationUnity u in units)
                if (u.EntryPoint != null)
                    assembler.EmitCall(u.EntryPoint.EntryLabel);

            if (program.EntryPoint != null)
                assembler.EmitCall(program.EntryPoint.EntryLabel);

            assembler.EmitHalt();

            assembler.Emit(tempAssembler);

            assembler.ReserveConstantBuffer(globalVariableOffset);

            foreach (CompilationUnity u in units)
                u.WriteConstants(assembler);

            program.WriteConstants(assembler);

            foreach (Label label in labels)
                if (label.BindedIP != -1)
                    label.UpdateReferences(assembler);

            assembler.AddExternalFunctionNames(externalFunctions.ToArray());
        }

        private bool Compile(string fileName, TextReader input, Assembler assembler)
        {
            try
            {
                Initialize();
                Parse(fileName, input);
                Compile(assembler);
                return true;
            }
            catch (CompilerException e)
            {
                OnCompileError?.Invoke(e.Interval, e.Message);
            }

            return false;
        }
    }
}