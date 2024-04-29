using System.Net;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos
{
    public class HttpError
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Message { get; set; }

        private HttpError() { }

        public static HttpError InternalServerError(string message) =>
            new() { Message = message, StatusCode = HttpStatusCode.InternalServerError };

        public static HttpError BadRequest(string message) =>
            new() { Message = message, StatusCode = HttpStatusCode.BadRequest };

        public static HttpError NotFound(string message) =>
            new() { Message = message, StatusCode = HttpStatusCode.NotFound };

        public static HttpError Custom(HttpStatusCode statusCode, string message) =>
            new() { StatusCode = statusCode, Message = message };

        public override string ToString() => $"Http request failure. Status Code: {(int)StatusCode}-{StatusCode}, Message: {Message}";
    }
}
