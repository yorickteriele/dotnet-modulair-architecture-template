import { execFileSync } from 'node:child_process'
import { mkdir, readFile, writeFile } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { modules } from './modules.js'

// Exports each module's OpenAPI document from the built Host with the Swashbuckle CLI, so no running API or database
// is needed. The CLI runs Program.cs without a server: the placeholders below only satisfy startup validation and are
// never used, while real values from the environment still win.
const root = fileURLToPath(new URL('../../../', import.meta.url))
const hostOutput = path.join(root, 'src/backend/Host/bin/Debug/net10.0')
execFileSync('dotnet', ['build', path.join(root, 'src/backend/Host'), '--nologo', '-v', 'q'], { stdio: 'inherit' })
const env = {
  ASPNETCORE_ENVIRONMENT: 'Development',
  ConnectionStrings__DefaultConnection: 'Host=openapi-export-unused',
  Jwt__Secret: 'openapi-export-placeholder-secret-0123456789',
  ...process.env,
  ASPNETCORE_CONTENTROOT: hostOutput,
}
await mkdir('openapi', { recursive: true })
for (const module of modules) {
  const file = `openapi/${module.specFile}`
  execFileSync('dotnet', ['tool', 'run', 'swagger', 'tofile', '--output', file, path.join(hostOutput, 'Host.dll'), module.name],
    { env, stdio: ['ignore', 'ignore', 'inherit'] })
  const spec = JSON.parse(await readFile(file, 'utf8'))
  if (!spec.paths || !spec.openapi) throw new Error(`Invalid OpenAPI document: ${module.name}`)
  await writeFile(file, JSON.stringify(spec, null, 2))
}
