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
                MatchStatus = dto.MatchStatus,
                CreatedAt = dto.CreatedAt,
                JoinedAt = dto.JoinedAt,
                CategoryVotingStartedAt = dto.CategoryVotingStartedAt,
                CategoryVotingEndsAt = dto.CategoryVotingEndsAt
            };
        }
    }
}
