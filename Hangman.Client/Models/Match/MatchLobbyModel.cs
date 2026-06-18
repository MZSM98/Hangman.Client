using Hangman.Contracts.Match;
using System;

namespace Hangman.Client.Models.Match
{
    public class MatchLobbyModel
    {
        public int MatchId { get; set; }

        public int HostId { get; set; }

        public string HostFullName { get; set; }

        public string HostLanguageCode { get; set; }

        public int? GuestId { get; set; }

        public string GuestFullName { get; set; }

        public string GuestLanguageCode { get; set; }

        public int? SelectedCategoryId { get; set; }

        public string SelectedCategoryName { get; set; }

        public int? SelectedWordId { get; set; }

        public DateTime? WordSelectionStartedAt { get; set; }

        public DateTime? WordSelectionEndsAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public int? WinnerId { get; set; }

        public int? PenalizedUserId { get; set; }

        public string MatchStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? JoinedAt { get; set; }

        public DateTime? CategoryVotingStartedAt { get; set; }

        public DateTime? CategoryVotingEndsAt { get; set; }

        public bool HasGuest
        {
            get { return GuestId.HasValue; }
        }

        public bool IsWaitingForGuest
        {
            get { return MatchStatus == "WaitingForGuest"; }
        }

        public bool IsVotingCategory
        {
            get { return MatchStatus == "VotingCategory"; }
        }

        public bool IsWaitingForHostWord
        {
            get { return MatchStatus == "WaitingForHostWord"; }
        }

        public bool IsInProgress
        {
            get { return MatchStatus == "InProgress"; }
        }

        public bool IsResolved
        {
            get
            {
                return MatchStatus == "Finished" ||
                       MatchStatus == "Abandoned" ||
                       MatchStatus == "Cancelled";
            }
        }

        public bool HasSelectedCategory
        {
            get { return SelectedCategoryId.HasValue; }
        }

        public bool HasSelectedWord
        {
            get { return SelectedWordId.HasValue; }
        }

        public static MatchLobbyModel FromDto(MatchLobbyDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new MatchLobbyModel
            {
                MatchId = dto.MatchId,
                HostId = dto.HostId,
                HostFullName = dto.HostFullName,
                HostLanguageCode = dto.HostLanguageCode,
                GuestId = dto.GuestId,
                GuestFullName = dto.GuestFullName,
                GuestLanguageCode = dto.GuestLanguageCode,

                SelectedCategoryId = dto.SelectedCategoryId,
                SelectedCategoryName = dto.SelectedCategoryName,
                SelectedWordId = dto.SelectedWordId,
                WordSelectionStartedAt = dto.WordSelectionStartedAt,
                WordSelectionEndsAt = dto.WordSelectionEndsAt,
                StartedAt = dto.StartedAt,
                FinishedAt = dto.FinishedAt,
                WinnerId = dto.WinnerId,
                PenalizedUserId = dto.PenalizedUserId,

                MatchStatus = dto.MatchStatus,
                CreatedAt = dto.CreatedAt,
                JoinedAt = dto.JoinedAt,
                CategoryVotingStartedAt = dto.CategoryVotingStartedAt,
                CategoryVotingEndsAt = dto.CategoryVotingEndsAt
            };
        }
    }
}
