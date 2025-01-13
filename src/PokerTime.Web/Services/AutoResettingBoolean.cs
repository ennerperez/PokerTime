namespace PokerTime.Web.Services {
    public class AutoResettingBoolean {
        private readonly bool _initialValue;
        private bool _currentValue;

        public AutoResettingBoolean(bool initialValue) {
            _initialValue = initialValue;
            _currentValue = initialValue;
        }

        public bool GetValue() {
            var ret = _currentValue;
            _currentValue = _initialValue;
            return ret;
        }

        public void Set() => _currentValue = !_initialValue;
    }
}
