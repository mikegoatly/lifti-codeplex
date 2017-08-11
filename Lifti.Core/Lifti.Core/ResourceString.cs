namespace Lifti
{
    using System.Reflection;
    using System.Resources;

    internal static class ResourceString
    {
        private static readonly ResourceManager resourceManager = new ResourceManager("Lifti.Properties.Resources", typeof(ResourceString).GetTypeInfo().Assembly);

        public static string UnexpectedTokenEncountered => resourceManager.GetString(nameof(UnexpectedTokenEncountered));

        public static string UnableToPerformPositionalMatchBetweenDifferentItems => resourceManager.GetString(nameof(UnableToPerformPositionalMatchBetweenDifferentItems));

        public static string UnexpectedOperator => resourceManager.GetString(nameof(UnexpectedOperator));

        public static string UnexpectedOperatorInternal => resourceManager.GetString(nameof(UnexpectedOperatorInternal));

        public static string UnexpectedEndOfQuery => resourceManager.GetString(nameof(UnexpectedEndOfQuery));

        public static string ExpectedToken => resourceManager.GetString(nameof(ExpectedToken));

        public static string UnknownOperator => resourceManager.GetString(nameof(UnknownOperator));
    }
}