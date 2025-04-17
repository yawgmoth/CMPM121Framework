using UnityEngine;
using System.Collections;
using System;
/*
 * This class here is just to help w/ Reverse Polish Notation (RPN) calculations
 * 
 * It assumes the sequence given is legal and doesn't check
*/
public class ReversePolishCalc
{
    Stack numbers;
    public ReversePolishCalc() {
        numbers = new Stack();
    }
    public int Calculate(string[] sequence)
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            string token = sequence[i];
            //Parse the value of the token
            if (Int32.TryParse(token, out int val))
            {
                numbers.Push(val);
            }
            else 
            {
                //It must be an operator, so pop the values and perform operation
                int second = (int) numbers.Pop();
                int first = (int) numbers.Pop();
                switch (token) 
                {
                    case "+":
                        val = first + second; break;
                    case "-":
                        val = first - second; break;
                    case "*":
                        val = first * second; break;
                    case "/":
                        val = first / second; break;
                    case "%":
                        val = first % second; break;
                    default:
                        string s = "Invalid operator \'%\' given";
                        s = s.Replace("%", token);
                        Debug.Log(s);
                        break;
                }
                numbers.Push(val);
            }
        }
        return (int) numbers.Pop();
    }
}
