using Hangman.Contracts.Match;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangman.Client.Models.Match
{
    public class CategoryVotingStateModel
    {
        public int MatchId { get; set; }

        public string MatchStatus { get; set; }

        public int? SelectedCategoryId { get; set; }

        public string SelectedCategoryName { get; set; }

        public DateTime? CategoryVotingStartedAt { get; set; }

        public DateTime? CategoryVotingEndsAt { get; set; }

        public DateTime? WordSelectionStartedAt { get; set; }

        public DateTime? WordSelectionEndsAt { get; set; }

        public int RemainingVotingSeconds { get; set; }

        public bool CanVote { get; set; }

        public bool IsVotingResolved { get; set; }

        public bool CanCurrentPlayerSelectWord { get; set; }

        public IList<CategoryVoteModel> Votes { get; set; }

        public bool HasVotes
        {
            get { return Votes != null && Votes.Count > 0; }
        }

        public bool HasSelectedCategory
        {
            get { return SelectedCategoryId.HasValue; }
        }

        public static CategoryVotingStateModel FromDto(CategoryVotingStateDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            IList<CategoryVoteModel> votes = new List<CategoryVoteModel>();

            if (dto.Votes != null)
            {
                votes = dto.Votes
                    .Select(CategoryVoteModel.FromDto)
                    .Where(vote => vote != null)
                    .ToList();
            }

            return new CategoryVotingStateModel
            {
                MatchId = dto.MatchId,
                MatchStatus = dto.MatchStatus,
                SelectedCategoryId = dto.SelectedCategoryId,
                SelectedCategoryName = dto.SelectedCategoryName,
                CategoryVotingStartedAt = dto.CategoryVotingStartedAt,
                CategoryVotingEndsAt = dto.CategoryVotingEndsAt,
                WordSelectionStartedAt = dto.WordSelectionStartedAt,
                WordSelectionEndsAt = dto.WordSelectionEndsAt,
                RemainingVotingSeconds = dto.RemainingVotingSeconds,
                CanVote = dto.CanVote,
                IsVotingResolved = dto.IsVotingResolved,
                CanCurrentPlayerSelectWord = dto.CanCurrentPlayerSelectWord,
                Votes = votes
            };
        }
    }
}
