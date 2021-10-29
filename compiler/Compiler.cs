using System;
using System.Collections.Generic;
using System.Text;

namespace compiler
{
    public class Compiler
    {
        public static int GetSizeInDWords(int sizeInBytes)
        {
            int r = sizeInBytes % 4;
            int d = sizeInBytes / 4;
            if (r != 0)
                return d + 1;

            return d;
        }

        private Lexer lexer;
        private List<GlobalVariable> globals;
        private List<StructType> structs;
        private List<Function> functions;
        private int globalVariableOffset;
        private Function entryPoint;

        public Compiler()
        {
            lexer = new Lexer();
            globals = new List<GlobalVariable>();
            structs = new List<StructType>();
            functions = new List<Function>();

            globalVariableOffset = 0;
            entryPoint = null;
        }

        public GlobalVariable FindGlobalVariable(string name)
        {
            for (int i = 0; i < globals.Count; i++)
            {
                GlobalVariable var = globals[i];
                if (var.Name == name)
                    return var;
            }

            return null;
        }

        public GlobalVariable DeclareGlobalVariable(string name, AbstractType type)
        {
            GlobalVariable result = FindGlobalVariable(name);
            if (result != null)
                return null;

            result = new GlobalVariable(name, type, globalVariableOffset);
            globalVariableOffset += GetSizeInDWords(type.Size());
            globals.Add(result);
            return result;
        }

        public StructType FindStruct(string name)
        {
            for (int i = 0; i < structs.Count; i++)
            {
                StructType st = structs[i];
                if (st.Name == name)
                    return st;
            }

            return null;
        }

        public StructType DeclareStruct(string name)
        {
            StructType result = FindStruct(name);
            if (result != null)
                return null;

            result = new StructType(name);
            structs.Add(result);
            return result;
        }

        public Function FindFunction(string name)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                Function f = functions[i];
                if (f.Name == name)
                    return f;
            }

            return null;
        }

        public Function DeclareFunction(string name)
        {
            Function result = FindFunction(name);
            if (result != null)
                return null;

            result = new Function(name);
            functions.Add(result);
            return result;
        }

        private AbstractType ParsePrimaryExpression(Context context, Assembler assembler)
        {
            Token token = lexer.NextToken();
            if (token == null)
                throw new ParserException("End of expression reached but primary expression expected.");

            if (token is NumericLiteral)
            {
                NumericLiteral number = (NumericLiteral)token;
                if (number is ByteLiteral)
                {
                    assembler.EmitLoadConst(number.AsByte());
                    return PrimitiveType.BYTE;
                }

                if (number is ShortLiteral)
                {
                    assembler.EmitLoadConst(number.AsShort());
                    return PrimitiveType.SHORT;
                }

                if (number is IntLiteral)
                {
                    assembler.EmitLoadConst(number.AsInt());
                    return PrimitiveType.INT;
                }

                if (number is LongLiteral)
                {
                    assembler.EmitLoadConst(number.AsLong());
                    return PrimitiveType.LONG;
                }

                if (number is FloatLiteral)
                {
                    assembler.EmitLoadConst(number.AsFloat());
                    return PrimitiveType.FLOAT;
                }

                if (number is DoubleLiteral)
                {
                    assembler.EmitLoadConst(number.AsDouble());
                    return PrimitiveType.DOUBLE;
                }

                throw new ParserException("Unknow numeric literal token: " + number);
            }

            if (token is Identifier)
            {
                Identifier id = (Identifier)token;

                if (lexer.NextSymbol("(", false) != null)
                {
                    Function f = FindFunction(id.Name);
                    if (f == null)
                        throw new ParserException("Undeclared function '" + id.Name + "'.");

                    AbstractType returnType = f.ReturnType;
                    if (returnType != null)
                    {
                        if (returnType is PrimitiveType)
                        {
                            PrimitiveType p = (PrimitiveType)returnType;
                            switch (p.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitLoadConst(0);
                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(0L);
                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(0F);
                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(0.0);
                                    break;
                            }
                        }
                        else
                            // TODO Implementar
                            throw new ParserException("???");
                    }

                    if (lexer.NextSymbol(")", false) != null)
                    {
                        assembler.EmitCall();
                        return f.ReturnType;
                    }

                    do
                    {
                        AbstractType paramType = ParseExpression(context, assembler);
                    }
                    while (lexer.NextSymbol(",", false) != null);

                    lexer.NextSymbol(")");

                    assembler.EmitLabel(f.EntryLabel);
                    assembler.EmitCall();
                    return f.ReturnType;
                }

                Variable var = context.FindVariable(id.Name);
                if (var == null)
                {
                    var = FindGlobalVariable(id.Name);
                    if (var == null)
                        throw new ParserException("Undeclared identifier '" + id.Name + "'");

                    // variável local ou parâmetro
                    int offset = var.Offset;
                    assembler.EmitLoadConst(offset);
                }
                else
                {
                    // variável local ou parâmetro
                    int offset = var.Offset;
                    assembler.EmitLoadBP();
                    assembler.EmitLoadConst(offset);
                    assembler.EmitAdd();
                }

                AbstractType type = var.Type;

                if (type is PrimitiveType)
                {
                    PrimitiveType p = (PrimitiveType)type;
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                        case Primitive.INT:
                        case Primitive.FLOAT:
                            assembler.EmitLoadStack();
                            break;

                        case Primitive.LONG:
                        case Primitive.DOUBLE:
                            assembler.EmitLoadStack64();
                            break;
                    }
                }
                else
                    // TODO Implementar
                    throw new ParserException("???");

                return type;
            }

            if (token is Symbol)
            {
                Symbol symbol = (Symbol)token;
                if (symbol.Value != "(")
                    throw new ParserException("'(' expected but '" + symbol.Value + "' found");

                AbstractType type = ParseAdditiveExpression(context, assembler);

                lexer.NextSymbol(")");

                return type;
            }

            throw new ParserException("Unexpected token: " + token);
        }

        private AbstractType ParseUnaryExpression(Context context, Assembler assembler)
        {
            bool neg = false;
            Symbol symbol = lexer.NextSymbol(false);
            if (symbol != null)
            {
                switch (symbol.Value)
                {
                    case "+":
                        break;

                    case "-":
                        neg = true;
                        break;

                    default:
                        lexer.PreviusToken();
                        break;
                }
            }

            AbstractType type = ParsePrimaryExpression(context, assembler);

            if (neg)
            {
                PrimitiveType p = (PrimitiveType)type;
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.CHAR:
                        throw new ParserException("This operation is not allowed with a non numeric operand.");

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
            }

            return type;
        }

        private void AssembleImplicitCast(Assembler assembler, PrimitiveType fromType, PrimitiveType toType)
        {
            if (fromType.Primitive == toType.Primitive)
                return;

            if (PrimitiveType.IsUpTo32BitsInt(fromType))
            {
                if (PrimitiveType.Is64BitsInt(toType))
                    assembler.EmitInt32ToInt64();
                else if (PrimitiveType.Is32BitsFloat(toType))
                    assembler.EmitInt32ToFloat32();
                else if (PrimitiveType.Is64BitsFloat(toType))
                    assembler.EmitInt32ToFloat64();
                else
                    throw new ParserException("assert fail");
            }
            else if (PrimitiveType.Is64BitsInt(fromType))
            {
                if (PrimitiveType.Is32BitsFloat(toType))
                {
                    assembler.EmitInt64ToFloat64();
                    assembler.EmitFloat64ToFloat32();
                }
                else if (PrimitiveType.Is64BitsFloat(toType))
                    assembler.EmitInt64ToFloat64();
                else
                    throw new ParserException("assert fail");
            }
            else if (PrimitiveType.Is32BitsFloat(fromType))
            {
                if (PrimitiveType.Is64BitsFloat(toType))
                    assembler.EmitFloat32ToFloat64();
                else
                    throw new ParserException("assert fail");
            }
            else
                throw new ParserException("assert fail");
        }

        private PrimitiveType AssembleOperandsImplicitCast(Assembler left, Assembler right, PrimitiveType pt1, PrimitiveType pt2)
        {
            if (pt1.Primitive == pt2.Primitive)
            {
                left.Emit(right);
                return pt1;
            }

            PrimitiveType result;
            if (pt1.Size() < pt2.Size())
            {
                if (PrimitiveType.IsPrimitiveFloat(pt1) && PrimitiveType.IsPrimitiveInteger(pt2))
                {
                    AssembleImplicitCast(right, pt2, pt1);
                    result = pt1;
                }
                else
                {
                    AssembleImplicitCast(left, pt1, pt2);
                    result = pt2;
                }
            }
            else if (pt1.Size() > pt2.Size())
            {
                if (PrimitiveType.IsPrimitiveFloat(pt2) && PrimitiveType.IsPrimitiveInteger(pt1))
                {
                    AssembleImplicitCast(left, pt1, pt2);
                    result = pt2;
                }
                else
                {
                    AssembleImplicitCast(right, pt2, pt1);
                    result = pt1;
                }
            }
            else
            {
                if (PrimitiveType.IsPrimitiveFloat(pt1) && PrimitiveType.IsPrimitiveInteger(pt2))
                {
                    AssembleImplicitCast(right, pt2, pt1);
                    result = pt1;
                }
                else if (PrimitiveType.IsPrimitiveFloat(pt2) && PrimitiveType.IsPrimitiveInteger(pt1))
                {
                    AssembleImplicitCast(left, pt1, pt2);
                    result = pt2;
                }
                else
                {
                    result = pt1;
                }
            }

            left.Emit(right);
            return result;

        }

        private AbstractType ParseMultiplicativeExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseUnaryExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "*":
                        break;

                    case "/":
                        break;

                    case "%":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseUnaryExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Multiplicative operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                switch (symbol.Value)
                {
                    case "*":
                        switch (result.Primitive)
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
                        }

                        break;

                    case "/":
                        switch (result.Primitive)
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
                        }

                        break;

                    case "%":
                        switch (result.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitMod();
                                break;

                            case Primitive.LONG:
                                assembler.EmitMod64();
                                break;

                            case Primitive.FLOAT:
                            case Primitive.DOUBLE:
                                throw new ParserException("Operation not allowed with non integer operands.");
                        }

                        break;
                }

                type1 = result;
            }
        }

        private AbstractType ParseAdditiveExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseMultiplicativeExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "+":
                        break;

                    case "-":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseMultiplicativeExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Additive operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                switch (symbol.Value)
                {
                    case "+":
                        switch (result.Primitive)
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

                        break;

                    case "-":
                        switch (result.Primitive)
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

                        break;
                }

                type1 = result;
            }
        }

        private AbstractType ParseShiftExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseAdditiveExpression(context, assembler);

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return type1;

            switch (symbol.Value)
            {
                case "<<":
                    break;

                case ">>":
                    break;

                case ">>>":
                    break;

                default:
                    lexer.PreviusToken();
                    return type1;
            }

            Assembler tempAssembler = new Assembler();
            AbstractType type2 = ParseAdditiveExpression(context, tempAssembler);

            if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                throw new ParserException("Shift operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

            PrimitiveType pt1 = (PrimitiveType)type1;
            PrimitiveType pt2 = (PrimitiveType)type2;

            PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

            switch (symbol.Value)
            {
                case "<<":
                    switch (result.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitShl();
                            break;

                        case Primitive.LONG:
                            assembler.EmitShl64();
                            break;

                        case Primitive.FLOAT:
                        case Primitive.DOUBLE:
                            break;
                    }

                    break;

                case ">>":
                    switch (result.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitShr();
                            break;

                        case Primitive.LONG:
                            assembler.EmitShr64();
                            break;

                        case Primitive.FLOAT:
                        case Primitive.DOUBLE:
                            break;
                    }

                    break;

                case ">>>":
                    switch (result.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitUShr();
                            break;

                        case Primitive.LONG:
                            assembler.EmitUShr64();
                            break;

                        case Primitive.FLOAT:
                        case Primitive.DOUBLE:
                            break;
                    }

                    break;
            }

            return result;
        }

        private AbstractType ParseInequalityExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseShiftExpression(context, assembler);

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return type1;

            switch (symbol.Value)
            {
                case ">":
                    break;

                case "<":
                    break;

                case ">=":
                    break;

                case "<=":
                    break;

                default:
                    lexer.PreviusToken();
                    return type1;
            }

            Assembler tempAssembler = new Assembler();
            AbstractType type2 = ParseShiftExpression(context, tempAssembler);

            if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                throw new ParserException("Inequality operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

            PrimitiveType pt1 = (PrimitiveType)type1;
            PrimitiveType pt2 = (PrimitiveType)type2;

            PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

            switch (symbol.Value)
            {
                case ">":
                    switch (result.Primitive)
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

                    break;

                case "<":
                    switch (result.Primitive)
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

                    break;

                case ">=":
                    switch (result.Primitive)
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

                    break;

                case "<=":
                    switch (result.Primitive)
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

                    break;
            }

            return PrimitiveType.BOOL;
        }

        private AbstractType ParseEqualityExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseInequalityExpression(context, assembler);

            Symbol symbol = lexer.NextSymbol(false);
            if (symbol == null)
                return type1;

            switch (symbol.Value)
            {
                case "==":
                    break;

                case "!=":
                    break;

                default:
                    lexer.PreviusToken();
                    return type1;
            }

            Assembler tempAssembler = new Assembler();
            AbstractType type2 = ParseInequalityExpression(context, tempAssembler);

            if ((type1 is PrimitiveType) && (type2 is PrimitiveType))
            {
                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                switch (symbol.Value)
                {
                    case "==":
                        switch (result.Primitive)
                        {
                            case Primitive.BOOL:
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

                        break;

                    case "!=":
                        switch (result.Primitive)
                        {
                            case Primitive.BOOL:
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

                        break;
                }
            }
            else
            {
                // TODO Implementar
            }

            return PrimitiveType.BOOL;
        }

        private AbstractType ParseBitwiseAndExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseEqualityExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "&":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseEqualityExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Bitwise & operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                switch (result.Primitive)
                {
                    case Primitive.BOOL:
                        throw new ParserException("Bitwise & operation is not defined for boolean operands.");

                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitAnd();
                        break;

                    case Primitive.LONG:
                        assembler.EmitAnd64();
                        break;

                    case Primitive.FLOAT:
                    case Primitive.DOUBLE:
                        throw new ParserException("Bitwise & operation is not defined for float point operands.");
                }

                type1 = result;
            }
        }

        private AbstractType ParseBitwiseXorExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseBitwiseAndExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "^":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseBitwiseAndExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Bitwise ^ operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                switch (result.Primitive)
                {
                    case Primitive.BOOL:
                        throw new ParserException("Bitwise ^ operation is not defined for boolean operands.");

                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitXor();
                        break;

                    case Primitive.LONG:
                        assembler.EmitXor64();
                        break;

                    case Primitive.FLOAT:
                    case Primitive.DOUBLE:
                        throw new ParserException("Bitwise ^ operation is not defined for float point operands.");
                }

                type1 = result;
            }
        }

        private AbstractType ParseBitwiseOrExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseBitwiseXorExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "|":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseBitwiseXorExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Bitwise | operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                switch (result.Primitive)
                {
                    case Primitive.BOOL:
                        throw new ParserException("Bitwise | operation is not defined for boolean operands.");

                    case Primitive.BYTE:
                    case Primitive.SHORT:
                    case Primitive.INT:
                        assembler.EmitOr();
                        break;

                    case Primitive.LONG:
                        assembler.EmitOr64();
                        break;

                    case Primitive.FLOAT:
                    case Primitive.DOUBLE:
                        throw new ParserException("Bitwise | operation is not defined for float point operands.");
                }

                type1 = result;
            }
        }

        private AbstractType ParseLogicalAndExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseBitwiseOrExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "&&":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseBitwiseOrExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Logical && operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                if (result.Primitive != Primitive.BOOL)
                    throw new ParserException("Logical && operation is not defined for non boolean operands.");

                assembler.EmitAnd();

                type1 = result;
            }
        }

        private AbstractType ParseLogicalXorExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseLogicalAndExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "^^":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseLogicalAndExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Logical ^^ operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                if (result.Primitive != Primitive.BOOL)
                    throw new ParserException("Logical ^^ operation is not defined for non boolean operands.");

                assembler.EmitXor();

                type1 = result;
            }
        }

        private AbstractType ParseLogicalOrExpression(Context context, Assembler assembler)
        {
            AbstractType type1 = ParseLogicalXorExpression(context, assembler);

            while (true)
            {
                Symbol symbol = lexer.NextSymbol(false);
                if (symbol == null)
                    return type1;

                switch (symbol.Value)
                {
                    case "||":
                        break;

                    default:
                        lexer.PreviusToken();
                        return type1;
                }

                Assembler tempAssembler = new Assembler();
                AbstractType type2 = ParseLogicalXorExpression(context, tempAssembler);

                if (!(type1 is PrimitiveType) || !(type2 is PrimitiveType))
                    throw new ParserException("Logical || operation not allowed betweeen '" + type1 + "' and '" + type2 + "'.");

                PrimitiveType pt1 = (PrimitiveType)type1;
                PrimitiveType pt2 = (PrimitiveType)type2;

                PrimitiveType result = AssembleOperandsImplicitCast(assembler, tempAssembler, pt1, pt2);

                if (result.Primitive != Primitive.BOOL)
                    throw new ParserException("Logical || operation is not defined for non boolean operands.");

                assembler.EmitOr();

                type1 = result;
            }
        }

        private AbstractType ParseExpression(Context context, Assembler assembler)
        {
            Identifier id = lexer.NextIdentifier(false);
            if (id == null)
                return ParseLogicalOrExpression(context, assembler);

            Variable var = context.FindVariable(id.Name);
            if (var == null)
            {
                var = FindGlobalVariable(id.Name);
                if (var == null)
                    lexer.PreviusToken();
            }

            if (var == null)
                return ParseLogicalOrExpression(context, assembler);

            if (lexer.NextSymbol("=", false) != null)
            {
                int offset = var.Offset;
                if (var is GlobalVariable)
                    assembler.EmitLoadConst(offset);
                else
                {
                    // variável local ou parâmetro
                    assembler.EmitLoadBP();
                    assembler.EmitLoadConst(offset);
                    assembler.EmitAdd();
                }

                AbstractType type = ParseExpression(context, assembler);
                if (type is PrimitiveType)
                {
                    PrimitiveType p = (PrimitiveType)type;
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                        case Primitive.INT:
                        case Primitive.FLOAT:
                            assembler.EmitLoadStack();
                            break;

                        case Primitive.LONG:
                        case Primitive.DOUBLE:
                            assembler.EmitLoadStack64();
                            break;
                    }
                }
                else
                    // TODO Implementar
                    throw new ParserException("???");
            }
            else
            {
                lexer.PreviusToken();
                return ParseLogicalOrExpression(context, assembler);
            }

            return var.Type;
        }

        public AbstractType ParseType()
        {
            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "bool":
                        return PrimitiveType.BOOL;

                    case "byte":
                        return PrimitiveType.BYTE;

                    case "char":
                        return PrimitiveType.CHAR;

                    case "short":
                        return PrimitiveType.SHORT;

                    case "int":
                        return PrimitiveType.INT;

                    case "long":
                        return PrimitiveType.LONG;

                    case "float":
                        return PrimitiveType.FLOAT;

                    case "real":
                        return PrimitiveType.DOUBLE;
                }

                lexer.PreviusToken();
            }

            Identifier id = lexer.NextIdentifier();
            StructType st = FindStruct(id.Name);
            if (st == null)
                throw new ParserException("Undeclared type '" + id.Name + "'");

            return st;
        }

        public void ParseParamsDeclaration(Function function)
        {
            while (true)
            {
                Identifier id = lexer.NextIdentifier();
                lexer.NextSymbol(":");
                AbstractType type = ParseType();

                Parameter p = function.DeclareParameter(id.Name, type);
                if (p == null)
                    throw new ParserException("Parameter '" + id.Name + "' already declared.");

                if (lexer.NextSymbol(",", false) == null)
                    return;
            }
        }

        public void ParseFieldsDeclaration(StructType st)
        {
            while (true)
            {
                Identifier id = lexer.NextIdentifier();
                lexer.NextSymbol(":");
                AbstractType type = ParseType();

                Field field = st.DeclareField(id.Name, type);
                if (field == null)
                    throw new ParserException("Field '" + id.Name + "' already declared.");

                lexer.NextSymbol(";");

                if (lexer.NextSymbol("}", false) != null)
                    return;
            }
        }

        public void ParseStatement(Function function, Context context, Assembler assembler)
        {
            if (lexer.NextSymbol(";", false) != null)
                return;

            if (lexer.NextSymbol("{", false) != null)
            {
                Context newContext = new Context(function, context);
                ParseBlock(function, newContext, assembler);
                return;
            }

            AbstractType type;
            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "declare":
                        {
                            ParseVariableDeclaration(function, context, assembler);
                            lexer.NextSymbol(";");
                            return;
                        }

                    case "se":
                        {
                            return;
                        }

                    case "para":
                        {
                            return;
                        }

                    case "enquanto":
                        {
                            return;
                        }

                    case "repita":
                        {
                            return;
                        }

                    case "leia":
                        {
                            Identifier id = lexer.NextIdentifier();
                            lexer.NextSymbol(";");

                            Variable var = context.FindVariable(id.Name);
                            if (var == null)
                            {
                                var = FindGlobalVariable(id.Name);
                                if (var == null)
                                    throw new ParserException("Undeclared variable '" + id.Name + "'.");
                            }

                            int offset = var.Offset;
                            if (var is GlobalVariable)
                                assembler.EmitLoadConst(offset);
                            else
                            {
                                // variável local ou parâmetro
                                assembler.EmitLoadBP();
                                assembler.EmitLoadConst(offset);
                                assembler.EmitAdd();
                            }

                            type = var.Type;
                            if (!(type is PrimitiveType))
                                throw new ParserException("Expected an expression of primitive type.");

                            PrimitiveType p = (PrimitiveType)type;
                            switch (p.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitScan();
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
                            }

                            return;
                        }

                    case "escreva":
                        {
                            type = ParseExpression(context, assembler);
                            lexer.NextSymbol(";");

                            if (!(type is PrimitiveType))
                                throw new ParserException("Expected an expression of primitive type.");

                            PrimitiveType p = (PrimitiveType)type;
                            switch (p.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.BYTE:
                                case Primitive.CHAR:
                                case Primitive.SHORT:
                                case Primitive.INT:
                                    assembler.EmitPrint();
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
                            }

                            return;
                        }

                    case "retorne":
                        {
                            assembler.EmitLoadBP();
                            assembler.EmitLoadConst(function.ReturnOffset);
                            assembler.EmitAdd();

                            type = ParseExpression(context, assembler);
                            lexer.NextSymbol(";");

                            assembler.EmitStoreStack();

                            assembler.EmitLabel(function.ReturnLabel);
                            assembler.EmitJump();
                            return;
                        }

                    case "quebra":
                        {
                            lexer.NextSymbol(";");
                            return;
                        }
                }

                lexer.PreviusToken();
            }

            type = ParseExpression(context, assembler);
            lexer.NextSymbol(";");

            int size = type != null ? type.Size() : 0;
            if (size > 0)
            {
                if (size <= 4)
                    assembler.EmitPop();
                else if (size <= 8)
                    assembler.EmitPop2();
                else
                    assembler.EmitPopN(GetSizeInDWords(size));
            }
        }

        public void ParseBlock(Function function, Context context, Assembler assembler)
        {
            while (lexer.NextSymbol("}", false) == null)
                ParseStatement(function, context, assembler);
        }

        public void ParseVariableDeclaration(Function function, Context context, Assembler assembler)
        {
            Identifier id = lexer.NextIdentifier();
            lexer.NextSymbol(":");
            AbstractType type = ParseType();

            Variable var = function == null ? (Variable) DeclareGlobalVariable(id.Name, type) : context.DeclareLocalVariable(function, id.Name, type);
            if (var == null)
                throw new ParserException("Variable '" + id.Name + "' already declared.");

            if (lexer.NextSymbol("=", false) != null)
            {
                if (function == null)
                    throw new ParserException("Global variable can't be initialized in declaretion session.");

                assembler.EmitLoadBP();
                assembler.EmitLoadConst(var.Offset);
                assembler.EmitAdd();

                AbstractType rightType = ParseExpression(context, assembler);
                if (rightType is PrimitiveType)
                {
                    PrimitiveType p = (PrimitiveType)rightType;
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                        case Primitive.CHAR:
                        case Primitive.SHORT:
                        case Primitive.INT:
                        case Primitive.FLOAT:
                            assembler.EmitLoadStack();
                            break;

                        case Primitive.LONG:
                        case Primitive.DOUBLE:
                            assembler.EmitLoadStack64();
                            break;
                    }
                }
                else
                    // TODO Implementar
                    throw new ParserException("???");
            }
        }

        public void ParseFunctionDeclaration(Assembler assembler)
        {
            Identifier id = lexer.NextIdentifier();
            Function f = DeclareFunction(id.Name);
            if (f == null)
                throw new ParserException("Function '" + id.Name + "' already declared.");

            lexer.NextSymbol("(");
            if (lexer.NextSymbol(")", false) == null)
            {
                ParseParamsDeclaration(f);
                lexer.NextSymbol(")");
            }

            if (lexer.NextSymbol(":", false) != null)
            {
                AbstractType type = ParseType();
                f.ReturnType = type;
            }

            lexer.NextSymbol("{");
            Context context = new Context(f);
            Assembler tempAssembler = new Assembler();

            f.CreateReturnLabel(tempAssembler);
            ParseBlock(f, context, tempAssembler);
            f.BindReturnLabel(tempAssembler);

            f.CreateEntryLabel(assembler);
            f.BindEntryLabel(assembler);

            f.BeginBlock(assembler);
            assembler.Emit(tempAssembler);
            f.EndBlock(assembler);
        }

        public void ParseStructDeclaration()
        {
            Identifier id = lexer.NextIdentifier();
            StructType st = DeclareStruct(id.Name);
            if (st == null)
                throw new ParserException("Struct '" + id.Name + "' already declared.");

            lexer.NextSymbol("{");
            if (lexer.NextSymbol("}", false) == null)
                ParseFieldsDeclaration(st);
        }

        public bool ParseDeclaration(Assembler assembler)
        {
            Keyword kw = lexer.NextKeyword(false);
            if (kw != null)
            {
                switch (kw.Value)
                {
                    case "declare":
                        ParseVariableDeclaration(null, null, null);
                        lexer.NextSymbol(";");
                        return false;

                    case "função":
                        ParseFunctionDeclaration(assembler);
                        return false;

                    case "estrutura":
                        ParseStructDeclaration();
                        return false;
                }

                lexer.PreviusToken();
            }

            if (lexer.NextSymbol("{", false) != null)
            {
                Function f = DeclareFunction("@main");
                if (f == null)
                    throw new ParserException("Entry point already declared.");

                entryPoint = f;

                Context context = new Context(f);
                Assembler tempAssembler = new Assembler();

                f.CreateReturnLabel(tempAssembler);
                ParseBlock(f, context, tempAssembler);
                f.BindReturnLabel(tempAssembler);

                f.CreateEntryLabel(assembler);
                f.BindEntryLabel(assembler);

                f.BeginBlock(assembler);
                assembler.Emit(tempAssembler);
                f.EndBlock(assembler);

                return true;
            }

            return false;
        }

        public void ParseProgram(Assembler assembler)
        {
            lexer.NextKeyword("programa");
            lexer.NextIdentifier();
            lexer.NextSymbol("{");

            Assembler tempAssembler = new Assembler();
            while (lexer.NextSymbol("}", false) == null)
            {
                if (ParseDeclaration(tempAssembler))
                {
                    lexer.NextSymbol("}");
                    break;
                }
            }

            assembler.EmitLoadSP();
            assembler.EmitLoadConst(globalVariableOffset);
            assembler.EmitAdd();
            assembler.EmitStoreSP();

            if (entryPoint != null)
            {
                assembler.EmitLabel(entryPoint.EntryLabel);
                assembler.EmitCall();
            }

            assembler.Emit(tempAssembler);
        }

        public AbstractType CompileAdditiveExpression(string expression, Assembler assembler)
        {
            globals.Clear();
            structs.Clear();
            functions.Clear();

            lexer.Input = expression;
            Context context = new Context(null);
            AbstractType type = ParseAdditiveExpression(context, assembler);

            Token token = lexer.NextToken();
            if (token != null)
                throw new ParserException("End of expression expected but " + token + " found.");

            return type;
        }

        public void CompileProgram(string source, Assembler assembler)
        {
            globals.Clear();
            structs.Clear();
            functions.Clear();

            lexer.Input = source;

            ParseProgram(assembler);

            Token token = lexer.NextToken();
            if (token != null)
                throw new ParserException("End of expression expected but " + token + " found.");
        }
    }
}
