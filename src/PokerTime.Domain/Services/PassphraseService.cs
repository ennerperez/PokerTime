using System;
using System.Text;

namespace PokerTime.Domain.Services {
    using System.Security.Cryptography;
    using PokerTime.Common;

    public interface IPassphraseService {
        bool ValidatePassphrase(string passphrase, string hashed);

        string CreateHashedPassphrase(string passphrase);
    }

    public sealed class PassphraseService : IPassphraseService {
        public bool ValidatePassphrase(string passphrase, string hashedPassphrase) {
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));
            if (hashedPassphrase == null) throw new ArgumentNullException(nameof(hashedPassphrase));
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentException("Empty passphrase not allowed", nameof(passphrase));

            return string.Equals(
                CreateHashedPassphrase(passphrase),
                hashedPassphrase,
                StringComparison.OrdinalIgnoreCase
            );
        }

        public string CreateHashedPassphrase(string passphrase) {
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));
            if (string.IsNullOrEmpty(passphrase)) throw new ArgumentException("Empty passphrase not allowed", nameof(passphrase));

            using var sha256 = SHA256.Create();

            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) {
                sb.AppendFormat(Culture.Invariant, "{0:x2}", b);
            }

            return sb.ToString();
        }
    }
}
