namespace Domain.Constants
{
    public static class MessageConstant
    {
        public static class CommonMessage
        {
            public const string UNAUTHORIZED = "Common_401";              // 401 - Chưa xác thực
            public const string ACCESS_DENIED = "Common_403";             // 403 - Không có quyền
            public const string NOT_FOUND = "Common_404";                 // 404 - Không tìm thấy
            public const string INTERNAL_SERVER_ERROR = "Common_500";     // 500 - Lỗi server
            public const string MISSING_PARAM = "Common_501";             // 501 - Thiếu tham số
            public const string SUCCESS = "SUCCESS";
            public const string FAILED = "FAILED";
        }

        public static class UserMessage
        {
            public const string USER_EXIST = "User_001";              // Tên đăng nhập đã tồn tại
            public const string EMAIL_EXIST = "User_002";             // Email đã tồn tại
            public const string USER_NOT_FOUND = "User_404";          // Không tìm thấy người dùng
        }
    }
}
