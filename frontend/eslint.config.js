import js from '@eslint/js';
import stylistic from '@stylistic/eslint-plugin';
import jsdoc from 'eslint-plugin-jsdoc';
import svelte from 'eslint-plugin-svelte';
import globals from 'globals';
import ts from 'typescript-eslint';

const allowedNumbers = [-1, 0, 1, 2];

export default ts.config(
  {ignores: ['.svelte-kit/', 'build/', '.vercel/', 'node_modules/']},
  js.configs.recommended,
  ...ts.configs.strict,
  ...svelte.configs.recommended,
  {
    languageOptions: {globals: {...globals.browser, ...globals.node}},
    plugins: {'@stylistic': stylistic, jsdoc},
    rules: {
      '@stylistic/indent': ['error', 2],
      '@stylistic/semi': ['error', 'always'],
      '@stylistic/quotes': ['error', 'single', {avoidEscape: true}],
      '@stylistic/brace-style': ['error', 'stroustrup'],
      '@stylistic/object-curly-spacing': ['error', 'never'],
      '@stylistic/comma-dangle': ['error', 'always-multiline'],
      '@stylistic/max-len': ['error', {code: 120, ignoreStrings: true, ignoreUrls: true}],
      'curly': ['error', 'all'],
      'prefer-const': ['error', {destructuring: 'all'}],
      'svelte/no-navigation-without-resolve': ['error', {ignoreLinks: true}],
      'no-ternary': 'error',
      'no-console': 'error',
      '@typescript-eslint/no-magic-numbers': ['error', {
        ignore: allowedNumbers,
        ignoreEnums: true,
        ignoreReadonlyClassProperties: true,
        ignoreTypeIndexes: true,
      }],
      'jsdoc/require-jsdoc': ['error', {
        publicOnly: false,
        require: {
          FunctionDeclaration: true,
          ArrowFunctionExpression: false,
          ClassDeclaration: true,
          MethodDefinition: true,
        },
        contexts: ['ExportNamedDeclaration > VariableDeclaration > VariableDeclarator > ArrowFunctionExpression'],
      }],
    },
  },
  {
    files: ['**/*.svelte', '**/*.svelte.ts'],
    languageOptions: {parserOptions: {parser: ts.parser, extraFileExtensions: ['.svelte']}},
    rules: {'@stylistic/indent': 'off'},
  },
  {
    files: ['**/*.test.ts'],
    rules: {'@typescript-eslint/no-magic-numbers': 'off'},
  },
);
