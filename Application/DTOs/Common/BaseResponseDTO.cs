using System.Collections.Generic;

namespace Application.DTOs.Common
{
    public class BaseResponseDTO<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static BaseResponseDTO<T> SuccessResponse(T data, string message = "SUCCESS", int statusCode = 200)
        {
            return new BaseResponseDTO<T>
            {
                IsSuccess = true,
                Message = message,
                StatusCode = statusCode,
                Data = data
            };
        }

        public static BaseResponseDTO<T> FailResponse(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new BaseResponseDTO<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors
            };
        }
    }
}
