# AIDFlow project index

## Workflow source
- entry: `.aidflow/method/aidflow.md`

| Source path | Role | Ref (commit/tag) | SHA-256 |
| --- | --- | --- | --- |
| `aidflow.md` | Workflow entry point | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `6b086cdee7b85be9a654b9f1bdbf717a53c2212b5d42807b06021797b004cffc` |
| `flows/concept.mmd` | Concept phase | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `92abfaecfc4571bcf2b9bd23005ae92c01f31a9f484734c62b4fc54407236735` |
| `flows/definition.mmd` | Feature definition phase | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `5f29f6d05f0bf939fca6f9d2eae9fb0541ebcce91b3e9948fc17b1dc88666d2e` |
| `flows/design.mmd` | Release design phase | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `a67e11880a1745985f640b11e816238b4feefbc2aebbb4105a2eaea2cb8f78cc` |
| `flows/feature-intake.mmd` | Feature intake phase | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `fb0937defe05305c3479569064b836ed63c3cc6c3c2e2ad8488fb2e55af83217` |
| `flows/implementation.mmd` | Feature implementation phase | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `e4d3e1675e213027452523a4d6e661b5281d0eff114c08d222d1255d60201072` |
| `flows/project.mmd` | Project orchestrator | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `9c3c32bb7a7e22564b63babc507050dcf3e1e8316b33981253aebd063a4945d4` |
| `flows/release-planning.mmd` | Release planning phase | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `9d053775e6d6554a7e22c4feaf61c1f9f144ef557688c9c7f5373c7765964215` |
| `flows/verification.mmd` | Feature and release verification phase | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `73a384da340d4fc1807c90b25050e631e4e9830d30fff2fd360ffee4c1a0df3d` |
| `reference/approvals.md` | Approval evidence rules | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `b1d622606c23239ff1a863ecc4e4093f4047cceb167df637921e8e2b5d7c9a5c` |
| `reference/concept-example.md` | Initial Concept questions | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `3d02c078f824ebb0630060b02542f78ba755956539586b7276eac67711f13c52` |
| `reference/interaction-record.md` | Resumable interaction record | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `72998835232027bd8271c2bb7e14f18faf794de8ab0d84ef4df5d8501be865a3` |
| `reference/interaction.md` | Interaction protocol | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `526a89a52b0c8b68e50379cd76cf89e86b2600878f285ed5cbfd3ccd4c53d8a8` |
| `reference/release-lifecycle.md` | Release lifecycle rules | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `32e7ef1707eacaf0d3e92ffb0ae8c182ce07461a83dc264324333969917ae541` |
| `reference/resume.md` | Resume validation | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `9aa20a0c602e11946c981989600a992b1bace52f2b5be901f6dccd1a21dbb832` |
| `reference/statuses.md` | Record and lifecycle statuses | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `b3140f58d55a937032259e12392a86b82684b55c027be7e7e4e6fe55368f1c68` |
| `reference/workspace.md` | Workspace and index rules | `c4d56ea90014ea5893df3c20252f1430c077bb07` | `ec497c0eb7cf7d02e2c17feb7ee9fa0608ed75fb7c56a8781a58dff137de19c3` |

## Tracker integration
- provider: none
- project_or_board: none

## Current execution
- status: waiting-human
- phase: concept
- node_id: concept
- call_stack: project.conceptNeeded -> concept.start -> concept
- call_arguments: none
- pending_targets: none
- feature_id: none
- release_id: none
- next_action: Await the project owner's answers to the pending Concept Question; then draft `.aidflow/concept.md` and request review and explicit approval.

## Records
| ID | Type | Path | record_status | Current SHA-256 | Approved SHA-256 | Depends on | Tracker reference |
| --- | --- | --- | --- | --- | --- | --- | --- |
| R-0001 | Operational | `.aidflow/README.md` | none | `c12cdcc48b8c4a1d4bd061372035d05f0830de18c4015c049bedae065634fa9d` | none | none | none |

## Features
| Feature ID | Definition record ID | feature_status | Active release ID or none | Prior status if blocked | Event reference |
| --- | --- | --- | --- | --- | --- |

## Releases
| Release ID | Scope record ID | Design record ID or none | release_status | Prior status if blocked | Event reference |
| --- | --- | --- | --- | --- | --- |

## Approval and change events
| Event ID | Kind: decision/change | Record/entity | Old/new SHA-256 | Decision or change classification | Actor | Timestamp | Evidence and rationale | Affected records/checks/events | Next state |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

## Verification evidence
| Feature/release | Check | Result | Evidence path/reference | Digest | Current/stale and reason |
| --- | --- | --- | --- | --- | --- |

## Pending interaction
| ID | Type | State | Target record/digest | Request and allowed outcomes | Response/evidence | Prior node/call stack | Next action or blocker |
| --- | --- | --- | --- | --- | --- | --- | --- |
| I-0001 | Question | awaiting-input | none | What should this project do, or what problem should it solve? Who has the problem and how do they handle it today? What outcome would make the project useful, and are there known boundaries or non-goals? Answer what is known; unresolved points remain open. | Awaiting response: [PR comment](https://github.com/jvanlangen/TestAIDFlow/pull/1#issuecomment-6762212712). | concept / project.conceptNeeded -> concept.start -> concept | Use the answers to clarify the Concept and draft `.aidflow/concept.md`; do not proceed to feature intake before Concept approval. |
