using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Models.Match;
using Hangman.Client.Services.Match;
using Hangman.Client.Services.Session;
using Hangman.Client.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.Client.ViewModels
{
    public sealed class MatchChatViewModel : ServiceViewModelBase, IDisposable
    {
        private const string MatchChatServiceName = "MatchChatService";

        private readonly IMatchChatWorkflow matchChatWorkflow;
        private readonly RelayCommand sendMessageCommand;

        private int matchId;
        private string messageText;
        private bool disposed;

        public MatchChatViewModel(int matchId)
            : this(
                  matchId,
                  new MatchChatWorkflow(
                      new MatchChatClient(),
                      new MatchSessionContext()),
                  new ClientValidationMessageProvider(),
                  new ServerMessageProvider(),
                  ClientLoggerFactory.Create<MatchChatViewModel>())
        {
        }

        internal MatchChatViewModel(
            int matchId,
            IMatchChatWorkflow matchChatWorkflow,
            ClientValidationMessageProvider validationMessageProvider,
            IServerMessageProvider serverMessageProvider,
            IClientLogger logger)
            : base(validationMessageProvider, serverMessageProvider, logger)
        {
            this.matchChatWorkflow = matchChatWorkflow ??
                throw new ArgumentNullException(nameof(matchChatWorkflow));

            MatchId = matchId;
            Messages = new ObservableCollection<MatchChatMessageModel>();

            sendMessageCommand = new RelayCommand(
                SendMessageAsync,
                CanSendMessage);
        }

        public int MatchId
        {
            get { return matchId; }
            private set { SetProperty(ref matchId, value); }
        }

        public ObservableCollection<MatchChatMessageModel> Messages { get; private set; }

        public string MessageText
        {
            get { return messageText; }
            set
            {
                if (SetProperty(ref messageText, value))
                {
                    sendMessageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SendMessageCommand
        {
            get { return sendMessageCommand; }
        }

        public void AddIncomingMessage(MatchChatMessageModel message)
        {
            if (message == null ||
                message.MatchId != MatchId)
            {
                return;
            }

            Messages.Add(message);
        }

        public void Dispose()
        {
            disposed = true;
        }

        private async Task SendMessageAsync()
        {
            await ExecuteServiceOperationAsync(
                "SendMatchChatMessageAsync",
                MatchChatServiceName,
                async () =>
                {
                    ClearMessages();

                    MatchChatOperationResult result =
                        await matchChatWorkflow.SendMessageAsync(
                            MatchId,
                            MessageText);

                    if (result == null)
                    {
                        SetCommonUnexpectedError();
                        return;
                    }

                    if (!result.Success)
                    {
                        ApplyFailure(result);
                        return;
                    }

                    MessageText = string.Empty;
                },
                null,
                sendMessageCommand);
        }

        private void ApplyFailure(MatchWorkflowResultBase result)
        {
            if (result == null || result.IsUnexpectedError)
            {
                SetCommonUnexpectedError();
                return;
            }

            if (result.IsSessionInvalid)
            {
                SetCommonRuntimeError();
                return;
            }

            SetError(GetServerMessage(ServerMessageModuleName.Match, result.MessageCode));
        }

        private bool CanSendMessage()
        {
            return !disposed &&
                   !IsBusy &&
                   MatchId > 0 &&
                   !string.IsNullOrWhiteSpace(MessageText);
        }
    }
}
