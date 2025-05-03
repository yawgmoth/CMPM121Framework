using System;
using System.Collections.Generic;

public static class RPNEvaluator
{
    public static float Evaluate(string expression, Dictionary<string, float> variables)
    {
        if (string.IsNullOrWhiteSpace(expression)) return 0;

        Stack<float> stack = new Stack<float>();
        string[] tokens = expression.Split(' ');

        foreach (var token in tokens)
        {
            if (float.TryParse(token, out float number))
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

                float b = stack.Pop();
                float a = stack.Pop();

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
