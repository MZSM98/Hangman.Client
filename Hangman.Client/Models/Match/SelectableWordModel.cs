using Hangman.Contracts.Match;

namespace Hangman.Client.Models.Match
{
    public class SelectableWordModel
    {
        public int WordId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string WordText { get; set; }

        public string Description { get; set; }

        public string LanguageCode { get; set; }

        public static SelectableWordModel FromDto(SelectableWordDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new SelectableWordModel
            {
                WordId = dto.WordId,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                WordText = dto.WordText,
                Description = dto.Description,
                LanguageCode = dto.LanguageCode
            };
        }
    }
}
