using Hangman.Client.Coordinators.Match;
using Hangman.Client.Infrastructure.Logging;
using Hangman.Client.Infrastructure.Threading;
using Hangman.Client.Localization.Messages;
using Hangman.Client.Services.Match;
using Hangman.Client.Services.Session;
using System;

namespace Hangman.Client.ViewModels.Match
{
    internal sealed class MatchGuessViewModelDependencies
    {
        public IMatchGuessOperationHandler OperationHandler { get; set; }

        public IMatchGuessNotificationController NotificationController { get; set; }

        public IMatchGuessSlotLayoutCalculator SlotLayoutCalculator { get; set; }

        public IMatchGuessCountdownController CountdownController { get; set; }

        public IMatchGuessStateProjector StateProjector { get; set; }

        public IMatchGuessRoleEvaluator RoleEvaluator { get; set; }

        public ClientValidationMessageProvider ValidationMessageProvider { get; set; }

        public IServerMessageProvider ServerMessageProvider { get; set; }

        public IClientLogger Logger { get; set; }

        public static MatchGuessViewModelDependencies CreateDefault()
        {
            MatchSessionContext sessionContext = new MatchSessionContext();

            IMatchGuessNotificationController notificationController =
                new MatchGuessNotificationController(
                    new MatchLobbyNotificationCoordinator(
                        new MatchNotificationClient(),
                        sessionContext),
                    new WpfUiDispatcher(),
                    ClientLoggerFactory.Create<MatchGuessNotificationController>());

            return new MatchGuessViewModelDependencies
            {
                NotificationController = notificationController,

                OperationHandler = new MatchGuessOperationHandler(
                    new MatchGuessWorkflow(
                        new MatchGuessClient(),
                        sessionContext),
                    new MatchLobbyWorkflow(
                        new MatchClient(),
                        sessionContext),
                    notificationController),

                SlotLayoutCalculator = new MatchGuessSlotLayoutCalculator(),
                CountdownController = new MatchGuessCountdownController(),
                StateProjector = new MatchGuessStateProjector(),
                RoleEvaluator = new MatchGuessRoleEvaluator(),
                ValidationMessageProvider = new ClientValidationMessageProvider(),
                ServerMessageProvider = new ServerMessageProvider(),
                Logger = ClientLoggerFactory.Create<MatchGuessViewModel>()
            };
        }

        public void Validate()
        {
            EnsureRequired(OperationHandler, nameof(OperationHandler));
            EnsureRequired(NotificationController, nameof(NotificationController));
            EnsureRequired(SlotLayoutCalculator, nameof(SlotLayoutCalculator));
            EnsureRequired(CountdownController, nameof(CountdownController));
            EnsureRequired(StateProjector, nameof(StateProjector));
            EnsureRequired(RoleEvaluator, nameof(RoleEvaluator));
            EnsureRequired(ValidationMessageProvider, nameof(ValidationMessageProvider));
            EnsureRequired(ServerMessageProvider, nameof(ServerMessageProvider));
            EnsureRequired(Logger, nameof(Logger));
        }

        private static void EnsureRequired<T>(
            T value,
            string propertyName)
            where T : class
        {
            if (value == null)
            {
                throw new InvalidOperationException(
                    propertyName + " is required.");
            }
        }
    }
}
