using Hangman.Client.Models.Match;

namespace Hangman.Client.ViewModels.Match
{
    internal interface IMatchGuessStateProjector
    {
        MatchGuessStateProjection Project(MatchGameStateModel state);
    }
}
