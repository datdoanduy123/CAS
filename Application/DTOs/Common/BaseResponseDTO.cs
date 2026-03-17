using System.Collections.Generic;

namespace Application.DTOs.Common
{
    public class BaseResponseDTO<T>
    {
        public int Code { get; set; } = 0;
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public T? Data { get; set; }
        public MetaDataDTO? MetaData { get; set; }

        public static BaseResponseDTO<T> SuccessResponse(T data, MetaDataDTO? meta = null, string? message = null, int code = 200)
            => new() { Data = data, MetaData = meta, Message = message, Code = code, Success = true };

        public static BaseResponseDTO<T> FailResponse(string message, int code = 500)
            => new() { Message = message, Code = code, Success = false };
    }
}
