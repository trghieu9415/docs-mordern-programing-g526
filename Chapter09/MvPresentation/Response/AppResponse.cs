using Microsoft.AspNetCore.Mvc;
using MvApplication.Models;

namespace MvPresentation.Response;

public static class AppResponse {
  public static ObjectResult Success(string? message = null) {
    var response = new {
      Status = 200,
      Message = message ?? "Thao tác thành công"
    };


    return new ObjectResult(response) {
      StatusCode = 200
    };
  }

  public static ObjectResult Success(string? message, int status) {
    var response = new {
      Status = status,
      Message = message ?? "Thao tác thành công"
    };


    return new ObjectResult(response) {
      StatusCode = status
    };
  }

  public static ObjectResult Success<T>(T data, string? message = null, int status = 200) {
    var response = new {
      Status = status,
      Message = message ?? "Thao tác thành công",
      Data = data
    };

    return new ObjectResult(response) {
      StatusCode = status
    };
  }

  public static ObjectResult Success<T>(T data, Meta meta, string? message = null, int status = 200) {
    var response = new {
      Status = status,
      Message = message ?? "Thao tác thành công",
      Data = data,
      Meta = meta
    };

    return new ObjectResult(response) {
      StatusCode = status
    };
  }

  public static ObjectResult Success(Guid id, string? message = null, int status = 200) {
    var response = new {
      Status = status,
      Message = message ?? "Thao tác thành công",
      Data = new {
        Id = id
      }
    };

    return new ObjectResult(response) {
      StatusCode = status
    };
  }


  public static ObjectResult Fail(string? error = null, int status = 400) {
    var response = new {
      Status = status,
      Error = error ?? "Đã có lỗi xảy ra!!!"
    };

    return new ObjectResult(response) {
      StatusCode = status
    };
  }

  public static ObjectResult Fail<T>(T data, string? error = null, int status = 400) {
    var response = new {
      Status = status,
      Error = error ?? "Đã có lỗi xảy ra!!!",
      Data = data
    };

    return new ObjectResult(response) {
      StatusCode = status
    };
  }
}
