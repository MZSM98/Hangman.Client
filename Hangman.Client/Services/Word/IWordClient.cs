using Hangman.Contracts.Word;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Word
{
    public interface IWordClient
    {
        Task<GetCategoriesByLanguageResponse> GetCategoriesByLanguageAsync(
            GetCategoriesByLanguageRequest request);
    }
}
