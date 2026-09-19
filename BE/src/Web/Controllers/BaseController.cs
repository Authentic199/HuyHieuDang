using HuyHieuDang.Core.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BaseController : ControllerBase
{
    protected ActionResult<SuccessResultWrapper<TData>> OkWrapper<TData>(TData? data = default, string? message = default)
    {
        return Ok(new SuccessResultWrapper<TData>(data, message));
    }

    protected ActionResult<SuccessResultWrapper<TData>> CreatedWrapper<TData>(string uri, TData? data = default, string? message = default)
    {
        return Created(uri, new SuccessResultWrapper<TData>(data, message));
    }

    protected ActionResult<SuccessResultWrapper<TData>> AcceptedWrapper<TData>(TData? data = default, string? message = default)
    {
        return Accepted(new SuccessResultWrapper<TData>(data, message));
    }
}
