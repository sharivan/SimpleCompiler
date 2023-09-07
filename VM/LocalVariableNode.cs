using System.Collections.Generic;

using Comp;

namespace SimpleCompiler.VM;

public class LocalVariableNode(IPRange scope, params LocalVariable[] variables)
{
    private List<LocalVariableNode> children = new();
    private List<LocalVariable> variables = new(variables);

    public IPRange Scope
    {
        get;
    } = scope;

    public IEnumerable<LocalVariableNode> Children => children;

    public IEnumerable<LocalVariable> Variables => variables;

    public LocalVariableNode CheckAndInsert(IPRange scope, LocalVariable variable)
    {
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
            var node = child.CheckAndInsert(scope, variable);
            if (node != null)
                return node;
        }

        var result = new LocalVariableNode(scope, variable);
        children.Add(result);
        return result;
    }

    public LocalVariableNode CheckAndInsert(LocalVariableNode node)
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

    public void FetchVariables(int ip, List<Variable> result)
    {
        if (!Scope.Contains(ip))
            return;

        result.AddRange(variables);

        foreach (var child in children)
            child.FetchVariables(ip, result);
    }

    public void FetchVariables(IPRange scope, List<Variable> result)
    {
        if (!Scope.Contains(scope))
            return;

        if (Scope.Equals(scope))
            result.AddRange(variables);

        foreach (var child in children)
            child.FetchVariables(scope, result);
    }
}