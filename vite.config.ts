import vue from '@vitejs/plugin-vue'
import { createReadStream, existsSync, statSync } from 'node:fs'
import { cp } from 'node:fs/promises'
import { extname, join, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig, type Plugin } from 'vite'

// Vditor 会在运行时按 `${cdn}/dist/...` 动态加载 lute、图标、语言包、代码高亮等资源，
// 这里把 node_modules 里的产物挂到本地，避免依赖外部 CDN。
const VDITOR_DIST = fileURLToPath(new URL('./node_modules/vditor/dist', import.meta.url))
const VDITOR_ASSET_DIRS = ['css', 'images', 'js/lute', 'js/i18n', 'js/icons', 'js/highlight.js', 'js/katex']

const MIME_TYPES: Record<string, string> = {
  '.js': 'text/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.map': 'application/json',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.gif': 'image/gif',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
  '.ttf': 'font/ttf'
}

function vditorAssets(): Plugin {
  let mount = '/vditor/dist'
  let outDir = 'dist'

  return {
    name: 'vditor-assets',
    configResolved(config) {
      mount = `${config.base.replace(/\/$/, '')}/vditor/dist`
      outDir = resolve(config.root, config.build.outDir)
    },
    configureServer(server) {
      server.middlewares.use(mount, (req, res, next) => {
        const pathname = decodeURIComponent((req.url ?? '/').split('?')[0])
        const file = resolve(VDITOR_DIST, `.${pathname}`)
        if (!file.startsWith(VDITOR_DIST) || !existsSync(file) || !statSync(file).isFile()) {
          next()
          return
        }
        res.setHeader('Content-Type', MIME_TYPES[extname(file).toLowerCase()] ?? 'application/octet-stream')
        createReadStream(file).pipe(res)
      })
    },
    async closeBundle() {
      for (const dir of VDITOR_ASSET_DIRS) {
        await cp(join(VDITOR_DIST, dir), join(outDir, 'vditor/dist', dir), { recursive: true })
      }
    }
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), vditorAssets()],
  server: {
    port: 5574
  }
})
