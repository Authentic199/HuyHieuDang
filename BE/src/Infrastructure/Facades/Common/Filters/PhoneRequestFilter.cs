using HuyHieuDang.Infrastructure.Facades.Common.Requests;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HuyHieuDang.Infrastructure.Facades.Common.Filters
{
    public class PhoneRequestFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            IPhoneRequest? request = (IPhoneRequest?)context.ActionArguments.Values.FirstOrDefault(x => x?.GetType().IsAssignableTo(typeof(IPhoneRequest)) == true);

            if (request != null)
            {
                request.PhoneNumber = request.PhoneNumber == null ? null : string.Concat("0", request.PhoneNumber.AsSpan(request.PhoneNumber.Length - 9));
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Method intentionally left empty.
        }
    }
}