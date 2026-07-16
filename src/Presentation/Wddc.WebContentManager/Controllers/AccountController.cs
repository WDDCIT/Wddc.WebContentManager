using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Controllers
{
    /// <summary>
    /// Windows Integrated Authentication has no real "sign out" — the browser silently
    /// re-sends the cached Windows credentials on every request, so clearing the session
    /// and redirecting just logs the same user straight back in.
    ///
    /// Flow (ported from Wddc.WebApp, where it is battle-tested):
    /// SignOut clears the session, remembers who signed out (hashed, short-lived cookie)
    /// and lands on the SignedOut page (no prompt). The "Log in" button goes through
    /// Login → LoginChallenge, which rejects the signed-out account with 401 + fresh
    /// challenge so the native Windows credential prompt appears. A different account
    /// is accepted immediately; the SAME account is accepted once it provably came from
    /// the prompt — either the same auth scheme completed twice in one cycle (browsers
    /// try default credentials only once per scheme per navigation) or the arrival came
    /// MinTypedDelay after the first challenge. On acceptance, the new identity is
    /// pinned for a few minutes and middleware in Startup re-challenges requests riding
    /// stale keep-alive connections still authenticated as the previous user.
    /// </summary>
    public class AccountController : Controller
    {
        private const string SignedOutCookie = ".ContentManager.SignedOut";
        private const string SwitchAttemptCookie = ".ContentManager.SwitchAttempt";

        // Must match the session cookie configured in Startup (AddSession).
        private const string SessionCookie = ".ContentManager.Session";

        // Set (to the new user's hash) when a login-as-other-user succeeds; read by the
        // identity convergence middleware in Startup.
        public const string ExpectedUserCookie = ".ContentManager.ExpectedUser";
        public static readonly TimeSpan ExpectedUserWindow = TimeSpan.FromMinutes(3);

        // Backstop: a same-account arrival this long after the first challenge counts
        // as human-typed. The browser's SILENT retries (auto-logon, Negotiate→NTLM
        // scheme fallback) complete within milliseconds and never wait this long.
        // The PRIMARY signal is scheme repetition (see LoginChallenge): the browser
        // tries default credentials only once per scheme per navigation, so the same
        // scheme arriving twice in one cycle means a human used the prompt — this is
        // what lets a fast click on a prefilled sign-in box through immediately.
        private static readonly TimeSpan MinTypedDelay = TimeSpan.FromSeconds(3);

        // "N" = Negotiate, "T" = NTLM, "O" = anything else, "" = no Authorization header.
        private static string SchemeLetter(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader)) return "";
            if (authorizationHeader.StartsWith("Negotiate", StringComparison.OrdinalIgnoreCase)) return "N";
            if (authorizationHeader.StartsWith("NTLM", StringComparison.OrdinalIgnoreCase)) return "T";
            return "O";
        }

        // Session.Clear() empties the data but keeps the session ID — another open tab
        // of the old user can repopulate it after logout, and the next user would share
        // it. Deleting the cookie forces a brand-new session ID on the next request.
        // Options must mirror how AddSession issues the cookie (explicit, or the cookie
        // policy middleware mangles the deletion into samesite=none without Secure,
        // which Chrome rejects — same pitfall as FlagCookieOptions).
        private void DeleteSessionCookie() =>
            Response.Cookies.Delete(SessionCookie, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
            });

        // Explicit options are required on BOTH append and delete: the cookie policy
        // middleware (MinimumSameSitePolicy=None + CheckConsentNeeded) rewrites a bare
        // Cookies.Delete() into "samesite=none" without Secure, which Chrome rejects —
        // making the deletion a silent no-op.
        private static CookieOptions FlagCookieOptions(bool forDelete = false, TimeSpan? maxAge = null) => new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            MaxAge = forDelete ? null : (maxAge ?? TimeSpan.FromMinutes(5)),
        };

        // GET /Account/SignOut
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public new IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            DeleteSessionCookie();

            // Remember who signed out (hashed — avoids putting DOMAIN\name in the cookie).
            // Short-lived on purpose: if the user abandons the prompt, things self-heal.
            Response.Cookies.Append(SignedOutCookie, Hash(User.Identity?.Name), FlagCookieOptions());
            Response.Cookies.Delete(SwitchAttemptCookie, FlagCookieOptions(forDelete: true));
            Response.Cookies.Delete(ExpectedUserCookie, FlagCookieOptions(forDelete: true));

            return RedirectToAction(nameof(SignedOut));
        }

        // GET /Account/SignedOut
        // The landing page after logout. Always renders (200) — the user lands here
        // directly with NO credential prompt. The prompt only appears when they click
        // "Log in" (Login below).
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignedOut()
        {
            return View();
        }

        // GET /Account/Login
        // Entry point for the "Log in" button. Resets the attempt cookie so every
        // click starts a fresh challenge cycle (otherwise a leftover stamp from a
        // cancelled cycle would let the silent auto-logon through).
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login()
        {
            Response.Cookies.Delete(SwitchAttemptCookie, FlagCookieOptions(forDelete: true));
            return RedirectToAction(nameof(LoginChallenge));
        }

        // GET /Account/LoginChallenge
        // The challenge endpoint — see the class summary for the acceptance rules.
        // Cancelling the prompt displays the SignedOut view (this response's body).
        // Re-arms the flag if it expired, so the button always produces the prompt.
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> LoginChallenge()
        {
            var current = Hash(User.Identity?.Name);
            var signedOutUser = Request.Cookies[SignedOutCookie];

            if (signedOutUser == null)
            {
                signedOutUser = current;
                Response.Cookies.Append(SignedOutCookie, current, FlagCookieOptions());
            }

            // Attempt cookie format: "<unixMs of first challenge>-<scheme letters seen>".
            // '-' is used because it survives cookie encoding round-trips — Append
            // escapes characters like '|' to %7C but request parsing does NOT unescape
            // them. Anything unparseable degrades to a fresh cycle.
            var attemptRaw = Request.Cookies[SwitchAttemptCookie] ?? "";
            var attemptParts = attemptRaw.Split('-');
            long.TryParse(attemptParts[0], out var challengedAtMs);
            var seenSchemes = attemptParts.Length > 1 ? attemptParts[1] : "";
            var scheme = SchemeLetter(Request.Headers.Authorization);

            var typed =
                // same scheme completed twice in this cycle → a human used the prompt
                (scheme.Length > 0 && seenSchemes.Contains(scheme))
                // backstop: arrived long after the first challenge → credentials were typed
                || (challengedAtMs > 0
                    && DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(challengedAtMs) >= MinTypedDelay);

            if (signedOutUser != current || typed)
            {
                // Either a different account arrived, or the same account provably came
                // from the prompt. Let them in with a fresh session.
                Response.Cookies.Delete(SignedOutCookie, FlagCookieOptions(forDelete: true));
                Response.Cookies.Delete(SwitchAttemptCookie, FlagCookieOptions(forDelete: true));
                DeleteSessionCookie();

                // Pin the new identity so the convergence middleware re-challenges any
                // request still riding a connection authenticated as the previous user.
                Response.Cookies.Append(ExpectedUserCookie, current,
                    FlagCookieOptions(maxAge: ExpectedUserWindow));
                return RedirectToAction("Index", "Home");
            }

            // Silent arrival of the signed-out account — challenge (again). Keep the
            // FIRST challenge timestamp (never reset by silent retries) and record
            // which scheme this arrival used, so a repeat of it is recognized as human.
            var firstChallengeMs = challengedAtMs > 0
                ? challengedAtMs
                : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Response.Cookies.Append(SwitchAttemptCookie,
                $"{firstChallengeMs}-{seenSchemes}{scheme}", FlagCookieOptions());

            // Diagnostic breadcrumb (visible in devtools/curl).
            Response.Headers["X-ContentManager-Login"] = $"challenged scheme={scheme} seen={seenSchemes}{scheme}";

            await HttpContext.ChallengeAsync(IISDefaults.AuthenticationScheme);
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return View("SignedOut");
        }

        // GET /Account/SignBackIn
        // Unlinked escape hatch: continue with the current Windows account without
        // re-prompting, clearing all sign-out state (useful if a user gets stuck).
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult SignBackIn()
        {
            Response.Cookies.Delete(SignedOutCookie, FlagCookieOptions(forDelete: true));
            Response.Cookies.Delete(SwitchAttemptCookie, FlagCookieOptions(forDelete: true));
            Response.Cookies.Delete(ExpectedUserCookie, FlagCookieOptions(forDelete: true));
            return RedirectToAction("Index", "Home");
        }

        // Public: also used by the identity convergence middleware in Startup.
        public static string Hash(string userName) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userName ?? string.Empty)));
    }
}
