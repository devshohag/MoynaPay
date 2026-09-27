# MoynaPay merchant app — design preview

This is a separate Kotlin Android app in the YoPay repository. The existing
`yopay-android` project still serves as the payment capture device app. Open
**this** folder (`moynapay-android`) in Android Studio for the merchant app.

The visual reference is `YoOrder-App-Design.pdf`, with **MoynaPay** as the app
name. `YoOrder-API-Reference.pdf` maps future UI actions to the planned API;
this design preview makes no network calls. The figures, orders, merchant name,
and service states shown on the home screen are sample content.

## Four design phases

| Phase | Screens and UI work | API reference for later integration |
| --- | --- | --- |
| 1 — foundation | Brand styles, login, home, five-tab shell. **Implemented as a static preview.** | `/app/v1/auth/*`, `/app/v1/bootstrap`, `/app/v1/home` |
| 2 — orders | Filtered list, order detail and event timeline, recording and courier/action states. **Implemented as a local preview.** | `/app/v1/orders/*` |
| 3 — calls | Needs-human queue, manual decision states, call script and retry settings. **Implemented as a local preview.** | `/app/v1/queue/human`, `/app/v1/settings/calls` |
| 4 — payments and settings | Payment overview, linked phone, services, masked credentials, webhook and profile UI. **Implemented as a local preview.** | `/app/v1/payments`, `/app/v1/device`, `/app/v1/settings/*`, `/app/v1/subscription`, `/app/v1/profile` |

The source PDF describes eight principal screens. The needs-human state remains
distinct from cancellation, and API keys must never be represented as a
recoverable full secret after creation. Phase 4 must show only a masked key in
the regular settings screen.

## Design preview

Launch the app and tap **ডেমো হোম দেখুন**. The number and OTP fields are visual
only; no SMS is sent and no login is performed. Home displays sample numbers
and orders. Tap **অর্ডার**, **সব দেখুন**, or a recent order card to see phase 2.
The list filters seven sample orders locally. "Waiting" includes the sample
calling order and three needs-human orders; "Confirmed" includes the shipped
sample. The home counters are visual examples from the design brief, not totals
of these seven sample orders. Tap the home alert or **কল** tab for the three
sample needs-human orders. Open call settings from the gear in that queue.
Script tokens insert into the text field, and the speech switch changes locally.
Save, time and retry controls display preview messages; changes are not stored.
The call, recording, cancel and courier buttons only display a preview message;
they cannot start a call, play audio, decide an order, or ship. All five tabs
now open their preview screens. The payments tab shows a sample linked phone,
wallet, totals and three sample transactions. The settings tab shows service
states, a masked **demo** key, and webhook/profile preview dialogs. The
`api.moynapay.example` address is deliberately an unusable example, not a live
endpoint. No API key is generated, retrieved or copied. Service switches return
to their sample state on tap; no configuration or logout is performed.

Build in Android Studio using JDK 17 and Android SDK 35. Gradle 8.9, Android
Gradle Plugin 8.7.3 and Kotlin 2.0.21 match the existing device project. There
is no Gradle wrapper JAR in this repository, so use Android Studio's Gradle
installation or a local Gradle 8.9 installation.

## Debug APK from GitHub Actions

Pushing changes in `moynapay-android/` (including the first commit that adds the
project) triggers `.github/workflows/moynapay-android.yml`. You can also run it
manually from **Actions → moynapay-android → Run workflow**. After the job
succeeds, download **moynapay-merchant-debug-apk** from the run's **Artifacts**
section. The workflow builds the separate merchant app, not the
`yopay-android` capture device app. The APK is a design preview with sample
data and no API calls.
