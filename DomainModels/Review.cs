using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Booking_System.DomainModels
{
    public class Review : INotifyPropertyChanged
    {
        [Key]
        public string ReviewID { get; set; }
        public string UserID { get; set; }
        public string HotelID { get; set; }
        public string RoomID { get; set; }
        public string BookingID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        private string? _adminReply;
        public string? AdminReply
        {
            get => _adminReply;
            set
            {
                if (_adminReply == value)
                {
                    return;
                }

                _adminReply = value;
                OnPropertyChanged(nameof(AdminReply));
            }
        }

        [NotMapped]
        private string _reviewerName = string.Empty;
        public string ReviewerName
        {
            get => _reviewerName;
            set
            {
                if (_reviewerName == value)
                {
                    return;
                }

                _reviewerName = value;
                OnPropertyChanged(nameof(ReviewerName));
            }
        }

        [NotMapped]
        private string _reviewerAvatarUrl = string.Empty;
        public string ReviewerAvatarUrl
        {
            get => _reviewerAvatarUrl;
            set
            {
                if (_reviewerAvatarUrl == value)
                {
                    return;
                }

                _reviewerAvatarUrl = value;
                OnPropertyChanged(nameof(ReviewerAvatarUrl));
            }
        }

        [NotMapped]
        private string _adminReplyDraft = string.Empty;
        public string AdminReplyDraft
        {
            get => _adminReplyDraft;
            set
            {
                if (_adminReplyDraft == value)
                {
                    return;
                }

                _adminReplyDraft = value;
                OnPropertyChanged(nameof(AdminReplyDraft));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
