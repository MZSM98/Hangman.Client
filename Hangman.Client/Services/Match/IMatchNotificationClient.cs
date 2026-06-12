using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Client.Services.Match
{
    public interface IMatchNotificationClient : IDisposable
    {
        event EventHandler<MatchLobbyUpdatedEventArgs> LobbyUpdated;

        event EventHandler<MatchLobbyClosedEventArgs> LobbyClosed;

        event EventHandler<MatchStatusChangedEventArgs> MatchStatusChanged;

        event EventHandler AvailableLobbiesChanged;

        Task<SubscribeAvailableLobbiesResponse> SubscribeAvailableLobbiesAsync(
            SubscribeAvailableLobbiesRequest request);

        Task<UnsubscribeAvailableLobbiesResponse> UnsubscribeAvailableLobbiesAsync(
            UnsubscribeAvailableLobbiesRequest request);

        Task<SubscribeMatchResponse> SubscribeAsync(SubscribeMatchRequest request);

        Task<UnsubscribeMatchResponse> UnsubscribeAsync(UnsubscribeMatchRequest request);

        void Close();
    }
}
