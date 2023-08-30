using System.Collections.Generic;

using Comp;

namespace SimpleCompiler.VM;

public class SourceIntervalNode(SourceInterval scope, params LocalVariable[] variables)
{
    private List<SourceIntervalNode> children = new();
    private List<LocalVariable> variables = new(variables);

    public SourceInterval Scope
    {
        get;
    } = scope;

    public IEnumerable<SourceIntervalNode> Children => children;

    public IEnumerable<Variable> Variables => variables;

    public SourceIntervalNode CheckAndInsert(LocalVariable variable)
    {
        var scope = variable.Scope;

        if (scope.Equals(Scope))
        {
            if (!variables.Contains(variable))
                variables.Add(variable);

            return this;
        }

        if (!Scope.Contains(scope))
            return null;

        foreach (var child in children)
        {
            var node = child.CheckAndInsert(variable);
            if (node != null)
                return node;
        }

        var result = new SourceIntervalNode(scope, variable);
        children.Add(result);
        return result;
    }

    public SourceIntervalNode CheckAndInsert(SourceIntervalNode node)
    {
        var scope = node.Scope;

        if (scope.Equals(Scope))
        {
            foreach (var variable in node.variables)
            {
                if (!variables.Contains(variable))
                    variables.Add(variable);
            }

            return this;
        }

        if (!Scope.Contains(scope))
            return null;

        foreach (var child in children)
        {
            var added = child.CheckAndInsert(node);
            if (added != null)
                return added;
        }

        children.Add(node);
        return node;
    }

    public void FetchVariables(SourceInterval scope, List<Variable> result)
    {
        if (!Scope.Contains(scope))
            return;

        if (Scope.Equals(scope))
            result.AddRange(variables);

        foreach (var child in children)
            child.FetchVariables(scope, result);
    }

    public void FetchVariablesFromPos(string fileName, int pos, List<Variable> result)
    {
        if (!Scope.Contains(fileName, pos))
            return;

        result.AddRange(variables);

        foreach (var child in children)
            child.FetchVariablesFromPos(fileName, pos, result);
    }

    public void FetchVariablesFromLine(string fileName, int lineNumber, List<Variable> result)
    {
        if (!Scope.ContainsLine(fileName, lineNumber))
            return;

        result.AddRange(variables);

        foreach (var child in children)
            child.FetchVariablesFromLine(fileName, lineNumber, result);
    }
}