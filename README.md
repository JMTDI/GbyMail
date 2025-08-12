# 🔍 GbyMail
## Search & Email Results Application with Enhanced PDF Viewer

> **Version:** 2.3.0  
> **Created by:** JMTDI  
> **Last Updated:** 2025-08-12 20:42:49 UTC  

---

## 📋 Table of Contents
- [Overview](#-overview)
- [Features](#-features)
- [System Requirements](#-system-requirements)
- [Installation](#-installation)
- [Email Client Setup](#email-client-setup)
- [Usage](#-usage)
- [Search Platforms](#-search-platforms)
- [File Management](#-file-management)
- [Troubleshooting](#-troubleshooting)
- [Technical Details](#-technical-details)
- [Support](#-support)

---

## 🎯 Overview

GbyMail is a powerful Windows application that combines web search functionality with an enhanced PDF viewer. The application automatically composes emails to `auto@ibyfax.com` when you perform searches or click links in PDF documents. All emails are sent with **subject-only content (no body)** for streamlined communication.

**Key Highlights:**
- 🌐 Multi-platform search (Google, DuckDuckGo, eBay, Amazon)
- 📄 Advanced PDF viewer with automatic https:// link detection
- 📧 Auto-email composition with configurable recipients
- 🗂️ Search history management
- 🔗 One-click link sharing from PDFs

---

## ✨ Features

### 🔍 Search Functionality
- **Multi-Platform Search**: Google, DuckDuckGo, eBay, Amazon
- **Search History**: Automatically saves and manages up to 25 recent searches
- **Full-Width Interface**: Optimized for large screens and productivity
- **Keyboard Shortcuts**: Press Enter to search with Google

### 📄 PDF Management
- **Enhanced PDF Viewer**: Full native PDF rendering with WebView2
- **Automatic Link Detection**: Clicks on https:// links auto-compose emails
- **Import & Organize**: Copy PDFs to managed GbyMail folder
- **File Operations**: Open containing folders, send file links

### 📧 Email Integration
- **Auto-Composition**: Mailto links with subject-only content
- **No Body Mode**: Clean, minimal email format
- **Instant Feedback**: Visual notifications for all email actions
- **Timestamp Tracking**: Full audit trail with UTC timestamps

### 🎨 User Experience
- **Modern Interface**: Clean, professional Windows design
- **Responsive Layout**: Adapts to different screen sizes
- **Tab-Based Navigation**: Search, Inbox, PDF Viewer tabs
- **Visual Indicators**: Clear status and progress feedback

---

## 💻 System Requirements

### Minimum Requirements
- **Operating System**: Windows 10 (version 1803 or later)
- **Architecture**: x64 (64-bit)
- **.NET Runtime**: .NET 6.0 Windows Runtime
- **WebView2**: Microsoft Edge WebView2 Runtime
- **Memory**: 4 GB RAM
- **Storage**: 100 MB available space
- **Email Client**: Default email client configured (Thunderbird, Outlook, etc.)

### Recommended Requirements
- **Operating System**: Windows 11
- **Memory**: 8 GB RAM or more
- **Storage**: 500 MB available space (for PDF storage)
- **Network**: Active internet connection for search functionality

---

## 🛠️ Installation

### Method 1: Using the Installer (Recommended)
1. Download `GbyMail_v2.3.0_Setup_JMTDI_2025-08-12_20-42-49.exe`
2. **Run as Administrator** (right-click → "Run as administrator")
3. Follow the installation wizard
4. Choose installation directory (default: `C:\Program Files\GbyMail`)
5. Select additional options:
   - ✅ Create desktop shortcut
   - ✅ Associate with PDF files
   - ✅ Add to Start Menu
6. Complete installation

### Method 2: Manual Installation
1. Extract files to desired folder
2. Ensure all dependencies are present:
   - `GbyMail.exe` (main application)
   - `GbyMail.exe.WebView2` folder
   - `runtimes` folder
   - All `.dll` files
3. Run `GbyMail.exe`

### Post-Installation
- **Desktop Shortcut**: Located on desktop as "GbyMail"
- **Start Menu**: Found under "Programs" → "GbyMail"
- **File Association**: PDFs can be opened with "Open with GbyMail"
- **Registry Entries**: Application registered with Windows

---

## 📧 Email Client Setup

### For Thunderbird & Betterbird Users

**Important**: To enable GbyMail to automatically open PDFs when clicking email links, follow these steps:

#### Step 1: Access Settings
1. Open **Thunderbird** or **Betterbird**
2. Go to **Settings** (or **Preferences** on older versions)
3. Navigate to **General** tab

#### Step 2: Configure File Handling
1. Scroll down to **Files & Attachments** section
2. Look for **Content Type** table
3. Find **Portable Document Format (PDF)** entry
4. Under the **Action** column, click the dropdown

#### Step 3: Set GbyMail as Handler
1. Select **Use other...**
2. Click **Choose...** (or **Other...**)
3. Navigate to GbyMail installation directory

#### Step 4: Locate GbyMail Executable
**Default Installation Path:**
