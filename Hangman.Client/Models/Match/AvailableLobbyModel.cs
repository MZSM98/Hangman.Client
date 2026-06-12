using Hangman.Contracts.Match;
using System;

namespace Hangman.Client.Models.Match
{
    public class AvailableLobbyModel
    {
        public int MatchId { get; set; }

        public int HostId { get; set; }

        public string HostFullName { get; set; }

        public string HostEmail { get; set; }

        public string HostLanguageCode { get; set; }

        public string MatchStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public static AvailableLobbyModel FromDto(AvailableLobbyDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new AvailableLobbyModel
            {
                MatchId = dto.MatchId,
                HostId = dto.HostId,
                HostFullName = dto.HostFullName,
                HostEmail = dto.HostEmail,
                HostLanguageCode = dto.HostLanguageCode,
                MatchStatus = dto.MatchStatus,
                CreatedAt = dto.CreatedAt
            };
        }
    }
}
