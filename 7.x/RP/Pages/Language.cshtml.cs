using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace RP.Pages
{
    public class LanguageModel : PageModel
    {
        private readonly IOptions<RequestLocalizationOptions> _requestLocalizationOptions;
        public LanguageModel(IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            _requestLocalizationOptions = requestLocalizationOptions;
        }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public string? Lang { get; set; }

        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                if (_requestLocalizationOptions.Value.RequestCultureProviders.FirstOrDefault()
                    is CookieRequestCultureProvider cookieRequestCultureProvider)
                {
                    Response.Cookies.Append(cookieRequestCultureProvider.CookieName,
                    //CookieRequestCultureProvider
                    //.MakeCookieValue(Lang is not null ? new RequestCulture(Lang) :
                    //_requestLocalizationOptions.Value.DefaultRequestCulture),
                    //new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
                    CookieRequestCultureProvider
                            .MakeCookieValue(Lang is not null ? new RequestCulture(Lang) :
                            _requestLocalizationOptions.Value.DefaultRequestCulture));
                }
                return LocalRedirect(ReturnUrl ?? "/");
            }
            return LocalRedirect("/");
        }
    }
}
