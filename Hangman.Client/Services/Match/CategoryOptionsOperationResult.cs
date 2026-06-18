using Hangman.Client.Models.Match;
using System.Collections.Generic;

namespace Hangman.Client.Services.Match
{
    public sealed class CategoryOptionsOperationResult : MatchWorkflowResultBase
    {
        public IList<CategoryOptionModel> Categories { get; private set; }

        public static CategoryOptionsOperationResult SuccessResult(
            IList<CategoryOptionModel> categories,
            string messageCode)
        {
            return new CategoryOptionsOperationResult
            {
                Success = true,
                Categories = categories ?? new List<CategoryOptionModel>(),
                MessageCode = messageCode
            };
        }

        public static CategoryOptionsOperationResult ServerFailure(string messageCode)
        {
            return new CategoryOptionsOperationResult
            {
                Success = false,
                MessageCode = messageCode,
                Categories = new List<CategoryOptionModel>()
            };
        }

        public static CategoryOptionsOperationResult SessionInvalid()
        {
            return new CategoryOptionsOperationResult
            {
                Success = false,
                IsSessionInvalid = true,
                Categories = new List<CategoryOptionModel>()
            };
        }

        public static CategoryOptionsOperationResult UnexpectedError()
        {
            return new CategoryOptionsOperationResult
            {
                Success = false,
                IsUnexpectedError = true,
                Categories = new List<CategoryOptionModel>()
            };
        }
    }
}
