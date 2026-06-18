using Hangman.Contracts.Match;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchGameplayClient
    {
        Task<VoteCategoryResponse> VoteCategoryAsync(VoteCategoryRequest request);

        Task<GetCategoryVotingStateResponse> GetCategoryVotingStateAsync(
            GetCategoryVotingStateRequest request);

        Task<ResolveCategoryVotingResponse> ResolveCategoryVotingAsync(
            ResolveCategoryVotingRequest request);

        Task<GetSelectableWordsResponse> GetSelectableWordsAsync(
            GetSelectableWordsRequest request);

        Task<SelectWordResponse> SelectWordAsync(SelectWordRequest request);
    }
}
