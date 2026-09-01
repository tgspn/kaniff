# Security Policy

## Supported Versions

Only the latest released version of Kaniff receives security fixes.

| Version | Supported |
| --- | --- |
| latest | ✅ |
| older | ❌ |

## Reporting a Vulnerability

**Please do not open a public issue for security problems.**

Report vulnerabilities privately through GitHub Security Advisories:

1. Go to <https://github.com/tgspn/kaniff/security/advisories/new>
2. Describe the issue, the affected component (CLI, desktop, or core), and a
   reproduction case.

You can expect an initial response within 7 days. If the report is confirmed, a
fix and an advisory will be published as soon as reasonably possible.

## Scope and design notes

Kaniff is a local developer utility. Some notes that may affect what counts as a
vulnerability:

- **JWT decoding does not verify signatures.** The JWT tool inspects the header
  and payload only. It is a decoder, not a validator, and must not be used to
  make trust decisions.
- **MD5 and SHA-1 are provided for interoperability** with legacy systems. They
  are not considered secure for cryptographic purposes.
- **All tools run offline**, except the three network tools:
  - The public IP lookup contacts `ifconfig.me`, `ipify.org`, `icanhazip.com`
    and `ifconfig.co` (including their IPv4- and IPv6-only hostnames, queried
    separately to report both families). Those requests send no user input
    beyond what any HTTP request reveals: your IP address and a `User-Agent`.
  - The DNS lookup resolves the name you type, using your system's configured
    resolver. No third-party DNS server is contacted directly.
  - The port check opens a TCP connection to the host and port you type, then
    closes it immediately. It sends no payload and reads nothing back; only the
    connection outcome is reported.

  Both the DNS lookup and the port check contact only the host you ask for, and
  neither sends any other data. No other tool transmits data anywhere.

Issues we are interested in include remote code execution, arbitrary file
write/read via crafted input, and unintended network transmission of user data.
