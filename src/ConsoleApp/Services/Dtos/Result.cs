namespace EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos
{
    [Serializable]
    public class Result<TData, TError>
    {
        private readonly bool _isSuccess;
        public TData? Data { get; }
        public TError? Error { get; }

        private Result(TData data)
        {
            _isSuccess = true;
            Data = data;
            Error = default;
        }

        private Result(TError error)
        {
            _isSuccess = false;
            Data = default;
            Error = error;
        }

        public bool IsSuccess() => _isSuccess;

        public static implicit operator Result<TData, TError>(TData success) => new(success);
        public static implicit operator Result<TData, TError>(TError failure) => new(failure);
    }
}
