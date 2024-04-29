using EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos;
using System.Net;

namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions
{
    public static class AzureResponseExtensions
    {
        public static Result<T, HttpError> Result<T>(this Azure.Response<T> response)
        {
            try
            {
                var rawResponse = response.GetRawResponse();
                if (rawResponse.IsError)
                {
                    return HttpError.Custom((HttpStatusCode)rawResponse.Status, rawResponse.ReasonPhrase);
                }

                return response.Value;
            }
            catch (Exception e)
            {
                return HttpError.Custom(0, e.Message);
            }
        }

        public static Result<bool, HttpError> Result(this Azure.Response response)
        {
            try
            {
                if (response.IsError)
                {
                    return HttpError.Custom((HttpStatusCode)response.Status, response.ReasonPhrase);
                }

                return true;
            }
            catch (Exception e)
            {
                return HttpError.Custom(0, e.Message);
            }
        }
    }
}
