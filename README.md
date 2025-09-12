# Hotel Booking System

This project is a WPF-based hotel booking application.

## Configuration

The application loads environment variables from a `.env` file at startup via the
[`DotNetEnv`](https://github.com/tonerdo/dotnet-env) library.
Create a `.env` file in the project root and define the following variables:

```env
GEMINI_API_KEY=your_gemini_api_key
GEMINI_MODEL=gemini-pro
```

- `GEMINI_API_KEY` – API key used to authenticate with the Gemini API.
- `GEMINI_MODEL` – The Gemini model to use for AI features (for example `gemini-pro` or `gemini-1.5-flash`).
  Set this value to switch models without modifying code.

Ensure these variables are configured in your deployment environment to enable the AI chat features.
