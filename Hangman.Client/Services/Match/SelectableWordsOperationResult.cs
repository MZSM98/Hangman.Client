using Hangman.Client.Models.Match;
using System.Collections.Generic;

namespace Hangman.Client.Services.Match
{
    public sealed class SelectableWordsOperationResult : MatchWorkflowResultBase
    {
        public IList<SelectableWordModel> Words { get; private set; }

        public static SelectableWordsOperationResult SuccessResult(
            IList<SelectableWordModel> words,
            string messageCode)
        {
            return new SelectableWordsOperationResult
            {
                Success = true,
                Words = words ?? new List<SelectableWordModel>(),
                MessageCode = messageCode
            };
        }

        public static SelectableWordsOperationResult ServerFailure(string messageCode)
        {
            return new SelectableWordsOperationResult
            {
                Success = false,
                MessageCode = messageCode,
                Words = new List<SelectableWordModel>()
            };
        }

        public static SelectableWordsOperationResult SessionInvalid()
        {
            return new SelectableWordsOperationResult
            {
                Success = false,
                IsSessionInvalid = true,
                Words = new List<SelectableWordModel>()
            };
        }

        public static SelectableWordsOperationResult UnexpectedError()
        {
            return new SelectableWordsOperationResult
            {
                Success = false,
                IsUnexpectedError = true,
                Words = new List<SelectableWordModel>()
            };
        }
    }
}
