using System;
using System.Collections.Generic;

public static class RPNEvaluator
{
    public static int Evaluate(string expression, Dictionary<string, int> variables)
    {
        if (string.IsNullOrWhiteSpace(expression)) return 0;

        Stack<int> stack = new Stack<int>();
        string[] tokens = expression.Split(' ');

        foreach (var token in tokens)
        {
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else if (variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            else
            {
                if (stack.Count < 2)
                {
                    throw new InvalidOperationException($"Invalid RPN expression: not enough values for '{token}'");
                }

                int b = stack.Pop();
                int a = stack.Pop();

                switch (token)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/": stack.Push(b != 0 ? a / b : 0); break;
                    case "%": stack.Push(b != 0 ? a % b : 0); break;
                    default:
                        throw new InvalidOperationException($"Unknown token: {token}");
                }
            }
        }

        if (stack.Count != 1)
        {
            throw new InvalidOperationException("Invalid RPN expression: leftover values in stack");
        }

        return stack.Pop();
    }
}
