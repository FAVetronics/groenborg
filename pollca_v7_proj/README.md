# pollca_v7

Polls a ccTalk coin acceptor over a serial port and POSTs each accepted coin's
value to a callback URL. Terminates when the accumulated amount reaches a
configurable closing amount.

## Layout

- `ccTalk/`      — ccTalk protocol library (netstandard2.1)
- `pollca_v7/`   — console exe project (net8.0)
- `ccTalk.sln`   — solution containing both projects
- `.vscode/`     — VS Code debug/build configuration

## Build

```
dotnet build ccTalk.sln -c Release
```

Self-contained Linux ARM publish (matches the original deployment target):

```
dotnet publish pollca_v7/pollca_v7.csproj -c Release -r linux-arm --self-contained true
```

## Run

```
pollca_v7 <port> [callbackUrl] [extendedLogging] [closingAmount_x100]
```

| arg                    | required | default                                 | description                                                            |
|------------------------|----------|-----------------------------------------|------------------------------------------------------------------------|
| `port`                 | yes      | —                                       | Serial port (e.g. `COM12` on Windows, `/dev/ttyUSB0` on Linux)         |
| `callbackUrl`          | no       | `http://localhost:5000/coinacceptor/local` | URL receiving POST `{method, data:{amountreceived, DateTime}}`       |
| `extendedLogging`      | no       | `n`                                     | `y` to log HTTP response bodies                                        |
| `closingAmount_x100`   | no       | `9999999`                               | Total (in coin minor units * 100) at which the program exits cleanly   |

A POST of `amountreceived: 0` is sent once at startup to signal "ready".

Auth: HTTP Basic with user `root` and empty password (matches the original C++
reference implementation).

## Notes

- stdout is configured to auto-flush, so the program is safe to launch from
  `subprocess.Popen(..., stdout=logfile)` without losing real-time output.
- The native serial-port library is bundled into the single-file exe via
  `IncludeNativeLibrariesForSelfExtract` in `pollca_v7.csproj`.
