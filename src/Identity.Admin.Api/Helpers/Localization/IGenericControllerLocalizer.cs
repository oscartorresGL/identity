using Microsoft.Extensions.Localization;

namespace Identity.Admin.Api.Helpers.Localization
{
    public interface IGenericControllerLocalizer<T> : IStringLocalizer<T>
    {

    }
}