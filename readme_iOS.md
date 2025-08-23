# GbyMail

GbyMail is an iOS productivity app and share extension that lets you quickly search for information, manage PDFs, and email links from within PDFs directly to `auto@ibyfax.com`. Designed for seamless sharing and fast workflow, GbyMail integrates with the iOS share sheet and provides a powerful PDF viewer with unique link-to-email functionality.

---

## Features

- **Share Extension**  
  - Instantly preview PDFs shared from any app using the iOS Share Sheet.
  - Tap any link inside a PDF to compose an email to `auto@ibyfax.com` with the link as the subject—no need to switch apps.

- **App Functionality**  
  - **Search Tab:**  
    - Perform Google, eBay, or Amazon searches directly in the app.
    - Instantly generate a mail draft to `auto@ibyfax.com` with your search URL as the subject.
  - **Inbox Tab:**  
    - View and manage emails received at `auto@ibyfax.com`.
    - Import PDFs directly from the Files app or other sources.
  - **PDF Viewer Tab:**  
    - View any PDF imported or received.
    - Tap links in PDFs to email them instantly.

- **SwiftUI & PDFKit Powered**  
  - Modern, responsive UI.
  - Rich PDF viewing experience with link detection.

---

## How It Works

### 1. Share PDFs from Any App
- Open a PDF in Files, Safari, or any other app.
- Tap the Share button, and choose **GbyMail**.
- The extension instantly previews the PDF.
- Tap any link in the PDF to create an email draft to `auto@ibyfax.com` (subject set to the link).

### 2. Search and Send
- Use the Search tab to find info on Google, eBay, or Amazon.
- The app prepares an email draft to `auto@ibyfax.com` with your search as the subject.

### 3. Manage Your Inbox
- View and open PDFs received via email.
- Import new PDFs from Files at any time.

---

## Installation

### 📲 Install GbyMail on Your Device

GbyMail is distributed as an .ipa file for sideloading. You can install it using [AltStore](https://altstore.io) or [Sideloadly](https://sideloadly.io) on your iPhone or iPad.

> **Note:** You cannot install GbyMail directly from the App Store.  
> A paid Apple Developer account is NOT required for personal use, but you will need your Apple ID.

---

### 🔵 **AltStore Instructions**

AltStore is a popular and free sideloading solution for iOS apps.

**1. Download and Install AltServer**
- Visit [altstore.io](https://altstore.io) and download AltServer for Windows or macOS.
- Install AltServer following the instructions for your platform.

**2. Set Up AltStore on Your iOS Device**
- Connect your iPhone/iPad to your computer.
- Open AltServer and install AltStore to your device (see AltStore’s website for detailed steps).

**3. Trust the Developer Profile**
- On your device, go to `Settings > General > Device Management` (or Profiles & Device Management).
- Trust the profile associated with your Apple ID.

**4. Download the GbyMail .ipa**
- Download the latest [GbyMail.ipa](https://github.com/JMTDI/GbyMail/releases/latest/download//GbyMail_2.3.0.ipa) from the Releases section or build one with Xcode.

**5. Install GbyMail via AltStore**
- Open AltStore on your device.
- Tap the `+` icon and select the GbyMail .ipa to install.

**6. Open GbyMail**
- After installation, GbyMail will appear on your home screen.

---

### 🟣 **Sideloadly Instructions**

Sideloadly is another user-friendly sideloading tool (Windows & macOS).

**1. Download and Install Sideloadly**
- Visit [sideloadly.io](https://sideloadly.io) and install Sideloadly for your operating system.

**2. Download the GbyMail .ipa**
- Download the latest [GbyMail.ipa](link-to-your-ipa) from the Releases section.

**3. Sideload the App**
- Open Sideloadly, connect your device, and drag the .ipa into the app.
- Enter your Apple ID and follow on-screen instructions.
- After installation, trust the developer profile on your device.

**4. Open GbyMail**
- GbyMail will appear on your home screen.

---

## Screenshots

*(Add screenshots here once available)*

---

## Usage Tips

- For the share extension to appear, enable it after first install via the iOS share menu.
- Make sure your device is set up to send emails (Mail accounts configured).
- Tap any hyperlink in a PDF to quickly email it without manually copying.

---

## Development

- Built with **SwiftUI**, **PDFKit**, and **UIKit** interoperability.
- Modular codebase for easy extension.
- Contributions welcome—open an issue or submit a pull request!

---

## License

MIT License

---

## Credits

Developed by [JMTDI](https://github.com/JMTDI)

---
