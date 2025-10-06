using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotel_Manager.FrameWorks;

namespace Hotel_Booking_System.DomainModels
{
    public class AIChat : Bindable
    {
        [Key]
        public string ChatID {  get; set; } = null!;
        public string UserID { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        private string _response = string.Empty;
        public string Response
        {
            get => _response;
            set => Set(ref _response, value);
        }

        public DateTime CreatedAt { get; set; }

        [NotMapped]
        private bool _isTyping;
        [NotMapped]
        public bool IsTyping
        {
            get => _isTyping;
            set => Set(ref _isTyping, value);
        }

        [NotMapped]
        private string _typingIndicator = string.Empty;
        [NotMapped]
        public string TypingIndicator
        {
            get => _typingIndicator;
            set => Set(ref _typingIndicator, value);
        }

        [NotMapped]
        private ObservableCollection<ChatRoomSuggestion> _suggestedRooms = new();

        public AIChat()
        {
            _suggestedRooms.CollectionChanged += SuggestedRoomsChanged;
        }

        [NotMapped]
        public ObservableCollection<ChatRoomSuggestion> SuggestedRooms
        {
            get => _suggestedRooms;
            set
            {
                _suggestedRooms.CollectionChanged -= SuggestedRoomsChanged;
                var newValue = value ?? new ObservableCollection<ChatRoomSuggestion>();
                Set(ref _suggestedRooms, newValue);
                _suggestedRooms.CollectionChanged += SuggestedRoomsChanged;
                OnPropertyChanged(nameof(HasSuggestions));
            }
        }

        [NotMapped]
        public bool HasSuggestions => SuggestedRooms?.Count > 0;

        private void SuggestedRoomsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSuggestions));
        }
    }
}
