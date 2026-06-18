using Hangman.Client.Models.Match;
using Hangman.Client.Services.Session;
using Hangman.Client.Services.Word;
using Hangman.Contracts.Match;
using Hangman.Contracts.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public sealed class MatchGameplayWorkflow : IMatchGameplayWorkflow
    {
        private const string InvalidMatchIdCode = "InvalidMatchId";
        private const string InvalidCategoryIdCode = "InvalidCategoryId";
        private const string InvalidWordIdCode = "InvalidWordId";

        private readonly IMatchGameplayClient matchGameplayClient;
        private readonly IWordClient wordClient;
        private readonly IMatchSessionContext sessionContext;

        public MatchGameplayWorkflow(
            IMatchGameplayClient matchGameplayClient,
            IWordClient wordClient,
            IMatchSessionContext sessionContext)
        {
            this.matchGameplayClient = matchGameplayClient ??
                throw new ArgumentNullException(nameof(matchGameplayClient));
            this.wordClient = wordClient ??
                throw new ArgumentNullException(nameof(wordClient));
            this.sessionContext = sessionContext ??
                throw new ArgumentNullException(nameof(sessionContext));
        }

        public async Task<CategoryOptionsOperationResult> GetCategoriesAsync()
        {
            if (!sessionContext.HasValidSession)
            {
                return CategoryOptionsOperationResult.SessionInvalid();
            }

            GetCategoriesByLanguageRequest request =
                new GetCategoriesByLanguageRequest
                {
                    LanguageCode = sessionContext.PreferredLanguageCode
                };

            GetCategoriesByLanguageResponse response =
                await wordClient.GetCategoriesByLanguageAsync(request);

            if (response == null)
            {
                return CategoryOptionsOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return CategoryOptionsOperationResult.ServerFailure(response.MessageCode);
            }

            IList<CategoryOptionModel> categories = new List<CategoryOptionModel>();

            if (response.Categories != null)
            {
                categories = response.Categories
                    .Select(CategoryOptionModel.FromDto)
                    .Where(category => category != null)
                    .ToList();
            }

            return CategoryOptionsOperationResult.SuccessResult(
                categories,
                response.MessageCode);
        }

        public async Task<CategoryVotingOperationResult> VoteCategoryAsync(
            int matchId,
            int categoryId)
        {
            if (!sessionContext.HasValidSession)
            {
                return CategoryVotingOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return CategoryVotingOperationResult.ServerFailure(
                    InvalidMatchIdCode,
                    null);
            }

            if (categoryId <= 0)
            {
                return CategoryVotingOperationResult.ServerFailure(
                    InvalidCategoryIdCode,
                    null);
            }

            VoteCategoryRequest request = new VoteCategoryRequest
            {
                MatchId = matchId,
                AccountId = sessionContext.AccountId,
                CategoryId = categoryId
            };

            VoteCategoryResponse response =
                await matchGameplayClient.VoteCategoryAsync(request);

            if (response == null)
            {
                return CategoryVotingOperationResult.UnexpectedError();
            }

            CategoryVotingStateModel state =
                CategoryVotingStateModel.FromDto(response.VotingState);

            if (!response.Success)
            {
                return CategoryVotingOperationResult.ServerFailure(
                    response.MessageCode,
                    state);
            }

            return CategoryVotingOperationResult.SuccessResult(
                state,
                null,
                response.MessageCode);
        }

        public async Task<CategoryVotingOperationResult> GetCategoryVotingStateAsync(
            int matchId)
        {
            if (!sessionContext.HasValidSession)
            {
                return CategoryVotingOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return CategoryVotingOperationResult.ServerFailure(
                    InvalidMatchIdCode,
                    null);
            }

            GetCategoryVotingStateRequest request =
                new GetCategoryVotingStateRequest
                {
                    MatchId = matchId,
                    AccountId = sessionContext.AccountId
                };

            GetCategoryVotingStateResponse response =
                await matchGameplayClient.GetCategoryVotingStateAsync(request);

            if (response == null)
            {
                return CategoryVotingOperationResult.UnexpectedError();
            }

            CategoryVotingStateModel state =
                CategoryVotingStateModel.FromDto(response.VotingState);

            if (!response.Success)
            {
                return CategoryVotingOperationResult.ServerFailure(
                    response.MessageCode,
                    state);
            }

            return CategoryVotingOperationResult.SuccessResult(
                state,
                null,
                response.MessageCode);
        }

        public async Task<CategoryVotingOperationResult> ResolveCategoryVotingAsync(
            int matchId)
        {
            if (!sessionContext.HasValidSession)
            {
                return CategoryVotingOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return CategoryVotingOperationResult.ServerFailure(
                    InvalidMatchIdCode,
                    null);
            }

            ResolveCategoryVotingRequest request =
                new ResolveCategoryVotingRequest
                {
                    MatchId = matchId,
                    AccountId = sessionContext.AccountId
                };

            ResolveCategoryVotingResponse response =
                await matchGameplayClient.ResolveCategoryVotingAsync(request);

            if (response == null)
            {
                return CategoryVotingOperationResult.UnexpectedError();
            }

            CategoryVotingStateModel state =
                CategoryVotingStateModel.FromDto(response.VotingState);

            if (!response.Success)
            {
                return CategoryVotingOperationResult.ServerFailure(
                    response.MessageCode,
                    state);
            }

            return CategoryVotingOperationResult.SuccessResult(
                state,
                MatchLobbyModel.FromDto(response.Lobby),
                response.MessageCode);
        }

        public async Task<SelectableWordsOperationResult> GetSelectableWordsAsync(
            int matchId)
        {
            if (!sessionContext.HasValidSession)
            {
                return SelectableWordsOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return SelectableWordsOperationResult.ServerFailure(InvalidMatchIdCode);
            }

            GetSelectableWordsRequest request = new GetSelectableWordsRequest
            {
                MatchId = matchId,
                AccountId = sessionContext.AccountId
            };

            GetSelectableWordsResponse response =
                await matchGameplayClient.GetSelectableWordsAsync(request);

            if (response == null)
            {
                return SelectableWordsOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return SelectableWordsOperationResult.ServerFailure(response.MessageCode);
            }

            IList<SelectableWordModel> words = new List<SelectableWordModel>();

            if (response.Words != null)
            {
                words = response.Words
                    .Select(SelectableWordModel.FromDto)
                    .Where(word => word != null)
                    .ToList();
            }

            return SelectableWordsOperationResult.SuccessResult(
                words,
                response.MessageCode);
        }

        public async Task<WordSelectionOperationResult> SelectWordAsync(
            int matchId,
            int wordId)
        {
            if (!sessionContext.HasValidSession)
            {
                return WordSelectionOperationResult.SessionInvalid();
            }

            if (matchId <= 0)
            {
                return WordSelectionOperationResult.ServerFailure(InvalidMatchIdCode);
            }

            if (wordId <= 0)
            {
                return WordSelectionOperationResult.ServerFailure(InvalidWordIdCode);
            }

            SelectWordRequest request = new SelectWordRequest
            {
                MatchId = matchId,
                AccountId = sessionContext.AccountId,
                WordId = wordId
            };

            SelectWordResponse response =
                await matchGameplayClient.SelectWordAsync(request);

            if (response == null)
            {
                return WordSelectionOperationResult.UnexpectedError();
            }

            if (!response.Success)
            {
                return WordSelectionOperationResult.ServerFailure(response.MessageCode);
            }

            return WordSelectionOperationResult.SuccessResult(
                MatchLobbyModel.FromDto(response.Lobby),
                response.MessageCode);
        }
    }
}
