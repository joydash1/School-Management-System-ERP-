using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.DTOs.CommonDtos
{
    // Base non-generic Result using record
    public record Result
    {
        public bool IsSuccess { get; init; }
        public string Message { get; init; } = string.Empty;
        public List<string> Errors { get; init; } = new();
        public int StatusCode { get; init; }

        // Protected constructor for inheritance
        protected Result(bool isSuccess, string message, List<string> errors, int statusCode)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<string>();
            StatusCode = statusCode;
        }

        // Success
        public static Result Success(string message = "Operation successful")
            => new Result(true, message, null, StatusCodes.Status200OK);

        public static Result Success(int statusCode, string message = "Operation successful")
            => new Result(true, message, null, statusCode);

        // Failure
        public static Result Failure(string error, int statusCode = StatusCodes.Status400BadRequest)
            => new Result(false, string.Empty, new List<string> { error }, statusCode);

        public static Result Failure(List<string> errors, int statusCode = StatusCodes.Status400BadRequest)
            => new Result(false, string.Empty, errors, statusCode);

        // Common HTTP status helpers
        public static Result NotFound(string message = "Resource not found")
            => Failure(message, StatusCodes.Status404NotFound);

        public static Result Unauthorized(string message = "Unauthorized access")
            => Failure(message, StatusCodes.Status401Unauthorized);

        public static Result Forbidden(string message = "Access forbidden")
            => Failure(message, StatusCodes.Status403Forbidden);

        public static Result Conflict(string message = "Conflict occurred")
            => Failure(message, StatusCodes.Status409Conflict);

        public static Result ValidationFailed(List<string> errors)
            => Failure(errors, StatusCodes.Status422UnprocessableEntity);
    }

    // Generic Result<T> using record
    public record Result<T> : Result
    {
        public T Data { get; init; }

        private Result(bool isSuccess, T data, string message, List<string> errors, int statusCode)
            : base(isSuccess, message, errors, statusCode)
        {
            Data = data;
        }

        // Success
        public static Result<T> Success(T data, string message = "Operation successful")
            => new Result<T>(true, data, message, null, StatusCodes.Status200OK);

        public static Result<T> Success(T data, int statusCode, string message = "Operation successful")
            => new Result<T>(true, data, message, null, statusCode);

        // Failure
        public static new Result<T> Failure(string error, int statusCode = StatusCodes.Status400BadRequest)
            => new Result<T>(false, default!, string.Empty, new List<string> { error }, statusCode);

        public static new Result<T> Failure(List<string> errors, int statusCode = StatusCodes.Status400BadRequest)
            => new Result<T>(false, default!, string.Empty, errors, statusCode);

        public static Result<T> Failure(string error, T data, int statusCode = StatusCodes.Status400BadRequest)
            => new Result<T>(false, data, string.Empty, new List<string> { error }, statusCode);

        // Common HTTP status helpers
        public static new Result<T> NotFound(string message = "Resource not found")
            => Failure(message, StatusCodes.Status404NotFound);

        public static new Result<T> Unauthorized(string message = "Unauthorized access")
            => Failure(message, StatusCodes.Status401Unauthorized);

        public static new Result<T> Forbidden(string message = "Access forbidden")
            => Failure(message, StatusCodes.Status403Forbidden);

        public static new Result<T> Conflict(string message = "Conflict occurred")
            => Failure(message, StatusCodes.Status409Conflict);

        public static new Result<T> ValidationFailed(List<string> errors)
            => Failure(errors, StatusCodes.Status422UnprocessableEntity);
    }
}