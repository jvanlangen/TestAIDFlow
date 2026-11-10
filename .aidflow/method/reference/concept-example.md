# Concept phase example

Read this in the Concept phase when a new project's first questions are needed. It is a walkthrough of the [Interaction Protocol](interaction.md), not an executable workflow.

For a new repository where the user initially says, “I want to create a new software
product”:

1. Read the Interaction Protocol, Project and the Concept subflow. Initialize `.aidflow/` only if
    missing; inspect existing records first. The initial sentence establishes intent
    but does not yet define or approve a Concept.
2. Check existing `.aidflow/` records and prior interactions. Do not ask for facts already
    supplied. Start a Question interaction for the missing project-level information.
3. In an interactive environment, ask a small number of focused questions, for
    example: who has the problem, what are they trying to achieve, how is it handled
    today, and what outcome would make the product useful? Ask further questions only
    when the answers reveal a material gap. In an asynchronous environment, send the
    related questions together and pause in `awaiting-input`.
4. Draft `.aidflow/concept.md` from the answers. Distinguish confirmed facts from
    assumptions and open questions. Do not add technical design or turn every idea
    into an approved feature.
5. Start `awaiting-review`: summarize the problem, intended users, outcome, boundaries,
    success measures, assumptions and unresolved points; provide the artifact path/diff.
    Incorporate feedback and resubmit if changed.
6. When review is complete, request explicit `awaiting-approval` for the exact Concept
    digest. Record approval evidence. A review response alone is not approval.
7. Only after Concept approval does Project enter Feature intake/Definition. If the
    user provides sufficient Concept information in one message, use it and avoid
    unnecessary questions; the review and approval gates still apply.

This is a documentation walkthrough, not an automatically executed workflow.
