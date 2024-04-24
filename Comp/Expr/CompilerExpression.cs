using Asm;

using Comp.Types;

namespace Comp;

public partial class Compiler
{
    private AbstractType CompileAssignableFieldAccessorExpression(Context context, Assembler assembler, FieldAccessorExpression expression, out bool isPointerDeference, bool loadHostAddress = false)
    {
        AbstractType result;
        isPointerDeference = false;

        var operand = expression.Operand;
        string fieldName = expression.Field;

        Assembler operandAssembler = new();
        var operandType = CompileAssignableExpression(context, operandAssembler, operand, out _, out _);

        switch (operandType)
        {
            case StructType s:
            {
                var field = s.FindField(fieldName) ?? throw new CompilerException(expression.Interval, $"Campo '{fieldName}' não encontrado na estrutura: '{s.Name}'.");

                assembler.Emit(operandAssembler);

                assembler.EmitLoadConst(field.Offset);
                assembler.EmitAdd();

                if (loadHostAddress)
                    assembler.EmitResidentToHostAddress();

                result = field.Type;
                break;
            }

            case PointerType ptr:
            {
                if (ptr.Type == null || ptr.Type is not StructType s2)
                    throw new CompilerException(operand.Interval, "Pointeiro de estrutura esperado.");

                var field = s2.FindField(fieldName) ?? throw new CompilerException(expression.Interval, $"Campo '{fieldName}' não encontrado na estrutura: '{s2.Name}'.");

                assembler.Emit(operandAssembler);

                assembler.EmitLoadStackPtr();

                assembler.EmitLoadConst(field.Offset);
                assembler.EmitPtrAdd();

                isPointerDeference = true;
                result = field.Type;
                break;
            }

            case StringType:
            {
                if (fieldName != "tamanho")
                    throw new CompilerException(expression.Interval, $"Campo '{fieldName}' não encontrado no texto.");

                assembler.EmitAddSP(PrimitiveType.INT.Size);
                assembler.Emit(operandAssembler);

                assembler.EmitLoadStackPtr();

                var func = unitySystem.FindFunction("ComprimentoTexto");
                int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                assembler.EmitExternCall(index);

                result = PrimitiveType.INT;
                break;
            }

            default:
                throw new CompilerException(operand.Interval, $"Acesso de membros em um tipo que não é estrutura, ponteiro ou texto: '{operandType}'.");
        }

        return result;
    }

    public AbstractType CompileAssignableArrayAccessorExpression(Context context, Assembler assembler, ArrayAccessorExpression a, out bool isPointerDeference, bool loadHostAddress = false)
    {
        Expression indexer;
        var operand = a.Operand;
        var operandType = CompileAssignableExpression(context, assembler, operand, out _, out _);

        if (operandType is ArrayType at)
        {
            int rank = at.Rank;

            if (a.IndexerCount == 0)
                throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o array.");

            if (a.IndexerCount != rank)
                throw new CompilerException(operand.Interval, "Número de inídices fornecidos é diferente da dimensão do array.");

            indexer = a[0];
            CompileArrayIndexer(context, assembler, indexer);

            for (int i = 1; i < rank; i++)
            {
                indexer = a[i];
                assembler.EmitLoadConst(at[i]);
                assembler.EmitMul();
                CompileArrayIndexer(context, assembler, indexer);
                assembler.EmitAdd();
            }

            assembler.EmitLoadConst(at.Type.Size);
            assembler.EmitMul();
            assembler.EmitAdd();

            if (loadHostAddress)
                assembler.EmitResidentToHostAddress();

            isPointerDeference = false;
            return at.Type;
        }

        AbstractType elementType;
        switch (operandType)
        {
            case PointerType ptr:
                if (ptr.Type == null)
                    throw new CompilerException(operand.Interval, "Não é possível realizar esta operação em um ponteiro do tipo void.");

                elementType = ptr.Type;
                break;

            case StringType:
                elementType = PrimitiveType.CHAR;
                break;

            default:
                throw new CompilerException(operand.Interval, $"O tipo '{operandType}' não é um array, um ponteiro ou uma string.");
        }

        if (a.IndexerCount == 0)
            throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o array.");

        if (a.IndexerCount != 1)
            throw new CompilerException(operand.Interval, "Deve-se fornecer somente um índice para o array.");

        assembler.EmitLoadStackPtr();

        indexer = a[0];
        CompileArrayIndexer(context, assembler, indexer);
        assembler.EmitLoadConst(elementType.Size);
        assembler.EmitMul();
        assembler.EmitPtrAdd();

        isPointerDeference = true;
        return elementType;
    }

    private AbstractType CompileAssignablePrimaryExpression(Context context, Assembler assembler, PrimaryExpression p, out Variable storeVar, out bool isPointerDeference, bool loadHostAddress = false)
    {
        if (p.PrimaryType != PrimaryType.IDENTIFIER)
            throw new CompilerException(p.Interval, "Tipo de expressão não atribuível.");

        var id = (IdentifierExpression) p;
        string name = id.Name;

        isPointerDeference = false;
        var var = context.FindVariable(name);
        if (var == null)
        {
            var = unity.FindGlobalVariable(name);
            if (var == null)
                throw new CompilerException(id.Interval, $"Identificador'{name}' não declarado.");

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
                isPointerDeference = true;
                assembler.EmitLoadLocalPtr(offset);
            }
            else if (loadHostAddress)
            {
                assembler.EmitLoadLocalHostAddress(offset);
            }
            else
            {
                assembler.EmitLoadLocalResidentAddress(offset);
            }
        }

        storeVar = !isPointerDeference && var.Type is PrimitiveType or PointerType or StringType ? var : null;
        return var.Type;
    }

    private AbstractType CompileAssignableExpression(Context context, Assembler assembler, Expression expression, out Variable storeVar, out bool isPointerDeference, bool loadHostAddress = false)
    {
        storeVar = null;
        isPointerDeference = false;

        return expression switch
        {
            UnaryExpression u => CompileAssignableUnaryExpression(context, assembler, u, out isPointerDeference),
            FieldAccessorExpression f => CompileAssignableFieldAccessorExpression(context, assembler, f, out isPointerDeference, loadHostAddress),
            ArrayAccessorExpression a => CompileAssignableArrayAccessorExpression(context, assembler, a, out isPointerDeference, loadHostAddress),
            PrimaryExpression p => CompileAssignablePrimaryExpression(context, assembler, p, out storeVar, out isPointerDeference, loadHostAddress),
            _ => throw new CompilerException(expression.Interval, "Tipo de expressão não atribuível."),
        };
    }

    private AbstractType CompileFieldAccessorExpression(Context context, Assembler assembler, FieldAccessorExpression expression, out Variable tempVar, bool getArrayAddress = false)
    {
        tempVar = null;

        var operand = expression.Operand;
        string fieldName = expression.Field;

        Assembler operandAssembler = new();
        var operandType = CompileAssignableExpression(context, operandAssembler, operand, out _, out _);

        switch (operandType)
        {
            case StructType s:
            {
                var field = s.FindField(fieldName) ?? throw new CompilerException(expression.Interval, $"Campo '{fieldName}' não encontrado na estrutura: '{s.Name}'.");

                assembler.Emit(operandAssembler);

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

            case PointerType ptr:
            {
                if (ptr.Type == null)
                    throw new CompilerException(operand.Interval, "Não é possível realizar essa operação em um ponteiro do tipo void.");

                if (ptr.Type is not StructType s2)
                    throw new CompilerException(operand.Interval, "Pointeiro de estrutura esperado.");

                var field = s2.FindField(fieldName) ?? throw new CompilerException(expression.Interval, $"Campo '{fieldName}' não encontrado na estrutura: '{s2.Name}'.");

                assembler.Emit(operandAssembler);

                assembler.EmitLoadStackPtr();

                assembler.EmitLoadConst(field.Offset);
                assembler.EmitPtrAdd();

                if (getArrayAddress && field.Type is ArrayType at2)
                    return new PointerType(at2.Type, true);

                CompileLoadPointer(assembler, field.Type, expression.Interval);
                return field.Type;
            }

            case StringType:
            {
                if (fieldName != "tamanho")
                    throw new CompilerException(expression.Interval, $"Campo '{fieldName}' não encontrado no texto.");

                assembler.EmitAddSP(PrimitiveType.INT.Size);
                assembler.Emit(operandAssembler);

                assembler.EmitLoadStackPtr();

                var func = unitySystem.FindFunction("ComprimentoTexto");
                int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                assembler.EmitExternCall(index);

                return PrimitiveType.INT;
            }
        }

        throw new CompilerException(operand.Interval, $"Acesso de membros em um tipo que não é estrutura, ponteiro ou texto: '{operandType}'.");
    }

    private AbstractType CompileArrayAccessorExpression(Context context, Assembler assembler, ArrayAccessorExpression expression, out Variable tempVar, bool getArrayAddress = false)
    {
        tempVar = null;

        Expression indexer;
        var operand = expression.Operand;
        var operandType = CompileAssignableExpression(context, assembler, operand, out _, out _);
        AbstractType result;

        if (operandType is ArrayType at)
        {
            int rank = at.Rank;

            if (expression.IndexerCount == 0)
                throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o array.");

            if (expression.IndexerCount != rank)
                throw new CompilerException(operand.Interval, "Número de inídices fornecidos é diferente da dimensão do array.");

            indexer = expression[0];
            CompileArrayIndexer(context, assembler, indexer);

            for (int i = 1; i < rank; i++)
            {
                indexer = expression[i];
                assembler.EmitLoadConst(at[i]);
                assembler.EmitMul();
                CompileArrayIndexer(context, assembler, indexer);
                assembler.EmitAdd();
            }

            assembler.EmitLoadConst(at.Type.Size);
            assembler.EmitMul();
            assembler.EmitAdd();

            if (getArrayAddress && at.Type is ArrayType at2)
            {
                assembler.EmitResidentToHostAddress();
                result = new PointerType(at2.Type, true);
            }
            else
            {
                CompileLoadStack(assembler, at.Type, expression.Interval);
                result = at.Type;
            }
        }
        else
        {
            AbstractType elementType;
            if (operandType is PointerType ptr)
            {
                if (ptr.Type == null)
                    throw new CompilerException(operand.Interval, "Não é possível realizar essa operação em um ponteiro do tipo void.");

                elementType = ptr.Type;
            }
            else
            {
                elementType = operandType is StringType
                ? (AbstractType) PrimitiveType.CHAR
                : throw new CompilerException(operand.Interval, $"O tipo '{operandType}' não é um array, um ponteiro ou uma string.");
            }

            if (expression.IndexerCount == 0)
                throw new CompilerException(operand.Interval, "Não foi fornecido nenhum índice para o ponteiro.");

            if (expression.IndexerCount != 1)
                throw new CompilerException(operand.Interval, "Deve-se fornecer somente um índice único para o ponteiro.");

            assembler.EmitLoadStackPtr();

            indexer = expression[0];
            CompileArrayIndexer(context, assembler, indexer);
            assembler.EmitLoadConst(elementType.Size);
            assembler.EmitMul();
            assembler.EmitPtrAdd();

            if (getArrayAddress && elementType is ArrayType at3)
            {
                result = new PointerType(at3.Type, true);
            }
            else
            {
                CompileLoadPointer(assembler, elementType, expression.Interval);
                result = elementType;
            }
        }

        return result;
    }

    private AbstractType CompilePrimaryExpression(Context context, Assembler assembler, PrimaryExpression expression, out Variable tempVar, bool getArrayAddress = false)
    {
        tempVar = null;
        AbstractType result;

        switch (expression)
        {
            case BoolLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.BOOL;
                break;

            case ByteLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.BYTE;
                break;

            case CharLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.CHAR;
                break;

            case ShortLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.SHORT;
                break;

            case IntLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.INT;
                break;

            case LongLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.LONG;
                break;

            case FloatLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.FLOAT;
                break;

            case DoubleLiteralExpression l:
                assembler.EmitLoadConst(l.Value);
                result = PrimitiveType.DOUBLE;
                break;

            case StringLiteralExpression l:
            {
                int offset = unity.GetStringOffset(l.Value);
                assembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + offset);
                result = PointerType.STRING;
                break;
            }

            case NullLiteralExpression:
                assembler.EmitLoadConst((byte) 0);
                assembler.EmitInt32ToPointer();
                result = PointerType.NULL;
                break;

            case IdentifierExpression id:
            {
                string name = id.Name;

                var var = context.FindVariable(name);
                if (var == null)
                {
                    var = unity.FindGlobalVariable(name);
                    if (var == null)
                        throw new CompilerException(id.Interval, $"Identificador'{name}' não declarado.");
                }

                if (getArrayAddress && var.Type is ArrayType at2)
                {
                    if (var is GlobalVariable)
                        assembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + var.Offset);
                    else
                        assembler.EmitLoadLocalHostAddress(var.Offset);

                    result = new PointerType(at2.Type, true);
                }
                else
                {
                    CompileLoad(assembler, var, id.Interval);
                    result = var.Type;
                }

                break;
            }

            default:
                throw new CompilerException(expression.Interval, $"Tipo de expressão primária desconhecido: '{expression}'");
        }

        return result;
    }

    private AbstractType CompileCallExpression(Context context, Assembler assembler, CallExpression c, out Variable tempVar)
    {
        tempVar = null;

        var operand = c.Operand;
        if (operand is not IdentifierExpression id)
            throw new CompilerException(c.Interval, "Expressão de nome de função inválida.");

        string functionName = id.Name;
        var func = unity.FindFunction(functionName) ?? throw new CompilerException(id.Interval, $"Função '{functionName}' não declarada.");

        var returnType = func.ReturnType;
        if (!PrimitiveType.IsPrimitiveVoid(returnType))
        {
            if (returnType is StringType)
            {
                tempVar = context.DeclareTemporaryVariable(returnType, c.Interval);
                assembler.EmitLoadLocalHostAddress(tempVar.Offset);
            }

            assembler.EmitAddSP(GetAlignedSize(returnType.Size));
        }

        if (func.ParamCount != c.ParameterCount)
            throw new CompilerException(id.Interval, "A quantidade de parâmetros fornecido é diferente da quantidade total de parâmetros esperada.");

        var castTempVars = new Variable[func.ParamCount];
        for (int j = 0; j < func.ParamCount; j++)
        {
            var parameter = func[j];
            var expressionParameter = c[j];

            if (parameter.ByRef)
            {
                var paramType = CompileAssignableExpression(context, assembler, expressionParameter, out _, out _, true);
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
                Assembler castAssembler = new();
                var paramType = CompileExpression(context, castAssembler, expressionParameter, out var paramTempVar, parameter.Type is PointerType);
                CompileCast(context, castAssembler, assembler, paramType, parameter.Type, false, expressionParameter.Interval, out castTempVars[j]);
                assembler.Emit(castAssembler);

                paramTempVar?.Release();
            }
        }

        if (func.IsExtern)
        {
            int index = GetOrAddExternalFunction(functionName, func.ParameterSize);
            assembler.EmitExternCall(index);
        }
        else
        {
            assembler.EmitCall(func.EntryLabel);
        }

        foreach (var castTempVar in castTempVars)
            castTempVar?.Release();

        if (tempVar != null)
        {
            var f = unitySystem.FindFunction("AtribuiTexto");
            int index = GetOrAddExternalFunction(f.Name, f.ParameterSize);
            assembler.EmitExternCall(index);

            assembler.EmitLoadLocalPtr(tempVar.Offset);
        }

        return func.ReturnType;
    }

    private AbstractType CompileCastExpression(Context context, Assembler assembler, CastExpression cs, out Variable tempVar)
    {
        var operand = cs.Operand;
        var type = cs.Type;

        Assembler castAssembler = new();
        var operandType = CompileExpression(context, castAssembler, operand, out var operandTempVar);
        CompileCast(context, castAssembler, assembler, operandType, type, true, operand.Interval, out tempVar);
        assembler.Emit(castAssembler);

        operandTempVar?.Release();

        return type;
    }

    private AbstractType CompileExpression(Context context, Assembler assembler, Expression expression, out Variable tempVar, bool getArrayAddress = false)
    {
        switch (expression)
        {
            case UnaryExpression u:
                tempVar = null;
                return CompileUnaryExpression(context, assembler, u);

            case BinaryExpression b:
                return CompileBinaryExpression(context, assembler, b, out tempVar);

            case FieldAccessorExpression f:
                return CompileFieldAccessorExpression(context, assembler, f, out tempVar, getArrayAddress);

            case ArrayAccessorExpression a:
                return CompileArrayAccessorExpression(context, assembler, a, out tempVar, getArrayAddress);

            case PrimaryExpression p:
                return CompilePrimaryExpression(context, assembler, p, out tempVar, getArrayAddress);

            case CallExpression c:
                return CompileCallExpression(context, assembler, c, out tempVar);

            case CastExpression cs:
                return CompileCastExpression(context, assembler, cs, out tempVar);
        }

        throw new CompilerException(expression.Interval, $"Tipo desconhecido de expressão: {expression}");
    }
}