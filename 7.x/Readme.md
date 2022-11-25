# Globalization and Localization in ASP.NET 7

In this article you will learn how to configure a ASP.NET 7 website to accept several languages.
If you are new to localization, we recommend that you follow in order the steps below.
Localizing a website requires understanding subtle behaviors of the .Net runtime, servers, browsers
and the middleware and localization services that are introduced hereafter.

[Terms and definitions](#terms-and-definitions)

[Prerequisites](#prerequisites)

[Built-in localization](#built-in-localization)

[Two steps for localization](#two-steps-for-localization)

[Step I: Control the Cultures and UICultures](#step-i-control-the-cultures-and-uicultures)

[The Query String Request Culture Provider](#the-query-string-request-culture-provider)

[The Accept Language Header Request Culture Provider](#the-accept-language-header-request-culture-provider)

[The Cookie Request Culture Provider](#the-cookie-request-culture-provider)

[HTML Text Direction and Language attributes](#html-text-direction-and-language-attributes)

[Step II: Content localization](#step-ii-content-localization)

[Localize with IStringLocalizer](#localize-with-istringlocalizer)

[Localize with IHtmlLocalizer](#localize-with-ihtmllocalizer)

[Localize with IViewLocalizer](#localize-with-iviewlocalizer)

[Localize Data Annotations](#localize-data-annotations)

[IStringLocalizerFactory](#istringlocalizerfactory)

[Resource file naming](#resource-file-naming)

[RootNameSpaceAttribute](#rootnamespaceattribute)

[Model binding route data and query strings](#model-binding-route-data-and-query-strings)

[Additional resources](#additional-resources)

## Terms and definitions

Internationalization involves [Globalization](/dotnet/api/system.globalization)
and [Localization](/dotnet/standard/globalization-localization/localization).
Globalization is the process of designing apps to support different cultures. Globalization adds support for input, display,
and output of a defined set of language scripts that relate to specific geographic areas.
Localization is the process of adapting a globalized app to a particular culture/locale.

The process of localizing your app requires a basic understanding of relevant character sets commonly used in software development
as well as an understanding of the issues associated with them. Although computers store text as numbers (codes),
different systems store the same text using different numbers.

[Localizability](/dotnet/standard/globalization-localization/localizability-review) is an intermediate process for verifying
that a globalized app is ready for localization.

The [RFC 4646](https://www.ietf.org/rfc/rfc4646.txt) format for the culture name is `<languagecode2>-<country/regioncode2>`,
where `<languagecode2>` is the language code and `<country/regioncode2>` is the subculture code.
For example, `es-CL` for Spanish (Chile), `en-US` for English (United States), and `en-AU` for English (Australia).
[RFC 4646](https://www.ietf.org/rfc/rfc4646.txt) is a combination of an ISO 639 two-letter lowercase culture code associated
with a language and an ISO 3166 two-letter uppercase subculture code associated with a country or region.

Internationalization is often abbreviated to "I18N". The abbreviation takes the first and last letters and the number of
letters between them, so 18 stands for the number of letters between the first "I" and the last "N".
The same applies to Globalization (G11N), and Localization (L10N).

Summary of terms:

* Globalization (G11N): The process of making an app support different languages and regions.
* Localization (L10N): The process of customizing an app for a given language and region.
* Internationalization (I18N): Describes both globalization and localization.
* Culture: It's a language and, optionally, a region.
* Neutral culture: A culture that has a specified language, but not a region. (for example "en", "es")
* Specific culture: A culture that has a specified language and region. (for example "en-US", "en-GB", "es-CL")
* Parent culture: The neutral culture that contains a specific culture. (for example, "en" is the parent culture of "en-US" and "en-GB")
* Locale: A locale is the same as a culture.

## Prerequisites

Create a simple Razor Pages web app using ASP.NET 7 and name it `RP` in order to be consistent with the code below.
You may refer to the beginning of the get started guide for [Razor Pages](../getting-started/index.md).
While the tutorial uses a Razor Pages website as a basis, we will show MVC localization code whenever
an important distinction occurs.

To implement localization in an application, you need to understand:

1. What is the responsibility and scope of each part of the localization services
1. How to configure your app to have a full control over localization
1. How to store and retrieve your translated content

[View or download the complete sample project](RP/)

## Built-in localization

In .NET, the <xref:System.Globalization.CultureInfo?displayProperty=fullName> object encompasses the
[RFC 4646](https://www.ietf.org/rfc/rfc4646.txt) format and already understands localization.
This is why understanding localization starts with understanding the `CultureInfo` object.
In particular, we will focus on the `CurrentCulture` and `CurrentUICulture`, both of which are handled by `CultureInfo`.

`Culture` refers to operations such as letter casing, formatting of dates and numbers of that specific culture,
whereas `UICulture` represents the current user interface culture used by the Resource Manager to translate your text at run time.

Let's display key `CultureInfo` static properties as well as some dates and a number formatted as a currency using the
`ToString("c")` overload. 
Open `_ViewImports.cshtml` and add a using statement for `System.Globalization`

````cshtml
@using RP
@using System.Globalization
@namespace RP.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
````

Open `index.cshtml` and replace all content with:

````cshtml
@page
@model IndexModel
@{
    var price = 1501999.568m;
}
<!--Place everything inside this div-->
<div class="text-center">
    <strong>CultureInfo</strong>
    <div>CurrentCulture: <span>@CultureInfo.CurrentCulture</span></div>
    <div>CurrentUICulture: <span>@CultureInfo.CurrentUICulture</span></div>
    
    <br />

    <strong>Dates and currency</strong>
    <div>@DateTime.Now.ToLongDateString()</div>
    <div>@DateTimeFormatInfo.CurrentInfo.GetDayName(DateTime.Now.DayOfWeek)</div>
    <div>@CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DateTime.Now.DayOfWeek)</div>
    <div>@price.ToString("c")</div>
    <br />
</div>
````

Run the website and you should see something like this

   ![Show CultureInfo and formats](_static/PartI/cultureinfoformats1.png)

The server returns `en-GB` for both `Culture` and `UICulture`, and dates and currency are in this culture.
Notice how it suffices to use the `ToString("c")` overload in order to display the British Pound currency, placed on the left,
together with the price being truncated to two decimals as well as the presence of commas to separate thousands.
This conforms with the `en-GB` culture format.

The reason the server chose `en-GB` traces back to the OS's display language and regional format,
which in our case is `en-GB` on Windows 10.
You may have a different output culture. Take notice of the culture output in your testing,
because we will see how we circumvent this culture as we carry on. Bear in mind that this is a response
from the server which is independent from the browser's language settings. Indeed, if we set our browser's language to, say,
only `es-ES` (Spanish, Spain), the result will remain `en-GB`.

Now we change the regional format of the OS, say for example by setting it to `fr-FR` (French, France).
On Windows 10 this is done in Settings -> Regional Format. You need to run the server again (Kestrel in our case):

   ![CultureInfo with a French regional format](_static/PartI/cultureinfoformats2.png)

This time, the `CurrentCulture` is changed to `fr-FR`, as well as the date, in French and lower cased,
and the price in Euros, placed on the number's right side, and the 6-digit number spaced according to France's standard.
However, the `UICulture` remains `en-GB`. To change it, we would need to change the entire language display in the OS.

If you have experimented with a change of the regional format and/or display language,
revert to the original configuration for the rest of the tutorial.

## Two steps for localization

The take-away message from the previous section is that the runtime already has built-in features to display
formats for each culture.
But now we need to instruct the server to localize whatever content in the way we or our users choose to.

A complete localization in ASP.NET 7 and ASP.NET Core requires two steps, each of which addresses different parts of the libraries.
The first step is the use of the `AddRequestLocalization` extension service and middleware and the second step is the use
of `AddLocalization` and related extension services to allow you to inject localizers as services that communicate
with your translated content.
Addressing the task in this way is important to highlight the separation of concerns,
and to see how far one can get, one step at the time.

## Step I: Control the Cultures and UICultures

In `Program.cs`, add the localization middleware to the pipeline, after the `UseRouting` middelware

````csharp
app.UseRequestLocalization();
````

This middleware requires a `RequestLocalizationOptions`.
You configure the latter, as a singleton service, by using the `AddRequestLocalization` extension (not `AddLocalization`):

````csharp
builder.Services.AddRequestLocalization(options =>
{
    options.AddSupportedCultures(new[] { "fr-FR", "it-IT", "es-ES" });
    options.AddSupportedUICultures(new[] { "fr-FR", "it-IT", "es-ES" });
});
builder.Services.AddRazorPages();
````

The methods `AddSupportedCultures` and `AddSupportedUICultures` do nothing more than creating a list of `CultureInfo`
objects for the cultures that we provided. To follow along, we recommend that you choose cultures and uicultures
that are different from those of your OS. Therefore, in our case, we did not put in `en-GB`,
or any culture related to English. In addition, and for the moment,
make sure that the browser is not configured for those languages that you added to the service.
On Chromium browsers, this can be done by checking the language tab in the browser's settings.

Now run the server and you should not see any change; it is displaying `en-GB` in our system.
The reason being we did not yet set a default culture, which we purposely leave for later.

## The Query String Request Culture Provider

Now, assuming you are running on port 5001, browse to the following url:

````
https://localhost:5001/?culture=fr-fr&ui-culture=fr-fr
````

   ![Show CultureInfo and formats](_static/PartI/cultureinfoformats3.png)

The middleware properly returned the culture and uiculture we have requested via two query strings.
You or your users now have a way to instruct which culture the server should display. 
Try out all cultures you have set, and it should work. Try out a culture that is not listed, e.g. `ar` (Arabic, neutral language)
and the server will return `en-GB` because it does not find anything else to show you.

Pass in one of the values, `culture` or `ui-culture`, and the query string provider
will set this value to both, so that the above query is equivalent to

```
https://localhost:5001/?culture=fr-fr
```

As a rule, it is important to be specific with the cultures. For example, prefer `fr-FR` or `fr-CA` (for French, Canada)
over the neutral, parent culture `fr`.
Indeed, the latter will lead, for instance, to numbers without currency when using `ToString(“c”)`,
because `fr` is a parent culture of many French cultures and the server cannot choose for you.
However, sometimes it is a good idea to add parent cultures such as `fr` to provide a fallback.
For instance, if you navigate to

`https://localhost:5001/?culture=fr-ca&ui-culture=fr-ca`

the server will return `en-GB`, which is not ideal for users who may not be at ease with English.
But if we add `fr`

````csharp
options.AddSupportedCultures(new[] { "fr-FR", "it-IT", "es-ES", "fr" });
options.AddSupportedUICultures(new[] { "fr-FR", "it-IT", "es-ES", "fr" });
````

we get some generic French formats when we request with `fr-CA`:

   ![Show CultureInfo and formats](_static/PartI/cultureinfoformats4.png)

Notice how the currency symbol is now missing, but at least you make it easier for French Canadians.

Modify the code to set as default language `de-DE` (German, Germany) and run again

````csharp
options.SetDefaultCulture("de-DE");
````

   ![Show CultureInfo and formats](_static/PartI/cultureinfoformats5.png)


Now the displayed information is in German for Germany by default, and no more `en-GB`.
This is important because you are gaining more control on how and when cultures should be used.
If we wish to have `en-GB` as a culture in our app, then it is better to set the culture explicitly in the service
rather than relying on the culture gotten from the server's host.

Before proceeding with the analysis, let's display all the registered cultures, uicultures and the default culture
in the `index.cshtml` page. Go to `_ViewImports.cshtml` and inject the options we have configured

````cshtml
@using Microsoft.Extensions.Options
@using RP
@using System.Globalization
@inject IOptions<RequestLocalizationOptions> RequetLocalizationOptions
@namespace RP.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
````

In the `index.cshtml` paste the following code at the bottom but inside the parent div to keep the text centered:

````cshtml
<strong>Default Culture & UICulture</strong>
<span>@RequetLocalizationOptions.Value.DefaultRequestCulture.Culture,</span>
<span>@RequetLocalizationOptions.Value.DefaultRequestCulture.UICulture</span>

<br />

<strong>Cultures</strong>
@if(RequetLocalizationOptions.Value.SupportedCultures is not null)
@foreach (var culture in RequetLocalizationOptions.Value.SupportedCultures)
{
    <span>@culture,</span>
}

<br />

<strong>UICultures</strong>
@if(RequetLocalizationOptions.Value.SupportedUICultures is not null)
@foreach (var uiculture in RequetLocalizationOptions.Value.SupportedUICultures)
{
    <span>@uiculture,</span>
}

<br />
````

The `QueryStringRequestCultureProvider` which allowed you to get the culture from query strings is the first among three providers
registered automatically by the service. The two others are, in the following order, the `CookieRequestCultureProvider` and
`AcceptLanguageHeaderRequestCultureProvider`. We discuss these later.
This default list goes from most specific to least specific.
If none of the providers can determine the request culture, the `DefaultRequestCulture` is used (`de-DE` in our case),
if it has been set, otherwise it will use the host's culture (`en-GB` in our case).

Display the providers by adding the following to the `index.cshtml` page:

````cshtml

 <br />

 <strong>Request Culture Providers</strong>
 @foreach (var provider in RequetLocalizationOptions.Value.RequestCultureProviders)
 {
     <div>@provider.GetType().Name</div>
 }
````

In addition, it is possible to know which provider the middleware selected as a "winner" among the three.
Since this is decided during the HTTP pipeline, we need to invoke the `HttpContext` (or `Context` in MVC).
Add a using `@using Microsoft.AspNetCore.Localization` and add the following to the `index.cshtml` page:

````cshtml
<br />
@{
    var requestCultureFeature = HttpContext.Features.Get<IRequestCultureFeature>();
    var winner = requestCultureFeature?.Provider?.GetType().Name ?? "There is no winner!";
}
<strong>Winner provider: </strong><span>@winner</span>
<br />
<strong>RequestCulture from HTTP:</strong>
<span>@requestCultureFeature?.RequestCulture.Culture, @requestCultureFeature?.RequestCulture.UICulture</span>
````

Notice that we also display the culture that has been selected during the HTTP pipeline. You can view the `RequestCulture`
object as a holder of two properties, named `Culture` and `UICulture` which are nothing more than `CultureInfo` objects.

Running the app, you should get something like this:

  ![Show CultureInfo, formats and providers](_static/PartI/cultureinfoformats6.png)

As you can see, the HTTP pipeline outputs `de-DE` as a culture. But there is no winner provider (`null`). This is expected
because we are using the default and not querying. 

Now query with `de-DE`, `https://localhost:5001/?culture=de-DE&ui-culture=de-DE`.
There is no winner provider neither. This time the reason is we did not register `de-DE` as a supported culture or uiculture.
You can remediate this by adding this culture in the service when using `options.AddSupportedCultures` or
`options.AddSupportedUICultures`.
This will simplify your code when using cookies as we will see later.
However, for learning purposes we make explicit the separation of concerns
by leaving the default language out of the supported cultures and uicultures.

If now you query a registered culture, e.g., `https://localhost:5001/?culture=fr&ui-culture=fr`,
you get the `QueryStringRequestCultureProvider` as a winner

   ![Show providers and winner](_static/PartI/cultureinfoformats7.png)

You can remove the query string provider `options.RequestCultureProviders.RemoveAt(0);`, and querying won't work anymore.

You can also customize the query string provider.

````csharp
options.RequestCultureProviders
.Add(new QueryStringRequestCultureProvider() { QueryStringKey = "lang", UIQueryStringKey = "uilang" });
````

so that you can query like this `https://localhost:5001/?lang=it-it`.

In general, using query strings to localize your application is practical for testing, but it is not recommended in production,
see [Managing multi-regional sites](https://developers.google.com/search/docs/advanced/crawling/managing-multi-regional-sites?hl=en&visit_id=637654323788088100-3821504457&rd=1#locale-specific-urls).

## The Accept Language Header Request Culture Provider

We come back to the initial service configuration:

````csharp
builder.Services.AddRequestLocalization(options =>
{
    options.AddSupportedCultures(new[] { "fr-FR", "it-IT", "es-ES", "fr" });
    options.AddSupportedUICultures(new[] { "fr-FR", "it-IT", "es-ES", "fr" });
    options.SetDefaultCulture("de-DE");
});
````
In particular, we have set `es-ES` (Spanish, Spain) as a culture.
Now, we set the browser to accept a new language, and our choice precisely is `es-ES`, in addition to `en-GB`.
Depending on the browser, there might be no need to set this new culture as the default for the browser.
We recommend that you choose a culture in the browser's settings that you know you do not have on the machine.

Just refresh your browser (if the server is already running),
and the language displayed is `es-ES` and not the default `de-DE` (or `en-GB` if no default was set):

   ![Show providers](_static/PartI/cultureinfoformats8.png)

This is the role of the header request provider, it found `es-ES` so it skips the default.
Check that the Winner provider is indeed `AcceptLanguageHeaderRequestCultureProvider`.

The [Accept-Language header](https://www.w3.org/International/questions/qa-accept-lang-locales) is settable
in most browsers and was originally intended to specify the user's language.
This setting indicates what the browser has been set to send or has inherited from the underlying operating system.
The Accept-Language HTTP header from a browser request isn't an infallible way to detect the user's preferred language
(see [Setting language preferences in a browser](https://www.w3.org/International/questions/qa-lang-priorities.en.php)).
A production app should include a way for users to customize their choice of culture. We do this below.

[Content-Language](https://developer.mozilla.org/docs/Web/HTTP/Headers/Content-Language) is another language header.
This header is used to describe the language(s) intended for the audience, and allows a user to differentiate according to
the users' own preferred language. The `Content-Language` header can be added by setting the property:

````csharp
options.ApplyCurrentCultureToResponseHeaders = true;
````

The `RequestLocalizationMiddleware` will set the `Content-Language` header with the
`CurrentUICulture`. This also eliminates the need to set the response header `Content-Language` explicitly.

Notice that this header can be added regardless of the presence of
`AcceptLanguageHeaderRequestCultureProvider`.
Test this by removing all providers altogether `options.RequestCultureProviders.Clear();`

## The Cookie Request Culture Provider

Use the following configuration:

````csharp
builder.Services.AddRequestLocalization(options =>
{
    options.AddSupportedCultures(new[] { "fr-FR", "it-IT", "es-ES" });
    options.AddSupportedUICultures(new[] { "fr-FR", "it-IT", "es-ES" });
    options.SetDefaultCulture("de-DE");
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider { CookieName = ".Contoso.Culture" });
});
````

We cleared all request culture providers to focus only on the cookie culture provider.
Clearing all providers prior to adding those you need is a good strategy because it eliminates the risk of
unwanted middleware behavior. For example, we have seen above how the mere presence of the accept language header provider
in `RequestCultureProviders` can lead to an undesired result for users with peculiar browsers' culture settings
(that they may not even be aware of).

We also created a new cookie request culture provider, and set its name to `.Contoso.Culture`.
The default cookie name is `.AspNetCore.Culture`.

Notice that we have removed the parent culture `fr`. This is because we want users to pick up a culture from a list of cultures,
and this is the only way for them to set the language. This makes our mechanism deterministic by eliminating culture fallbacks.
A Canadian French user will choose `fr-FR` if no other French language is available, and thus will expect the
formatting system to be the one used in France.
Providing this user with a generic `fr` will lead to inconsistencies in formatting, as we have seen with currencies, for example.
Of course, ideally, you would provide French Canadian users with `fr-CA`, to be added into the configuration.

At this stage, the query string provider and accept language header provider will not work.
In addition, as such the cookie request provider will do nothing until we instruct in code how to persist the culture cookie.
For this, we can provide some code to allow the user to select a culture among the list of cultures we have registered, and a cookie
will be persisted in the browser containing the information about that culture. We assume that you are familiar with adding a
Razor Page, a partial View, and familiar with Model Binding.

Create a partial view, named `_LanguagePartial.cshtml` and put it in the Shared folder.

````cshtml
@using Microsoft.AspNetCore.Localization;
@{
    var usedCulture = Context.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture;

    // Set returnUrl to where the user was.
    // However, if the user clicks several times the language button, set the returnUrl to root
    var path = @Context.Request.Path.ToUriComponent();
    var returnUrl = path == "/Language" ? "/" : path;
}
<ul class="nav nav-pills">
    <li class="nav-item">
        <a class="btn btn-success" asp-area="" asp-page="/Language"
        asp-route-returnUrl="@returnUrl">@usedCulture?.NativeName | @usedCulture?.EnglishName</a>
    </li>
</ul>
````

Notice how we are using features already discussed previously. However, since we are working on a partial View,
we use `Context` and not `HttpContext`, the former being an MVC naming convention. Besides this, we have a `returnUrl`
which will bring back the users to the page they came from. Eventually,
we display each culture both in its native language as well as in English.

Invoke this partial View in `_Layout.cshtml`:

````cshtml
<div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
    <ul class="navbar-nav flex-grow-1">
        <li class="nav-item">
            <a class="nav-link text-dark" asp-area="" asp-page="/Index">Home</a>
        </li>
        <li class="nav-item">
            <a class="nav-link text-dark" asp-area="" asp-page="/Privacy">Privacy</a>
        </li>
    </ul>
    <partial name="_LanguagePartial" />
</div>
````

`_LanguagePartial` requires a `Language` page to route to. Create a Razor Page named `Language` and place it next to the Index page.
In `Language.cshtml`, paste the following code:

````cshtml
@page
@using Microsoft.AspNetCore.Localization
@model RP.Pages.LanguageModel
@{
    var setCultureName = HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.Name;
    var languages = new List<CultureInfo>();
    languages.Add(RequetLocalizationOptions.Value.DefaultRequestCulture.Culture);
    languages.AddRange(RequetLocalizationOptions.Value.SupportedCultures);
}

<div class="container">
    <div class="row">
        <div class="col-6 offset-3">
            <h4>Set the language here</h4>
            @foreach (var item in languages)
            {
                var btnColor = @item.Name == @setCultureName ? "btn-success" : "btn-secondary";
                <form asp-route-lang="@item.Name" asp-route-returnUrl=@Model.ReturnUrl method="post">
                    <button type="submit" class="btn btn-block @btnColor">@item.NativeName | @item.EnglishName</button>
                </form>
            }
        </div>
    </div>
</div>
````

Notice how the `languages` variable is built up from the `DefaultRequestCulture` (`de-DE`) and the supported cultures.
As mentioned earlier, this is because we did not include the default culture as a supported culture.
In practice, you probably do not want this, and instead put the default culture as part of other cultures your site supports.
In this case, the language variable will be `SupportedCultures` (or `SupportedUICultures` if you prefer), which will result
in a simpler code.

Next, the culture selected by the user is sent to the page as a `POST` request together with binding route parameters,
`lang` and `returnUrl`. In the code behind, `Language.cshtml.cs`, paste the following:

````csharp
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace RP.Pages;
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
````

We are injecting the Singleton culture service to retrieve the `CookieRequestCultureProvider` and make use of
the static method `CookieRequestCultureProvider.MakeCookieValue` to build the culture cookie.
We chose a session cookie but leave commented code to show how you can persist the cookie for a year.

Run the app, pick a language, and navigate through pages. Check the Contoso cookie in the browser's dev tools.
The cookie format is `c=%LANGCODE%|uic=%LANGCODE%`, where `c` is `Culture` and `uic` is `UICulture`, for example:

````
c=en-UK|uic=en-US
````

Index page

   ![cookie provider](_static/PartI/cookieprovider1.png)

Language page navigated to

   ![cookie provider](_static/PartI/cookieprovider2.png)

Redirect to Index with a new language set by the cookie

   ![cookie provider](_static/PartI/cookieprovider3.png)

## HTML Text Direction and Language attributes

You can declare the text direction `dir` and language `lang` in the `html` tag, in *_Layout.cshtml*:

````cshtml
@using Microsoft.AspNetCore.Localization
<!DOCTYPE html>
@{
    var requestCulture = Context.Features.Get<IRequestCultureFeature>()?.RequestCulture ??
    RequetLocalizationOptions.Value.DefaultRequestCulture;
    var lang = requestCulture.Culture.Name;
    var dir = requestCulture.Culture.TextInfo.IsRightToLeft ? "rtl" : "ltr";
}
<html dir="@dir" lang="@lang">
````

You can test this with, e.g., the `ar-LB` (Arabic, Lebanon) culture:

````csharp
builder.Services.AddRequestLocalization(options =>
{
    options.AddSupportedCultures(new[] { "fr-FR", "ar-LB" });
    options.AddSupportedUICultures(new[] { "fr-FR", "ar-LB" });
    options.SetDefaultCulture("de-DE");
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider { CookieName = ".Contoso.Culture" });
});
````
Run and watch the direction going from left-to-right to right-to-left as you switch to `ar-LB`.
View the page source in the browser. Get further information in, e.g.
[Structural markup and right-to-left text in HTML](https://www.w3.org/International/questions/qa-html-dir#:~:text=The%20dir%20attribute%20is%20used,Ko%2C%20Syriac%2C%20and%20Thaana.)

## Step II: Content localization

In Step I, the configuration of `RequestLocalizationOptions` allowed you to establish a solid basis for the localization process.
We could define unambiguously the cultures we want to be available in the application. Users can choose among languages,
and you have already built-in localization of important content, such as dates, numbers, calendars, and currencies.
But what about translating the simple word "Hello", or any other specific content you need in your application? The responsibility of
the `RequestLocalizationOptions` as well as what we went through in Step I, stops here.

Assume that you have a set of content, such as a word, sentences, error or greeting messages as well as entire paragraphs,
sections or page which is specific to your application and that you need to be translated for users.
The standard strategy is to put such content in files or inside a database, for every culture that your application supports.
We call these resource files, or resources in general. The .NET ecosystem has an established mechanism for dealing with
internationalization, in particular through the [Resource Manager](/dotnet/core/extensions/retrieve-resources) to manage
[Resources](/dotnet/core/extensions/resources) and [Resource files](/dotnet/core/extensions/create-resource-files).
You can decide to choose among virtually any resource files format starting with simple text files. In .NET it is common to
use the so called *.resx* files, which unlike text files, can store not only strings, but also binary data such as images,
icons, and audio clips, and programmatic objects. Those three links are provided in case you wish to know more
about this well-established technology. However, in practice, you rarely need to know about the internals of the Resource Manager.
In ASP.NET 7 and prior versions of ASP.NET Core, the resource manager is abstracted away using interfaces.
Such interfaces have a convenient indexer and an `IEnumerable` for returning all localized strings.
Because such interfaces do not require storing the default language strings in a resource file,
you can develop an app targeted for localization without the need to create resource files early in development.

Suppose that your application should target French users in France
and German users in Germany. The default culture is German in Germany.
In addition, your team has mainly English-speaking developers,
so, you want to build the application in English and create resource files for French and German only.
When the development team is done, translators will translate all English content to German and French.

This means that Step I concludes with the following configuration

````csharp
builder.Services.AddRequestLocalization(options =>
{
    options.AddSupportedCultures(new[] { "fr-FR" });
    options.AddSupportedUICultures(new[] { "fr-FR" });
    options.SetDefaultCulture("de-DE");
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider { CookieName = ".Contoso.Culture" });
});
````

You need to store the resource files somewhere, say in the folder *MyResources*. For this, create the *MyResources* folder,
and add the extension

````csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "MyResources");
````
We discuss and use this extension in the next section. For the moment, you need to decide how to organize your resource files.
Your application supports two languages, `de-DE` by default and `fr-FR`,
so you will need **at minimum one resource file per culture**.
As we will see later, you could provide, for each culture, resource files for each razor page, each
controller or area. For now, we will consider just one file per culture. We will expand later this important remark,
because you will have to take important structural decisions for your resource files.

Create two *.resx* files, which you can easily do with Visual Studio, *Add New Item --> resource*. Call the first file
*SharedResources.de-DE.resx* and the second file *SharedResources.fr-FR.resx*.
By "SharedResources" we indicate to developers that content from such files may be injected in any class,
controller page or partial view.
Let's say you want to translate the sentence "Good morning", and the sentence "Welcome, John Doe", where "John" and "Doe" are
placeholders for the logged user's first and last names, which you would retrieve from an identity provider.

Edit the `de-DE` resource file and put as Keys and Values:

| Key | Value |
| --- | ----- |
| `Good morning` | `Guten Morgen` |
| `Welcome, {0} {1}` | `Willkommen {0} {1}` |


   ![Resx file for the German, Germany culture](_static/PartII/resxfilegerman.png)

Notice the comment section in the *.resx* file, which you can use for translators or other developers.

Similarly, for the `fr-FR` culture:

| Key | Value |
| --- | ----- |
| `Good morning` | `Bonjour` |
| `Welcome, {0} {1}` | `Bienvenue {0} {1}` |

## Localize with IStringLocalizer

The `AddLocalization` extension configures two services. The first service is a concrete implementation of the
generic interface `IStringLocalizer<T>`, as a transient, and the second is a concrete implementation of `IStringLocalizerFactory`,
as a singleton. <xref:Microsoft.Extensions.Localization.IStringLocalizer%601> is one of the interfaces that we mentionned earlier;
namely, they wire up with the Resource Manager and Reader and provide you with an indexer to retrieve content that
you will put in the *MyResources* folder.

Since we want to make the sentences above available anywhere, we need to create
a class with the same base name as the resource files and use it as a type for the generic interface `IStringLocalizer<T>`.
The namespace for this class must be the namespace of the project, `RP` in our case.
In our scenario we put this class in the
*MyResources* folder. Create `SharedResources.cs` in this folder and rename the namespace if need be:

````csharp
/// <summary>
/// Object class referring to resources to be used anywhere 
/// </summary>
namespace RP;
public class SharedResources
{
}
````

By default, ASP.NET 7 as well as versions of ASP.NET Core above 3.0 will not read `SharedResources.cs` if it is not placed
at the root of the project. This is due to a
![breaking change behavior of MSBuild since ASP.NET Core 3.0](/dotnet/core/compatibility/3.0#resource-manifest-file-name-change).
Since we want to keep `Sharedresources.cs` in *MyResources* we need to instruct MSBuild to use the old convention. This can be done
in several ways (see link above), the simplest way being to edit the `.csproj` file by setting
`EmbeddedResourceUseDependentUponConvention` to false

````csproj
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <Nullable>enable</Nullable>
	  <EmbeddedResourceUseDependentUponConvention>false</EmbeddedResourceUseDependentUponConvention>
  </PropertyGroup>
````
Once this is done, you can inject `IStringLocalizer<SharedResources>`, for example in `_ViewImports.cshtml`

````cshtml
@using Microsoft.Extensions.Localization
@inject IStringLocalizer<SharedResources> SharedLocalizer
````

Display the following strings from the resource files at the beginning of `index.cshtml` by using the indexer or the indexer
with parameters as C# objects for placeholders:

````cshtml
    <!--Part II-->
    <strong>From SharedLocalizer</strong>
    <div>@SharedLocalizer["Good morning"]</div>
    <div>@SharedLocalizer["Welcome, {0} {1}", "Joe", "Doe"]</div>
    <div>@SharedLocalizer["I am not yet in the resx"]</div>

    <br />
    <br />
````

Run the app

   ![Show translated strings, German culture](_static/PartII/sharedlocalizer.png)

The first two string are automatically returned in the German version. Switch to French to check out the French version.
Apart from your localized strings, the localizer will return any string in the indexer -as is- when it is not found
in the *.resx* file. This is the case for the string "I am not yet in the resx".
This means that your developers can continue working on the website without any translation, but they can prepare the website for
translation by putting the strings as indices in the localizer. Bear in mind, though,
that sooner or later you will have long strings, such as paragraphs.
In this case you use a short but otherwise evocative key names, such as

````cshtml
@SharedLocalizer["IntroductionKey"]
````

and you would use as a value the text to be translated.
For very long text, such as those found in e.g., *Terms and Conditions*, it might be better to leave them out of the
localization library, not include them in the *resx* files,
and rather render translated pages, with URLs for French and German.

You can inject the `SharedLocalizer` virtually anywhere. Try it out in the `IndexModel.cs` file, or a controller class:

````csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace RP.Pages;
public class IndexModel : PageModel
{
    private readonly IStringLocalizer<SharedResources> _sharedLocalizer;

    public IndexModel(IStringLocalizer<SharedResources> sharedLocalizer)
    {
        _sharedLocalizer = sharedLocalizer;
    }

    public LocalizedString Message { get; private set; }
    public LocalizedString AnotherMessage { get; private set; }

    public void OnGet()
    {
        Message = _sharedLocalizer["Good morning"];
        AnotherMessage = _sharedLocalizer["I am not in the resx"];
    }
}
````

and output in the `Index.cshtml` view the strings as well as self-explanatory debug information

````cshtml
<strong>From SharedLocalizer IndexModel</strong>
<div>Key :@Model.Message?.Name</div>
<div>Value: @Model.Message</div>
<div>ResourceNotFound ? @Model.Message?.ResourceNotFound</div>
<div>SearchedLocation: @Model.Message?.SearchedLocation</div>

<br />

<div>Key : @Model.AnotherMessage?.Name</div>
<div>Value: @Model.AnotherMessage</div>
<div>ResourceNotFound ? @Model.AnotherMessage?.ResourceNotFound</div>
<div>SearchedLocation: @Model.AnotherMessage?.SearchedLocation</div>

<br />
<br />
````

   ![Show translated strings, German culture](_static/PartII/sharedlocalizer2.png)

Combining `IStringLocalizer` with a shared resource class allows for great flexibility. For instance,
you can use it for localizing strings in layout or partial views.
The `SharedLocalizer` being injected in `_ViewImports.cshtml`, you can set the following footer in `_Layout.cshtml`

````cshtml
<footer class="border-top footer text-muted">
    <div class="container">
        &copy; 2021 - RP - <a asp-area="" asp-page="/Privacy">Privacy</a> @SharedLocalizer["Good morning"]
    </div>
</footer>
````

Some developers prefer to organize their resource files per page, view, controller or area. Therefore, instead of putting in all
content to be translated in one shared resource file, per culture, you would have instead several resource files for the different
parts of your application.

For example, imagine you want to use a specific resource file for the *Privacy* page.
Where would you place the resource files, and how would you name them?
In the *Privacy* page, inject `IStringLocalizer` with the Type `PrivacyModel` instead of `SharedResources`.
As before, you can inject in the `Privacy.cshtml.cs` file, or in the Razor View `Privacy.cshtml`, as shown:

````cshtml
@inject IStringLocalizer<PrivacyModel> PrivacyLocalizer
````

You could inject in the `_ViewImports.cshtml`, but it defeats the purpose of having this transient service for the Privacy page only.
Now display the localizer results in the `Privacy.cshtml`

````cshtml
@page
@model PrivacyModel
@inject IStringLocalizer<PrivacyModel> PrivacyLocalizer

<strong>From PrivacyLocalizer</strong>
<div>Key :@PrivacyLocalizer["Hello"].Name</div>
<div>Value: @PrivacyLocalizer["Hello"]</div>
<div>ResourceNotFound ? @PrivacyLocalizer["Hello"].ResourceNotFound</div>
<div>SearchedLocation: @PrivacyLocalizer["Hello"].SearchedLocation</div>
````

Run, navigate to the privacy page and observe that the resource is not found (you do not have the necessary resource files, yet).
Focus on the searched location, `RP.MyResources.Pages.PrivacyModel`.

   ![Privacy localizer](_static/PartII/privacylocalizer.png)

Therefore, you need to create a *Pages* folder inside *MyResources* and put in two *.resx* files, `PrivacyModel.de-DE.resx`
and `PrivacyModel.fr-FR.resx`. Add the key "Hello" together with the translated values ("Hallo" for `de-DE` and "Salut" for `fr-FR`).
Your resources folder should look like this:

   ![Resources in solution explorer](_static/PartII/solutionexplorerprivacy.png)

Run and check that the key "Hello" is now translated.

Alternatively, you can skip creating the *Pages* folder inside *MyResources* and use instead the `Dot` convention for
naming files and place
such resources directly in *MyResources*. For instance, you could name your file `Pages.PrivacyModel.de-DE.resx` and place it in
*MyResources*. The structure would look like this:

   ![Resources in solution explorer](_static/PartII/solutionexplorerprivacy2.png)

For Razor Pages in folders, e.g., a *Movies* folder, you could create a subfolder *MyResources/Pages/Movies*
to hold the resource files. For the index page of the movies folder, you would refer to the namespace where that file lives,
for instance, you would inject as such:

````cshtml
@inject IStringLocalizer<Movies.IndexModel> MoviesLocalizer
````
See the [Resource file naming section](#resource-file-naming) for a general overview of the different notation conventions.

## Localize with IHtmlLocalizer

`IStringLocalizer` cannot be used to localize content that contains HTML. 
Instead, use the `IHtmlLocalizer<T>` implementation for resources that contain HTML.

This service can be injected as such

````csharp
builder.Services.AddRazorPages()
    .AddViewLocalization();
````

`IHtmlLocalizer` HTML encodes arguments that are formatted in the resource string,
but does not HTML encode the resource string itself. In the sample highlighted below,
only the value of the `name` parameter is HTML encoded.

Other than that, the patterns for using `IHtmlLocalizer` are like those for `IStringLocalizer`:

````csharp
ViewData["Message"] = _htmlLocalizer["<b>Hello</b><i> {0}</i>", name];
````
> [!NOTE]
> Generally, only localize text, not HTML.

## Localize with IViewLocalizer

The `IViewLocalizer` interface inherits `IHtmlLocalizer` and is provided via the same `AddViewLocalization` extension.
This extension adds an implementation of `IHtmlLocalizerFactory` as a singleton and two transient services which are
implementations of `IHtmlLocalizer` and `IViewLocalizer`.
The `IViewLocalizer` interface service provides localized strings for a [view](xref:mvc/views/overview).
The `ViewLocalizer` class implements this interface and finds the resource location from the view file path.
There is no option to use a global shared resource file.

As an example, you can use this interface to localize content in the layout file without resorting to a `Sharedresources.cs` file.
Open `_ViewImports.cshtml` and inject the service.

````cshtml
@inject IViewLocalizer ViewLocalizer
````

We will use the `Dot` convention to locate the resource file for the `_Layout.cshtml` file.
Therefore, in *MyResources*, create a file named `Pages.Shared._Layout.de-DE.resx`,
and another one named `Pages.Shared._Layout.fr-FR.resx`.

   ![Resources in solution explorer](_static/PartII/solutionexplorerprivacy3.png)

For a key, use "Goodbye" and for the value use "Auf Wiedersehen" for `de-DE` and "Au revoir" for `fr-FR`.
Use the following as a footer in `_Layout.cshtml` and run.

````cshtml
<footer class="border-top footer text-muted">
    <div class="container">
        &copy; 2021 - RP - <a asp-area="" asp-page="/Privacy">Privacy</a> @SharedLocalizer["Good morning"]
        | @ViewLocalizer["Goodbye"]
    </div>
</footer>
````

In this footer example you have two ways of localizing strings. The shared localizer based on `IStringLocalizer` with the class type
`SharedResources.cs` that we have built before, and the `ViewLocalizer` implementation of `IViewLocalizer`.

## Localize Data Annotations

You can localize [data annotation](/aspnet/core/tutorials/razor-pages/validation)
messages during form validation by using the `AddDataAnnotationsLocalization` extension,
which is an extension on `IMvcBuilder` in the same way as `AddViewLocalization`.
You can localize by using a shared resource file,
in the same way as we have done in [Localize with IStringLocalizer](#localize-with-istringlocalizer). This is convenient
because, usually, translations for data annotations are needed in more than one page. We could use the same `SharedResources.cs` and
put the data annotation localized strings in the corresponding `resx` files we already have. However, in order to keep it more general, we
will set the resources in files.

Therefore, create a `DataAnnotationResources.cs` file in the *MyResources* folder

````csharp
/// <summary>
/// Object class referring to resources to be used for data annotations
/// </summary>
namespace RP;
public class DataAnnotationResources
{
}
````

In `Program.cs`, use the following:

````csharp
builder.Services.AddRazorPages()
        .AddViewLocalization()
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
                factory.Create(typeof(DataAnnotationResources));
        });
````

`AddDataAnnotationsLocalization` now adds support for localized `DataAnnotations`
validation messages through `IStringLocalizer` abstractions and uses the `SharedResources` resource files.

> [!NOTE]
> You can use `AddDataAnnotationsLocalization` without `AddViewLocalization`.
In our code example we keep `AddViewLocalization` because it is already used to display localized information in the footer. 

The default ASP.NET 7 template comes with a `_ValidationScriptsPartial.cshtml` file in the *Shared* folder. This partial view
loads `jquery-validation` and `jquery-validation-unobtrusive` Javascript files for client-side validation.
Check if this partial view is not already invoked, usually in the `_Layout.cshtml` file,
otherwise invoke it after the JQuery files, towards the end of the body tag in `_Layout.cshtml`:

````cshtml
     <partial name="_ValidationScriptsPartial" />
````

Put a simple form in the *Privacy* page, both in the view and the code behind. Open *Privacy.cshtml.cs* and replace the entire
code with

````csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace RP.Pages;
public class PrivacyModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public void OnGet()
    {
    }
    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            return LocalRedirect("/");
        }
        return Page();
    }
}

public class InputModel
{
    [Required(ErrorMessage = "Required")]
    [MinLength(3, ErrorMessage = "{1} characters or more")]
    [Display(Prompt = "Username", Name = "Your Username")]
    public string UserName { get; set; } = null!;
}
````

The `InputModel` class plays the role of a *ViewModel* and is the one we need to provide translations for.
Add the following key/value pairs to *SharedResources.de-DE.rex*

| Key | Value |
| --- | ----- |
| `Required` | `Pflichtfeld` |
| `{1} characters or more` | `Mehr als {1} Zeichen` |
| `Username` | `Nutzername` |
| `Your Username` | `Ihre Benutzername` |

Add the following key/value pairs to *SharedResources.fr-FR.rex*

| Key | Value |
| --- | ----- |
| `Required` | `Requis` |
| `{1} characters or more` | `Plus de {1} caractères` |
| `Username` | `Pseudo` |
| `Your Username` | `Votre nom d'utilisateur` |


In the *Privacy.cshtml* view, replace all code with:

````cshtml
@page
@model PrivacyModel
@inject IStringLocalizer<PrivacyModel> PrivacyLocalizer

<!--Part II DataAnnotation-->
<div class="row">
    <div class="col-md-4">
        <form method="post">
            <h4>Try validation</h4>
            <hr />
            <div asp-validation-summary="All" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="Input.UserName"></label>
                <input asp-for="Input.UserName" class="form-control" />
                <span asp-validation-for="Input.UserName" class="text-danger"></span>
            </div>
            <button type="submit" class="btn btn-primary">Register</button>
        </form>
    </div>
</div>
<br />
<br />
<!--Part I-->
<strong>From PrivacyLocalizer</strong>
<div>Key :@PrivacyLocalizer["Hello"].Name</div>
<div>Value: @PrivacyLocalizer["Hello"]</div>
<div>ResourceNotFound ? @PrivacyLocalizer["Hello"].ResourceNotFound</div>
<div>SearchedLocation: @PrivacyLocalizer["Hello"].SearchedLocation</div>
````

Run and navigate to the *Privacy* page, edit the input form and trigger the validation messages.
Change language and try again. You should see something like this:

   ![Show data annotations](_static/PartII/dataannotation.png)

   ![Show data annotations](_static/PartII/dataannotation2.png)


You can override the `jquery-validation-unobtrusive` CSS styling classes by removing the `class="text-danger"` in the `span` tag
and use, e.g.:

````CSS
<style>
    .input-validation-error {
        border-color: blue;
        background-color: pink;
    }
    .field-validation-error {
        color: darkorange;
        word-break: break-all;
    }
</style>
````

<!--Decimal comma jquery issue-->
[!INCLUDE[](~/includes/localization/currency.md)]

## IStringLocalizerFactory

At the lowest level, you can retrieve `IStringLocalizerFactory` out of [Dependency Injection](dependency-injection.md).
For example, you can configure a singleton service `MyService`:

````csharp
public class MyService
{
    private readonly IStringLocalizer _localizer;

    public MyService(IStringLocalizerFactory factory)
    {
        var type = typeof(SharedResources);
        _localizer = factory.Create(type);
    }
}
````

The `Create` method returns an `IStringLocalizer` and has an overload:

````csharp
        var type = typeof(SharedResources);
        var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
        _localizer = factory.Create(nameof(SharedResources), assemblyName.Name);
````

## Resource file naming

Resource file naming has been introduced "hands-on" in [Step II: Content localization](#step-II-content-localization).
We summarize in more general terms. Resources are named for the full type name of their class minus the assembly name.
For example, a French resource in a project whose main assembly is `LocalizationWebsite.Web.dll` for the class
`LocalizationWebsite.Web.Startup` would be named *Startup.fr.resx*.
A resource for the class `LocalizationWebsite.Web.Controllers.HomeController` would be named
*Controllers.HomeController.fr.resx*.
If your targeted class namespace is not the same as the assembly’s name you will need the full type name.
For example, a resource for the type `ExtraNamespace.Tools` would be named *ExtraNamespace.Tools.fr.resx*.

In this tutorial we have set the `ResourcesPath` to *MyResources*,
so, the path for the Home's controller French resource file can be either of the following:

* *MyResources/Controllers.HomeController.fr.resx*

* *MyResources/Controllers/HomeController.fr.resx*.

If you do not set the `ResourcesPath` option, the *.resx* file would go in the base directory of the project and
the resource file for `HomeController` would be named *Controllers.HomeController.fr.resx*.
The choice of using the `Dot` or `Path` naming convention depends on how you want to organize your resource files.

| Resource path/name | Dot or Path naming |
| ------------   | ------------- |
| Resources/Controllers.HomeController.fr.resx | Dot  |
| Resources/Controllers/HomeController.fr.resx  | Path |

Resource files using `@inject IViewLocalizer` in Razor views follow a similar pattern.
Razor view resource files mimic the path of their associated view file.
Assuming we set the `ResourcesPath` to *MyResources*, the French resource file associated with the
*Views/Home/About.cshtml* view could be either of the following:

* *MyResources/Views/Home/About.fr.resx*

* *MyResources/Views.Home.About.fr.resx*

If you do not use the `ResourcesPath` option, the *.resx* file for a view would be in the same folder as the view.


When searching for a resource, localization engages in culture fallback.
Starting from the requested culture, if not found, it reverts to the parent culture of that culture.

For example, if your site has a default culture set to `de-DE` and receives a request with the culture `fr-CA`,
the localization system looks for the following resources in order, and selects the first match:

* *Welcome.fr-CA.resx*
* *Welcome.fr.resx*
* *Welcome:de-DE.resx*

> [!NOTE]
> If you create a resource file in Visual Studio without a culture in the file name
(for example, *Welcome.resx*), Visual Studio will create a C# class with a property for each string.
When you create a *.resx* file with a culture in the file name, Visual Studio will not generate the class file.

## RootNameSpaceAttribute

The <xref:Microsoft.Extensions.Localization.RootNamespaceAttribute> attribute provides the root namespace of an assembly
when the root namespace of an assembly is different from the assembly’s name. 
This attribute is useful when a project's name is not a valid .NET identifier.
For instance, `my-project-name.csproj` will use the root namespace `my_project_name` and the assembly’s name `my-project-name`.
In such a situation, localization does not work by default and
fails due to the way resources are searched for within the assembly.
`RootNamespace` is a build-time value which is not available to the executing process. 
You can rectify this by pointing to the resource folder name and root namespace in the *AssemblyInfo.cs* file:

```csharp
using System.Reflection;
using Microsoft.Extensions.Localization;

[assembly: ResourceLocation("My Resource Folder Name")]
[assembly: RootNamespace("App My Root Namespace")]
```

## Model binding route data and query strings

See [Globalization behavior of model binding route data and query strings](xref:mvc/models/model-binding#glob).

## Additional resources

* [Sample project](RP/)
* [Managing multi-regional sites](https://developers.google.com/search/docs/advanced/crawling/managing-multi-regional-sites?hl=en&visit_id=637654323788088100-3821504457&rd=1#locale-specific-urls)
* [Structural markup and right-to-left text in HTML](https://www.w3.org/International/questions/qa-html-dir#:~:text=The%20dir%20attribute%20is%20used,Ko%2C%20Syriac%2C%20and%20Thaana.)
* <xref:fundamentals/troubleshoot-aspnet-core-localization>
* [Globalizing and localizing .NET applications](/dotnet/standard/globalization-localization/index)
* [Resources in .resx Files](/dotnet/framework/resources/working-with-resx-files-programmatically)
* [Microsoft Multilingual App Toolkit](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308)
* [Localization & Generics](http://hishambinateya.com/localization-and-generics)
