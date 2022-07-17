using System.Net;
using System.Runtime.CompilerServices;
using EtcdManager.API.Extensions;

namespace EtcdManager.API.Models
{
    public class ResponseModel
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public HttpStatusCode Status { get; set; }
        public string? ErrorControlId { get; set; }
        public string? ErrorFormId { get; set; }
        public string? DebugContent { get; set; }
        public string? StackTrace { get; set; }
        public string? ErrorCheckExist { get; set; }
        public ResponseModel Cast()
        {
            return this;
        }

        public static ResponseModel ResponseWithError(string error, string message = "", HttpStatusCode status = HttpStatusCode.BadRequest,
          string errorControlId = "", string errorCheckExist = "", string errorFormId = "",
          [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null) => new ResponseModel()
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(message) ? null : message,
                Error = error,
                Status = status,
                ErrorControlId = errorControlId.ToCamelCaseString(),
                ErrorFormId = errorFormId,
                ErrorCheckExist = errorCheckExist,
                StackTrace = $"{caller}: line {lineNumber}"
            };

        public ResponseModel<TOut> ConvertIfError<TOut>()
        {
            return ResponseModel<TOut>.ResponseWithError(this.Error, this.Status, this.Message);
        }

        public ResponseModel ConvertIfError()
        {
            return ResponseModel.ResponseWithError(this.Error, this.Message, this.Status);
        }

        public static ResponseModel ResponseWithAccessDenined()
        {
            return ResponseWithError("ERR_FORBIDDEN", "Bạn không đủ quyền để lấy dữ liệu đã yêu cầu", HttpStatusCode.Forbidden);
        }

        public static ResponseModel ResponseWithNotFound()
        {
            return ResponseWithError("ERR_NOT_FOUND", "Không tìm thấy dữ liệu đã yêu cầu", HttpStatusCode.NotFound);
        }
        public static ResponseModel ResponseWithSuccess(string message = "")
        {
            return new ResponseModel()
            {
                Success = true,
                Message = message
            };
        }

        public static ResponseModel ResponseWithException(
          Exception ex,
          [CallerLineNumber] int lineNumber = 0,
          [CallerMemberName] string caller = null
      ) => new ResponseModel()
      {
          Success = false,
          Error = "INTERNAL_ERROR",
          Message = "Có lỗi xảy ra. Liên hệ quản trị viên để biết thêm chi tiết!",
          Status = HttpStatusCode.InternalServerError,
          DebugContent = ex.GetDebugMessage(),
          StackTrace = $"{caller}: line {lineNumber}"
      };
    }


    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public int TotalRecord { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public HttpStatusCode Status { get; set; }
        public string? ErrorControlId { get; set; }
        public string? ErrorFormId { get; set; }
        public string? DebugContent { get; set; }
        public string? StackTrace { get; set; }
        public string? ErrorCheckExist { get; set; }
        public static ResponseModel<T> ResponseWithData(T data, string message = "", int totalRecord = 0) => new ResponseModel<T>()
        {
            Success = true,
            Status = HttpStatusCode.OK,
            Data = data,
            Message = message,
            TotalRecord = totalRecord
        };

        public static ResponseModel<T> ResponseWithError(string error, HttpStatusCode status = HttpStatusCode.BadRequest, string message = "",
            T data = default, string errorControlId = "", string errorCheckExist = "", string errorFormId = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null) => new ResponseModel<T>()
            {
                Success = false,
                Error = error,
                Message = message,
                Status = status,
                Data = data,
                ErrorControlId = errorControlId.ToCamelCaseString(),
                ErrorFormId = errorFormId,
                ErrorCheckExist = errorCheckExist,
                StackTrace = $"{caller}: line {lineNumber}"
            };

        public static ResponseModel ResponseWithException(
              Exception ex,
              [CallerLineNumber] int lineNumber = 0,
              [CallerMemberName] string caller = null
          ) => new ResponseModel()
          {
              Success = false,
              Error = "INTERNAL_ERROR",
              Message = "Có lỗi xảy ra. Liên hệ quản trị viên để biết thêm chi tiết!",
              Status = HttpStatusCode.InternalServerError,
              DebugContent = ex.GetDebugMessage(),
              StackTrace = $"{caller}: line {lineNumber}"
          };

        public static ResponseModel<T> ResponseWithAccessDenined(string message = "")
        {
            return ResponseWithError("ERR_FORBIDDEN", HttpStatusCode.Forbidden,
                string.IsNullOrWhiteSpace(message) ? "Bạn không đủ quyền để thực hiện yêu cầu" : message
                );
        }

        public static ResponseModel<T> ResponseWithErrorObjectIsNull(string message = "")
        {
            return ResponseWithError("ERR_OBJECT_IS_NULL", HttpStatusCode.BadRequest,
                string.IsNullOrWhiteSpace(message) ? "Dữ liệu bị null" : message
                );
        }

        public static ResponseModel<T> ResponseWithNotFound()
        {
            return ResponseWithError("ERR_NOT_FOUND", HttpStatusCode.NotFound, "Không tìm thấy dữ liệu đã yêu cầu");
        }

        public static ResponseModel<T> ResultSingleItemOrNotFound(T item)
        {
            if (item == null)
                return ResponseWithNotFound();
            return ResponseWithData(item);
        }

        public ResponseModel<TOut> ConvertIfError<TOut>()
        {
            return ResponseModel<TOut>.ResponseWithError(this.Error, this.Status, this.Message);
        }

        public ResponseModel ConvertIfError()
        {
            return ResponseModel.ResponseWithError(this.Error, this.Message, this.Status);
        }

        public ResponseModel<T> Cast()
        {
            return this;
        }
    }
}