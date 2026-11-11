> Managed by AIDFlow. Request changes through the agent rather than editing
> this document directly; review and approve proposed changes through AIDFlow.

# Release design: v1.0

## Scope reference

- Approved scope: `docs/aidflow/releases/v1.0/scope.md`
- Scope SHA-256: `ab087a00a7c3e0626a22957bea1af5e41ad0558e9c28ab344a272e83993cbf09`
- Approved feature: `F-0001`, `docs/aidflow/features/F-0001/definition.md`
- Definition SHA-256: `900f82d15dd5b05d9177f20ef32cc28d0f7dfe5441e1235f1581addf455909e0`

## Integrated approach

Deliver a browser-accessible, responsive web application for household devices.
The user-selected React/Vite TypeScript frontend is built and served by the
user-selected ASP.NET Web API in C#. The API uses PostgreSQL in a separate
Docker Compose service on the household's local server.

The browser client communicates with the API on the same origin. The application
is available only on the household network, has no first-version authentication,
and does not require remote services. The PostgreSQL service is reachable by the
API inside the Compose network, not directly by household browsers.

Keep application data in PostgreSQL storage that persists across container
restarts and local server reboots. The application container can be recreated
without losing meals, ingredients, measure definitions or saved lists.

## Components and interfaces

| Component | Responsibility | Interface |
| --- | --- | --- |
| Browser application | Manage ingredients, recipe-measure names and equivalents, meals, meal selections, package options and saved grocery lists. Present responsive screens on the requested desktop and mobile platforms. | Same-origin requests to the ASP.NET API. |
| ASP.NET Web API and web host | Serve the built frontend; validate requests; calculate recipe totals and package combinations; persist and retrieve application records. | Browser-facing operations for ingredients, recipe measures, meals and grocery lists; PostgreSQL connection available only on the Compose network. |
| PostgreSQL | Persist shared ingredient, measure, meal, package and saved-list data. | Private Compose-network connection from the API service. |
| Docker Compose deployment | Start the application and database services on the household's local server and retain database storage across restarts. | Local Compose configuration and persistent PostgreSQL storage. |

The browser flow is: select meals and whole-person counts, request a grocery
calculation, review required quantities and package combinations, then save the
list with a user-entered name. A saved list can later be reopened and edited.

## Data and end-to-end behavior

- **Ingredient:** shared ingredient identity and its exact underlying quantity
  category/unit (mass, volume or pieces), plus its available package sizes.
- **Recipe-measure name:** a user-added shared label such as cup, teaspoon,
  tablespoon or pinch. It has no assumed universal conversion.
- **Ingredient measure equivalent:** for each ingredient and recipe-measure
  name, the exact quantity and underlying unit. A recipe cannot use that measure
  for that ingredient until the equivalent is defined. Changing the equivalent
  updates calculations for recipes that use it.
- **Meal:** a name and ingredient quantities defined for one person. A quantity
  may use an exact underlying unit or a recipe-measure name with its
  ingredient-specific equivalent.
- **Package size:** an available package quantity in an underlying unit
  compatible with the ingredient's quantity category. A household may select
  which package sizes to consider for a grocery calculation.
- **Grocery list:** a saved snapshot of the selected meals, people counts,
  required ingredient quantities and calculated package combinations, with a
  user-entered name and creation date/time. Editing a saved list changes that
  list only, not meal definitions. Subsequent meal or measure-equivalent changes
  do not recalculate an existing saved list.

For a new grocery calculation, scale each one-person meal quantity by its
whole-person count. Resolve recipe-measure quantities through the current
ingredient-specific equivalents, convert only compatible underlying units, and
sum each ingredient's required amount. Do not infer conversions between mass,
volume and pieces.

Show the actual required amount separately from purchase quantities. For the
package sizes selected by the household, find combinations that cover the
required amount using the fewest packages. Show every combination tied at that
minimum. Do not round the required amount up to the package amount. If no package
size is configured or selected for an ingredient, show its required amount
without claiming a package recommendation.

## Feature-to-design traceability

| Definition behavior | Design mapping |
| --- | --- |
| Shared ingredient list and one-person meals | Ingredient and Meal records managed through the browser application and API. |
| Whole-person meal selection without weekday assignment | Meal-selection flow accepts a whole-person count per selected meal; it has no weekday schedule. |
| Custom recipe-measure names and exact per-ingredient equivalents | Shared Recipe-measure names and Ingredient measure equivalents; block recipe use until the equivalent exists. |
| Grocery calculation with repeated ingredients | API scales recipe amounts and sums using exact equivalents and compatible underlying units. |
| Multiple package sizes, minimum package count and tied alternatives | Package-size records and calculation over the sizes selected for the list; display all minimum-count combinations and the required amount separately. |
| Saved grocery lists with independent edits | Persist a list snapshot; list edits do not mutate Meal or Ingredient records. |
| Shared data and survival across restarts | PostgreSQL on the local server with persistent storage managed by Docker Compose. |
| Requested desktop and mobile platforms | Responsive browser UI; no native application is required by this design. |
| No authentication and household-network-only access | Do not add authentication in v1.0; limit application availability to the household network and keep PostgreSQL private to the Compose network. |

## Risks and tradeoffs

- With no authentication, anyone who can reach the application on the household
  network can view or change household data. The application must not be exposed
  beyond that network. Local-network transport protection has not been specified.
- Ingredient-specific recipe-measure equivalents must be entered accurately;
  they can differ by ingredient. Missing equivalents prevent use of that measure.
- Minimizing package count can leave surplus, and tied combinations may remain;
  no price-based or waste-based optimization was requested.
- Persistent container storage is not a backup. Backup and recovery expectations
  have not been specified.
- Supported operating-system and browser versions have not been specified.

## Rollout and test strategy

Deploy the application and PostgreSQL services to the household's local server
using Docker Compose. Keep PostgreSQL data in persistent storage and verify that
restarting the services and rebooting the server do not remove saved records.
Expose the browser application only within the household network; do not expose
the database service to household browsers.

Before release, verify:

- Responsive browser access on the requested macOS/OS X, Windows, Linux,
  Android and iOS/iPhone platforms; exact OS/browser versions remain to be set.
- A recipe measure is unavailable for an ingredient until its exact equivalent
  is defined, and grocery calculations use that equivalent.
- The user's example: 1000 g plus 1.5 kg of potatoes produces a required total
  of 2.5 kg; a selected 3 kg package is shown separately from the 2.5 kg needed.
- Package combinations cover the required quantity, use the fewest packages,
  and show all tied combinations.
- Saving and editing a grocery list does not change its source meals; changing
  a measure equivalent updates recipes but leaves existing saved-list snapshots
  unchanged.
- Meal, ingredient, measure and saved-list data survive application/container
  restarts and a local server reboot.
- The application is reachable from the household network but the PostgreSQL
  service is not directly reachable from household browsers.
