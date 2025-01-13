namespace PokerTime.Domain.Services {
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using ValueObjects;

    public interface ISessionIdentifierService {
        bool IsValid(string str);

        /// <summary>
        ///     Creates a new log message id
        /// </summary>
        /// <returns></returns>
        SessionIdentifier CreateNew();
    }

    /// <summary>
    ///     Represents the identifier of an uploaded file
    /// </summary>
    public sealed class SessionIdentifierService : ISessionIdentifierService {
        private static int _mask = ~1;

        private static readonly char[] Chars =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public bool IsValid(string str) {
            if (str == null) throw new ArgumentNullException(nameof(str));

            foreach (var ch in str) {
                if (Array.IndexOf(array: Chars, value: ch) == -1) {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        ///     Creates a new log message id
        /// </summary>
        /// <returns></returns>
        public SessionIdentifier CreateNew() => CreateNewInternal();

        internal static SessionIdentifier CreateNewInternal() {
            Interlocked.Increment(location: ref _mask);
            return new SessionIdentifier(MakeStringRepresentation(Guid.NewGuid()));
        }

        private static unsafe string MakeStringRepresentation(Guid id) {
            // Convert Guid to bytes
            var buffer = new GuidBuffer(guid: id);
            var bytes = buffer.buffer;

            // Target string
            var size = sizeof(Guid) * 2;
            var result = stackalloc char[size + 1 /* \0 terminator */];
            var start = result;

            for (var i = 0; i < sizeof(Guid); i++) {
                var src = *bytes;
                var carry = 0;

                {
                    var index = src % Chars.Length;

                    var current = Chars[index];

                    *result = current;
                    result++;

                    carry = (src - index) / Chars.Length - 1;
                }

                if (carry > 0) {
                    var index = carry % Chars.Length;

                    var current = Chars[index];

                    *result = current;
                    result++;
                }

                bytes++;
            }

            return new string(value: start);
        }

        [StructLayout(layoutKind: LayoutKind.Explicit)]
        private unsafe struct GuidBuffer {
            [FieldOffset(offset: 0)] public fixed byte buffer[16];

            [FieldOffset(offset: 0)] private readonly Guid Guid;

            public GuidBuffer(Guid guid) : this() {
                Guid = guid;
            }
        }
    }
}
