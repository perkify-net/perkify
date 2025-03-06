// <copyright file="BudgetExceededException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Perkify.Core;

/// <summary>
/// Exception thrown when a budget operation exceeds the allowed limit.
/// </summary>
public class BudgetExceededException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetExceededException"/> class.
    /// </summary>
    public BudgetExceededException()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetExceededException"/> class with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BudgetExceededException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetExceededException"/> class with a specified error message and reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BudgetExceededException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}