import { mkdir, writeFile } from 'node:fs/promises'
import { modules } from './modules.js'

const base = process.env.OPENAPI_BASE_URL ?? 'http://127.0.0.1:5001'
await mkdir('openapi', { recursive: true })
for (const module of modules) {
  const response = await fetch(`${base}/swagger/${module.name}/swagger.json`)
  if (!response.ok) throw new Error(`OpenAPI fetch for ${module.name}: ${response.status}`)
  const spec = await response.json()
  if (!spec.paths || !spec.openapi) throw new Error(`Invalid OpenAPI document: ${module.name}`)
  await writeFile(`openapi/${module.specFile}`, JSON.stringify(spec, null, 2))
}
