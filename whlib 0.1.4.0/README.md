# pollca

Polls a ccTalk coin acceptor over a serial port and POSTs each accepted coin's
value to a callback URL. Terminates when the accumulated amount reaches a
configurable closing amount.

## Layout

- `whlib/`   — ccTalk protocol library (C++)
- `pollca/`  — console executable project (NetBeans/g++)

## Dependencies

Install on the RPi before building:

```
sudo apt install g++ libcurl4-openssl-dev
```

## Build

Build directly on the RPi. Copy the entire `whlib 0.1.4.0/` folder to the RPi, then:

```
cd pollca
make CONF=Debug
```

The binary is output to:

```
dist/Debug/GNU-Linux-x86/pollca
```

Note: the Release configuration is missing `-lcurl` in its Makefile, so use `CONF=Debug` (the default if `CONF` is omitted).

Copy to deployment location:

```
sudo cp dist/Debug/GNU-Linux-x86/pollca /home/comforttan/pollca
sudo chmod +x /home/comforttan/pollca
```

## Run

```
pollca <port> [callbackUrl] [extendedLogging] [closingAmount_x100] [transactionID] [reference]
```

| arg                  | required | default                            | description                                                  |
|----------------------|----------|------------------------------------|--------------------------------------------------------------|
| `port`               | yes      | —                                  | Serial port (e.g. `/dev/ttyUSB0`) or tty number             |
| `callbackUrl`        | no       | `localhost:5000/coinacceptor/local` | URL receiving POST `{method, data:{amountreceived, DateTime}}` |
| `extendedLogging`    | no       | `n`                                | `Y` to enable verbose curl output                            |
| `closingAmount_x100` | no       | `9999999`                          | Total (in coin minor units × 100) at which the program exits |
| `transactionID`      | no       | `0`                                | Integer transaction ID included in each POST                 |
| `reference`          | no       | `""`                               | Reference string included in each POST                       |

A POST of `amountreceived: 0` is sent once at startup to signal "ready".

Auth: HTTP Basic with user `root` and empty password.
