# Approval evidence

Every human approval must bind to the exact content version before a status transitions
to approved. Compute SHA-256 over the exact bytes of each approved file; this works
before commit and does not require adding or staging the file. Use an available,
permitted local SHA-256 tool. For a scope/design approval that covers multiple files,
list the path and digest for every file.

Append an approval event in `.aidflow/index.md` with at least:

| Field | Required value |
| --- | --- |
| `event_id` | Stable, unique decision ID |
| `record` | Record ID and repository-relative path |
| `sha256` | Digest of the exact approved file bytes |
| `decision` | `approved`, `deferred`, `rejected`, `withdrawn`, `cancelled`, `reopened` or `publication-authorized` |
| `actor` | Human identity if known; otherwise literal `unknown` |
| `timestamp` | Reliable timestamp if available; otherwise literal `unknown` |
| `evidence` | Durable review/chat/tracker reference, or a quoted approval with its source context |

Never fabricate an identity, timestamp, link, quote, digest or approval. If the hash
cannot be computed or the approval cannot be tied to the exact version, leave the
record `pending-approval` and ask the user to approve through a version-bound review
or provide an authorized hashing method. Do not proceed on a generic “looks good” if
multiple records or versions are pending.

Approval events are append-only. Record invalidation in a new change event that
references the old event, reason and replacement event if available. Derive current
validity from these events; never edit or delete the original approval event. The current
record status must describe the current digest, not a historical approved version.
