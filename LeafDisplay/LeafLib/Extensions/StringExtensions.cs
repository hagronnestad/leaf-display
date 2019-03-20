namespace LeafLib.Extensions {

    public static class StringExtensions {

        public static int StringToInt(this string value) {
            return int.TryParse(value, out var result) ? result : 0;
        }
    }

}