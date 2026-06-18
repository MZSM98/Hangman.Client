using Hangman.Client.ViewModels;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hangman.Client.Views.UserControls
{
    public partial class MatchChatView : UserControl
    {
        private INotifyCollectionChanged observedMessages;

        public MatchChatView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SubscribeToMessages();
            ScrollToLastMessage();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeFromMessages();
        }

        private void OnDataContextChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeFromMessages();
            SubscribeToMessages();
            ScrollToLastMessage();
        }

        private void SubscribeToMessages()
        {
            MatchChatViewModel viewModel = DataContext as MatchChatViewModel;

            if (viewModel == null || viewModel.Messages == null)
            {
                return;
            }

            observedMessages = viewModel.Messages;
            observedMessages.CollectionChanged += OnMessagesCollectionChanged;
        }

        private void UnsubscribeFromMessages()
        {
            if (observedMessages == null)
            {
                return;
            }

            observedMessages.CollectionChanged -= OnMessagesCollectionChanged;
            observedMessages = null;
        }

        private void OnMessagesCollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                new System.Action(ScrollToLastMessage));
        }

        private void ScrollToLastMessage()
        {
            if (MessagesListBox == null ||
                MessagesListBox.Items == null ||
                MessagesListBox.Items.Count == 0)
            {
                return;
            }

            object lastItem =
                MessagesListBox.Items[MessagesListBox.Items.Count - 1];

            MessagesListBox.ScrollIntoView(lastItem);
            MessagesListBox.SelectedItem = lastItem;
        }

        private void OnMessageTextBoxKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            MatchChatViewModel viewModel = DataContext as MatchChatViewModel;

            if (viewModel == null ||
                viewModel.SendMessageCommand == null ||
                !viewModel.SendMessageCommand.CanExecute(null))
            {
                return;
            }

            e.Handled = true;
            viewModel.SendMessageCommand.Execute(null);
        }
    }
}
