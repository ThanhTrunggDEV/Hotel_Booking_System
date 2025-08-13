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
    internal class MailService : IMailService
    {

        public void SendOTP(string otp, string receiver)
        {
           
            string s = Env.GetString("ZOHO_MAIL_USER");
            string userEmail = "marketing@lichuni.id.vn";//Environment.GetEnvironmentVariable("ZOHO_MAIL_USER")!;
            string password = "jKSDbyc4zcqY"; //Environment.GetEnvironmentVariable("ZOHO_MAIL_PASSWORD")!;

            var fromAddress = new MailAddress(userEmail, "NTT Hotel");
            var toAddress = new MailAddress(receiver, "Test");

            
            string subject = "Test email from NTT";
            string body = "Hello, this is a test email sent via Zoho SMTP!";

            var smtp = new SmtpClient
            {
                Host = "smtp.zoho.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, password)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }

            
        }


    }
}

