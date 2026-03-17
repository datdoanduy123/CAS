using Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using static Domain.Constants.MessageConstant;

namespace Api.Controllers
{
    public class BaseController : ControllerBase
    {
        public Guid UserId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsAdmin { get; set; }

        protected async Task<BaseResponseDTO<T>> HandleException<T>(Task<T> task, MetaDataDTO? meta = null)
        {
            try
            {
                var data = await task;
                return BaseResponseDTO<T>.SuccessResponse(data, meta);
            }
            catch (ApplicationException ex)
            {
                // Lỗi nghiệp vụ từ Application layer
                return BaseResponseDTO<T>.FailResponse(ex.Message, 200);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Lỗi xác thực
                return BaseResponseDTO<T>.FailResponse(ex.Message, 401);
            }
            catch (KeyNotFoundException ex)
            {
                // Lỗi không tìm thấy dữ liệu
                return BaseResponseDTO<T>.FailResponse(ex.Message, 404);
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống chưa xác định
                return BaseResponseDTO<T>.FailResponse(CommonMessage.INTERNAL_SERVER_ERROR, 500);
            }
        }
    }
}
