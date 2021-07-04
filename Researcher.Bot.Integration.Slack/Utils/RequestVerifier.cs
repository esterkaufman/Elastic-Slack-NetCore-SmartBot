using System;
using System.Security.Cryptography;
using System.Text;

namespace Researcher.Bot.Integration.Slack.Utils
{
    public class RequestVerifier
    {
        private const int MINUTES = 60 * 1000;

        public const string TimestampHeaderName = "X-Slack-Request-Timestamp";
        public const string SignatureHeaderName = "X-Slack-Signature";

        private readonly string _secret;
        private readonly int _tolerance;

        public RequestVerifier(string signingSecret, int? timestampTolerance = null)
        {
            _secret = signingSecret ?? throw new ArgumentNullException(nameof(signingSecret));
            _tolerance = timestampTolerance ?? 5 * MINUTES;
        }

        public bool Verify(string expectedSig, long timestamp, string body, out string sig)
        {
            return Verify(expectedSig, _secret, timestamp, body, _tolerance, out sig);
        }

        private static bool Verify(string expectedSig, string signingSecret, long timestamp, string body, int tolerance,
            out string sig)
        {
            if (string.IsNullOrWhiteSpace(expectedSig))
            {
                throw new ArgumentNullException(nameof(expectedSig));
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            var currTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long xMinsAgo = currTimestamp - tolerance;

            if (timestamp < xMinsAgo)
            {
                throw new InvalidTimeZoneException("Auth Error: invalid request, params have been changed");
            }

            sig = GenerateSignature(signingSecret, timestamp, body);
            return expectedSig == sig;
        }


        public static string GenerateSignature(string signingSecret, long timestamp, string body)
        {

            var sig = $"v0:{timestamp}:{body}";
            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret));
            var output = hasher.ComputeHash(Encoding.UTF8.GetBytes(sig));
            var osb = new StringBuilder("v0=");

            foreach (var b in output)
            {
                osb.Append(b.ToString("x2"));
            }

            return osb.ToString();
        }
    }
}