# Resume validation

Before resuming, verify all of the following against the current files:

1. Workflow source/ref matches the recorded version. Check every registered method
   file: `aidflow.md`, Project, used subflows and used `reference/*.md`. Verify
   available recorded digests against the exact bytes, including for pinned sources.
   For an unpinned source, compare every registered digest with the current source
   before resuming. A missing digest or unavailable old source is an unresolved
   consistency check, not permission to assume compatibility. Load any newly
   needed file from the same recorded version, following
   [workflow source registration](workspace.md#workflow-source-registration).
2. The recorded subflow, node ID and call stack exist in that workflow version and
   form a valid path from Project.
3. Each active record path exists and its current digest matches the index. Handle
   a changed `docs/aidflow/` document as described below before continuing an
   affected transition. For other records, classify digest changes using Change
   handling in [`aidflow.md`](../aidflow.md) and update the index only when their
   origin and impact are clear; otherwise block and ask.
4. Approval evidence matches the current digest or has a documented editorial
   derivation. Verification evidence is current for the current feature/design.
5. The index, records and optional tracker do not have an unresolved conflict.

## Handling a directly changed project document

Apply this also when a mismatch is discovered during a run, before using or
modifying the document. An edit is a proposed change, not an approval. Preserve
the current file; do not silently restore, discard or overwrite it.

1. Compare the current bytes and digest with the indexed version. Use available
   Git history, review evidence or other reliable prior content to understand
   the difference; never assume Git contains uncommitted bytes. If the earlier
   content cannot be recovered, treat the change as material rather than claiming
   it is editorial. A missing file or digest is not a proposed edit: block and
   ask how to reconcile it.
2. Record the old and new digests, source evidence and classification in a new
   change event in `.aidflow/index.md`. Use `unknown` for an unavailable actor or
   timestamp; lack of attribution alone does not prevent handling a clear change.
   Update the record's current digest without replacing historical approval
   evidence or its approved digest.
3. For a material change to an approved record, immediately invalidate its
   approval and move it to `draft`; for a changed `pending-approval` record, return
   it to `draft`. Keep a changed draft in `draft`. An editorial change may retain
   approval only with the documented derivation required by Change handling.
   Apply Change handling and `reference/release-lifecycle.md`: assess dependent
   records, checks and publication authorization. Record affected IDs and the
   rationale, invalidate affected approvals and evidence, and preserve unaffected
   ones only when justified. If impact cannot be bounded or the meaning or
   intended resolution is unclear, block the affected work and ask a focused
   question; do not guess or silently discard the change.
4. Route through Project's document-change repair path and the owning phase,
   self-review the current content, then request review and explicit approval for
   the exact new digest when required. Never infer approval from the edit, commit,
   tests or merge.
5. Preserve the interrupted execution position and resume it only after the
   affected approvals and evidence are valid, applying repair routes as needed.
   Published releases and released features remain terminal; follow the lifecycle
   rules for subsequent changes rather than silently rewriting their history.

Do not try to validate a self-digest for `.aidflow/index.md`. Validate its structure,
record references and evidence instead. If an older index contains a self-record,
remove that row as an operational correction, preserve history and any referenced
events, and record the correction; never infer approval from its old digest.

If a source, record or index check cannot be completed, set execution status to `blocked`, list the exact
mismatch and ask how to reconcile it. Do not skip nodes, reconstruct missing
approvals, or infer current state from a tracker, branch name, issue order or artifact.

If the method copy in `.aidflow/method/` differs from its registered digests, set
execution to `blocked`, list the mismatch and ask. Never replace the copy; version
migration is not supported.
