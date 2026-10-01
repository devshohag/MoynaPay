# MoynaPay merchant app — design preview

This is a separate Kotlin Android app in the YoPay repository. Open
**this** folder (`moynapay-android`) in Android Studio for the merchant app.

The visual reference is `YoOrder-App-Design.pdf`, with **MoynaPay** as the app
name. `YoOrder-API-Reference.pdf` maps UI actions to the API. The default APK
still runs in fake mode with sample content, but the app now has real HTTP and
payment-listener wiring behind `ApiConfig`.

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
`yopay-android` capture device app. The default APK uses fake API mode unless
`ApiConfig.current` is switched to `ApiMode.Real`.

## Fake API mode for app wiring

The app now has a small API abstraction so the UI can be connected before the
real mobile API client is finished.

- `ApiConfig.kt` controls the mode.
- `ApiMode.Fake` is the default.
- Fake OTP is `1234`.
- `LoginActivity` now follows the real shape: request OTP, verify OTP, save a
  local session, then open Home.
- `PaymentsActivity` can fake-pair this phone as a payment listener and stores
  a separate device token locally.
- Dashboard auth and payment-listener credentials are saved separately in
  `SessionStore`.

When the real API is ready, implement `RealMoynaPayApi` behind the same
`MoynaPayApi` interface and switch `ApiConfig.current.mode` to `ApiMode.Real`.
The activities should not need to be rewritten.

## Real HTTP client

`RealMoynaPayApi` is wired with `HttpURLConnection` and no third-party
dependency. It currently supports:

- `POST /app/v1/auth/otp`
- `POST /app/v1/auth/token`
- `GET /app/v1/bootstrap`
- `GET /app/v1/home/numbers`
- `GET /app/v1/orders`
- `POST /app/v1/devices/pairing-token`
- `POST /app/v1/devices/pair`
- `POST /app/v1/devices/{deviceId}/heartbeat`
- `POST /app/v1/devices/{deviceId}/events`

Keep `ApiMode.Fake` for design/demo APKs. For local backend testing, set
`ApiMode.Real` in `ApiConfig.kt` and point `baseUrl` at the tunnel, emulator
host (`http://10.0.2.2:port`) or LAN address.

## Payment listener

The payments tab can pair this phone as the listener device. In fake mode this
stores a fake device token; in real mode it uses the pairing endpoints. The APK
can request SMS receive permission, open Android notification listener settings,
queue bKash-looking SMS/notification events locally, and sync the queue to
`/app/v1/devices/{deviceId}/events`. A heartbeat is sent after a flush when a
paired device token is available.

## Real phone test checklist

Use this flow after GitHub Actions builds the debug APK.

1. Install the APK and test fake mode first with OTP `1234`.
2. Run the backend locally and expose it with a temporary HTTPS tunnel or LAN
   address.
3. Open Settings, tap the API address row, enable **Use real API**, and enter
   the backend base URL without a trailing path, for example
   `https://your-tunnel.example.com`.
4. Login again so the app gets a real dashboard token.
5. Open Payments, tap **Pair phone**, then enable SMS and notification listener
   access.
6. Send or receive a real bKash message/notification on the phone.
7. Tap **Sync queued events** if the event did not upload immediately.

Changing API mode or base URL clears the saved login/device session so fake and
real tokens do not get mixed.
