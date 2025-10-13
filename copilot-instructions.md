# Copilot Instructions for Novex Project

## Package Manager

- **ALWAYS use `pnpm` instead of `npm` or `yarn`** for all package management operations
- When installing packages: `pnpm install` or `pnpm add <package>`
- When running scripts: `pnpm run <script>`
- When removing packages: `pnpm remove <package>`

## Project Structure

- Main Blazor application: `code/Novex.Web/`
- CodeMirror editor build: `code/editor-build/`
- Data layer: `code/Novex.Data/`
- Test data: `TestData/`
- Documentation: `docs/`

## Development Workflow

1. For CodeMirror changes: work in `code/editor-build/`
2. Build with: `pnpm run build:prod`
3. Copy to Blazor project with: `pnpm run copy`
4. Test in Blazor application

## Code Style

- Use TypeScript for JavaScript projects
- Use C# coding conventions for .NET projects
- Prefer explicit types and interfaces
- Add comprehensive JSDoc comments for public methods

## Editor Build Project

- Uses esbuild for fast compilation
- TypeScript with strict settings
- CodeMirror 6 with Markdown support
- Outputs bundled JavaScript for Blazor integration
