namespace PokerTime.Web.Tests.Integration.Common {
    using System;
    using AutoMapper;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using NUnit.Framework;

    /// <summary>
    /// Allows within a, for instance, test fixture, to temporary change a setting
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class TemporarySettingsScope<T> where T : class, new() {
        private readonly PokerTimeAppFactory _app;
        private readonly IMapper _mapper;
        private T _savedSettings;

        public TemporarySettingsScope(PokerTimeAppFactory app) {
            _app = app;

            var config =
                new MapperConfiguration(configure: cfg => { cfg.CreateMap<T, T>(); });

            _mapper = config.CreateMapper();
        }

        public void SaveSettings(Action<T> callback) {
            var settingsAccessor = _app.Services.GetRequiredService<IOptions<T>>();

            _savedSettings = _mapper.Map<T>(settingsAccessor.Value);

            TestContext.WriteLine($"Entering temporary settings scope for {typeof(T)}");

            callback.Invoke(settingsAccessor.Value);
        }

        public void RestoreSettings() {
            TestContext.WriteLine($"Exiting temporary settings scope for {typeof(T)}");

            if (_savedSettings == null) {
                throw new InvalidOperationException($"{GetType().FullName}: Unable to restore settings, settings not set");
            }

            var settingsAccessor = _app.Services.GetRequiredService<IOptions<T>>();
            _mapper.Map(_savedSettings, settingsAccessor.Value);

            _savedSettings = null;
            TestContext.WriteLine($"Exited temporary settings scope for {typeof(T)}");
        }
    }
}
