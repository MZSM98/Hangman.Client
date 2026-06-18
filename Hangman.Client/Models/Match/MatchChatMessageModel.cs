using Hangman.Contracts.Match;
using System;

namespace Hangman.Client.Models.Match
{
    public sealed class MatchChatMessageModel
    {
        public int MatchId { get; set; }

        public int SenderAccountId { get; set; }

        public int SenderPlayerId { get; set; }

        public string SenderFullName { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; }

        public static MatchChatMessageModel FromDto(MatchChatMessageDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new MatchChatMessageModel
            {
                MatchId = dto.MatchId,
                SenderAccountId = dto.SenderAccountId,
                SenderPlayerId = dto.SenderPlayerId,
                SenderFullName = dto.SenderFullName,
                Message = dto.Message,
                SentAt = dto.SentAt
            };
        }
    }
}
