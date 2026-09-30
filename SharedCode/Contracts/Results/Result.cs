namespace SharedCode.Contracts.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }

        public ErrorType? ErrorType { get; init; }
        protected Result(
            bool isSuccess,
            string? error = null,
            ErrorType? errorType = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorType = errorType;
        }

        public static Result Success()
            => new(true);

        public static Result Failure(string error, ErrorType? errorType = null)
            => new(false, error, errorType);
    }

    public class Result<T> : Result
    {
        public T? Data { get; }

        private Result(
            T? data,
            bool isSuccess,
            string? error = null,
            ErrorType? errorType = null)
            : base(isSuccess, error, errorType)
        {
            Data = data;
        }

        public static Result<T> Success(T value)
            => new(value, true);

        public static new Result<T> Failure(string error, ErrorType? errorType = null)
            => new(default, false, error)
            {
                ErrorType = errorType
            };
    }

    public enum ErrorType
    {
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden,
        Failure
    }

}
