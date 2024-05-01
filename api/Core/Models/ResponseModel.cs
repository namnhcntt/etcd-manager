using EtcdManager.API.Core.Helpers;

namespace EtcdManager.API.Core.Models
{
    public class ErrorResponseModel
    {
        protected string? _stackTrace;

        /// <summary>
        /// Lỗi trả về từ thư viện fluent validator
        /// </summary>
        public List<ErrorDetail> Errors { get; set; } = new List<ErrorDetail>();

        /// <summary>
        /// Thông báo chi tiết (nếu có), thường dùng để thông báo chi tiết lỗi
        /// </summary>
        /// <value></value>
        public string? Message { get; set; }

        /// <summary>
        /// Nội dung phục vụ debug (development only), không show thông tin này ở môi trường prod
        /// </summary>
        /// <value></value>
        public string? StackTrace
        {
            get { return _stackTrace; }
            set
            {
                if (!EnvironmentHelper.IsProduction())
                {
                    _stackTrace = value;
                }
            }
        }

        /// <summary>
        /// Lấy danh sách lỗi đưa vào log
        /// </summary>
        public string ErrorsToString()
        {
            if (Errors != null)
            {
                return string.Join(", ", Errors.Select(x => x.ErrorMessage));
            }
            return string.Empty;
        }

        public string RequestId { get; set; } = null!;
    }

    /// <summary>
    /// Tiêu chuẩn model response về client trong mọi trường hợp gọi api trong hệ thống, sử dụng model này nếu cần chắc chắn property data trả về phải có kiểu T
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class ResponseModel<T>
    {
        /// <summary>
        /// Dữ liệu trả về, có thể là bất kỳ kiểu gì (Generic T)
        /// </summary>
        /// <value></value>
        public T? Data { get; set; }

        /// <summary>
        /// Tổng số bản ghi trong truy vấn (vì có thể dữ liệu chi tiết trả về chỉ là 1 trang trong tổng số bản ghi truy vấn được)
        /// </summary>
        /// <value></value>
        public int? TotalRecord { get; set; }
    }
}
