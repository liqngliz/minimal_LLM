using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Plugins;

public sealed class MathPlugin
{
    [KernelFunction, Description("Take the square root of a number")]
    public static double Sqrt(
        [Description("The number to take a square root of")] double number1) 
    =>  Math.Sqrt(number1);
    

    [KernelFunction, Description("Sum of two numbers")]
    public static double Add(
        [Description("The first number to add")] double number1,
        [Description("The second number to add")] double number2) 
    => number1 + number2;


    [KernelFunction, Description("Subtraction of two numbers")]
    public static double Subtract(
        [Description("The first number to subtract from")] double number1,
        [Description("The second number to subtract away")] double number2)
    => number1 - number2;
    

    [KernelFunction, Description("Multiplication of two numbers.")]
    public static double Multiply(
        [Description("The first number to multiply")] double number1,
        [Description("The second number to multiply")] double number2) 
    => number1 * number2;

    [KernelFunction, Description("Division of two numbers")]
    public static double Divide(
        [Description("The first number to divide from")] double number1,
        [Description("The second number to divide by")] double number2) 
    => number1 / number2;

    [KernelFunction, Description("Raise a number to a power")]
    public static double Power(
        [Description("The number to raise")] double number1,
        [Description("The power to raise the number to")] double number2) 
    => Math.Pow(number1, number2);
}