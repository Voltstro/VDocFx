import { defineConfig, UserConfig } from 'vite'
import { resolve } from 'path'

export default defineConfig(({mode}) => {
  let config: UserConfig = {
    build: {
      //No minify in dev builds, speeds shit up
      minify: false,
      emptyOutDir: false,
      rollupOptions: {
        input: {
            main: resolve(__dirname, 'src/docfx/docfx.ts')
        },
        output: {
            entryFileNames: () => 'assets/[name].js',
            assetFileNames: () => 'assets/[name][extname]',
            sourcemap: false
        },
      },
    }
  }

  if(mode == 'production') {
    //In non-dev builds, we will use terser to minify everything
    console.log('Non-dev build, using terser...')
    config.build!.minify = 'terser';
    config.build!.terserOptions = {
        format: {
            comments: false
        },
        compress: {
            defaults: true,
            drop_console: true,
            drop_debugger: true
        },
        ecma: 2020
    };
  }

  return config;
})