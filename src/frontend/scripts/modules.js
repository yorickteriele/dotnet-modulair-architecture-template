import { access, readdir } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

function pascalCase(value) {
  return value
    .split(/[^a-zA-Z0-9]+/)
    .filter(Boolean)
    .map((segment) => segment.charAt(0).toUpperCase() + segment.slice(1))
    .join('')
}

async function discoverModules() {
  const __filename = fileURLToPath(import.meta.url)
  const __dirname = path.dirname(__filename)
  const frontendRoot = path.resolve(__dirname, '..')
  const modulesDir = path.join(frontendRoot, 'src', 'modules')

  const entries = await readdir(modulesDir, { withFileTypes: true })
  const moduleNames = entries
    .filter((entry) => entry.isDirectory())
    .map((entry) => entry.name)
    .filter((name) => name !== 'shared')

  const moduleConfigs = await Promise.all(
    moduleNames.map(async (name) => {
      try {
        await access(path.join(modulesDir, name, 'api'))
      } catch {
        return null
      }

      return {
        name,
        specFile: `${name}.openapi.json`,
        output: `src/modules/${name}/api/generated/api-client.ts`,
        className: `${pascalCase(name)}ApiClient`,
      }
    }),
  )

  return moduleConfigs.filter(Boolean)
}

export const modules = await discoverModules()
