using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Hotel_Booking_System.Interfaces;
using DotNetEnv;

namespace Hotel_Booking_System.Services
{
    internal class MailService
    {
        private static readonly string userEmail = Environment.GetEnvironmentVariable("ZOHO_MAIL_USER")!;
        private static readonly string password = Environment.GetEnvironmentVariable("ZOHO_MAIL_PASSWORD")!;
        public static async Task<bool> SendOTP(string otp, string receiver)
        {
            try
            {
                var fromAddress = new MailAddress(userEmail, "NTT Hotel");
                var toAddress = new MailAddress(receiver);

                string subject = "NTT Hotel Xin Chào!";
                string body = $"Đây là mã OTP của bạn: {otp}\nVui lòng không chia sẻ mã này cho bất cứ ai!";

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.zoho.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, password)
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    await smtp.SendMailAsync(message);
                }

                return true; 
            }
            catch
            {
                return false; 
            }
        }



    }
}

