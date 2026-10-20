# Kdock WPF app spec

The legacy WinForms dock stays in the solution and keeps shipping as today’s `Kdock.exe`. The new product is a separate WPF app. No legacy window is redesigned.

## Product split

| App | Project | Role |
|---|---|---|
| Legacy | existing `Kdock` WinForms exe | Unchanged. Same dock, LAN window, chat, files, remote view. |
| New | `Kdock.App` WPF exe | The only app we add features to. |
| Wire | `Kdock.Wire` class library | Packet format shared by both, so the bytes do not drift. |
| Meeting | existing `Kdock.Meeting` + `Kdock.Server` | Server stays. WPF and the service both use it. |

The only legacy edit allowed later is a project reference to `Kdock.Wire` if packet types move there, with no window or behavior change.

New app data lives in `%AppData%\Kdock\`. Legacy keeps `%AppData%\Task\`. On first launch, if `desk.id` exists in the legacy folder and the new folder has none, copy that id so the same desk number continues.

## 1. WPF shell

One process, two windows.

**Edge dock.** A borderless bar on the left, right, or center, remembered per machine. It shows the clock, machine name, desk ID, and online or offline. Buttons: People, Apps, Notes, Lock, Exit. The Apps and Notes panels open beside the bar and are the same width as the bar. The menu list opens downward and stops at the last item. Width and height come from layout, not from a pixel timer. Dragging the bar snaps it to a screen edge.

**People window.** Opened from the dock. Three columns:

- Left, about 280 px: your ID with Copy, a search box, Favorites, On this network, Recent.
- Center: the open conversation. Header (name, ID, presence), transcript, composer.
- Right, hidden until needed: files or the shared screen. A close control collapses it.

Closing People returns to the dock. The dock does not host the transcript.

**Visual rules.** Dark surface, one accent color, Segoe UI, 4 px spacing scale. Every screen uses the same window chrome. Text is laid out in device-independent pixels so 100% and 150% DPI keep the same alignment.

**Legacy windows are not opened.** File browsing and the remote picture are panels inside People. The old `frmChat`, `frmRemote`, `RemoteFiles`, and `LAN` forms stay only in the legacy exe.

## 2. People instead of addresses

A person is a record: desk ID (10 digits), display name, last address (LAN IP or blank), favorite flag, trusted flag, presence (`offline`, `online`, `inSession`).

**Your ID.** Read or create `%AppData%\Kdock\desk.id`. The dock and the People header show it grouped as `463 150 6990` with Copy.

**New chat.** A box accepts a desk ID or a LAN address (`host` or `host:12604`). An ID dials the meeting server. A dotted address connects straight to port 12604. The row is stored under Recent.

**Favorites.** A star on the row. Stored in `%AppData%\Kdock\favorites.json`. Favorites stay listed while offline.

**On this network.** Each running new app broadcasts a UDP beacon on port 12606 every 2 seconds: desk ID, machine name, protocol version. Listeners on the same subnet add those rows. A beacon older than 6 seconds drops to offline. This list is not saved as favorites unless the user stars it.

**Presence.** For an ID, the app also calls `GET /api/online/{id}` on the meeting server every 15 seconds while People is open. LAN beacons win when both exist. `inSession` is set locally while a call is up.

**Direct LAN and meeting server stay different.** The meeting server is never required for two PCs on the same subnet. An ID always uses the server.

## 3. Ask inside the chat

The composer has Attach, Emoji, and Send. The header has Files, Watch their screen, and Share my screen.

**Messages in the transcript.**

- Text, with emoji from the composer picker. Emoji are Unicode characters in the message, not images.
- Attachment. Sending uses the existing `chatfile:{id}` offer plus the file bytes. The bubble shows name, progress, and Open when the file is complete. Bytes start only after the user attaches, not when a file is copied.
- Request card. Files, watch, or share. The other side sees Allow and Deny. The card stays in the transcript with the result.

**Nothing starts before Allow.**

| Action | Card on the other PC | After Allow |
|---|---|---|
| Files | “Browse your files” | Right panel lists their drives. Existing `files:` instructions. |
| Watch their screen | “Watch your screen” | They capture and send. You see the picture in the right panel. |
| Share my screen | “Look at my screen” | You capture. They see the picture. |

Deny posts “Declined” on both transcripts and sends no frames and no file listing.

**Screen panel.** The picture keeps the remote aspect ratio, letterboxed. The pointer maps from the fitted image to the remote resolution. Movement is sent only when the mapped point changes. Their local pointer wins when they move the mouse. Keyboard, right-click, double-click, and wheel work. A hover bar at the top center holds quality (low, balanced, sharp) and clipboard on or off. Clipboard files still transfer only on paste, in either direction. Share stops when the panel closes.

**Files panel.** Details list: Name, Date modified, Type, Size. Sort by column, folders first. Double-click a folder to browse. Copy to this PC asks for a local folder. Progress is a card on the destination machine. Cancel stops the transfer and deletes a partial file.

**One session per PC.** Chat, files, and screen share that one connection. Each file has its own progress. A second screen request while one is open is declined with “Already sharing.”

## 4. Meeting server that stays up

The server is `Kdock.Server`, not the dock. Go Online on the WPF app only dials out.

**Address.** `%AppData%\Kdock\server.txt` holds `host:12605`. The People window has a Server field. Leaving the field saves it and reconnects. Both PCs use the same value.

**Process.** A Windows service named `Kdock.Meeting` runs `Kdock.Server.exe` and restarts it if it stops. The console `Kdock.Server.exe 12605` remains for a machine without a service install. The process listens on `http://0.0.0.0:12605` until it is stopped.

**Existing API, unchanged.**

- `GET /api/health`
- `GET /api/online/{id}`
- `POST /api/hello`
- `POST /api/call`
- WebSocket `/api/events?id=`
- WebSocket `/api/join/{token}`

**Added for points 5 and 6.**

- `GET /api/version` returns `{ "protocol": 2, "product": "1.0.0", "packageUrl": "" }`
- `POST /api/pair` with `{ "fromId", "targetId", "code" }` checks the one-time code the callee registered for this call. Wrong code returns 403. The existing `/api/call` still creates the token only after this check passes for an untrusted pair.

Port 12605 is opened on the server machine only. Desktops do not open a port.

## 5. Trust

The first time A calls B, B’s People window shows a 6-digit code and a card: “A wants to connect.” A must type that code. The server checks it with `POST /api/pair`. A wrong code does not open the WebSocket pair.

After B allows, both PCs store the other’s desk ID in `%AppData%\Kdock\trusted.json`. The next call between those IDs skips the code. Either side can remove trust from the person row. The next call asks again.

Screen and file requests are still Allow / Deny cards even after trust. Trust means “this ID may ring me.” It does not mean “this ID may see my screen.”

The code expires after 60 seconds or when the card is dismissed.

## 6. One current exe

`Kdock.App` publishes as one self-contained exe, the same way the legacy app does now. The file to copy is `Kdock.App.exe` alone.

**Version handshake.** Every session starts with `hello:{protocol}:{product}` before chat or screen data. Today’s wire is protocol 1. The WPF app speaks protocol 2. If the other side’s protocol differs, the transcript shows “Update Kdock on both PCs” and the session does not send frames or files.

On launch, and once a day, the app calls `GET /api/version`. If `protocol` is newer than the app, the dock shows “Update available” and the link from `packageUrl` when that field is set. The app does not download or replace itself in the first version of this work. It only warns, so both PCs can be updated to the same exe on purpose.

## Wire rules the new app must keep

These already work. The WPF client sends the same instructions. It does not invent a second file or screen format.

- Text: `MSG:{machine}`
- Attachment offer: `chatfile:{id}` then file chunks with the same id
- Browse: `files:getdrives`, `files:getfile`, delete and move as they are now
- Screen: `remote:request`, `remote:ready`, `remote:frame`, `remote:delta`, `remote:input`, `remote:stop`
- Clipboard: `remote:clipboard`, `remote:clipget`, `remote:clipon`, `remote:clipoff`
- Cancel: `transfer:cancel`

**New instructions, protocol 2 only.**

- `hello:{protocol}:{product}`
- `permit:{id}:{files|watch|share}` from the requester
- `permit-answer:{id}:allow` or `permit-answer:{id}:deny`

Legacy protocol 1 ignores these and will fail the hello check, so a legacy exe and a WPF exe do not enter a half-working session. That is intended. Both sides of a call run the new exe.

## Build order

1. `Kdock.Wire` and a WPF dock that shows the ID and opens an empty People window.
2. People list: recent, favorites, UDP discovery, server field, online checks.
3. Text, emoji, and attachments on one session.
4. Permit cards, then the files panel and the screen panel.
5. Pairing code and `trusted.json`.
6. `/api/version`, the hello check, the single-file publish, and the Windows service for `Kdock.Server`.

Each step is usable before the next. Legacy `Kdock.exe` keeps building the whole time.

## Done when

- The WPF dock lines up at 100% and 150% DPI, and the side panel meets the dock edge.
- Two new exes on one LAN see each other with no server, and two exes on different networks connect through `Kdock.Server` with the pairing code the first time.
- Files and a screen start only after Allow, and a mismatched protocol shows the update message instead of a broken picture.
- The legacy exe still opens the old dock and is not required to run the new one.
