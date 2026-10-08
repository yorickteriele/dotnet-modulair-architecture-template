import { mkdir } from 'node:fs/promises'
import { spawnSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'
import path from 'node:path'
import { modules } from '../../src/frontend/scripts/modules.js'

const root = fileURLToPath(new URL('../../', import.meta.url))
if (!modules.length) throw new Error('No API modules discovered')
const packages = path.join(root, '.local/packages')
await mkdir(packages, { recursive: true })
const result = spawnSync('tar', ['-czf', path.join(packages, 'api-clients.tar.gz'),
  '-C', path.join(root, 'src/frontend'), 'openapi', ...modules.map(module => module.output)], { stdio: 'inherit' })
if (result.error) throw result.error
if (result.status !== 0) process.exit(result.status ?? 1)
