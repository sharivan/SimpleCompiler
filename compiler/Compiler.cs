using System;
using System.Collections.Generic;
using System.IO;

using compiler.lexer;
using compiler.types;
using assembler;

namespace compiler
{
    public partial class Compiler
    {
        public static int GetAlignedSize(int sizeInBytes, int alignSize = sizeof(int))
        {
            int r = sizeInBytes % alignSize;
            return r != 0 ? sizeInBytes + alignSize - r : sizeInBytes;
        }

        public delegate void CompileError(SourceInterval interval, string message);

        private Lexer lexer;
        private readonly List<Label> labels;
        private readonly List<CompilationUnity> units;
        private readonly Dictionary<string, CompilationUnity> unityTable;
        private CompilationUnity program;
        private readonly List<(string, int)> externalFunctions;
        private readonly Dictionary<string, int> externalFunctionMap;

        internal CompilationUnity unity;
        internal CompilationUnity unitySystem;
        internal Function function;

        internal int globalVariableOffset;

        public event CompileError OnCompileError;

        public string UnityPath
        {
            get;
            set;
        }

        public Compiler(string unityPath = null)
        {
            unityPath ??= Directory.GetCurrentDirectory();

            UnityPath = unityPath;

            labels = new List<Label>();
            units = new List<CompilationUnity>();
            unityTable = new Dictionary<string, CompilationUnity>();
            externalFunctions = new List<(string, int)>();
            externalFunctionMap = new Dictionary<string, int>();

            unity = null;
            function = null;
        }

        public int AddExternalFunction(string name, int paramSize)
        {
            if (externalFunctionMap.ContainsKey(name))
                throw new Exception($"Função externa '{name}' já adicionada.");

            int index = externalFunctions.Count;
            externalFunctions.Add((name, paramSize));
            externalFunctionMap.Add(name, index);
            return index;
        }

        public int GetExternalFunctionIndex(string name) => externalFunctionMap.TryGetValue(name, out int index) ? index : -1;

        public int GetOrAddExternalFunction(string name, int paramSize)
        {
            int index = GetExternalFunctionIndex(name);
            if (index != -1)
                return index;

            index = externalFunctions.Count;
            externalFunctions.Add((name, paramSize));
            externalFunctionMap.Add(name, index);
            return index;
        }

        public string GetExternalFunctionName(int index) => externalFunctions[index].Item1;

        internal Label CreateLabel()
        {
            Label result = new();
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

        private void CompileCast(Context context, Assembler assembler, Assembler beforeCastAssembler, AbstractType fromType, AbstractType toType, bool isExplicit, SourceInterval interval, out LocalVariable tempVar)
        {
            tempVar = null;

            switch (fromType)
            {
                case PrimitiveType p:
                {
                    switch (toType)
                    {
                        case PrimitiveType tp:
                            switch (p.Primitive)
                            {
                                case Primitive.VOID:
                                    throw new CompilerException(interval, $"Conversão inválida de 'void' para '{tp}'.");

                                case Primitive.BOOL:
                                    if (!isExplicit && tp.Primitive != Primitive.BOOL)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    // TODO Implementar
                                    throw new CompilerException(interval, $"Operação não implementada para o tipo '{p}'.");

                                case Primitive.BYTE:
                                    if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive is < Primitive.BYTE or Primitive.CHAR)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    CompileInt32Conversion(assembler, tp);
                                    break;

                                case Primitive.CHAR:
                                    if (isExplicit ? tp.Primitive != Primitive.BOOL : tp.Primitive != Primitive.CHAR)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    CompileInt32Conversion(assembler, tp);
                                    break;

                                case Primitive.SHORT:
                                    if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.SHORT)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    CompileInt32Conversion(assembler, tp);
                                    break;

                                case Primitive.INT:
                                    if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.INT)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    CompileInt32Conversion(assembler, tp);
                                    break;

                                case Primitive.LONG:
                                    if (isExplicit ? tp.Primitive == Primitive.BOOL : tp.Primitive < Primitive.LONG)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

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
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

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
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

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

                        case PointerType:
                            switch (p.Primitive)
                            {
                                case Primitive.VOID:
                                case Primitive.BOOL:
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                    throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                case Primitive.INT:
                                    if (!isExplicit)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    assembler.EmitInt32ToPointer();
                                    break;

                                case Primitive.LONG:
                                    if (!isExplicit)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    assembler.EmitInt64ToPointer();
                                    assembler.EmitInt64ToInt32();
                                    break;

                                case Primitive.FLOAT:
                                case Primitive.DOUBLE:
                                    throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");
                            }

                            return;

                        default:
                            throw new CompilerException(interval, $"Tipo desconhecido: '{toType}'.");
                    }
                }

                case StructType s:
                    if (!s.Equals(toType))
                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                    return;

                case ArrayType a:
                    if (!a.Equals(toType))
                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                    return;

                case PointerType fptr:
                {
                    switch (toType)
                    {
                        case PrimitiveType tp:
                            switch (tp.Primitive)
                            {
                                case Primitive.INT:
                                    if (!isExplicit)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    assembler.EmitPointerToInt32();
                                    return;

                                case Primitive.LONG:
                                    if (!isExplicit)
                                        throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                                    assembler.EmitPointerToInt64();
                                    return;

                                default:
                                    throw new CompilerException(interval, "O tipo '" + fromType + "' não pode ser convertido para o tipo '" + toType + "'.");
                            }

                            throw new CompilerException(interval, $"Tipo desconhecido: '{toType}'.");

                        case PointerType tptr:
                        {
                            if (fptr.Type == null)
                                return;

                            AbstractType otherType = tptr.Type;
                            if (!isExplicit && !PrimitiveType.IsPrimitiveVoid(otherType) && fptr.Type != otherType)
                                throw new CompilerException(interval, $"O tipo '{fromType}' não pode ser convertido implicitamente para o tipo '{toType}'.");

                            return;
                        }

                        case StringType:
                        {
                            AbstractType type = fptr.Type;
                            if (type == null)
                                throw new CompilerException(interval, "Não é permitido atribuir um ponteiro nulo para uma string.");

                            if (!PrimitiveType.IsPrimitiveChar(type))
                                throw new CompilerException(interval, $"Não é permitido atribuir um pointeiro do tipo '{type}' para uma string.");

                            tempVar = context.AcquireTemporaryVariable(function, StringType.STRING, interval);
                            beforeCastAssembler.EmitLoadLocalHostAddress(tempVar.Offset);

                            Function f = unitySystem.FindFunction("NovoTexto2");
                            int index = GetOrAddExternalFunction(f.Name, f.ParameterSize);
                            assembler.EmitExternCall(index);

                            assembler.EmitLoadLocalPtr(tempVar.Offset);
                            return;
                        }

                        default:
                            throw new CompilerException(interval, $"Tipo desconhecido: '{toType}'.");
                    }
                }

                case StringType:
                {
                    switch (toType)
                    {
                        case PointerType tptr:
                        {
                            AbstractType otherType = tptr.Type;
                            if (PrimitiveType.IsPrimitiveChar(otherType))
                                return;

                            if (PrimitiveType.IsPrimitiveVoid(otherType))
                            {
                                if (!isExplicit)
                                    throw new CompilerException(interval, "Uma string não pode ser convertida implicitamente para um ponteiro do tipo 'void'.");

                                return;
                            }

                            throw new CompilerException(interval, $"Uma string não pode ser convertida para um ponteiro do tipo '{otherType}'.");
                        }

                        case StringType:
                            return;

                        default:
                            throw new CompilerException(interval, $"Uma string não pode ser convertida para o tipo '{toType}'.");
                    }
                }

                default:
                    throw new CompilerException(interval, $"Tipo desconhecido: '{fromType}'.");
            }
        }

        private void CompilePop(Assembler assembler, AbstractType type)
        {
            if (!PrimitiveType.IsPrimitiveVoid(type))
            {
                if (type is StringType)
                {
                    assembler.EmitLoadStackPtr();

                    Function f = unitySystem.FindFunction("DecrementaReferenciaString");
                    int index = GetOrAddExternalFunction(f.Name, f.ParameterSize);
                    assembler.EmitExternCall(index);
                }

                assembler.EmitSubSP(GetAlignedSize(type.Size));
            }
        }

        private void CompileArrayIndexer(Context context, Assembler assembler, Expression indexer)
        {
            AbstractType indexerType = CompileExpression(context, assembler, indexer, out _);

            if (indexerType is PrimitiveType pt)
            {
                switch (pt.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.CHAR:
                    case Primitive.LONG:
                    case Primitive.FLOAT:
                    case Primitive.DOUBLE:
                        throw new CompilerException(indexer.Interval, $"Tipo de indexador inválido: '{pt}'.");
                }
            }
        }

#pragma warning disable IDE0051 // Remover membros privados não utilizados
        private Variable CheckVariable(Context context, string name, SourceInterval interval)
#pragma warning restore IDE0051 // Remover membros privados não utilizados
        {
            Variable var = context.FindVariable(name);
            if (var == null)
            {
                var = unity.FindGlobalVariable(name);
                if (var == null)
                    throw new CompilerException(interval, $"Variável '{name}' não declarada.");
            }

            return var;
        }

        private void CompileStatement(Context context, Assembler assembler, Statement statement)
        {
            assembler.AddLine(statement.Interval.FileName, statement.Interval.FirstLine);

            switch (statement)
            {
                case ExpressionStatement e:
                {
                    Expression expression = e.Expression;
                    AbstractType type = CompileExpression(context, assembler, expression, out LocalVariable tempVar);
                    CompilePop(assembler, type);

                    tempVar?.Release();
                    break;
                }

                case DeclarationStatement decl:
                    CompileLocalVariableDeclaration(context, assembler, decl);
                    break;

                case ReturnStatement r:
                {
                    Expression expr = r.Expression;

                    if (expr != null && PrimitiveType.IsPrimitiveVoid(function.ReturnType))
                        throw new CompilerException(expr.Interval, "A função não possui tipo de retorno.");

                    if (expr == null && PrimitiveType.IsPrimitiveVoid(function.ReturnType))
                        throw new CompilerException(r.Interval, "Expressão de retorno esperada.");

                    if (expr != null)
                    {
                        Assembler beforeStoreAssembler = new();
                        Assembler beforeCastAssembler = new();
                        Assembler castAssembler = new();
                        Assembler storeAssembler = new();
                        LocalVariable castTempVar = null;
                        if (function.ReturnType is PrimitiveType or PointerType)
                        {
                            AbstractType returnType = CompileExpression(context, castAssembler, expr, out LocalVariable tempVar);
                            CompileCast(context, castAssembler, beforeCastAssembler, returnType, function.ReturnType, false, expr.Interval, out castTempVar);
                            CompileStoreLocal(storeAssembler, beforeStoreAssembler, function.ReturnType, function.ReturnOffset, expr.Interval);

                            tempVar?.Release();
                        }
                        else
                        {
                            storeAssembler.EmitLoadLocalResidentAddress(function.ReturnOffset);
                            AbstractType returnType = CompileExpression(context, castAssembler, expr, out LocalVariable tempVar);
                            CompileCast(context, castAssembler, beforeCastAssembler, returnType, function.ReturnType, false, expr.Interval, out castTempVar);
                            CompileStoreStack(storeAssembler, beforeStoreAssembler, function.ReturnType, expr.Interval);

                            tempVar?.Release();
                        }

                        assembler.Emit(beforeStoreAssembler);
                        assembler.Emit(beforeCastAssembler);
                        assembler.Emit(castAssembler);
                        assembler.Emit(storeAssembler);

                        castTempVar?.Release();
                    }

                    assembler.EmitJump(function.ReturnLabel);
                    break;
                }

                case BreakStatement b:
                {
                    Label breakLabel = context.FindNearestBreakLabel();
                    if (breakLabel == null)
                        throw new CompilerException(b.Interval, "Instrução 'quebra' deve estar dentro de um loop.");

                    assembler.EmitJump(breakLabel);
                    break;
                }

                case ReadStatement rd:
                {
                    foreach (Expression expr in rd)
                    {
                        AbstractType exprType = CompileAssignableExpression(context, assembler, expr, out _, out _, true);
                        switch (exprType)
                        {
                            case PrimitiveType p:
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

                                break;

                            case PointerType ptr when ptr.IsString:
                                assembler.EmitScanString();
                                break;

                            case ArrayType at when PrimitiveType.IsPrimitiveChar(at.Type):
                                assembler.EmitScanString();
                                break;

                            case StringType:
                                assembler.EmitScanDynamicString();
                                break;

                            default:
                                throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                        }
                    }

                    break;
                }

                case PrintStatement p:
                {
                    foreach (Expression expr in p)
                    {
                        AbstractType exprType = CompileExpression(context, assembler, expr, out LocalVariable tempVar, true);
                        switch (exprType)
                        {
                            case PrimitiveType pt:
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

                                break;

                            case PointerType ptr when ptr.IsString:
                                assembler.EmitPrintString();
                                break;

                            case ArrayType at when PrimitiveType.IsPrimitiveChar(at.Type):
                                assembler.EmitPrintString();
                                break;

                            case StringType:
                                assembler.EmitPrintString();
                                break;

                            default:
                                throw new CompilerException(expr.Interval, "Expressão de tipo primitivo ou string esperada.");
                        }

                        tempVar?.Release();
                    }

                    if (p.LineBreak)
                    {
                        int lineBreakOffset = unity.GetStringOffset("\n");
                        assembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + lineBreakOffset);
                        assembler.EmitPrintString();
                    }

                    break;
                }

                case IfStatement i:
                {
                    Expression expression = i.Expression;
                    Statement thenStatement = i.ThenStatement;
                    Statement elseStatement = i.ElseStatement;

                    AbstractType exprType = CompileExpression(context, assembler, expression, out LocalVariable tempVar);

                    if (!PrimitiveType.IsPrimitiveBool(exprType))
                        throw new CompilerException(expression.Interval, "Expressão do tipo bool experada.");

                    Label lblElse = CreateLabel();
                    assembler.EmitJumpIfFalse(lblElse);

                    tempVar?.Release();

                    CompileStatement(context, assembler, thenStatement);

                    Label lblEnd = CreateLabel();
                    assembler.EmitJump(lblEnd);

                    assembler.BindLabel(lblElse);
                    if (elseStatement != null)
                        CompileStatement(context, assembler, elseStatement);

                    assembler.BindLabel(lblEnd);
                    break;
                }

                case WhileStatement w:
                {
                    Expression expression = w.Expression;
                    Statement stm = w.Statement;

                    Label lblLoop = CreateLabel();
                    assembler.BindLabel(lblLoop);

                    AbstractType exprType = CompileExpression(context, assembler, expression, out LocalVariable tempVar);

                    Label lblEnd = CreateLabel();
                    context.PushBreakLabel(lblEnd);

                    assembler.EmitJumpIfFalse(lblEnd);

                    if (!PrimitiveType.IsPrimitiveBool(exprType))
                        throw new CompilerException(expression.Interval, "Expressão do tipo bool experada.");

                    tempVar?.Release();

                    CompileStatement(context, assembler, stm);

                    assembler.EmitJump(lblLoop);
                    assembler.BindLabel(lblEnd);

                    context.DropBreakLabel();
                    break;
                }

                case DoStatement d:
                {
                    Statement stm = d.Statement;
                    Expression expr = d.Expression;

                    Label lblLoop = CreateLabel();
                    assembler.BindLabel(lblLoop);

                    Label lblEnd = CreateLabel();
                    context.PushBreakLabel(lblEnd);

                    CompileStatement(context, assembler, stm);

                    AbstractType exprType = CompileExpression(context, assembler, expr, out LocalVariable tempVar);
                    if (!PrimitiveType.IsPrimitiveBool(exprType))
                        throw new CompilerException(expr.Interval, "Expressão do tipo bool experada.");

                    tempVar?.Release();

                    assembler.EmitJumpIfFalse(lblLoop);

                    assembler.BindLabel(lblEnd);
                    context.DropBreakLabel();
                    break;
                }

                case ForStatement f:
                {
                    Context forContext = new(function, f.Interval, context);

                    // inicializadores
                    foreach (InitializerStatement initializer in f.Initializers)
                        CompileStatement(forContext, assembler, initializer);

                    Label lblLoop = CreateLabel();
                    assembler.BindLabel(lblLoop);

                    // expressão de controle
                    Expression expression = f.Expression;
                    if (expression != null)
                    {
                        AbstractType expressionType = CompileExpression(forContext, assembler, expression, out _);
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
                    foreach (Expression updater in f.Updaters)
                    {
                        AbstractType updaterType = CompileExpression(forContext, assembler, updater, out LocalVariable tempVar);
                        CompilePop(assembler, updaterType);

                        tempVar?.Release();
                    }

                    assembler.EmitJump(lblLoop);
                    assembler.BindLabel(lblEnd);

                    forContext.DropBreakLabel();
                    forContext.Release(assembler);
                    break;
                }

                case BlockStatement bl:
                {
                    Context blockContext = new(function, bl.Interval, context);
                    foreach (Statement stm in bl)
                        CompileStatement(blockContext, assembler, stm);

                    blockContext.Release(assembler);
                    break;
                }

                default:
                    throw new CompilerException(statement.Interval, $"Tipo desconhecido de statement: {statement}");
            }
        }

        private void CompileLocalVariableDeclaration(Context context, Assembler assembler, DeclarationStatement statement)
        {
            statement.Resolve();
            AbstractType type = statement.Type;
            foreach (var (name, initializer) in statement)
            {
                Variable var = context.DeclareLocalVariable(function, name, type, statement.Interval);
                if (var == null)
                    throw new CompilerException(statement.Interval, $"Variável local '{name}' já declarada.");

                if (initializer != null)
                {
                    Assembler beforeStoreAssembler = new();

                    bool useVar = false;
                    if (var is GlobalVariable)
                        beforeStoreAssembler.EmitLoadConst(var.Offset);
                    else
                    {
                        useVar = var.Type is PrimitiveType or PointerType;
                        if (!useVar)
                            beforeStoreAssembler.EmitLoadLocalResidentAddress(var.Offset);
                    }

                    Assembler beforeCastAssembler = new();
                    Assembler castAssembler = new();
                    AbstractType initializerType = CompileExpression(context, castAssembler, initializer, out LocalVariable tempVar);
                    CompileCast(context, castAssembler, beforeCastAssembler, initializerType, type, false, initializer.Interval, out LocalVariable castTempVar);

                    tempVar?.Release();

                    Assembler storeAssembler = new();
                    if (useVar)
                        CompileStore(storeAssembler, beforeStoreAssembler, var, initializer.Interval);
                    else
                        CompileStoreStack(storeAssembler, beforeStoreAssembler, type, initializer.Interval);

                    assembler.Emit(beforeStoreAssembler);
                    assembler.Emit(beforeCastAssembler);
                    assembler.Emit(castAssembler);
                    assembler.Emit(storeAssembler);

                    castTempVar?.Release();
                }
            }
        }

        internal void CompileFunction(Assembler assembler)
        {
            assembler.AddLine(function.Interval.FileName, function.Interval.FirstLine);

            if (function.IsExtern)
                return;

            Context context = new(function, function.Interval);
            Assembler tempAssembler = new();

            CompileStatement(context, tempAssembler, function.Block);
            context.Release(tempAssembler);

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

            string path = UnityPath + '\\' + name + ".sl";
            if (!File.Exists(path))
                return null;

            CompilationUnity unity = ParseCompilationUnityFromFile(path, false);
            units.Add(unity);
            unityTable.Add(name, unity);
            return unity;
        }

        public bool CompileFromSource(int sourceID, string source, Assembler assembler)
        {
            using var input = new StringReader(source);
            return Compile("#" + sourceID, input, assembler);
        }

        public bool CompileFromSources(Tuple<int, string>[] sources, Assembler assembler)
        {
            try
            {
                Initialize();

                foreach (var source in sources)
                {
                    int sourceID = source.Item1;
                    string sourceText = source.Item2;
                    string fileName = "#" + sourceID;

                    using var input = new StringReader(sourceText);
                    Parse(fileName, input);
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
            using var input = File.OpenText(fileName);
            return Compile(fileName, input, assembler);
        }

        public bool CompileFromFiles(string[] fileNames, Assembler assembler)
        {
            try
            {
                Initialize();

                foreach (string fileName in fileNames)
                {
                    using var input = File.OpenText(fileName);
                    Parse(fileName, input);
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

        public bool CompileFromReader(int sourceID, TextReader input, Assembler assembler) => Compile("#" + sourceID, input, assembler);

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

            Assembler tempAssembler = new();
            foreach (CompilationUnity u in units)
                u.Compile(tempAssembler);

            program.Compile(tempAssembler);

            foreach (CompilationUnity u in units)
                if (u.EntryPoint != null)
                    assembler.EmitCall(u.EntryPoint.EntryLabel);

            if (program.EntryPoint != null)
                assembler.EmitCall(program.EntryPoint.EntryLabel);

            foreach (CompilationUnity u in units)
                u.EmitStringRelease(assembler);

            program.EmitStringRelease(assembler);
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