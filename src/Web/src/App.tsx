import { useEffect, useState, type FormEvent } from 'react'
import './App.css'

type Unit = 'g' | 'kg' | 'ml' | 'l' | 'pieces'
type PackageSize = { id: string; quantity: string; unit: Unit; label: string }
type MeasureEquivalent = {
  recipeMeasureId: string
  measureName: string
  quantity: string
  unit: Unit
}
type Ingredient = {
  id: string
  name: string
  baseUnit: Unit
  packageSizes: PackageSize[]
  measureEquivalents: MeasureEquivalent[]
}
type RecipeMeasure = { id: string; name: string }
type MealIngredient = {
  id: string
  ingredientId: string
  ingredientName: string
  quantity: string
  unit: Unit | null
  recipeMeasureId: string | null
  measureName: string | null
}
type Meal = { id: string; name: string; ingredients: MealIngredient[] }
type MealSelection = { mealId: string; mealName: string; peopleCount: number }
type PackageCombination = { packages: { label: string; count: number }[] }
type GroceryItem = {
  id: string
  ingredientId: string
  ingredientName: string
  quantity: string
  unit: Unit
  packageOptions: PackageSize[]
  packageCombinations: PackageCombination[]
}
type GroceryList = {
  id: string
  name: string
  createdAt: string
  selections: MealSelection[]
  items: GroceryItem[]
}
type AppState = {
  ingredients: Ingredient[]
  recipeMeasures: RecipeMeasure[]
  meals: Meal[]
  groceryLists: GroceryList[]
}
type MealDraftLine = {
  key: string
  ingredientId: string
  quantity: string
  kind: 'unit' | 'measure'
  unit: Unit
  recipeMeasureId: string
}
type ListDraftItem = { id: string; quantity: string }

const units: Unit[] = ['g', 'kg', 'ml', 'l', 'pieces']

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: {
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...init?.headers,
    },
  })

  if (!response.ok) {
    const body = (await response.json()) as {
      error?: string
      detail?: string
      title?: string
      errors?: Record<string, string[]>
    }
    const validation = body.errors ? Object.values(body.errors).flat().join(' ') : ''
    throw new Error(body.error ?? body.detail ?? validation ?? body.title ?? 'The request failed.')
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

function compatibleUnits(unit: Unit): Unit[] {
  if (unit === 'g' || unit === 'kg') return ['g', 'kg']
  if (unit === 'ml' || unit === 'l') return ['ml', 'l']
  return ['pieces']
}

function emptyLine(ingredient?: Ingredient): MealDraftLine {
  return {
    key: crypto.randomUUID(),
    ingredientId: ingredient?.id ?? '',
    quantity: '',
    kind: 'unit',
    unit: ingredient?.baseUnit ?? 'g',
    recipeMeasureId: '',
  }
}

function App() {
  const [data, setData] = useState<AppState | null>(null)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [loading, setLoading] = useState(true)
  const [ingredientName, setIngredientName] = useState('')
  const [ingredientUnit, setIngredientUnit] = useState<Unit>('g')
  const [measureName, setMeasureName] = useState('')
  const [packageIngredientId, setPackageIngredientId] = useState('')
  const [packageQuantity, setPackageQuantity] = useState('')
  const [packageUnit, setPackageUnit] = useState<Unit>('g')
  const [equivalentIngredientId, setEquivalentIngredientId] = useState('')
  const [equivalentMeasureId, setEquivalentMeasureId] = useState('')
  const [equivalentQuantity, setEquivalentQuantity] = useState('')
  const [equivalentUnit, setEquivalentUnit] = useState<Unit>('g')
  const [mealName, setMealName] = useState('')
  const [mealId, setMealId] = useState('')
  const [mealLines, setMealLines] = useState<MealDraftLine[]>([emptyLine()])
  const [mealPeople, setMealPeople] = useState<Record<string, string>>({})
  const [selectedPackageIds, setSelectedPackageIds] = useState<string[]>([])
  const [groceryName, setGroceryName] = useState('')
  const [listDrafts, setListDrafts] = useState<Record<string, ListDraftItem[]>>({})
  const [addIngredientId, setAddIngredientId] = useState('')
  const [addListId, setAddListId] = useState('')
  const [addQuantity, setAddQuantity] = useState('')
  const [addPackageIds, setAddPackageIds] = useState<string[]>([])
  const [renameIngredientId, setRenameIngredientId] = useState('')
  const [renameIngredientText, setRenameIngredientText] = useState('')

  async function refresh() {
    const next = await request<AppState>('/api/app')
    setData(next)
    setListDrafts(
      Object.fromEntries(
        next.groceryLists.map((list) => [
          list.id,
          list.items.map((item) => ({ id: item.id, quantity: item.quantity })),
        ]),
      ),
    )
    setSelectedPackageIds((current) =>
      current.filter((id) => next.ingredients.some((item) => item.packageSizes.some((size) => size.id === id))),
    )
  }

  useEffect(() => {
    let active = true
    void request<AppState>('/api/app')
      .then((next) => {
        if (!active) return
        setData(next)
        setListDrafts(
          Object.fromEntries(
            next.groceryLists.map((list) => [
              list.id,
              list.items.map((item) => ({ id: item.id, quantity: item.quantity })),
            ]),
          ),
        )
      })
      .catch((cause: unknown) => {
        if (active) setError(cause instanceof Error ? cause.message : 'Could not load household data.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => { active = false }
  }, [])

  async function run(action: () => Promise<void>, successMessage: string) {
    setError('')
    setNotice('')
    try {
      await action()
      setNotice(successMessage)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'The request could not be completed.')
    }
  }

  function submit(event: FormEvent<HTMLFormElement>, action: () => Promise<void>, successMessage: string) {
    event.preventDefault()
    void run(action, successMessage)
  }

  function updateLine(key: string, update: Partial<MealDraftLine>) {
    setMealLines((lines) => lines.map((line) => (line.key === key ? { ...line, ...update } : line)))
  }

  function startMealEdit(meal: Meal) {
    setMealId(meal.id)
    setMealName(meal.name)
    setMealLines(
      meal.ingredients.map((line) => ({
        key: line.id,
        ingredientId: line.ingredientId,
        quantity: line.quantity,
        kind: line.recipeMeasureId ? 'measure' : 'unit',
        unit: line.unit ?? 'g',
        recipeMeasureId: line.recipeMeasureId ?? '',
      })),
    )
    document.getElementById('meal-editor')?.scrollIntoView({ behavior: 'smooth' })
  }

  const selectedMeals = data?.meals.filter((meal) => Number(mealPeople[meal.id] ?? 0) > 0) ?? []
  const selectedIngredients = [
    ...new Map(
      selectedMeals.flatMap((meal) =>
        meal.ingredients.map((line) => [line.ingredientId, data?.ingredients.find((item) => item.id === line.ingredientId)] as const),
      ).filter((entry): entry is readonly [string, Ingredient] => entry[1] !== undefined),
    ).values(),
  ]

  if (loading) return <main className="shell"><p className="muted">Loading meal planner…</p></main>

  return (
    <main className="shell">
      <header className="topbar">
        <a className="brand" href="#top" id="top" aria-label="Meal planner home">
          <span className="brand-mark" aria-hidden="true">M</span>
          <span>Meal planner</span>
        </a>
        <nav aria-label="Main navigation">
          <a href="#ingredients">Ingredients</a>
          <a href="#meals">Meals</a>
          <a href="#lists">Grocery lists</a>
        </nav>
      </header>

      <section className="hero">
        <div>
          <p className="eyebrow">HOUSEHOLD PLANNING</p>
          <h1>Make a little room<br />for good meals.</h1>
          <p className="hero-copy">Plan meals for your household and turn them into a thoughtful grocery list.</p>
        </div>
        <div className="hero-note">
          <span className="note-icon" aria-hidden="true">✳</span>
          <p>Shared with your household<br />on your home network.</p>
        </div>
      </section>

      {(error || notice) && (
        <div className={error ? 'message error' : 'message success'} role={error ? 'alert' : 'status'}>
          {error || notice}
          <button type="button" className="message-close" aria-label="Dismiss message" onClick={() => { setError(''); setNotice('') }}>×</button>
        </div>
      )}

      {!data && error && (
        <section className="panel retry-panel">
          <h2>Could not connect to the planner</h2>
          <p>Check that the application and its database are running, then try again.</p>
          <button type="button" className="button primary" onClick={() => void refresh().then(() => setError('')).catch((cause: unknown) => setError(cause instanceof Error ? cause.message : 'Could not load household data.'))}>Try again</button>
        </section>
      )}

      {data && (
        <>
          <section className="stat-row" aria-label="Household overview">
            <div className="stat"><strong>{data.ingredients.length.toString().padStart(2, '0')}</strong><span>ingredients</span></div>
            <div className="stat"><strong>{data.meals.length.toString().padStart(2, '0')}</strong><span>meals</span></div>
            <div className="stat"><strong>{data.groceryLists.length.toString().padStart(2, '0')}</strong><span>saved lists</span></div>
            <a className="button dark" href="#grocery-builder">Plan a grocery list <span aria-hidden="true">↗</span></a>
          </section>

          <section className="section" id="ingredients">
            <div className="section-heading">
              <div><p className="eyebrow">YOUR PANTRY</p><h2>Ingredients & measures</h2></div>
              <p>Keep shared ingredients, package sizes and recipe measures in one place.</p>
            </div>
            <div className="management-grid">
              <form className="panel form-panel" onSubmit={(event) => submit(event, async () => {
                await request('/api/ingredients', { method: 'POST', body: JSON.stringify({ name: ingredientName, baseUnit: ingredientUnit }) })
                setIngredientName('')
                await refresh()
              }, 'Ingredient added.')}>
                <h3>Add an ingredient</h3>
                <label>Name<input required maxLength={120} value={ingredientName} onChange={(event) => setIngredientName(event.target.value)} placeholder="e.g. Potatoes" /></label>
                <label>Underlying unit
                  <select value={ingredientUnit} onChange={(event) => setIngredientUnit(event.target.value as Unit)}>
                    {units.map((unit) => <option key={unit}>{unit}</option>)}
                  </select>
                </label>
                <button className="button primary" type="submit">Add ingredient <span aria-hidden="true">＋</span></button>
              </form>

              <form className="panel form-panel" onSubmit={(event) => submit(event, async () => {
                await request('/api/recipe-measures', { method: 'POST', body: JSON.stringify({ name: measureName }) })
                setMeasureName('')
                await refresh()
              }, 'Recipe measure added.')}>
                <h3>Add a recipe measure</h3>
                <p className="form-hint">Add a shared name such as cup, tablespoon or pinch. Set its exact equivalent for each ingredient below.</p>
                <label>Measure name<input required maxLength={80} value={measureName} onChange={(event) => setMeasureName(event.target.value)} placeholder="e.g. cup" /></label>
                <button className="button secondary" type="submit">Add measure <span aria-hidden="true">＋</span></button>
              </form>
            </div>

            <form className="panel inline-form" onSubmit={(event) => submit(event, async () => {
              await request(`/api/ingredients/${packageIngredientId}/packages`, { method: 'POST', body: JSON.stringify({ quantity: packageQuantity, unit: packageUnit }) })
              setPackageQuantity('')
              await refresh()
            }, 'Package size added.')}>
              <div><h3>Add a package size</h3><p className="form-hint">Package sizes are saved per ingredient.</p></div>
              <label>Ingredient<select required value={packageIngredientId} onChange={(event) => {
                const ingredient = data.ingredients.find((item) => item.id === event.target.value)
                setPackageIngredientId(event.target.value)
                setPackageUnit(ingredient?.baseUnit ?? 'g')
              }}><option value="">Choose…</option>{data.ingredients.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
              <label>Quantity<input required inputMode="decimal" value={packageQuantity} onChange={(event) => setPackageQuantity(event.target.value)} placeholder="e.g. 3" /></label>
              <label>Unit<select value={packageUnit} onChange={(event) => setPackageUnit(event.target.value as Unit)}>
                {compatibleUnits(data.ingredients.find((item) => item.id === packageIngredientId)?.baseUnit ?? packageUnit).map((unit) => <option key={unit}>{unit}</option>)}
              </select></label>
              <button className="button secondary" type="submit" disabled={!data.ingredients.length}>Save size</button>
            </form>

            <form className="panel inline-form equivalent-form" onSubmit={(event) => submit(event, async () => {
              await request(`/api/ingredients/${equivalentIngredientId}/recipe-measures/${equivalentMeasureId}`, {
                method: 'PUT',
                body: JSON.stringify({ quantity: equivalentQuantity, unit: equivalentUnit }),
              })
              setEquivalentQuantity('')
              await refresh()
            }, 'Ingredient-specific equivalent saved.')}>
              <div><h3>Set an exact equivalent</h3><p className="form-hint">Recipes cannot use a measure until its equivalent is defined for that ingredient.</p></div>
              <label>Ingredient<select required value={equivalentIngredientId} onChange={(event) => {
                const ingredient = data.ingredients.find((item) => item.id === event.target.value)
                setEquivalentIngredientId(event.target.value)
                setEquivalentUnit(ingredient?.baseUnit ?? 'g')
              }}><option value="">Choose…</option>{data.ingredients.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
              <label>Measure<select required value={equivalentMeasureId} onChange={(event) => setEquivalentMeasureId(event.target.value)}><option value="">Choose…</option>{data.recipeMeasures.map((measure) => <option key={measure.id} value={measure.id}>{measure.name}</option>)}</select></label>
              <label>Equivalent<input required inputMode="decimal" value={equivalentQuantity} onChange={(event) => setEquivalentQuantity(event.target.value)} placeholder="e.g. 150" /></label>
              <label>Unit<select value={equivalentUnit} onChange={(event) => setEquivalentUnit(event.target.value as Unit)}>
                {compatibleUnits(data.ingredients.find((item) => item.id === equivalentIngredientId)?.baseUnit ?? equivalentUnit).map((unit) => <option key={unit}>{unit}</option>)}
              </select></label>
              <button className="button secondary" type="submit" disabled={!data.ingredients.length || !data.recipeMeasures.length}>Save equivalent</button>
            </form>

            <div className="ingredient-list">
              {data.ingredients.length === 0 && <div className="empty-state"><span aria-hidden="true">◌</span><p>Your shared ingredient list is waiting for its first addition.</p></div>}
              {data.ingredients.map((ingredient) => (
                <article className="ingredient-card" key={ingredient.id}>
                  <div className="ingredient-title">
                    <div className="ingredient-icon" aria-hidden="true">✳</div>
                    <div>
                      {renameIngredientId === ingredient.id ? (
                        <form className="rename-form" onSubmit={(event) => submit(event, async () => {
                          await request(`/api/ingredients/${ingredient.id}`, { method: 'PUT', body: JSON.stringify({ name: renameIngredientText }) })
                          setRenameIngredientId('')
                          await refresh()
                        }, 'Ingredient renamed.')}>
                          <input aria-label="Ingredient name" required maxLength={120} value={renameIngredientText} onChange={(event) => setRenameIngredientText(event.target.value)} />
                          <button className="text-button" type="submit">Save</button>
                          <button className="text-button muted" type="button" onClick={() => setRenameIngredientId('')}>Cancel</button>
                        </form>
                      ) : <><h3>{ingredient.name}</h3><span className="unit-tag">tracked in {ingredient.baseUnit}</span></>}
                    </div>
                    {renameIngredientId !== ingredient.id && <>
                      <button className="text-button edit-link" type="button" onClick={() => { setRenameIngredientId(ingredient.id); setRenameIngredientText(ingredient.name) }}>Rename</button>
                      <button className="text-button danger-text" type="button" aria-label={`Delete ${ingredient.name}`} onClick={() => {
                        if (window.confirm(`Delete ${ingredient.name}? Package sizes and recipe equivalents will also be deleted. Ingredients used in a meal must first be removed from that meal.`)) {
                          void run(async () => { await request(`/api/ingredients/${ingredient.id}`, { method: 'DELETE' }); await refresh() }, 'Ingredient deleted.')
                        }
                      }}>Delete</button>
                    </>}
                  </div>
                  <div className="ingredient-details">
                    <div><span className="detail-label">Package sizes</span>
                      {ingredient.packageSizes.length ? <div className="chips">{ingredient.packageSizes.map((size) => <span className="chip" key={size.id}>{size.label}<button type="button" aria-label={`Remove ${size.label} package size for ${ingredient.name}`} onClick={() => {
                        if (window.confirm(`Delete the ${size.label} package size for ${ingredient.name}?`)) {
                          void run(async () => { await request(`/api/ingredients/${ingredient.id}/packages/${size.id}`, { method: 'DELETE' }); await refresh() }, 'Package size removed.')
                        }
                      }}>×</button></span>)}</div> : <span className="muted">None added</span>}
                    </div>
                    <div><span className="detail-label">Recipe equivalents</span>
                      {ingredient.measureEquivalents.length ? <div className="chips">{ingredient.measureEquivalents.map((equivalent) => <span className="chip soft" key={equivalent.recipeMeasureId}>{equivalent.measureName} = {equivalent.quantity} {equivalent.unit}</span>)}</div> : <span className="muted">None defined</span>}
                    </div>
                  </div>
                </article>
              ))}
            </div>
          </section>

          <section className="section" id="meals">
            <div className="section-heading">
              <div><p className="eyebrow">THE RECIPE BOX</p><h2>Meals for your table</h2></div>
              <p>Define ingredient amounts for one person. Choose a familiar measure only after its exact equivalent is set.</p>
            </div>
            <div className="meal-layout">
              <form id="meal-editor" className="panel meal-editor" onSubmit={(event) => submit(event, async () => {
                const body = {
                  name: mealName,
                  ingredients: mealLines.map((line) => ({
                    ingredientId: line.ingredientId,
                    quantity: line.quantity,
                    unit: line.kind === 'unit' ? line.unit : null,
                    recipeMeasureId: line.kind === 'measure' ? line.recipeMeasureId : null,
                  })),
                }
                await request(mealId ? `/api/meals/${mealId}` : '/api/meals', { method: mealId ? 'PUT' : 'POST', body: JSON.stringify(body) })
                setMealName('')
                setMealId('')
                setMealLines([emptyLine(data.ingredients[0])])
                await refresh()
              }, mealId ? 'Meal updated.' : 'Meal saved.')}>
                <div className="form-title"><div><span className="eyebrow">ONE-PERSON RECIPE</span><h3>{mealId ? 'Edit meal' : 'Create a meal'}</h3></div>{mealId && <button className="text-button" type="button" onClick={() => { setMealId(''); setMealName(''); setMealLines([emptyLine(data.ingredients[0])]) }}>Cancel edit</button>}</div>
                <label>Meal name<input required maxLength={120} value={mealName} onChange={(event) => setMealName(event.target.value)} placeholder="e.g. Vegetable soup" /></label>
                <div className="line-heading"><span>Ingredients for one person</span><button className="text-button" type="button" onClick={() => setMealLines((lines) => [...lines, emptyLine(data.ingredients[0])])} disabled={!data.ingredients.length}>＋ Add row</button></div>
                {mealLines.map((line) => {
                  const ingredient = data.ingredients.find((item) => item.id === line.ingredientId)
                  const validMeasures = data.recipeMeasures.filter((measure) =>
                    ingredient?.measureEquivalents.some((equivalent) => equivalent.recipeMeasureId === measure.id),
                  )
                  return (
                    <div className="meal-line" key={line.key}>
                      <label className="wide">Ingredient<select required value={line.ingredientId} onChange={(event) => {
                        const nextIngredient = data.ingredients.find((item) => item.id === event.target.value)
                        updateLine(line.key, { ingredientId: event.target.value, unit: nextIngredient?.baseUnit ?? 'g', recipeMeasureId: '' })
                      }}><option value="">Choose…</option>{data.ingredients.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
                      <label>Amount<input required inputMode="decimal" value={line.quantity} onChange={(event) => updateLine(line.key, { quantity: event.target.value })} placeholder="e.g. 150" /></label>
                      <label>As<select value={line.kind} onChange={(event) => updateLine(line.key, { kind: event.target.value as MealDraftLine['kind'] })}><option value="unit">Unit</option><option value="measure">Measure</option></select></label>
                      {line.kind === 'unit' ? (
                        <label>Unit<select value={line.unit} onChange={(event) => updateLine(line.key, { unit: event.target.value as Unit })}>
                          {compatibleUnits(ingredient?.baseUnit ?? line.unit).map((unit) => <option key={unit}>{unit}</option>)}
                        </select></label>
                      ) : (
                        <label>Measure<select required value={line.recipeMeasureId} onChange={(event) => updateLine(line.key, { recipeMeasureId: event.target.value })}>
                          <option value="">Choose…</option>{data.recipeMeasures.map((measure) => <option key={measure.id} value={measure.id} disabled={!validMeasures.some((item) => item.id === measure.id)}>{measure.name}{!validMeasures.some((item) => item.id === measure.id) ? ' — define equivalent first' : ''}</option>)}
                        </select></label>
                      )}
                      <button className="remove-row" type="button" aria-label="Remove ingredient row" onClick={() => setMealLines((lines) => lines.filter((item) => item.key !== line.key))} disabled={mealLines.length === 1}>×</button>
                    </div>
                  )
                })}
                <button className="button primary full" type="submit" disabled={!data.ingredients.length}>{mealId ? 'Save meal changes' : 'Save meal'} <span aria-hidden="true">↗</span></button>
              </form>

              <div className="meal-list">
                {data.meals.length === 0 && <div className="empty-state"><span aria-hidden="true">◌</span><p>Your first saved meal will show up here.</p></div>}
                {data.meals.map((meal) => (
                  <article className="meal-card" key={meal.id}>
                    <div className="meal-card-top"><div><span className="eyebrow">MEAL</span><h3>{meal.name}</h3></div><div className="card-actions"><button className="text-button" type="button" onClick={() => startMealEdit(meal)}>Edit</button><button className="text-button danger-text" type="button" onClick={() => void run(async () => { await request(`/api/meals/${meal.id}`, { method: 'DELETE' }); setMealPeople((current) => { const next = { ...current }; delete next[meal.id]; return next }); await refresh() }, 'Meal removed.')}>Delete</button></div></div>
                    <ul className="meal-ingredients">{meal.ingredients.map((line) => <li key={line.id}><span>{line.ingredientName}</span><span>{line.quantity} {line.recipeMeasureId ? line.measureName : line.unit}</span></li>)}</ul>
                    <label className="people-picker">People for grocery list<input min="0" step="1" type="number" inputMode="numeric" value={mealPeople[meal.id] ?? ''} onChange={(event) => setMealPeople((current) => ({ ...current, [meal.id]: event.target.value }))} placeholder="0" /></label>
                  </article>
                ))}
              </div>
            </div>
          </section>

          <section className="section" id="grocery-builder">
            <div className="section-heading">
              <div><p className="eyebrow">THE WEEK AHEAD</p><h2>Build a grocery list</h2></div>
              <p>Choose meals and people counts. Required amounts stay separate from package recommendations.</p>
            </div>
            <form className="panel builder-panel" onSubmit={(event) => submit(event, async () => {
              const meals = selectedMeals.map((meal) => ({ mealId: meal.id, peopleCount: Number(mealPeople[meal.id]) }))
              await request('/api/grocery-lists', {
                method: 'POST',
                body: JSON.stringify({
                  name: groceryName,
                  meals,
                  selectedPackageSizeIds: selectedPackageIds.filter((id) =>
                    selectedIngredients.some((ingredient) => ingredient.packageSizes.some((size) => size.id === id)),
                  ),
                }),
              })
              setGroceryName('')
              await refresh()
              document.getElementById('lists')?.scrollIntoView({ behavior: 'smooth' })
            }, 'Grocery list saved.')}>
              <div className="builder-top">
                <label className="list-name">List name<input required maxLength={160} value={groceryName} onChange={(event) => setGroceryName(event.target.value)} placeholder="Give this list a name" /></label>
                <div className="selection-summary"><span>{selectedMeals.length}</span> meals selected</div>
              </div>
              {selectedMeals.length === 0 ? <p className="inline-hint">Set a people count on one or more meals above to start a list.</p> : (
                <div className="package-selection">
                  <h3>Package sizes to consider</h3>
                  <p className="form-hint">Choose any available sizes. Leave all unchecked for an ingredient to show its required amount without a package recommendation.</p>
                  {selectedIngredients.map((ingredient) => (
                    <fieldset className="package-group" key={ingredient.id}>
                      <legend>{ingredient.name}</legend>
                      {ingredient.packageSizes.length ? ingredient.packageSizes.map((size) => (
                        <label className="check-option" key={size.id}>
                          <input type="checkbox" checked={selectedPackageIds.includes(size.id)} onChange={(event) => setSelectedPackageIds((current) => event.target.checked ? [...current, size.id] : current.filter((id) => id !== size.id))} />
                          <span>{size.label}</span>
                        </label>
                      )) : <span className="muted">No package sizes configured</span>}
                    </fieldset>
                  ))}
                </div>
              )}
              <button className="button dark" type="submit" disabled={!selectedMeals.length}>Calculate & save list <span aria-hidden="true">↗</span></button>
            </form>
          </section>

          <section className="section lists-section" id="lists">
            <div className="section-heading">
              <div><p className="eyebrow">SAVED FOR LATER</p><h2>Grocery lists</h2></div>
              <p>Lists keep their saved contents even when meal definitions or measure equivalents change.</p>
            </div>
            <form className="panel inline-form add-item-form" onSubmit={(event) => submit(event, async () => {
              const ingredient = data.ingredients.find((item) => item.id === addIngredientId)
              if (!ingredient) throw new Error('Choose an ingredient.')
              await request(`/api/grocery-lists/${addListId}/items`, {
                method: 'POST',
                body: JSON.stringify({ ingredientId: ingredient.id, quantity: addQuantity, selectedPackageSizeIds: addPackageIds }),
              })
              setAddQuantity('')
              await refresh()
            }, 'Item added to the saved list.')}>
              <div><h3>Add an item to a saved list</h3><p className="form-hint">This changes only the saved list, never a meal.</p></div>
              <label>List<select required value={addListId} onChange={(event) => setAddListId(event.target.value)}><option value="">Choose…</option>{data.groceryLists.map((list) => <option key={list.id} value={list.id}>{list.name}</option>)}</select></label>
              <label>Ingredient<select required value={addIngredientId} onChange={(event) => { setAddIngredientId(event.target.value); setAddPackageIds([]) }}><option value="">Choose…</option>{data.ingredients.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
              <label>Required amount<input required inputMode="decimal" value={addQuantity} onChange={(event) => setAddQuantity(event.target.value)} placeholder="e.g. 200" /></label>
              <label>Unit<select value={data.ingredients.find((item) => item.id === addIngredientId)?.baseUnit ?? 'g'} disabled><option>{data.ingredients.find((item) => item.id === addIngredientId)?.baseUnit ?? 'g'}</option></select></label>
              <button className="button secondary" type="submit" disabled={!data.groceryLists.length || !data.ingredients.length}>Add item</button>
              {data.ingredients.find((item) => item.id === addIngredientId)?.packageSizes.map((size) => (
                <label className="check-option add-package-option" key={size.id}>
                  <input type="checkbox" checked={addPackageIds.includes(size.id)} onChange={(event) => setAddPackageIds((current) => event.target.checked ? [...current, size.id] : current.filter((id) => id !== size.id))} />
                  <span>{size.label}</span>
                </label>
              ))}
            </form>
            {data.groceryLists.length === 0 && <div className="empty-state"><span aria-hidden="true">◌</span><p>Saved grocery lists will appear here.</p></div>}
            <div className="saved-lists">
              {data.groceryLists.map((list) => {
                const draftItems = listDrafts[list.id] ?? list.items.map((item) => ({ id: item.id, quantity: item.quantity }))
                return (
                  <article className="saved-list-card" key={list.id}>
                    <div className="saved-list-heading">
                      <div><p className="eyebrow">{new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(list.createdAt))}</p><h3>{list.name}</h3></div>
                      <button className="text-button danger-text" type="button" onClick={() => void run(async () => { await request(`/api/grocery-lists/${list.id}`, { method: 'DELETE' }); await refresh() }, 'Grocery list deleted.')}>Delete list</button>
                    </div>
                    <p className="meal-summary">{list.selections.map((selection) => `${selection.mealName} · ${selection.peopleCount} ${selection.peopleCount === 1 ? 'person' : 'people'}`).join('  /  ')}</p>
                    {draftItems.length === 0 ? <p className="inline-hint">This list has no items.</p> : (
                      <div className="grocery-items">
                        {draftItems.map((draft) => {
                          const item = list.items.find((entry) => entry.id === draft.id)
                          if (!item) return null
                          return (
                            <div className="grocery-item" key={item.id}>
                              <div><strong>{item.ingredientName}</strong><span className="required-amount">Needed: {draft.quantity} {item.unit}</span></div>
                              <label className="amount-edit">Amount<input inputMode="decimal" value={draft.quantity} onChange={(event) => setListDrafts((current) => ({ ...current, [list.id]: draftItems.map((entry) => entry.id === draft.id ? { ...entry, quantity: event.target.value } : entry) }))} /></label>
                              <button className="text-button danger-text" type="button" onClick={() => setListDrafts((current) => ({ ...current, [list.id]: draftItems.filter((entry) => entry.id !== draft.id) }))}>Remove</button>
                              <div className="package-results">
                                {item.packageCombinations.length ? item.packageCombinations.map((combination, index) => <span className="package-result" key={`${item.id}-${index}`}>{combination.packages.map((pkg) => `${pkg.count} × ${pkg.label}`).join(' + ')}</span>) : <span className="muted">{item.packageOptions.length ? 'No selected package combination' : 'No package sizes selected'}</span>}
                              </div>
                            </div>
                          )
                        })}
                      </div>
                    )}
                    <div className="list-footer"><span className="muted">Edits are saved to this list only.</span><button className="button secondary" type="button" onClick={() => void run(async () => {
                      await request(`/api/grocery-lists/${list.id}/items`, {
                        method: 'PUT',
                        body: JSON.stringify({ items: draftItems.map((item) => ({ id: item.id, quantity: item.quantity })) }),
                      })
                      await refresh()
                    }, 'Saved list updated.')}>Save list changes</button></div>
                  </article>
                )
              })}
            </div>
          </section>
        </>
      )}
      <footer className="footer"><span>Made for everyday meals.</span><a href="#top">Back to top ↑</a></footer>
    </main>
  )
}

export default App
