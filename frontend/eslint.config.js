import js from '@eslint/js'
import pluginVue from 'eslint-plugin-vue'
import sonarjs from 'eslint-plugin-sonarjs'

export default [
  js.configs.recommended,
  ...pluginVue.configs['flat/recommended'],
  sonarjs.configs.recommended,
  {
    files: ['src/**/*.{js,vue}'],
  },
  {
    ignores: ['dist/', 'node_modules/']
  }
]
