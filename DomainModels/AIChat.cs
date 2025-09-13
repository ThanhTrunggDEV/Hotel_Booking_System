using System;
using System.Collections.Generic;
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
        public string ChatID {  get; set; }
        public string UserID { get; set; }
        public string Message { get; set; }

        private string _response;
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
    }
}
