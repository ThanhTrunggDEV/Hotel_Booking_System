# 🏨 Hotel Booking System - NTT

A comprehensive hotel management and booking system built with **WPF (Windows Presentation Foundation)** and **.NET 8.0**. This application provides a complete solution for hotel administration, customer bookings, and AI-powered room recommendations.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=flat-square&logo=windows)
![C#](https://img.shields.io/badge/C%23-Language-239120?style=flat-square&logo=csharp)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?style=flat-square&logo=sqlite)

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [User Roles](#-user-roles)
- [Screenshots](#-screenshots)
- [AI Integration](#-ai-integration)
- [Database Schema](#-database-schema)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### 🔐 Authentication & Authorization
- **Multi-role system**: Customer, Hotel Admin, Super Admin
- Secure password hashing and verification
- Email-based password recovery with OTP
- Session management and user authentication

### 👤 Customer Features
- **Hotel Search & Filter**: Search by location, price range, rating, amenities
- **Room Browsing**: View available rooms with detailed information
- **Booking Management**: Create, modify, and cancel bookings
- **AI Chatbot Assistant**: Get personalized room recommendations
- **Review System**: Leave ratings and reviews for hotels
- **Payment Processing**: Secure payment handling
- **Booking History**: Track past and upcoming bookings
- **Profile Management**: Update personal information and avatar

### 🏪 Hotel Admin Features
- **Hotel Management**: Add, edit, and delete hotels
- **Room Management**: Manage room inventory, pricing, and availability
- **Booking Management**: Approve/reject booking requests
- **Revenue Analytics**: 
  - Weekly, monthly, and yearly revenue charts (powered by OxyPlot)
  - Interactive revenue visualization
  - Cumulative revenue tracking
- **Review Management**: Respond to customer reviews
- **Dashboard**: Real-time booking insights and statistics
- **Amenity Management**: Configure hotel facilities (WiFi, Pool, Parking, etc.)

### 🛡️ Super Admin Features
- **User Management**: Manage all system users
- **Hotel Approval**: Review and approve new hotel registrations
- **System Overview**: Monitor platform-wide statistics
- **Admin Controls**: Comprehensive system administration tools

### 🤖 AI-Powered Features
- **Smart Room Suggestions**: AI analyzes user queries to recommend suitable rooms
- **Natural Language Processing**: Chat in Vietnamese with context-aware responses
- **Intelligent Filtering**: Automatic budget, location, and capacity matching
- **Conversation History**: Maintains chat context for better recommendations

---

## 🛠️ Tech Stack

### Frontend
- **WPF (Windows Presentation Foundation)**: Modern desktop UI framework
- **XAML**: Declarative UI markup
- **MVVM Pattern**: Model-View-ViewModel architecture
- **CommunityToolkit.Mvvm**: MVVM helpers and commands
- **OxyPlot.Wpf**: Advanced charting library for revenue analytics

### Backend
- **.NET 8.0**: Latest .NET framework
- **Entity Framework Core**: ORM for database operations
- **SQLite**: Lightweight embedded database

### External Services
- **Google Gemini AI**: AI chatbot integration
- **Cloudinary**: Image upload and storage service

### Libraries & Packages
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="DotNetEnv" Version="3.1.1" />
<PackageReference Include="OxyPlot.Wpf" Version="2.1.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.8" />
```

---

## 🏗️ Architecture

### MVVM Pattern
```
┌─────────────────────────────────────────────────┐
│                    View (XAML)                  │
│  LoginWindow, UserWindow, HotelAdminWindow, etc │
└────────────────────┬────────────────────────────┘
                     │ Data Binding
┌────────────────────▼────────────────────────────┐
│              ViewModel (C#)                     │
│  Commands, Properties, Business Logic           │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│         Services & Repositories                 │
│  Authentication, Navigation, AI, Data Access    │
└────────────────────┬────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────┐
│              Domain Models                      │
│  User, Hotel, Room, Booking, Payment, Review    │
└─────────────────────────────────────────────────┘
```

### Dependency Injection
The application uses constructor injection for all ViewModels and Services, managed through `IServiceProvider`.

---

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- .NET 8.0 SDK or later
- Visual Studio 2022 (recommended)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/ThanhTrunggDEV/Hotel_Booking_System.git
   cd Hotel_Booking_System
   ```

2. **Configure environment variables**
   
   Create a `.env` file in the project root:
   ```env
   GEMINI_API_KEY=your_gemini_api_key_here
   CLOUDINARY_CLOUD_NAME=your_cloud_name
   CLOUDINARY_API_KEY=your_api_key
   CLOUDINARY_API_SECRET=your_api_secret
   ```

3. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```
   Or press `F5` in Visual Studio

### Default Credentials

**Customer Account:**
```
Email: user@example.com
Password: User@123
```

**Hotel Admin Account:**
```
Email: admin@hotel.com
Password: Admin@123
```

**Super Admin Account:**
```
Email: superadmin@ntt.com
Password: SuperAdmin@123
```

---

## 📁 Project Structure

```
Hotel_Booking_System/
├── App.xaml                      # Application resources & styles
├── App.xaml.cs                   # Application entry point & DI setup
│
├── Behaviors/                    # Custom WPF behaviors
│   └── AutoScrollBehavior.cs     # Auto-scroll for chat
│
├── Converters/                   # Value converters for data binding
│   ├── BookingStatusToVietnameseConverter.cs
│   ├── DateDiffConverter.cs
│   ├── FilterHotelConverter.cs
│   ├── ImageUrlToBitMap.cs
│   └── ...
│
├── DomainModels/                 # Business entities
│   ├── User.cs
│   ├── Hotel.cs
│   ├── Room.cs
│   ├── Booking.cs
│   ├── Payment.cs
│   ├── Review.cs
│   ├── AIChat.cs
│   └── ...
│
├── Interfaces/                   # Abstractions & contracts
│   ├── IRepository.cs
│   ├── IAuthentication.cs
│   ├── INavigationService.cs
│   ├── IAIChatService.cs
│   └── ...
│
├── Repository/                   # Data access layer
│   ├── UserRepository.cs
│   ├── HotelRepository.cs
│   ├── BookingRepository.cs
│   └── ...
│
├── Services/                     # Business logic services
│   ├── AuthenticationService.cs
│   ├── NavigationService.cs
│   ├── AIChatService.cs
│   ├── UploadImageService.cs
│   └── ...
│
├── ViewModels/                   # MVVM ViewModels
│   ├── LoginViewModel.cs
│   ├── UserViewModel.cs
│   ├── HotelAdminViewModel.cs
│   ├── SuperAdminViewModel.cs
│   └── ...
│
├── Views/                        # WPF Windows & Dialogs
│   ├── LoginWindow.xaml
│   ├── UserWindow.xaml
│   ├── HotelAdminWindow.xaml
│   ├── SuperAdminWindow.xaml
│   ├── BookingDialog.xaml
│   ├── PaymentDialog.xaml
│   └── ...
│
├── Helpers/                      # Utility classes
│   └── StringGuidValueGenerator.cs
│
└── Resources/                    # Images, icons, assets
    └── Chatbotlogo.png
```

---

## 👥 User Roles

### 🛒 Customer (User)
- Browse hotels and rooms
- Make and manage bookings
- Chat with AI assistant
- Leave reviews
- Process payments
- View booking history

### 🏪 Hotel Admin
- Manage owned hotels
- Add/edit/delete rooms
- Approve/reject bookings
- View revenue analytics
- Respond to reviews
- Update hotel information

### 🛡️ Super Admin
- Oversee entire system
- Approve new hotels
- Manage all users
- View platform statistics
- System-wide administration

---

## 🖼️ Screenshots

### Login & Authentication
Beautiful gradient-based login screen with password recovery.

### Customer Dashboard
- Interactive hotel search with filters
- Room cards with hover animations
- AI chatbot panel
- Booking management

### Hotel Admin Dashboard
- Revenue charts (weekly, monthly, yearly)
- Booking status overview
- Room management interface
- Review management

### Super Admin Panel
- User management grid
- Hotel approval workflow
- System statistics

---

## 🤖 AI Integration

### Gemini AI Chatbot

The system integrates **Google Gemini AI** for intelligent room recommendations.

#### Features:
- **Vietnamese Language Support**: Full conversational AI in Vietnamese
- **Context-Aware**: Maintains chat history for coherent conversations
- **Smart Filtering**:
  - Budget extraction from natural language ("500k-1tr", "2 triệu")
  - Guest count detection ("3 người", "family room")
  - Location matching (accent-insensitive)
- **Room Suggestions**: Up to 3 personalized room recommendations
- **Retry Logic**: Automatic retry with exponential backoff
- **Data-Driven**: Uses real hotel/room data for accurate responses

#### System Prompt Strategy:
The AI receives:
- All available hotels and rooms
- User ratings and reviews
- User's booking history
- Strict rules to only use provided data
- Formatting guidelines for consistent responses

#### Configuration:
```csharp
new GeminiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY"),
    DefaultModel = "gemini-1.5-flash",
    MaxOutputTokens = 2048,
    Temperature = 0.4  // Balanced creativity/accuracy
}
```

---

## 🗄️ Database Schema

### Core Entities

**Users**
- UserID (PK)
- FullName, Email, Phone
- Password (hashed)
- Role (User/HotelAdmin/SuperAdmin)
- DateOfBirth, Gender, AvatarUrl

**Hotels**
- HotelID (PK)
- UserID (FK)
- HotelName, Address, City
- Rating, MinPrice, MaxPrice
- IsApproved, IsVisible
- Amenities (JSON)

**Rooms**
- RoomID (PK)
- HotelID (FK)
- RoomNumber, RoomType
- Capacity, PricePerNight
- Status (Available/Booked/Maintenance)

**Bookings**
- BookingID (PK)
- UserID (FK), HotelID (FK), RoomID (FK)
- CheckInDate, CheckOutDate
- NumberOfGuests, GuestName
- Status (Pending/Confirmed/Cancelled)

**Payments**
- PaymentID (PK)
- BookingID (FK)
- TotalPayment, PaymentMethod
- PaymentDate, Status

**Reviews**
- ReviewID (PK)
- UserID (FK), HotelID (FK)
- Rating (1-5), Comment
- AdminReply, CreatedAt

**AIChats**
- ChatID (PK)
- UserID (FK)
- Message, Response
- SuggestedRooms (JSON)
- CreatedAt

---

## 🎨 UI/UX Features

### Modern Design
- **Gradient Themes**: Purple/blue gradient accents
- **Card-Based Layout**: Clean, organized interface
- **Smooth Animations**: 
  - Hover effects with scale transforms
  - Drop shadow animations (0.2s duration)
  - Fade transitions

### Responsive Components
- **Auto-scrolling chat**: Behavior-driven smooth scrolling
- **Dynamic filters**: Real-time search and filtering
- **Interactive charts**: OxyPlot-powered revenue visualization
- **Image upload**: Cloudinary integration with preview

### Accessibility
- Clear visual hierarchy
- High contrast text
- Intuitive navigation
- Error feedback messages

---

## 🔒 Security Features

- **Password Hashing**: BCrypt-based password security
- **Role-Based Access Control**: Strict permission enforcement
- **Input Validation**: Client and server-side validation
- **SQL Injection Prevention**: EF Core parameterized queries
- **Session Management**: Secure user session handling

---

## 📊 Analytics & Reporting

### Revenue Analytics (Hotel Admin)
- **Time-based filtering**: Week/Month/Year/Cumulative
- **Visual charts**: Line charts with interactive tooltips
- **Revenue metrics**:
  - Total revenue per period
  - Maximum revenue point
  - Trend analysis

### Booking Insights
- Total bookings count
- Pending approvals
- Confirmed bookings
- Cancellation requests
- Today's check-ins

---

## 🔧 Configuration

### App Settings
Configure in `.env` file:
```env
# AI Configuration
GEMINI_API_KEY=your_key_here

# Image Upload
CLOUDINARY_CLOUD_NAME=your_cloud
CLOUDINARY_API_KEY=your_key
CLOUDINARY_API_SECRET=your_secret

# Database (optional - defaults to SQLite)
DATABASE_PATH=data.dat
```

---

## 🚧 Future Enhancements

- [ ] Multi-language support (English, Vietnamese)
- [ ] Email notifications for bookings
- [ ] Real-time chat between users and admins
- [ ] Advanced search with map integration
- [ ] Mobile app version (Xamarin/MAUI)
- [ ] Payment gateway integration (PayPal, Stripe)
- [ ] Booking calendar view
- [ ] Report generation (PDF exports)
- [ ] Two-factor authentication
- [ ] Social media login integration

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Style
- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation for public methods
- Write unit tests for new features

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Thanh Trung DEV**
- GitHub: [@ThanhTrunggDEV](https://github.com/ThanhTrunggDEV)
- Repository: [Hotel_Booking_System](https://github.com/ThanhTrunggDEV/Hotel_Booking_System)

---

## 🙏 Acknowledgments

- **OxyPlot** - For beautiful chart rendering
- **Google Gemini AI** - For intelligent chatbot capabilities
- **Cloudinary** - For image hosting services
- **CommunityToolkit.Mvvm** - For MVVM framework support
- **Entity Framework Core** - For database management

---

## 📞 Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Contact: trungff07@gmail.com

---

**⭐ If you find this project useful, please consider giving it a star!**

---

*Built with ❤️ using .NET 8.0 and WPF*
