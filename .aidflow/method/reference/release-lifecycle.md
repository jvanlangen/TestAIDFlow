# Release lifecycle and change impact

Read this when changing a release status, assessing the impact of a changed Concept, Definition, scope, Design or implementation, or choosing a repair route. Project's impact node applies these rules.

## Release revision and repair transitions

Normal release lifecycle: planning -> scope-pending-approval -> scope-approved
-> design-pending-approval -> design-approved -> implementing -> verifying
-> release-ready -> pending-publication-authorization -> published.
All except published are active. Deferred, blocked and cancelled are exceptions.
Initial status is `planning`. Apply the earliest affected prerequisite first:

- From; To; Required condition and evidence
- Any active status after `planning`; `planning`; Scope approval invalidated: revise scope draft, then request approval through the normal route. Invalidate dependent Design and release checks.
- `design-pending-approval` / `design-approved` / `implementing` / `verifying` / `release-ready` / `pending-publication-authorization`; `scope-approved`; Scope remains approved but Design approval is invalidated; revise Design draft, then request its approval.
- `verifying` / `release-ready` / `pending-publication-authorization`; `implementing`; Approvals remain valid; affected feature implementation or feature checks need work.
- `release-ready` / `pending-publication-authorization`; `verifying`; All feature evidence remains valid; repeat release-level checks only.
- `scope-pending-approval`; `planning`; Human requests scope revision; scope record returns to `draft`.
- `design-pending-approval`; `scope-approved`; Human requests Design revision; Design record returns to `draft`.

A Definition revision pauses affected work at `clarifying` and triggers impact
analysis. If scope approval is affected, use `planning`; if only Design approval
is affected, use `scope-approved`; otherwise use `implementing` for affected
feature work. An already earlier release status stays there. Re-entering
implementation never bypasses approval checks for the revised Definition.

Every revision after release checks invalidates affected checks and publication
authorization. Re-request authorization after required checks pass. Authorization
also binds the exact code/build being published, not just scope and Design files.
A user-requested repeat check suspends publication until it passes; if inputs are
unchanged, previously recorded authorization may remain valid with evidence.

For `blocked`, save the prior status and apply revision rules to that status before
resuming. For `deferred`, retain the hold; changes may invalidate evidence but do
not resume work. Explicit reopening enters `planning` and revalidates scope.
`published` and `cancelled` are terminal: use a new release version for later work.
Changing a version retires the old unpublished release through explicit cancellation
and creates a new planning entry; never silently rename a release ID.

- Change; Approval/evidence impact; Resume point
- Project purpose, users, outcome or boundary; Concept approval becomes stale; assess dependencies and mark only affected feature definitions, scopes and designs for reapproval.; Concept approval, then affected records; preserve unaffected approvals with evidence.
- Feature behavior, scope, non-goals, dependencies or acceptance criteria; Definition approval becomes stale. Use dependency impact analysis to mark only affected scope/design approvals and feature verification stale; preserve unaffected approvals with evidence.; Definition; then reapprove only affected scope/design before continuing.
- Release feature set, version, exclusions or goals; Scope approval and integrated design approval become stale; release-level verification becomes stale.; Release planning.
- Integrated design architecture, interface, data flow or constraints; Design approval becomes stale; implementation and verification evidence for impacted features becomes stale.; Design approval, then reimplement/reverify impacted features.
- Implementation code or configuration; Design/definition approval remains only if inputs are unchanged; checks covering changed behavior become stale.; Implementation checks and affected feature verification; rerun release integration checks for shared behavior.
