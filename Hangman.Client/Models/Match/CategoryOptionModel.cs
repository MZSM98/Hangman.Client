using Hangman.Contracts.Word;

namespace Hangman.Client.Models.Match
{
    public class CategoryOptionModel
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string LanguageCode { get; set; }

        public static CategoryOptionModel FromDto(CategoryDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new CategoryOptionModel
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                LanguageCode = dto.LanguageCode
            };
        }
    }
}
