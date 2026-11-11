> Managed by AIDFlow. Request changes through the agent rather than editing
> this document directly; review and approve proposed changes through AIDFlow.

# Feature F-0001: Meal and grocery planning

## Proposal source

The user's Concept responses and clarifications during this interactive session.

## Problem and user outcome

The user's household currently has to decide which meals to have for a week and
build a grocery list for them. The feature should let the household define meals
and their ingredients, select meals and the number of people for each, and
produce a grocery list with calculated ingredient amounts.

## In scope

- Maintain a shared ingredient list and use those ingredients when defining meals.
- Define each meal's ingredient amounts for one person.
- Select meals and a whole-number count of people for each selected meal.
- Select meals for a grocery list without assigning them to weekdays.
- Calculate ingredient amounts for the selected people counts.
- Store ingredient quantities in exact mass, volume or piece units, using g, kg,
  ml, l or pieces as appropriate.
- Let a household member add recipe-measure names such as teaspoon, tablespoon,
  cup or pinch to a shared list. For each ingredient, a household member must
  define the exact equivalent of a measure in a supported mass, volume or piece
  unit before using that measure in a recipe. Recipes may display the familiar
  measure; grocery calculations use its defined equivalent.
- Use the fixed units g, kg, ml, l and pieces for shared ingredients.
- Convert recipe measures to their predefined exact equivalents and combine
  repeated ingredient quantities in compatible underlying units. Do not infer
  conversions between mass, volume and pieces.
- Define multiple available package sizes for an ingredient, each with a
  quantity and underlying mass, volume or piece unit.
- For a grocery list, let the household choose which package sizes to consider.
  Calculate a combination that covers the required quantity using the fewest
  packages. Show the actual quantity needed separately from the package
  combination; show all combinations if multiple ones use the same minimum
  number of packages. Do not round the required amount up to full packages.
- Run on macOS/OS X, Windows, Linux, Android and iOS/iPhone.
- Share meal and ingredient data across household devices and preserve data
  across container restarts and server reboots.
- Save generated grocery lists for later, reopen them, and edit their items or
  quantities. Edits update the existing saved list and do not change meal
  definitions. Identify saved lists by a user-entered name and creation date/time.

## Out of scope

- Authentication is not needed for the first version.
- Assigning meals to weekdays is not required for the first version.
- No other exclusions have been specified.

## Acceptance criteria

- A household member can add an ingredient to the shared ingredient list and use
  it when defining meals.
- A meal can be defined with ingredient amounts based on one person.
- A household member can add a recipe-measure name to a shared list.
- A household member can define the exact equivalent of a shared recipe measure
  for an ingredient in a supported underlying unit.
- A recipe measure cannot be used with an ingredient until that ingredient's
  exact equivalent has been defined.
- Recipes may display the defined measure, and grocery calculations use its
  exact equivalent to combine quantities in compatible underlying units.
- A household member can select meals and specify a whole-number count of people
  for each selected meal.
- Meal selection does not require assigning meals to weekdays.
- The grocery list scales each selected meal's ingredient amounts for its
  selected whole-number people count and combines repeated ingredients using
  their defined exact equivalents.
- Shared ingredients use the fixed units g, kg, ml, l or pieces.
- Shared recipe-measure names such as teaspoon, tablespoon, cup or pinch require
  an exact equivalent to be defined for each ingredient before use.
- A household member can define multiple package sizes for an ingredient using
  a quantity and compatible unit.
- For a grocery list, the household can choose package sizes to consider. The
  system calculates the fewest packages that cover the required amount and
  shows the actual required amount separately from the package combination.
- If multiple package combinations use the same minimum number of packages, the
  system shows all tied combinations.
- A household member can save a generated grocery list and open it later.
- A household member can edit the items and quantities in a saved list; those
  edits update that saved list without changing the underlying meal definitions.
- A saved grocery list can be found by its user-entered name and creation
  date/time.
- The required amount is not rounded up to a full package.
- Meal and ingredient data are shared across household devices and survive
  container restarts and server reboots.
- The feature runs on macOS/OS X, Windows, Linux, Android and iOS/iPhone.

## Dependencies and constraints

- Meal definitions use ingredients from the shared ingredient list.
- Ingredient quantities use exact underlying mass, volume or piece units.
- Recipe measures are defined per ingredient with an exact equivalent before
  use; no conversion is inferred between mass, volume and pieces.
- The supported units are g, kg, ml, l and pieces.
- Shared recipe-measure names such as teaspoon, tablespoon, cup and pinch have
  ingredient-specific exact equivalents.
- Package sizes are configured per ingredient as a quantity and compatible unit.
- The household selects package sizes to consider; the system minimizes package
  count, then shows all combinations tied at the minimum.
- The required amount is shown separately and is not rounded up to whole packages.
- Saved grocery lists can be updated without modifying meal definitions.
- Saved lists are identified by user-entered name and creation date/time.
- People counts for selected meals are whole numbers.
- The intended users for the first version are the user's household.
- Authentication is not required for the first version.
- Meal and ingredient data persist across container restarts and server reboots.

## Open questions

- Supported operating-system and device versions have not been specified.

## Decisions and evidence

- Meals are based on one person, and the household selects a number of people
  for each selected meal.
- Repeated ingredient amounts use their ingredient-specific predefined exact
  equivalents for grocery totals.
- The underlying units are g, kg, ml, l and pieces. No conversion between mass,
  volume and pieces is inferred.
- Ingredients are maintained in a shared list and used in meal definitions.
- The requested platforms are macOS, OS X, Windows, Linux, Android and iOS/iPhone.
- Meal selection for a grocery list does not assign meals to weekdays.
- Generated grocery lists are saved for later and can be edited in place without
  changing meal definitions.
- Saved lists use a user-entered name and creation date/time.
- Package sizes are stored per ingredient. The household chooses which sizes
  to consider; the grocery list shows the required quantity and package counts
  separately, minimizing the number of packages and displaying all tied
  combinations.
- Meal and ingredient data are shared across household devices and survive
  container restarts and server reboots.
