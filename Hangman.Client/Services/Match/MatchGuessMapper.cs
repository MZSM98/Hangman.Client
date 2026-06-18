using Hangman.Client.Models.Match;
using Hangman.Contracts.Match;
using System.Collections.Generic;

namespace Hangman.Client.Services.Match
{
    internal static class MatchGuessMapper
    {
        public static MatchGameStateModel ToGameStateModel(
            MatchGameStateDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new MatchGameStateModel
            {
                MatchId = dto.MatchId,
                HostId = dto.HostId,
                HostFullName = dto.HostFullName,
                GuestId = dto.GuestId,
                GuestFullName = dto.GuestFullName,
                MatchStatus = dto.MatchStatus,
                FailedAttempts = dto.FailedAttempts,
                MaxAttempts = dto.MaxAttempts,
                GuessTurnStartedAt = dto.GuessTurnStartedAt,
                GuessTurnEndsAt = dto.GuessTurnEndsAt,
                RemainingSeconds = dto.RemainingSeconds,
                IsFinished = dto.IsFinished,
                WinnerId = dto.WinnerId,
                WinnerFullName = dto.WinnerFullName,
                WinnerEmail = dto.WinnerEmail,
                LetterSlots = ToLetterSlotModels(dto.LetterSlots),
                WordDescription = dto.WordDescription,
                GuessHistory = ToGuessHistoryModels(dto.GuessHistory),
                HangmanFigure = ToHangmanFigureModel(dto.HangmanFigure)
            };
        }

        private static IList<LetterSlotModel> ToLetterSlotModels(
            IEnumerable<LetterSlotDto> dtos)
        {
            IList<LetterSlotModel> models = new List<LetterSlotModel>();

            if (dtos == null)
            {
                return models;
            }

            foreach (LetterSlotDto dto in dtos)
            {
                if (dto == null)
                {
                    continue;
                }

                models.Add(new LetterSlotModel
                {
                    Position = dto.Position,
                    Letter = dto.Letter,
                    IsRevealed = dto.IsRevealed
                });
            }

            return models;
        }

        private static IList<GuessHistoryModel> ToGuessHistoryModels(
            IEnumerable<GuessHistoryDto> dtos)
        {
            IList<GuessHistoryModel> models = new List<GuessHistoryModel>();

            if (dtos == null)
            {
                return models;
            }

            foreach (GuessHistoryDto dto in dtos)
            {
                if (dto == null)
                {
                    continue;
                }

                models.Add(new GuessHistoryModel
                {
                    GuessType = dto.GuessType,
                    Value = dto.Value,
                    IsCorrect = dto.IsCorrect,
                    CreatedAt = dto.CreatedAt
                });
            }

            return models;
        }

        private static HangmanFigureModel ToHangmanFigureModel(
            HangmanFigureDto dto)
        {
            if (dto == null)
            {
                return new HangmanFigureModel();
            }

            return new HangmanFigureModel
            {
                FailedAttempts = dto.FailedAttempts,
                MaxAttempts = dto.MaxAttempts,
                ShowHead = dto.ShowHead,
                ShowTorso = dto.ShowTorso,
                ShowLeftArm = dto.ShowLeftArm,
                ShowRightArm = dto.ShowRightArm,
                ShowLeftLeg = dto.ShowLeftLeg,
                ShowRightLeg = dto.ShowRightLeg
            };
        }
    }
}
