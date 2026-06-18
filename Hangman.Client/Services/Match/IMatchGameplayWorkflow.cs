using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchGameplayWorkflow
    {
        Task<CategoryOptionsOperationResult> GetCategoriesAsync();

        Task<CategoryVotingOperationResult> VoteCategoryAsync(
            int matchId,
            int categoryId);

        Task<CategoryVotingOperationResult> GetCategoryVotingStateAsync(
            int matchId);

        Task<CategoryVotingOperationResult> ResolveCategoryVotingAsync(
            int matchId);

        Task<SelectableWordsOperationResult> GetSelectableWordsAsync(
            int matchId);

        Task<WordSelectionOperationResult> SelectWordAsync(
            int matchId,
            int wordId);
    }
}
