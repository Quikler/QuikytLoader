# Telegram Bot Setup Guide

This guide explains how to set up and use the Telegram bot integration in QuikytLoader.

## Prerequisites

1. A Telegram account
2. QuikytLoader application installed

## Step 1: Create Your Telegram Bot

1. Open Telegram and search for **@BotFather**
2. Start a chat and send the command: `/newbot`
3. Follow the prompts:
   - Choose a name for your bot (e.g., "My Music Bot")
   - Choose a username (must end with 'bot', e.g., "my_music_loader_bot")
4. **BotFather will send you a bot token** - It looks like this:
   ```
   1234567890:ABCdefGHIjklMNOpqrsTUVwxyz
   ```
5. **Copy this token** - you'll need it in Step 3

## Step 2: Get Your Chat ID

To get your Chat ID, use **@userinfobot**:

1. Search for **@userinfobot** in Telegram
2. Start a chat and send any message
3. The bot will reply with your user information, including your Chat ID
4. Copy the Chat ID number (it will be a number like `123456789`)

**Example response from @userinfobot:**
```
Id: 123456789
First name: John
Username: @john_doe
```

Copy the **Id** value - this is your Chat ID.

## Step 3: Configure QuikytLoader

1. Launch QuikytLoader:
   ```bash
   dotnet run
   ```

2. Navigate to the **Settings** tab (in the left sidebar)

3. Enter your credentials:
   - **Bot Token**: Paste the token from Step 1
   - **Chat ID**: Paste the Chat ID from Step 2

4. Click **"Save Settings"**

5. You should see: "Settings saved successfully!"

## Step 4: Using the Application

1. Navigate to the **Home** tab

2. Paste a YouTube URL (e.g., `https://www.youtube.com/watch?v=dQw4w9WgXcQ`)

3. Click **"Download and Send"**

4. The app will:
   - Download the video from YouTube
   - Convert it to MP3 with metadata and thumbnail
   - Send the audio file to your Telegram chat

5. Check your Telegram chat with the bot - the audio file should appear!

## Technical Details

### Where Settings Are Stored

Settings are saved to:
```
~/.config/QuikytLoader/settings.json
```

The file has restricted permissions (mode 600) for security.

### Bot Lifecycle

- The bot **initializes on first use** (lazy initialization) - only when you send your first file
- The bot **only sends files** - it doesn't listen for incoming messages
- Settings are **reloaded automatically** each time you send a file - no need to restart the app
- The bot **stops automatically** when you close QuikytLoader

## Troubleshooting

### "Bot token is not configured"
- Make sure you entered the token in Settings and clicked "Save Settings"
- Check that the token is correct (no extra spaces)

### "Chat ID is not configured"
- Follow Step 2 to get your Chat ID
- Enter it in Settings and click "Save Settings"

### "Failed to start Telegram bot"
- Check your internet connection
- Verify the bot token is correct
- Make sure you copied the complete token from @BotFather

### Audio file doesn't arrive
- Confirm the Chat ID is correct
- Check that the download completed successfully
- Look for error messages in the terminal

## Security Notes

- **Bot Token**: This is sensitive! Don't share it. It allows full control of your bot.
- **Chat ID**: This is your Telegram user ID. Less sensitive but still private.
- **File Permissions**: Settings are stored with user-only access (mode 600 on Linux)

## Example Complete Workflow

```bash
# 1. Get your Chat ID from @userinfobot in Telegram
#    Send any message to @userinfobot
#    Copy your Chat ID (e.g., 987654321)

# 2. Start the application
cd /home/quikler/Desktop/repos/QuikytLoader
dotnet run

# 3. In the app:
#    - Go to Settings
#    - Enter Bot Token: 1234567890:ABCdefGHIjklMNOpqrsTUVwxyz
#    - Enter Chat ID: 987654321
#    - Click Save

# 4. In the app:
#    - Go to Home
#    - Paste YouTube URL
#    - Click "Download and Send"

# 5. Check Telegram - audio file arrives! 🎵
```

## Support

If you encounter issues:
1. Check the terminal output for error messages
2. Verify all settings are correct
3. Try restarting the application
4. Check that yt-dlp is installed: `which yt-dlp`
