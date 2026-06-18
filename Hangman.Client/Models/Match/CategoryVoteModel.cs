using Hangman.Contracts.Match;
using System;

namespace Hangman.Client.Models.Match
{
    public class CategoryVoteModel
    {
        public int PlayerId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public static CategoryVoteModel FromDto(CategoryVoteDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new CategoryVoteModel
            {
                PlayerId = dto.PlayerId,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                LanguageCode = dto.LanguageCode,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}
