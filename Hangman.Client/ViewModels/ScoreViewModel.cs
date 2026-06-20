using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Auth;
using Hangman.Client.Services.Score;
using Hangman.Client.Validators;
using Hangman.Client.ViewModels.Base;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public class ScoreViewModel : ServiceViewModelBase
    {
        private readonly IScoreClient scoreClient;

        private readonly RelayCommand loadScoreCommand;
        private readonly RelayCommand backCommand;

        private int totalScore;
        private int guesserWinTotal;
        private int guesserWinCount;
        private int hostWinTotal;
        private int hostWinCount;
        private int penaltyTotal;
        private int penaltyCount;

        public ScoreViewModel()
            : this(
                  new ScoreClient(),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<ScoreViewModel>())
        {
        }

        public ScoreViewModel(
            IScoreClient scoreClient,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : base(validationMessageProvider, serverMessageProvider, logger)
        {
            this.scoreClient = scoreClient ??
                throw new ArgumentNullException(nameof(scoreClient));

            loadScoreCommand = new RelayCommand(
                async () => await LoadScoreAsync(),
                CanExecuteWhenNotBusy);

            backCommand = new RelayCommand(RequestBack, CanExecuteWhenNotBusy);
        }

        public event EventHandler BackRequested;

        public int TotalScore
        {
            get { return totalScore; }
            private set { SetProperty(ref totalScore, value); }
        }

        public int GuesserWinTotal
        {
            get { return guesserWinTotal; }
            private set { SetProperty(ref guesserWinTotal, value); }
        }

        public int GuesserWinCount
        {
            get { return guesserWinCount; }
            private set { SetProperty(ref guesserWinCount, value); }
        }

        public int HostWinTotal
        {
            get { return hostWinTotal; }
            private set { SetProperty(ref hostWinTotal, value); }
        }

        public int HostWinCount
        {
            get { return hostWinCount; }
            private set { SetProperty(ref hostWinCount, value); }
        }

        public int PenaltyTotal
        {
            get { return penaltyTotal; }
            private set { SetProperty(ref penaltyTotal, value); }
        }

        public int PenaltyCount
        {
            get { return penaltyCount; }
            private set { SetProperty(ref penaltyCount, value); }
        }

        public ICommand LoadScoreCommand
        {
            get { return loadScoreCommand; }
        }

        public ICommand BackCommand
        {
            get { return backCommand; }
        }

        private async Task LoadScoreAsync()
        {
            ClearMessages();

            if (!UserSession.IsAuthenticated || UserSession.CurrentUser == null)
            {
                SetCommonUnexpectedError();
                return;
            }

            await ExecuteServiceOperationAsync(
                "GetPlayerScoreAsync",
                "score service",
                LoadScoreCoreAsync,
                null,
                loadScoreCommand,
                backCommand);
        }

        private async Task LoadScoreCoreAsync()
        {
            GetPlayerScoreRequest request = new GetPlayerScoreRequest
            {
                PlayerId = UserSession.CurrentUser.PlayerId
            };

            GetPlayerScoreResponse response =
                await scoreClient.GetPlayerScoreAsync(request);

            if (response == null)
            {
                SetCommonUnexpectedError();
                Logger.Warn("GetPlayerScoreAsync returned a null response.");
                return;
            }

            string translatedMessage =
                GetServerMessage(ServerMessageModuleName.Score, response.MessageCode);

            if (!response.Success)
            {
                SetError(translatedMessage);
                return;
            }

            TotalScore = response.TotalScore;
            GuesserWinTotal = response.GuesserWinTotal;
            GuesserWinCount = response.GuesserWinCount;
            HostWinTotal = response.HostWinTotal;
            HostWinCount = response.HostWinCount;
            PenaltyTotal = response.PenaltyTotal;
            PenaltyCount = response.PenaltyCount;

            SetSuccess(translatedMessage);
        }

        private void RequestBack()
        {
            if (IsBusy)
            {
                return;
            }

            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
