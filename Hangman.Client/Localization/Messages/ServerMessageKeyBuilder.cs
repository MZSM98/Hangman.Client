using System;
using System.Linq;

namespace Hangman.Client.Localization.Messages
{
    public static class ServerMessageKeyBuilder
    {
        private const string ServerPrefix = "Server_";
        private const string MessageCodeSuffix = "MessageCode";
        private const string CodeSuffix = "Code";

        public static string Build(string moduleName, string messageCode)
        {
            return ServerPrefix
                + NormalizeModuleName(moduleName)
                + "_"
                + NormalizeMessageCode(messageCode);
        }

        public static string GetModuleName(Type messageCodeType)
        {
            if (messageCodeType == null)
            {
                return ServerMessageModuleName.Common;
            }

            string typeName = messageCodeType.Name;

            if (typeName.EndsWith(MessageCodeSuffix, StringComparison.Ordinal))
            {
                typeName = typeName.Substring(0, typeName.Length - MessageCodeSuffix.Length);
            }
            else if (typeName.EndsWith(CodeSuffix, StringComparison.Ordinal))
            {
                typeName = typeName.Substring(0, typeName.Length - CodeSuffix.Length);
            }

            return NormalizeModuleName(typeName);
        }

        public static string NormalizeModuleName(string moduleName)
        {
            string normalizedValue = NormalizeToken(moduleName);

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                return ServerMessageModuleName.Common;
            }

            return normalizedValue;
        }

        public static string NormalizeMessageCode(string messageCode)
        {
            return NormalizeToken(messageCode);
        }

        private static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var validCharacters = value.Trim().Where(c => char.IsLetterOrDigit(c) || c == '_');

            return string.Concat(validCharacters);
        }
    }
}
